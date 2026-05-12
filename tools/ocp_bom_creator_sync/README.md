# OCP BOM creator sync

This job runs on Linux host `10.11.10.101`.

Flow:

1. SQL Server finds material numbers whose `TCBomCreator` is empty.
2. The job claims a small batch from `dbo.t_material_bom_creator_task`.
3. The configured Teamcenter command receives one input file, one material number per line.
4. The Teamcenter command writes CSV or JSON output.
5. The job upserts `dbo.t_material_bom_creator` and updates task status.

Expected CSV output:

```csv
MaterialNumber,TCBomCreator
0402300341,zhangsan
```

The copied Teamcenter script is also supported directly. Its CSV fields are:

```csv
编码,名称,创建日期,发布日期,所有者,物料代码
0402300341,xxx,2026-01-01,,zhangsan,0402300341
```

Expected JSON output can be either a map:

```json
{"0402300341":"zhangsan"}
```

or a list:

```json
[{"MaterialNumber":"0402300341","TCBomCreator":"zhangsan"}]
```

Install on `10.11.10.101`:

```bash
cd /path/to/tools/ocp_bom_creator_sync
bash install_linux.sh
vi /home/d3h/ocp-bom-creator-sync/.env
```

Copy the original Teamcenter batch folder to:

```bash
/home/d3h/TeamcenterBatch
```

The default command in `.env` is:

```bash
/home/d3h/ocp-bom-creator-sync/.venv/bin/python /home/d3h/TeamcenterBatch/teamcenter_batch.py --url http://10.11.0.54:8000/webservice/service/teamcenter --codes {input} --out {output} --workers 6 --timeout 60
```

Enable nightly timer:

```bash
sudo cp systemd/ocp-bom-creator-sync.* /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now ocp-bom-creator-sync.timer
```

Manual run:

```bash
/home/d3h/ocp-bom-creator-sync/.venv/bin/python \
  /home/d3h/ocp-bom-creator-sync/sync_bom_creator.py \
  --env-file /home/d3h/ocp-bom-creator-sync/.env
```

Preview only:

```bash
/home/d3h/ocp-bom-creator-sync/.venv/bin/python \
  /home/d3h/ocp-bom-creator-sync/sync_bom_creator.py \
  --env-file /home/d3h/ocp-bom-creator-sync/.env \
  --dry-run
```
