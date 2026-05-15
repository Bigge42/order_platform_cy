from __future__ import annotations

import argparse
import csv
import json
import re
import zipfile
from collections import Counter, defaultdict
from datetime import datetime, timedelta
from decimal import Decimal, InvalidOperation
from pathlib import Path
import xml.etree.ElementTree as ET

from openpyxl import Workbook


NS = {"x": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}


def col_number(cell_ref: str) -> int:
    match = re.match(r"([A-Z]+)", cell_ref)
    if not match:
        return 0
    value = 0
    for ch in match.group(1):
        value = value * 26 + ord(ch) - 64
    return value


def normalize_text(value) -> str:
    if value is None:
        return ""
    return str(value).strip()


def normalize_decimal(value) -> Decimal:
    text = normalize_text(value).replace(",", "")
    if not text:
        return Decimal("0")
    try:
        return Decimal(text)
    except InvalidOperation:
        return Decimal("0")


def normalize_date(value) -> str:
    text = normalize_text(value)
    if not text:
        return ""

    for fmt in ("%Y-%m-%d", "%Y/%m/%d", "%Y.%m.%d", "%Y-%m-%d %H:%M:%S", "%Y/%m/%d %H:%M:%S"):
        try:
            return datetime.strptime(text, fmt).strftime("%Y-%m-%d")
        except ValueError:
            pass

    try:
        # Excel serial date, with the 1900 leap-year compatibility offset.
        serial = float(text)
        if 1 <= serial <= 60000:
            return (datetime(1899, 12, 30) + timedelta(days=serial)).strftime("%Y-%m-%d")
    except ValueError:
        pass

    return text


def read_shared_strings(zf: zipfile.ZipFile) -> list[str]:
    if "xl/sharedStrings.xml" not in zf.namelist():
        return []

    root = ET.fromstring(zf.read("xl/sharedStrings.xml"))
    values: list[str] = []
    for si in root.findall("x:si", NS):
        values.append("".join(t.text or "" for t in si.findall(".//x:t", NS)))
    return values


def read_cell_value(cell: ET.Element, shared_strings: list[str]) -> str:
    cell_type = cell.attrib.get("t", "")
    if cell_type == "inlineStr":
        return "".join(t.text or "" for t in cell.findall(".//x:t", NS))

    value_node = cell.find("x:v", NS)
    value = "" if value_node is None else value_node.text or ""

    if cell_type == "s" and value:
        try:
            return shared_strings[int(value)]
        except (ValueError, IndexError):
            return value

    return value


def read_excel_rows(path: Path) -> list[dict[str, str]]:
    with zipfile.ZipFile(path) as zf:
        shared_strings = read_shared_strings(zf)
        sheet = ET.fromstring(zf.read("xl/worksheets/sheet1.xml"))

    rows: list[dict[str, str]] = []
    for row_node in sheet.findall(".//x:sheetData/x:row", NS):
        row_index = int(row_node.attrib.get("r", "0"))
        values: dict[int, str] = {}
        for cell in row_node.findall("x:c", NS):
            values[col_number(cell.attrib.get("r", ""))] = read_cell_value(cell, shared_strings)

        if row_index == 1:
            continue

        bill_no = normalize_text(values.get(7))
        plan_no = normalize_text(values.get(22))
        production_date = normalize_date(values.get(21))
        qty = normalize_decimal(values.get(17))

        if not bill_no and not plan_no and not production_date:
            continue

        rows.append(
            {
                "SourceRow": str(row_index),
                "CreateDate": normalize_date(values.get(1)),
                "BillNo": bill_no,
                "PlanTrackingNo": plan_no,
                "ProductionDate": production_date,
                "SalesQty": str(qty),
                "MaterialNumber": normalize_text(values.get(14)),
                "MaterialName": normalize_text(values.get(15)),
                "BillStatus": normalize_text(values.get(8)),
            }
        )

    return rows


def key_for(row: dict[str, str]) -> tuple[str, str, str]:
    return (
        normalize_text(row.get("BillNo")),
        normalize_text(row.get("PlanTrackingNo")),
        normalize_date(row.get("ProductionDate")),
    )


def summarize(rows: list[dict[str, str]], qty_field: str) -> dict[tuple[str, str, str], dict]:
    result: dict[tuple[str, str, str], dict] = {}
    for row in rows:
        key = key_for(row)
        item = result.setdefault(key, {"rows": [], "qty": Decimal("0")})
        item["rows"].append(row)
        item["qty"] += normalize_decimal(row.get(qty_field))
    return result


def read_csv_rows(path: Path) -> list[dict[str, str]]:
    with path.open("r", encoding="utf-8-sig", newline="") as f:
        return list(csv.DictReader(f))


def write_csv(path: Path, rows: list[dict[str, str]], fieldnames: list[str]) -> None:
    with path.open("w", encoding="utf-8-sig", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)


def write_xlsx(path: Path, rows: list[dict[str, str]], fieldnames: list[str]) -> None:
    wb = Workbook()
    ws = wb.active
    ws.title = "对齐明细"
    ws.append(fieldnames)
    for row in rows:
        ws.append([row.get(name, "") for name in fieldnames])
    ws.freeze_panes = "A2"
    for column_cells in ws.columns:
        length = max(len(str(cell.value or "")) for cell in column_cells[:200])
        ws.column_dimensions[column_cells[0].column_letter].width = min(max(length + 2, 10), 40)
    wb.save(path)


def decimal_to_str(value: Decimal) -> str:
    normalized = value.quantize(Decimal("0.000001")).normalize()
    return format(normalized, "f")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--excel", required=True)
    parser.add_argument("--ocp-csv", required=True)
    parser.add_argument("--output-dir", default=r"E:\order_platform\exports")
    parser.add_argument("--start", default="2026-07-01")
    parser.add_argument("--end", default="2026-07-31")
    args = parser.parse_args()

    excel_path = Path(args.excel)
    ocp_path = Path(args.ocp_csv)
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    stamp = datetime.now().strftime("%Y%m%d_%H%M%S")

    excel_rows_all = read_excel_rows(excel_path)
    excel_rows = [
        row
        for row in excel_rows_all
        if args.start <= normalize_date(row["ProductionDate"]) <= args.end
    ]
    ocp_rows = read_csv_rows(ocp_path)

    excel_summary = summarize(excel_rows, "SalesQty")
    ocp_summary = summarize(ocp_rows, "OrderQty")

    all_keys = sorted(set(excel_summary) | set(ocp_summary), key=lambda k: (k[2], k[0], k[1]))

    aligned_rows: list[dict[str, str]] = []
    missing_rows: list[dict[str, str]] = []
    extra_rows: list[dict[str, str]] = []
    mismatch_rows: list[dict[str, str]] = []

    for key in all_keys:
        excel_item = excel_summary.get(key)
        ocp_item = ocp_summary.get(key)
        excel_qty = excel_item["qty"] if excel_item else Decimal("0")
        ocp_qty = ocp_item["qty"] if ocp_item else Decimal("0")
        diff = excel_qty - ocp_qty

        if excel_item and ocp_item and diff == 0 and len(excel_item["rows"]) == len(ocp_item["rows"]):
            status = "matched"
        elif excel_item and not ocp_item:
            status = "missing_in_ocp"
        elif ocp_item and not excel_item:
            status = "extra_in_ocp"
        else:
            status = "qty_or_row_mismatch"

        sample_excel = excel_item["rows"][0] if excel_item else {}
        sample_ocp = ocp_item["rows"][0] if ocp_item else {}

        aligned = {
            "Status": status,
            "BillNo": key[0],
            "PlanTrackingNo": key[1],
            "ProductionDate": key[2],
            "ExcelQty": decimal_to_str(excel_qty),
            "OcpQty": decimal_to_str(ocp_qty),
            "DiffQty": decimal_to_str(diff),
            "ExcelRows": str(len(excel_item["rows"]) if excel_item else 0),
            "OcpRows": str(len(ocp_item["rows"]) if ocp_item else 0),
            "ExcelSourceRow": sample_excel.get("SourceRow", ""),
            "ExcelCreateDate": sample_excel.get("CreateDate", ""),
            "OcpOrderCreateDate": sample_ocp.get("OrderCreateDate", ""),
            "ExcelMaterialNumber": sample_excel.get("MaterialNumber", ""),
            "OcpMaterialNumber": sample_ocp.get("MaterialNumber", ""),
            "ExcelMaterialName": sample_excel.get("MaterialName", ""),
            "OcpMaterialName": sample_ocp.get("MaterialName", ""),
            "OcpSOEntryID": sample_ocp.get("SOEntryID", ""),
            "OcpBillStatus": sample_ocp.get("BillStatus", ""),
            "OcpESBModifyDate": sample_ocp.get("ESBModifyDate", ""),
        }
        aligned_rows.append(aligned)

        if status == "missing_in_ocp":
            missing_rows.append(aligned)
        elif status == "extra_in_ocp":
            extra_rows.append(aligned)
        elif status == "qty_or_row_mismatch":
            mismatch_rows.append(aligned)

    fields = [
        "Status",
        "BillNo",
        "PlanTrackingNo",
        "ProductionDate",
        "ExcelQty",
        "OcpQty",
        "DiffQty",
        "ExcelRows",
        "OcpRows",
        "ExcelSourceRow",
        "ExcelCreateDate",
        "OcpOrderCreateDate",
        "ExcelMaterialNumber",
        "OcpMaterialNumber",
        "ExcelMaterialName",
        "OcpMaterialName",
        "OcpSOEntryID",
        "OcpBillStatus",
        "OcpESBModifyDate",
    ]

    aligned_csv = output_dir / f"sales_order_excel_vs_ocp_aligned_{stamp}.csv"
    aligned_xlsx = output_dir / f"sales_order_excel_vs_ocp_aligned_{stamp}.xlsx"
    missing_csv = output_dir / f"sales_order_excel_missing_in_ocp_{stamp}.csv"
    extra_csv = output_dir / f"sales_order_ocp_extra_not_in_excel_{stamp}.csv"
    mismatch_csv = output_dir / f"sales_order_excel_ocp_qty_mismatch_{stamp}.csv"
    summary_json = output_dir / f"sales_order_excel_vs_ocp_summary_{stamp}.json"

    write_csv(aligned_csv, aligned_rows, fields)
    write_xlsx(aligned_xlsx, aligned_rows, fields)
    write_csv(missing_csv, missing_rows, fields)
    write_csv(extra_csv, extra_rows, fields)
    write_csv(mismatch_csv, mismatch_rows, fields)

    status_counts = Counter(row["Status"] for row in aligned_rows)
    missing_by_date = defaultdict(lambda: {"rows": 0, "qty": Decimal("0")})
    for row in missing_rows:
        bucket = missing_by_date[row["ProductionDate"]]
        bucket["rows"] += int(row["ExcelRows"])
        bucket["qty"] += normalize_decimal(row["ExcelQty"])

    summary = {
        "excel_path": str(excel_path),
        "ocp_csv": str(ocp_path),
        "excel_rows_total": len(excel_rows_all),
        "excel_july_rows": len(excel_rows),
        "excel_july_distinct_keys": len(excel_summary),
        "excel_july_qty": decimal_to_str(sum((v["qty"] for v in excel_summary.values()), Decimal("0"))),
        "ocp_rows": len(ocp_rows),
        "ocp_distinct_keys": len(ocp_summary),
        "ocp_qty": decimal_to_str(sum((v["qty"] for v in ocp_summary.values()), Decimal("0"))),
        "status_counts": dict(status_counts),
        "missing_by_date_top": [
            {"ProductionDate": k, "Rows": v["rows"], "Qty": decimal_to_str(v["qty"])}
            for k, v in sorted(missing_by_date.items(), key=lambda item: item[1]["qty"], reverse=True)[:20]
        ],
        "reports": {
            "aligned_csv": str(aligned_csv),
            "aligned_xlsx": str(aligned_xlsx),
            "missing_in_ocp": str(missing_csv),
            "extra_not_in_excel": str(extra_csv),
            "qty_mismatch": str(mismatch_csv),
            "summary_json": str(summary_json),
        },
    }

    summary_json.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(summary, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
