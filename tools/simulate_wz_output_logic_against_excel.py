from __future__ import annotations

import argparse
import csv
import json
import sys
import time
import urllib.request
from collections import Counter, defaultdict
from datetime import datetime, timedelta
from decimal import Decimal
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
from compare_sales_order_excel_to_ocp import (  # noqa: E402
    decimal_to_str,
    normalize_date,
    normalize_decimal,
    normalize_text,
    read_excel_rows,
    write_csv,
)


ESB_URL = "http://10.11.0.101:8003/gateway/DataCenter/CXCNSJ"


def date_range_chunks(start: str, end: str, days: int = 7):
    s = datetime.strptime(start, "%Y-%m-%d").date()
    e = datetime.strptime(end, "%Y-%m-%d").date()
    while s <= e:
        ce = min(s + timedelta(days=days - 1), e)
        yield s.isoformat(), ce.isoformat()
        s = ce + timedelta(days=1)


def request_esb(start: str, end: str, retries: int = 3) -> list[dict]:
    payload = json.dumps({"FSTARTDATE": start, "FENDDATE": end}).encode("utf-8")
    last_error: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = urllib.request.Request(
                ESB_URL,
                data=payload,
                headers={"Content-Type": "application/json"},
                method="POST",
            )
            with urllib.request.urlopen(req, timeout=240) as resp:
                return json.loads(resp.read().decode("utf-8-sig"))
        except Exception as exc:  # noqa: BLE001
            last_error = exc
            if attempt < retries:
                time.sleep(attempt * 1.5)
    raise RuntimeError(f"ESB request failed {start}~{end}: {last_error}")


def request_esb_adaptive(start: str, end: str) -> list[dict]:
    try:
        return request_esb(start, end)
    except Exception:
        sd = datetime.strptime(start, "%Y-%m-%d").date()
        ed = datetime.strptime(end, "%Y-%m-%d").date()
        if sd >= ed:
            raise
        mid = sd + timedelta(days=(ed - sd).days // 2)
        left = request_esb_adaptive(sd.isoformat(), mid.isoformat())
        right = request_esb_adaptive((mid + timedelta(days=1)).isoformat(), ed.isoformat())
        return left + right


def pick_date(row: dict) -> str:
    for key in ("F_ORA_DATE1", "FDATE", "F_ORA_DATETIME"):
        value = normalize_date(row.get(key))
        if value:
            return value
    return ""


def business_key(row: dict) -> str:
    entry_id = normalize_text(row.get("FENTRYID"))
    if entry_id and entry_id != "0":
        return f"E:{entry_id}"
    bill_no = normalize_text(row.get("FBILLNO"))
    plan_no = normalize_text(row.get("FMTONO"))
    seq = normalize_text(row.get("FSEQ"))
    if not bill_no and not plan_no and not seq:
        return ""
    return f"B:{bill_no}|P:{plan_no}|S:{seq}"


def parse_row(row: dict) -> dict:
    return {
        "BusinessKey": business_key(row),
        "EntryId": normalize_text(row.get("FENTRYID")),
        "BillNo": normalize_text(row.get("FBILLNO")),
        "PlanTrackingNo": normalize_text(row.get("FMTONO")),
        "Seq": normalize_text(row.get("FSEQ")),
        "ProductionDate": pick_date(row),
        "ValveCategory": normalize_text(row.get("F_ORA_FMLB")),
        "ProductionLine": normalize_text(row.get("F_ORA_SCX")),
        "Quantity": normalize_decimal(row.get("FQTY")),
    }


def build_detail_rows(raw_rows: list[dict], source_start: str, source_end: str) -> tuple[list[dict], dict]:
    parsed = [parse_row(row) for row in raw_rows]
    groups: dict[str, list[dict]] = defaultdict(list)
    skipped_no_date = 0
    skipped_no_key = 0

    for row in parsed:
        if not row["ProductionDate"]:
            skipped_no_date += 1
            continue
        if not row["BusinessKey"]:
            skipped_no_key += 1
            continue
        groups[row["BusinessKey"]].append(row)

    details: list[dict] = []
    for key, rows in groups.items():
        first = rows[0]
        line_counter: Counter[tuple[str, str, str, Decimal]] = Counter()
        for row in rows:
            if row["ProductionDate"] and row["ValveCategory"] and row["ProductionLine"]:
                line_counter[
                    (
                        row["ProductionDate"],
                        row["ValveCategory"],
                        row["ProductionLine"],
                        row["Quantity"],
                    )
                ] += 1

        if line_counter:
            candidates = sorted(
                line_counter.items(),
                key=lambda item: (-item[1], item[0][0], item[0][1], item[0][2]),
            )
            chosen_key, _ = candidates[0]
            status = "matched" if len(candidates) == 1 else "conflict"
            production_date, valve_category, production_line, qty = chosen_key
        else:
            fallback = sorted(rows, key=lambda r: r["ProductionDate"])[0]
            candidates = []
            status = "missing_line"
            production_date = fallback["ProductionDate"]
            valve_category = ""
            production_line = ""
            qty = fallback["Quantity"]

        details.append(
            {
                "BusinessKey": key,
                "EntryId": first["EntryId"],
                "BillNo": first["BillNo"],
                "PlanTrackingNo": first["PlanTrackingNo"],
                "Seq": first["Seq"],
                "ProductionDate": production_date,
                "ValveCategory": valve_category,
                "ProductionLine": production_line,
                "Quantity": decimal_to_str(qty),
                "ClassifyStatus": status,
                "RawRowCount": str(len(rows)),
                "LineCandidateCount": str(len(candidates)),
                "SourceStartDate": source_start,
                "SourceEndDate": source_end,
            }
        )

    stats = {
        "raw_rows": len(raw_rows),
        "detail_rows": len(details),
        "skipped_no_date": skipped_no_date,
        "skipped_no_key": skipped_no_key,
        "status_counts": dict(Counter(row["ClassifyStatus"] for row in details)),
    }
    return details, stats


def key_for(row: dict) -> tuple[str, str, str]:
    return (
        normalize_text(row.get("BillNo")),
        normalize_text(row.get("PlanTrackingNo")),
        normalize_date(row.get("ProductionDate")),
    )


def summarize_by_key(rows: list[dict], qty_field: str) -> dict[tuple[str, str, str], dict]:
    result: dict[tuple[str, str, str], dict] = {}
    for row in rows:
        key = key_for(row)
        item = result.setdefault(key, {"rows": [], "qty": Decimal("0")})
        item["rows"].append(row)
        item["qty"] += normalize_decimal(row.get(qty_field))
    return result


def compare_excel(details: list[dict], excel_rows: list[dict]) -> tuple[list[dict], dict]:
    excel_summary = summarize_by_key(excel_rows, "SalesQty")
    detail_summary = summarize_by_key(details, "Quantity")
    all_keys = sorted(set(excel_summary) | set(detail_summary), key=lambda k: (k[2], k[0], k[1]))

    rows: list[dict] = []
    for key in all_keys:
        e = excel_summary.get(key)
        d = detail_summary.get(key)
        excel_qty = e["qty"] if e else Decimal("0")
        detail_qty = d["qty"] if d else Decimal("0")
        if e and d and excel_qty == detail_qty:
            status = "matched"
        elif e and not d:
            status = "missing_in_simulated_detail"
        elif d and not e:
            status = "extra_in_simulated_detail"
        else:
            status = "qty_mismatch"

        sample_e = e["rows"][0] if e else {}
        sample_d = d["rows"][0] if d else {}
        rows.append(
            {
                "Status": status,
                "BillNo": key[0],
                "PlanTrackingNo": key[1],
                "ProductionDate": key[2],
                "ExcelQty": decimal_to_str(excel_qty),
                "SimulatedQty": decimal_to_str(detail_qty),
                "DiffQty": decimal_to_str(excel_qty - detail_qty),
                "ExcelRows": str(len(e["rows"]) if e else 0),
                "SimulatedRows": str(len(d["rows"]) if d else 0),
                "ClassifyStatus": sample_d.get("ClassifyStatus", ""),
                "ValveCategory": sample_d.get("ValveCategory", ""),
                "ProductionLine": sample_d.get("ProductionLine", ""),
                "ExcelSourceRow": sample_e.get("SourceRow", ""),
                "ExcelCreateDate": sample_e.get("CreateDate", ""),
                "EntryId": sample_d.get("EntryId", ""),
                "RawRowCount": sample_d.get("RawRowCount", ""),
                "LineCandidateCount": sample_d.get("LineCandidateCount", ""),
            }
        )

    stats = {
        "excel_rows": len(excel_rows),
        "excel_distinct_keys": len(excel_summary),
        "excel_qty": decimal_to_str(sum((v["qty"] for v in excel_summary.values()), Decimal("0"))),
        "simulated_detail_rows": len(details),
        "simulated_detail_distinct_keys": len(detail_summary),
        "simulated_detail_qty": decimal_to_str(sum((v["qty"] for v in detail_summary.values()), Decimal("0"))),
        "compare_counts": dict(Counter(row["Status"] for row in rows)),
    }
    return rows, stats


def write_summary(rows: list[dict], path: Path) -> None:
    buckets: dict[tuple[str, str, str], dict] = {}
    for row in rows:
        if row["ClassifyStatus"] != "matched":
            continue
        key = (row["ProductionDate"], row["ValveCategory"], row["ProductionLine"])
        item = buckets.setdefault(
            key,
            {
                "ProductionDate": key[0],
                "ValveCategory": key[1],
                "ProductionLine": key[2],
                "DetailRows": "0",
                "Quantity": Decimal("0"),
            },
        )
        item["DetailRows"] = str(int(item["DetailRows"]) + 1)
        item["Quantity"] += normalize_decimal(row["Quantity"])

    out_rows = []
    for item in buckets.values():
        out_rows.append({**item, "Quantity": decimal_to_str(item["Quantity"])})
    out_rows.sort(key=lambda r: (r["ProductionDate"], r["ValveCategory"], r["ProductionLine"]))
    write_csv(path, out_rows, ["ProductionDate", "ValveCategory", "ProductionLine", "DetailRows", "Quantity"])


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--excel", required=True)
    parser.add_argument("--start", default="2026-03-01")
    parser.add_argument("--end", default="2026-05-16")
    parser.add_argument("--output-dir", default=r"E:\order_platform\exports")
    args = parser.parse_args()

    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    stamp = datetime.now().strftime("%Y%m%d_%H%M%S")

    raw_rows: list[dict] = []
    progress_path = output_dir / f"wz_simulation_progress_{stamp}.txt"
    with progress_path.open("w", encoding="utf-8") as progress:
        for s, e in date_range_chunks(args.start, args.end):
            msg = f"{datetime.now():%Y-%m-%d %H:%M:%S} fetching {s}~{e}"
            print(msg, flush=True)
            progress.write(msg + "\n")
            progress.flush()
            rows = request_esb_adaptive(s, e)
            raw_rows.extend(rows)
            msg = f"{datetime.now():%Y-%m-%d %H:%M:%S} fetched {s}~{e}: {len(rows)} rows, total {len(raw_rows)}"
            print(msg, flush=True)
            progress.write(msg + "\n")
            progress.flush()

    details, detail_stats = build_detail_rows(raw_rows, args.start, args.end)
    details = [row for row in details if "2026-07-01" <= row["ProductionDate"] <= "2026-07-31"]

    excel_rows = [
        row
        for row in read_excel_rows(Path(args.excel))
        if "2026-07-01" <= row["ProductionDate"] <= "2026-07-31"
    ]
    compare_rows, compare_stats = compare_excel(details, excel_rows)

    detail_path = output_dir / f"wz_simulated_detail_{stamp}.csv"
    summary_path = output_dir / f"wz_simulated_matched_summary_{stamp}.csv"
    compare_path = output_dir / f"wz_simulated_vs_excel_compare_{stamp}.csv"
    summary_json_path = output_dir / f"wz_simulated_vs_excel_summary_{stamp}.json"

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
    write_summary(details, summary_path)
    write_csv(compare_path, compare_rows, compare_fields)

    status_qty = defaultdict(Decimal)
    status_rows = Counter()
    for row in details:
        status_rows[row["ClassifyStatus"]] += 1
        status_qty[row["ClassifyStatus"]] += normalize_decimal(row["Quantity"])

    summary = {
        "source_window": f"{args.start}~{args.end}",
        "raw_stats": detail_stats,
        "july_detail_rows": len(details),
        "july_status_rows": dict(status_rows),
        "july_status_qty": {k: decimal_to_str(v) for k, v in status_qty.items()},
        "compare": compare_stats,
        "reports": {
            "progress": str(progress_path),
            "detail": str(detail_path),
            "matched_summary": str(summary_path),
            "compare": str(compare_path),
            "summary_json": str(summary_json_path),
        },
    }
    summary_json_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(summary, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
