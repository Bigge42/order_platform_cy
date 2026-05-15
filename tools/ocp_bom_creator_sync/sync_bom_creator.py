#!/usr/bin/env python3
from __future__ import annotations

import argparse
import csv
import json
import logging
import os
import re
import select
import subprocess
import sys
import time
import uuid
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Any

import pymssql


STATUS_FAILED = "Failed"
STATUS_NO_RESULT = "NoResult"
STATUS_SUCCESS = "Success"


@dataclass(slots=True)
class Config:
    sqlserver_host: str = field(default_factory=lambda: os.getenv("ORDER_SQLSERVER_HOST", "10.11.0.18"))
    sqlserver_port: int = field(default_factory=lambda: int(os.getenv("ORDER_SQLSERVER_PORT", "1433")))
    sqlserver_database: str = field(default_factory=lambda: os.getenv("ORDER_SQLSERVER_DATABASE", "OCP_Service"))
    sqlserver_user: str = field(default_factory=lambda: os.getenv("ORDER_SQLSERVER_USER", "SysUser"))
    sqlserver_password: str = field(default_factory=lambda: os.getenv("ORDER_SQLSERVER_PASSWORD", ""))
    batch_size: int = field(default_factory=lambda: int(os.getenv("TC_BOM_CREATOR_BATCH_SIZE", "50")))
    max_retry: int = field(default_factory=lambda: int(os.getenv("TC_BOM_CREATOR_MAX_RETRY", "5")))
    enqueue_limit: int = field(default_factory=lambda: int(os.getenv("TC_BOM_CREATOR_ENQUEUE_LIMIT", "500")))
    command_template: str = field(
        default_factory=lambda: os.getenv(
            "TC_BOM_CREATOR_COMMAND",
            "/opt/TeamcenterBatch/query_bom_creator.sh --input {input} --output {output}",
        )
    )
    heartbeat_seconds: int = field(default_factory=lambda: int(os.getenv("TC_BOM_CREATOR_HEARTBEAT_SECONDS", "15")))
    run_dir: Path = field(default_factory=lambda: Path(os.getenv("TC_BOM_CREATOR_RUN_DIR", "./runs")).resolve())
    log_dir: Path = field(default_factory=lambda: Path(os.getenv("TC_BOM_CREATOR_LOG_DIR", "./logs")).resolve())
    output_encoding: str = field(default_factory=lambda: os.getenv("TC_BOM_CREATOR_OUTPUT_ENCODING", "utf-8"))


def load_env_file(path: Path) -> None:
    if not path.exists():
        return
    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        key = key.strip()
        value = value.strip().strip("'").strip('"')
        if key and (key not in os.environ or os.environ.get(key) == ""):
            os.environ[key] = value


def setup_logging(config: Config) -> None:
    config.log_dir.mkdir(parents=True, exist_ok=True)
    log_file = config.log_dir / f"sync_bom_creator_{datetime.now():%Y%m%d}.log"
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s %(levelname)s %(message)s",
        handlers=[
            logging.StreamHandler(sys.stdout),
            logging.FileHandler(log_file, encoding="utf-8"),
        ],
    )


def connect(config: Config) -> pymssql.Connection:
    if not config.sqlserver_password:
        raise RuntimeError("ORDER_SQLSERVER_PASSWORD is required")
    return pymssql.connect(
        server=config.sqlserver_host,
        port=str(config.sqlserver_port),
        user=config.sqlserver_user,
        password=config.sqlserver_password,
        database=config.sqlserver_database,
        login_timeout=10,
        timeout=60,
        as_dict=True,
        charset="utf8",
    )


def enqueue_missing_tasks(conn: pymssql.Connection, limit: int) -> None:
    logging.info("enqueue missing BOM creator tasks, limit=%s", limit)
    with conn.cursor(as_dict=True) as cursor:
        cursor.execute("EXEC dbo.usp_OCP_EnqueueMissingTCBomCreatorTasks @BatchLimit=%s", (limit,))
        row = cursor.fetchone()
        if row:
            logging.info("pending tasks after enqueue: %s", row.get("PendingCount"))
    conn.commit()


def claim_tasks(conn: pymssql.Connection, batch_size: int, max_retry: int, batch_no: str) -> list[dict[str, Any]]:
    sql = """
    ;WITH Picked AS
    (
        SELECT TOP (%d)
            ID
        FROM dbo.t_material_bom_creator_task WITH (ROWLOCK, READPAST, UPDLOCK)
        WHERE Status IN (N'Pending', N'Failed')
          AND RetryCount < %%s
        ORDER BY CreatedAt, ID
    )
    UPDATE task
    SET
        Status = N'Running',
        BatchNo = %%s,
        RetryCount = RetryCount + 1,
        LastAttemptAt = SYSDATETIME(),
        UpdatedAt = SYSDATETIME()
    OUTPUT inserted.ID, inserted.MaterialNumber, inserted.RetryCount
    FROM dbo.t_material_bom_creator_task AS task
    JOIN Picked ON Picked.ID = task.ID;
    """ % max(1, batch_size)
    with conn.cursor(as_dict=True) as cursor:
        cursor.execute(sql, (max_retry, batch_no))
        rows = list(cursor.fetchall())
    conn.commit()
    logging.info("claimed %s task(s), batch=%s", len(rows), batch_no)
    return rows


def preview_missing_tasks(conn: pymssql.Connection, batch_size: int) -> list[dict[str, Any]]:
    sql = """
    SELECT TOP (%d)
        MaterialNumber = LTRIM(RTRIM(tm.MaterialNumber))
    FROM dbo.OCP_TechManagement AS tm
    OUTER APPLY (
        SELECT TOP (1)
            creator.TCBomCreator
        FROM dbo.t_material_bom_creator AS creator
        WHERE creator.MaterialNumber = tm.MaterialNumber
        ORDER BY
            CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(creator.TCBomCreator, N''))), N'') IS NULL THEN 1 ELSE 0 END,
            creator.UpdatedAt DESC
    ) AS mbc
    WHERE NULLIF(LTRIM(RTRIM(ISNULL(tm.MaterialNumber, N''))), N'') IS NOT NULL
      AND NULLIF(LTRIM(RTRIM(ISNULL(mbc.TCBomCreator, N''))), N'') IS NULL
    GROUP BY LTRIM(RTRIM(tm.MaterialNumber))
    ORDER BY LTRIM(RTRIM(tm.MaterialNumber));
    """ % max(1, batch_size)
    with conn.cursor(as_dict=True) as cursor:
        cursor.execute(sql)
        return list(cursor.fetchall())


def write_input_file(path: Path, material_numbers: list[str]) -> None:
    path.write_text("\n".join(material_numbers) + "\n", encoding="utf-8")
    logging.info("wrote input file: %s (%s material number(s))", path, len(material_numbers))


def run_tc_command(config: Config, input_path: Path, output_path: Path) -> str:
    command = config.command_template.format(
        input=str(input_path),
        output=str(output_path),
        batch_size=config.batch_size,
    )
    logging.info("start Teamcenter command: %s", command)
    process = subprocess.Popen(
        command,
        shell=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        encoding=config.output_encoding,
        errors="replace",
        bufsize=1,
    )

    captured: list[str] = []
    last_heartbeat = time.time()
    assert process.stdout is not None
    while process.poll() is None:
        readable, _, _ = select.select([process.stdout], [], [], 1)
        if readable:
            line = process.stdout.readline()
            if line:
                captured.append(line)
                logging.info("[tc] %s", line.rstrip())
        now = time.time()
        if now - last_heartbeat >= config.heartbeat_seconds:
            logging.info("Teamcenter command still running, pid=%s", process.pid)
            last_heartbeat = now

    remainder = process.stdout.read()
    if remainder:
        captured.append(remainder)
        for line in remainder.splitlines():
            logging.info("[tc] %s", line)

    if process.returncode != 0:
        raise RuntimeError(f"Teamcenter command failed with exit code {process.returncode}")

    logging.info("Teamcenter command finished, output=%s", output_path)
    return "".join(captured)


def read_text_best_effort(path: Path, default_encoding: str) -> str:
    for encoding in (default_encoding, "utf-8-sig", "utf-8", "gbk"):
        try:
            return path.read_text(encoding=encoding)
        except UnicodeDecodeError:
            continue
    return path.read_text(encoding=default_encoding, errors="replace")


def parse_results(output_path: Path, stdout_text: str, default_encoding: str) -> dict[str, str]:
    text = read_text_best_effort(output_path, default_encoding) if output_path.exists() else stdout_text
    text = text.strip()
    if not text:
        return {}

    parsed_json = try_parse_json(text)
    if parsed_json is not None:
        return parsed_json

    parsed_csv = try_parse_csv(text)
    if parsed_csv:
        return parsed_csv

    return try_parse_key_value_lines(text)


def try_parse_json(text: str) -> dict[str, str] | None:
    try:
        payload = json.loads(text)
    except json.JSONDecodeError:
        return None

    result: dict[str, str] = {}
    if isinstance(payload, dict):
        for key, value in payload.items():
            material = normalize_material(key)
            creator = normalize_creator(value)
            if material and creator:
                result[material] = creator
        return result

    if isinstance(payload, list):
        for row in payload:
            if not isinstance(row, dict):
                continue
            material = normalize_material(first_value(row, "MaterialNumber", "materialNumber", "MaterialCode", "materialCode", "编码", "物料代码"))
            creator = normalize_creator(first_value(row, "TCBomCreator", "tcBomCreator", "BomCreator", "bomCreator", "Creator", "creator", "所有者"))
            if material and creator:
                result[material] = creator
        return result

    return result


def try_parse_csv(text: str) -> dict[str, str]:
    sample = text[:4096]
    try:
        dialect = csv.Sniffer().sniff(sample, delimiters=",\t;")
    except csv.Error:
        dialect = csv.excel

    lines = [line for line in text.splitlines() if line.strip()]
    if not lines:
        return {}

    result: dict[str, str] = {}
    has_header = first_csv_row_is_header(lines, dialect)
    try:
        has_header = has_header or csv.Sniffer().has_header(sample)
    except csv.Error:
        pass

    if has_header:
        reader = csv.DictReader(lines, dialect=dialect)
        for row in reader:
            normalized = {normalize_header(key): value for key, value in row.items() if key is not None}
            material = normalize_material(first_value(normalized, "materialnumber", "materialcode", "编码", "物料代码"))
            creator = normalize_creator(first_value(normalized, "tcbomcreator", "bomcreator", "creator", "所有者"))
            if material and creator:
                result[material] = creator
        return result

    reader = csv.reader(lines, dialect=dialect)
    for row in reader:
        if len(row) < 2:
            continue
        material = normalize_material(row[0])
        creator = normalize_creator(row[1])
        if material and creator:
            result[material] = creator
    return result


def first_csv_row_is_header(lines: list[str], dialect: csv.Dialect) -> bool:
    if not lines:
        return False
    try:
        first_row = next(csv.reader([lines[0]], dialect=dialect))
    except csv.Error:
        return False
    normalized = {normalize_header(value) for value in first_row}
    material_headers = {"materialnumber", "materialcode", "编码", "物料代码"}
    creator_headers = {"tcbomcreator", "bomcreator", "creator", "所有者"}
    return bool(normalized & material_headers) and bool(normalized & creator_headers)


def try_parse_key_value_lines(text: str) -> dict[str, str]:
    result: dict[str, str] = {}
    pattern = re.compile(r"^\s*([^,=\t;:]+)\s*[,=\t;:]\s*(.+?)\s*$")
    for line in text.splitlines():
        match = pattern.match(line)
        if not match:
            continue
        material = normalize_material(match.group(1))
        creator = normalize_creator(match.group(2))
        if is_header_pair(material, creator):
            continue
        if material and creator:
            result[material] = creator
    return result


def first_value(row: dict[str, Any], *keys: str) -> Any:
    for key in keys:
        if key in row:
            return row[key]
    return None


def normalize_header(value: Any) -> str:
    text = str(value or "").strip()
    if text.isascii():
        return text.lower()
    return text


def is_header_pair(material: str, creator: str) -> bool:
    material_header = normalize_header(material) in {"materialnumber", "materialcode", "编码", "物料代码"}
    creator_text = normalize_header(creator)
    creator_header = any(header in creator_text for header in {"tcbomcreator", "bomcreator", "creator", "所有者"})
    return material_header and creator_header


def normalize_material(value: Any) -> str:
    return str(value or "").strip()


def normalize_creator(value: Any) -> str:
    return str(value or "").strip()


def upsert_creator(conn: pymssql.Connection, material_number: str, creator: str) -> None:
    sql = """
    UPDATE dbo.t_material_bom_creator
    SET
        TCBomCreator = %s,
        UpdatedAt = SYSDATETIME()
    WHERE MaterialNumber = %s;

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.t_material_bom_creator (MaterialNumber, TCBomCreator, CreatedAt, UpdatedAt)
        VALUES (%s, %s, SYSDATETIME(), SYSDATETIME());
    END;
    """
    with conn.cursor() as cursor:
        cursor.execute(sql, (creator, material_number, material_number, creator))


def update_task_status(
    conn: pymssql.Connection,
    task_id: int,
    status: str,
    error: str | None = None,
    max_retry: int = 5,
) -> None:
    if status == STATUS_SUCCESS:
        sql = """
        UPDATE dbo.t_material_bom_creator_task
        SET Status = N'Success',
            LastError = NULL,
            UpdatedAt = SYSDATETIME()
        WHERE ID = %s;
        """
        params: tuple[Any, ...] = (task_id,)
    else:
        sql = """
        UPDATE dbo.t_material_bom_creator_task
        SET Status = CASE WHEN RetryCount >= %s THEN N'NoResult' ELSE N'Failed' END,
            LastError = %s,
            UpdatedAt = SYSDATETIME()
        WHERE ID = %s;
        """
        params = (max_retry, (error or "")[:1000], task_id)
    with conn.cursor() as cursor:
        cursor.execute(sql, params)


def persist_results(
    conn: pymssql.Connection,
    tasks: list[dict[str, Any]],
    results: dict[str, str],
    max_retry: int,
    dry_run: bool,
) -> tuple[int, int]:
    success_count = 0
    miss_count = 0
    for task in tasks:
        task_id = int(task["ID"])
        material = normalize_material(task["MaterialNumber"])
        creator = results.get(material)
        if creator:
            logging.info("resolved material=%s creator=%s", material, creator)
            if not dry_run:
                upsert_creator(conn, material, creator)
                update_task_status(conn, task_id, STATUS_SUCCESS, max_retry=max_retry)
            success_count += 1
        else:
            message = "Teamcenter output did not include a non-empty creator"
            logging.warning("unresolved material=%s: %s", material, message)
            if not dry_run:
                update_task_status(conn, task_id, STATUS_FAILED, message, max_retry)
            miss_count += 1
    if not dry_run:
        conn.commit()
    return success_count, miss_count


def mark_batch_failed(conn: pymssql.Connection, tasks: list[dict[str, Any]], message: str, max_retry: int) -> None:
    for task in tasks:
        update_task_status(conn, int(task["ID"]), STATUS_FAILED, message, max_retry)
    conn.commit()


def run_once(config: Config, dry_run: bool = False) -> int:
    batch_no = str(uuid.uuid4())
    config.run_dir.mkdir(parents=True, exist_ok=True)
    batch_dir = config.run_dir / datetime.now().strftime("%Y%m%d_%H%M%S")
    batch_dir.mkdir(parents=True, exist_ok=True)
    input_path = batch_dir / "materials.txt"
    output_path = batch_dir / "bom_creators.csv"

    with connect(config) as conn:
        if dry_run:
            rows = preview_missing_tasks(conn, config.batch_size)
            logging.info("dry run preview: %s missing material number(s)", len(rows))
            for row in rows:
                logging.info("dry run material=%s", normalize_material(row["MaterialNumber"]))
            return 0

        enqueue_missing_tasks(conn, config.enqueue_limit)
        tasks = claim_tasks(conn, config.batch_size, config.max_retry, batch_no)
        if not tasks:
            logging.info("no pending task")
            return 0

        materials = [normalize_material(row["MaterialNumber"]) for row in tasks]
        write_input_file(input_path, materials)

        try:
            stdout_text = run_tc_command(config, input_path, output_path)
            results = parse_results(output_path, stdout_text, config.output_encoding)
            logging.info("parsed %s result(s)", len(results))
            success_count, miss_count = persist_results(conn, tasks, results, config.max_retry, dry_run=False)
            logging.info("batch finished: success=%s unresolved=%s", success_count, miss_count)
            return 0
        except Exception as exc:
            message = str(exc)
            logging.exception("batch failed: %s", message)
            mark_batch_failed(conn, tasks, message, config.max_retry)
            return 1


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Sync missing Teamcenter BOM creators into OCP_Service.")
    parser.add_argument("--env-file", default="", help="Optional env file path.")
    parser.add_argument("--dry-run", action="store_true", help="Preview missing material numbers without database writes.")
    parser.add_argument("--batch-size", type=int, default=None, help="Override TC_BOM_CREATOR_BATCH_SIZE.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    if args.env_file:
        load_env_file(Path(args.env_file))
    config = Config()
    if args.batch_size:
        config.batch_size = args.batch_size
    setup_logging(config)
    logging.info("sync job started")
    code = run_once(config, dry_run=args.dry_run)
    logging.info("sync job finished, exit_code=%s", code)
    return code


if __name__ == "__main__":
    raise SystemExit(main())
