from __future__ import annotations

import argparse
import csv
import json
import os
import sys
import time
from collections import Counter, defaultdict
from datetime import date, datetime
from decimal import Decimal
from pathlib import Path
from typing import Any

import pymssql
import requests

sys.path.append(str(Path(__file__).resolve().parent))
from compare_sales_order_excel_to_ocp import (  # noqa: E402
    decimal_to_str,
    normalize_date,
    normalize_decimal,
    normalize_text,
    read_excel_rows,
    write_csv,
)
from simulate_wz_output_logic_against_excel import compare_excel  # noqa: E402


RULE_URL = "http://10.11.10.101:8000/batch_infer?debug_trace=false"
DB_SERVER = os.environ.get("OCP_DB_SERVER", "10.11.0.18")
DB_NAME = os.environ.get("OCP_DB_NAME", "OCP_Service")
DB_USER = os.environ.get("OCP_DB_USER", "SysUser")
DB_PASSWORD = os.environ.get("OCP_DB_PASSWORD", "")

LINE_PREFIXES = ("旋转", "直通", "蝶阀")
SOFT_SEAL = "软密封球阀"
HARD_SEAL = "硬密封球阀"
STRAIGHT = "直通阀"
BUTTERFLY = "蝶阀"

BUTTERFLY_GROUP_1 = {"DN50", "DN65", "DN80", "DN100", "DN125", "DN150"}
BUTTERFLY_GROUP_2 = {"DN200", "DN250", "DN300", "DN100"}
BUTTERFLY_GROUP_3 = {"DN350", "DN400", "DN450", "DN500", "DN600"}
SOFT_SEAL_LINES = {"旋转1", "旋转2", "旋转3", "旋转4", "旋转5"}

MISSING_LIST = {"ZJFR-1001", "ZJFR-1100", "ZJFR-1101", "ZJFRB", "ZJFRF", "ZJGWG-0.1"}
SPECIAL_CASES = {
    "H41H-16C": SOFT_SEAL,
    "H41H-16P": SOFT_SEAL,
    "H420-T": SOFT_SEAL,
    "H44Y-10P": SOFT_SEAL,
    "REM": SOFT_SEAL,
    "VFR": HARD_SEAL,
    "JVBJG": BUTTERFLY,
}
SOFT_SEAL_NUMS = {1000, 1001, 1003, 1010, 1011, 2000, 2010, 4000, 4010}
HARD_SEAL_NUMS = {1100, 1101, 1110, 1111, 1113, 2100, 2110, 3110, 3111, 4100, 3}


def read_csv_rows(path: Path) -> list[dict[str, str]]:
    with path.open("r", encoding="utf-8-sig", newline="") as f:
        return list(csv.DictReader(f))


def chunked(values: list[Any], size: int):
    for index in range(0, len(values), size):
        yield values[index : index + size]


def to_json_date(value: Any) -> str | None:
    if value is None:
        return None
    if isinstance(value, datetime):
        return value.isoformat()
    if isinstance(value, date):
        return datetime(value.year, value.month, value.day).isoformat()
    text = normalize_text(value)
    return text or None


def normalize_sync_line(value: Any) -> str:
    text = normalize_text(value)
    if not text or "车间" in text:
        return ""
    return text if text.startswith(LINE_PREFIXES) else ""


def try_special_case(value: str) -> str | None:
    text = normalize_text(value)
    if not text:
        return None
    if text in SPECIAL_CASES:
        return SPECIAL_CASES[text]
    if text.upper().startswith("VFR"):
        return HARD_SEAL
    return None


def try_judge_product(product_name: str) -> str | None:
    text = normalize_text(product_name)
    if not text or text in MISSING_LIST:
        return None

    special = try_special_case(text)
    if special:
        return special

    upper = text.upper()
    if upper.startswith("ZJH"):
        if upper.startswith(("ZJHQR", "ZJHRF", "ZJHVF")):
            return SOFT_SEAL
        if upper.startswith("ZJHV"):
            return HARD_SEAL
        if upper.startswith("ZJHR-"):
            suffix = upper[len("ZJHR-") :]
            digits = ""
            for char in suffix:
                if char.isdigit():
                    digits += char
                else:
                    break
            if digits:
                number = int(digits)
                if number in SOFT_SEAL_NUMS:
                    return SOFT_SEAL
                if number in HARD_SEAL_NUMS:
                    return HARD_SEAL
            return HARD_SEAL
        if upper.startswith("ZJHR"):
            return HARD_SEAL

    if upper.startswith("ZZ"):
        return STRAIGHT
    if upper.startswith("V"):
        return BUTTERFLY
    if upper.startswith("K"):
        return STRAIGHT
    if upper.startswith(("P", "Q", "T")):
        return SOFT_SEAL
    if upper.startswith("J"):
        return SOFT_SEAL
    if upper.startswith("H"):
        return STRAIGHT
    if upper.startswith("R"):
        return HARD_SEAL
    if upper.startswith("Z") and not upper.startswith(("ZZ", "ZJH")):
        return SOFT_SEAL
    return None


def try_judge_by_spec_or_product(spec_model: str, product_name: str) -> str | None:
    return try_special_case(spec_model) or try_judge_product(product_name)


def calc_assigned_line(production_line: str, valve_category: str, nominal_diameter: str) -> str:
    line = normalize_text(production_line)
    category = normalize_text(valve_category)
    diameter = normalize_text(nominal_diameter)
    if not line:
        return ""
    if line.startswith("旋转") and category == BUTTERFLY:
        if diameter in BUTTERFLY_GROUP_1:
            return "蝶阀1"
        if diameter in BUTTERFLY_GROUP_2:
            return "蝶阀2"
        if diameter in BUTTERFLY_GROUP_3:
            return "蝶阀3"
        return "蝶阀4"
    if category.startswith("直通"):
        return line
    if line in SOFT_SEAL_LINES and category == SOFT_SEAL:
        return line + "A"
    return ""


def resolve_assignment(candidate: dict[str, Any], rule_line: str = "", by_rule: bool = False) -> dict[str, Any] | None:
    valve_category = normalize_text(candidate.get("ValveCategory"))
    used_rule = False
    if not valve_category:
        inferred = try_judge_by_spec_or_product(candidate.get("SpecModel", ""), candidate.get("ProductName", ""))
        if inferred:
            valve_category = inferred
            used_rule = True

    raw_line = normalize_text(rule_line or candidate.get("ProductionLine"))
    assigned_line = normalize_text(candidate.get("AssignedProductionLine"))
    if not assigned_line and raw_line:
        assigned_line = calc_assigned_line(raw_line, valve_category, candidate.get("NominalDiameter", ""))
        used_rule = used_rule or bool(assigned_line)
    if not assigned_line:
        assigned_line = raw_line
    if not valve_category or not assigned_line:
        return None

    status = "matched_rule" if by_rule or used_rule or candidate.get("IsRuleServiceCandidate") else "matched_sync_line"
    return {
        "EntryId": normalize_text(candidate.get("EntryId")),
        "BillPlanKey": bill_plan_key(candidate.get("SalesOrderNo"), candidate.get("PlanTrackingNo")),
        "ValveCategory": valve_category,
        "ProductionLine": assigned_line,
        "ProductionDate": normalize_date(candidate.get("ProductionDate")),
        "ClassifyStatus": status,
    }


def bill_plan_key(bill_no: Any, plan_no: Any) -> str:
    return normalize_text(bill_no) + "\x1f" + normalize_text(plan_no)


def detail_bill_plan_key(row: dict[str, str]) -> str:
    return bill_plan_key(row.get("BillNo"), row.get("PlanTrackingNo"))


def fetch_ocp_candidates(unresolved: list[dict[str, str]]) -> list[dict[str, Any]]:
    wanted_keys = {
        detail_bill_plan_key(row)
        for row in unresolved
        if normalize_text(row.get("BillNo")) and normalize_text(row.get("PlanTrackingNo"))
    }
    bill_nos = sorted({normalize_text(row.get("BillNo")) for row in unresolved if normalize_text(row.get("BillNo"))})
    if not bill_nos:
        return []
    if not DB_PASSWORD:
        raise RuntimeError("请先设置环境变量 OCP_DB_PASSWORD；可选 OCP_DB_SERVER/OCP_DB_NAME/OCP_DB_USER 覆盖默认连接信息。")

    conn = pymssql.connect(
        server=DB_SERVER,
        user=DB_USER,
        password=DB_PASSWORD,
        database=DB_NAME,
        login_timeout=10,
        timeout=120,
        charset="UTF-8",
    )
    try:
        cur = conn.cursor(as_dict=True)
        order_rows: list[dict[str, Any]] = []
        for chunk in chunked(bill_nos, 500):
            placeholders = ",".join(["%s"] * len(chunk))
            cur.execute(
                f"""
SELECT SOEntryID, SOBillNo, MtoNo, MaterialNumber, PrdScheduleDate,
       OrderAuditDate, ReplyDeliveryDate, DeliveryDate
FROM dbo.OCP_OrderTracking WITH (NOLOCK)
WHERE SOBillNo IN ({placeholders})
""",
                tuple(chunk),
            )
            for row in cur.fetchall():
                if bill_plan_key(row.get("SOBillNo"), row.get("MtoNo")) in wanted_keys:
                    order_rows.append(row)

        material_codes = sorted(
            {
                normalize_text(row.get("MaterialNumber"))
                for row in order_rows
                if normalize_text(row.get("MaterialNumber"))
            }
        )
        materials: dict[str, dict[str, Any]] = {}
        for chunk in chunked(material_codes, 500):
            placeholders = ",".join(["%s"] * len(chunk))
            cur.execute(
                f"""
SELECT MaterialCode, SpecModel, ProductModel, NominalDiameter, NominalPressure,
       BodyMaterial, TrimMaterial, InnerMaterial, FlangeConnection, BonnetForm,
       FlowCharacteristic, ActuatorModel, Accessories, FlangeSealType,
       SealFaceForm, ValveCategory, Workshop
FROM dbo.OCP_Material WITH (NOLOCK)
WHERE MaterialCode IN ({placeholders})
""",
                tuple(chunk),
            )
            for row in cur.fetchall():
                key = normalize_text(row.get("MaterialCode"))
                if key and key not in materials:
                    materials[key] = row
    finally:
        conn.close()

    candidates: list[dict[str, Any]] = []
    for order in order_rows:
        material = materials.get(normalize_text(order.get("MaterialNumber")), {})
        sync_line = normalize_sync_line(material.get("Workshop"))
        valve_category = normalize_text(material.get("ValveCategory"))
        if not valve_category and sync_line:
            valve_category = try_judge_by_spec_or_product(
                material.get("SpecModel", ""),
                material.get("ProductModel", ""),
            ) or ""

        candidates.append(
            {
                "EntryId": normalize_text(order.get("SOEntryID")),
                "SalesOrderNo": normalize_text(order.get("SOBillNo")),
                "PlanTrackingNo": normalize_text(order.get("MtoNo")),
                "MaterialNumber": normalize_text(order.get("MaterialNumber")),
                "ValveCategory": valve_category if sync_line else normalize_text(material.get("ValveCategory")),
                "ProductionLine": sync_line,
                "ProductionDate": normalize_date(order.get("PrdScheduleDate")),
                "NominalDiameter": normalize_text(material.get("NominalDiameter")),
                "NominalPressure": normalize_text(material.get("NominalPressure")),
                "SpecModel": normalize_text(material.get("SpecModel")),
                "ProductName": normalize_text(material.get("ProductModel")),
                "BodyMaterial": normalize_text(material.get("BodyMaterial")),
                "InnerMaterial": normalize_text(material.get("TrimMaterial") or material.get("InnerMaterial")),
                "FlangeConnection": normalize_text(material.get("FlangeConnection")),
                "BonnetForm": normalize_text(material.get("BonnetForm")),
                "FlowCharacteristic": normalize_text(material.get("FlowCharacteristic")),
                "Actuator": normalize_text(material.get("ActuatorModel")),
                "AccessoryConfig": normalize_text(material.get("Accessories")),
                "SealFaceForm": normalize_text(material.get("FlangeSealType") or material.get("SealFaceForm")),
                "OrderApprovedDate": order.get("OrderAuditDate"),
                "ReplyDeliveryDate": order.get("ReplyDeliveryDate"),
                "RequestedDeliveryDate": order.get("DeliveryDate"),
                "IsRuleServiceCandidate": True,
                "IsSyncProductionLineCandidate": bool(sync_line),
                "MatchKey": f"E:{normalize_text(order.get('SOEntryID'))}"
                if normalize_text(order.get("SOEntryID"))
                else bill_plan_key(order.get("SOBillNo"), order.get("MtoNo")),
            }
        )
    return candidates


def resolve_product_text(candidate: dict[str, Any], mode: str) -> str:
    spec = normalize_text(candidate.get("SpecModel"))
    product = normalize_text(candidate.get("ProductName"))
    return spec or product if mode == "spec" else product or spec


def build_rule_payload(candidate: dict[str, Any], mode: str) -> dict[str, Any]:
    return {
        "id": candidate["MatchKey"],
        "OrderApprovedDate": to_json_date(candidate.get("OrderApprovedDate")),
        "ReplyDeliveryDate": to_json_date(candidate.get("ReplyDeliveryDate")),
        "RequestedDeliveryDate": to_json_date(candidate.get("RequestedDeliveryDate")),
        "fa_ti_cai_zhi": candidate.get("BodyMaterial", ""),
        "nei_jian_cai_zhi": candidate.get("InnerMaterial", ""),
        "fa_lan_lian_jie": candidate.get("FlangeConnection", ""),
        "shang_gai_xing_shi": candidate.get("BonnetForm", ""),
        "liu_liang_te_xing": candidate.get("FlowCharacteristic", ""),
        "zhi_xing_ji_gou": candidate.get("Actuator", ""),
        "fu_jian_pei_zhi": candidate.get("AccessoryConfig", ""),
        "wai_gou_fa_ti": "",
        "fa_men_da_lei": "",
        "fa_men_lei_bie": candidate.get("ValveCategory", ""),
        "mi_feng_mian_xing_shi": candidate.get("SealFaceForm", ""),
        "te_pin": "",
        "wai_gou_biao_zhi": "",
        "chan_pin_ming_cheng": resolve_product_text(candidate, mode),
        "gong_cheng_tong_jing": candidate.get("NominalDiameter", ""),
        "gong_cheng_ya_li": candidate.get("NominalPressure", ""),
    }


def call_rule_service(candidates: list[dict[str, Any]], batch_size: int) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    eligible = [
        c
        for c in candidates
        if c.get("OrderApprovedDate")
        and c.get("ReplyDeliveryDate")
        and c.get("RequestedDeliveryDate")
        and normalize_text(c.get("MatchKey"))
    ]
    candidate_by_key = {c["MatchKey"]: c for c in eligible}
    assignments: list[dict[str, Any]] = []
    stats: dict[str, Any] = {
        "eligible": len(eligible),
        "spec_success": 0,
        "fallback_success": 0,
        "spec_failed": 0,
        "fallback_failed": 0,
        "fallback_attempted": 0,
        "reason_counts": Counter(),
    }

    session = requests.Session()

    def send(batch: list[dict[str, Any]], mode: str) -> set[str]:
        payload = [build_rule_payload(candidate, mode) for candidate in batch]
        response = session.post(RULE_URL, json=payload, timeout=300)
        response.raise_for_status()
        body = response.json()
        success_keys: set[str] = set()
        for item in body.get("results", []):
            key = normalize_text(item.get("id") or (item.get("result") or {}).get("id"))
            if item.get("success") and key in candidate_by_key:
                line = normalize_text((item.get("result") or {}).get("sheng_chan_xian"))
                assignment = resolve_assignment(candidate_by_key[key], rule_line=line, by_rule=True)
                if assignment:
                    assignments.append(assignment)
                    success_keys.add(key)
            else:
                stats["reason_counts"][normalize_text(item.get("reason")) or "FAILED"] += 1
        return success_keys

    for start in range(0, len(eligible), batch_size):
        batch = eligible[start : start + batch_size]
        print(f"{datetime.now():%H:%M:%S} rule batch {start + 1}-{start + len(batch)} / {len(eligible)} spec-first", flush=True)
        spec_success = send(batch, "spec")
        stats["spec_success"] += len(spec_success)
        stats["spec_failed"] += len(batch) - len(spec_success)

        fallback_batch = [
            c
            for c in batch
            if c["MatchKey"] not in spec_success
            and normalize_text(c.get("ProductName"))
            and resolve_product_text(c, "spec") != resolve_product_text(c, "product")
        ]
        if fallback_batch:
            stats["fallback_attempted"] += len(fallback_batch)
            print(
                f"{datetime.now():%H:%M:%S} rule batch {start + 1}-{start + len(batch)} / {len(eligible)} product fallback {len(fallback_batch)}",
                flush=True,
            )
            fallback_success = send(fallback_batch, "product")
            stats["fallback_success"] += len(fallback_success)
            stats["fallback_failed"] += len(fallback_batch) - len(fallback_success)
        time.sleep(0.05)
    return assignments, stats


def apply_assignments(details: list[dict[str, str]], assignments: list[dict[str, Any]]) -> dict[str, int]:
    by_entry: dict[str, dict[str, Any]] = {}
    by_bill_plan: dict[str, dict[str, Any]] = {}
    for assignment in assignments:
        entry_id = normalize_text(assignment.get("EntryId"))
        bill_plan = normalize_text(assignment.get("BillPlanKey"))
        if entry_id and entry_id not in by_entry:
            by_entry[entry_id] = assignment
        if bill_plan and bill_plan not in by_bill_plan:
            by_bill_plan[bill_plan] = assignment

    summary = Counter()
    for row in details:
        unresolved = (
            row.get("ClassifyStatus") not in {"matched", "matched_order_cycle", "matched_sync_line", "matched_rule"}
            or not normalize_text(row.get("ValveCategory"))
            or not normalize_text(row.get("ProductionLine"))
        )
        if not unresolved:
            continue
        assignment = by_entry.get(normalize_text(row.get("EntryId"))) or by_bill_plan.get(detail_bill_plan_key(row))
        if not assignment:
            continue
        row["ValveCategory"] = normalize_text(assignment.get("ValveCategory"))
        row["ProductionLine"] = normalize_text(assignment.get("ProductionLine"))
        if normalize_text(assignment.get("ProductionDate")):
            row["ProductionDate"] = normalize_date(assignment.get("ProductionDate"))
        row["ClassifyStatus"] = normalize_text(assignment.get("ClassifyStatus"))
        summary[row["ClassifyStatus"]] += 1
    return dict(summary)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--base-detail", required=True)
    parser.add_argument("--excel", required=True)
    parser.add_argument("--output-dir", default=r"E:\order_platform\exports")
    parser.add_argument("--batch-size", type=int, default=200)
    parser.add_argument("--start", default="2026-07-01")
    parser.add_argument("--end", default="2026-07-31")
    args = parser.parse_args()

    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    stamp = datetime.now().strftime("%Y%m%d_%H%M%S")

    details = read_csv_rows(Path(args.base_detail))
    details = [row for row in details if args.start <= normalize_date(row.get("ProductionDate")) <= args.end]
    unresolved = [
        row
        for row in details
        if row.get("ClassifyStatus") in {"missing_line", "conflict"}
        or not normalize_text(row.get("ValveCategory"))
        or not normalize_text(row.get("ProductionLine"))
    ]

    print(f"{datetime.now():%H:%M:%S} unresolved before: {len(unresolved)}", flush=True)
    candidates = fetch_ocp_candidates(unresolved)
    print(f"{datetime.now():%H:%M:%S} ocp candidates: {len(candidates)}", flush=True)

    direct_assignments = []
    for candidate in candidates:
        if candidate.get("IsSyncProductionLineCandidate"):
            assignment = resolve_assignment(candidate)
            if assignment:
                direct_assignments.append(assignment)
    direct_summary = apply_assignments(details, direct_assignments)

    still_unresolved_keys = {
        detail_bill_plan_key(row)
        for row in details
        if row.get("ClassifyStatus") in {"missing_line", "conflict"}
        or not normalize_text(row.get("ValveCategory"))
        or not normalize_text(row.get("ProductionLine"))
    }
    rule_candidates = [c for c in candidates if bill_plan_key(c.get("SalesOrderNo"), c.get("PlanTrackingNo")) in still_unresolved_keys]
    rule_assignments, rule_stats = call_rule_service(rule_candidates, args.batch_size)
    rule_summary = apply_assignments(details, rule_assignments)

    excel_rows = [
        row
        for row in read_excel_rows(Path(args.excel))
        if args.start <= normalize_date(row["ProductionDate"]) <= args.end
    ]
    compare_rows, compare_stats = compare_excel(details, excel_rows)

    detail_path = output_dir / f"wz_simulated_spec_first_rule_backfill_detail_{stamp}.csv"
    compare_path = output_dir / f"wz_simulated_spec_first_rule_backfill_vs_excel_{stamp}.csv"
    summary_path = output_dir / f"wz_simulated_spec_first_rule_backfill_summary_{stamp}.json"

    detail_fields = [
        "BusinessKey",
        "EntryId",
        "BillNo",
        "PlanTrackingNo",
        "Seq",
        "ProductionDate",
        "ValveCategory",
        "ProductionLine",
        "Quantity",
        "ClassifyStatus",
        "RawRowCount",
        "LineCandidateCount",
        "SourceStartDate",
        "SourceEndDate",
    ]
    compare_fields = [
        "Status",
        "BillNo",
        "PlanTrackingNo",
        "ProductionDate",
        "ExcelQty",
        "SimulatedQty",
        "DiffQty",
        "ExcelRows",
        "SimulatedRows",
        "ClassifyStatus",
        "ValveCategory",
        "ProductionLine",
        "ExcelSourceRow",
        "ExcelCreateDate",
        "EntryId",
        "RawRowCount",
        "LineCandidateCount",
    ]
    write_csv(detail_path, details, detail_fields)
    write_csv(compare_path, compare_rows, compare_fields)

    status_rows = Counter(row["ClassifyStatus"] for row in details)
    status_qty = defaultdict(Decimal)
    for row in details:
        status_qty[row["ClassifyStatus"]] += normalize_decimal(row.get("Quantity"))

    summary = {
        "base_detail_file": str(Path(args.base_detail)),
        "date_range": f"{args.start}~{args.end}",
        "unresolved_before": len(unresolved),
        "ocp_candidates": len(candidates),
        "direct_sync_line_assignments": len(direct_assignments),
        "direct_sync_line_applied": direct_summary,
        "rule_candidates": len(rule_candidates),
        "rule_assignments": len(rule_assignments),
        "rule_applied": rule_summary,
        "rule_stats": {
            **{k: v for k, v in rule_stats.items() if k != "reason_counts"},
            "reason_counts": dict(rule_stats["reason_counts"]),
        },
        "status_rows_after": dict(status_rows),
        "status_qty_after": {k: decimal_to_str(v) for k, v in status_qty.items()},
        "compare": compare_stats,
        "reports": {
            "detail": str(detail_path),
            "compare": str(compare_path),
            "summary": str(summary_path),
        },
    }
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(summary, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
