param(
    [datetime]$SyncStart = [datetime]'2025-05-15',
    [datetime]$SyncEnd = [datetime]'2026-05-15',
    [datetime]$TargetStart = [datetime]'2026-07-01',
    [datetime]$TargetEnd = [datetime]'2026-07-31',
    [string]$OutputDir = 'E:\order_platform\exports',
    [int]$ChunkDays = 7,
    [string]$Endpoint = $(if ($env:WZ_ESB_ENDPOINT) { $env:WZ_ESB_ENDPOINT } else { 'http://10.11.0.101:8003/gateway/DataCenter/CXCNSJ' }),
    [string]$ServiceConnectionString = $env:OCP_SERVICE_CONNECTION_STRING
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ServiceConnectionString)) {
    throw '请先设置环境变量 OCP_SERVICE_CONNECTION_STRING，或通过 -ServiceConnectionString 传入 OCP_Service 数据库连接串。'
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$detailPath = Join-Path $OutputDir "WZ_2026_07_dedup_details_$stamp.csv"
$summaryPath = Join-Path $OutputDir "WZ_2026_07_dedup_summary_$stamp.csv"
$progressPath = Join-Path $OutputDir "WZ_2026_07_export_progress_$stamp.txt"

function Write-ProgressLine([string]$message) {
    $line = "{0} {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $message
    $line | Tee-Object -FilePath $progressPath -Append
}

function Convert-MojibakeText($value) {
    if ($null -eq $value) { return '' }
    $text = [string]$value
    try {
        $bytes = [System.Text.Encoding]::GetEncoding('ISO-8859-1').GetBytes($text)
        $decoded = [System.Text.Encoding]::UTF8.GetString($bytes)
        if ([regex]::IsMatch($decoded, '\p{IsCJKUnifiedIdeographs}')) {
            return $decoded
        }
    } catch {
    }
    return $text
}

function Convert-DateOnly($value) {
    if ([string]::IsNullOrWhiteSpace([string]$value)) { return $null }
    try { return ([datetime]::Parse([string]$value)).Date } catch { return $null }
}

function Invoke-Cxcnsj([datetime]$startDate, [datetime]$endDate) {
    $payload = @{
        FSTARTDATE = $startDate.ToString('yyyy-MM-dd')
        FENDDATE = $endDate.ToString('yyyy-MM-dd')
    } | ConvertTo-Json -Compress

    try {
        $response = Invoke-RestMethod -Uri $Endpoint -Method Post -ContentType 'application/json; charset=utf-8' -Body $payload -TimeoutSec 180
        return @($response)
    } catch {
        Write-ProgressLine ("WARN api failed {0}~{1}: {2}" -f $startDate.ToString('yyyy-MM-dd'), $endDate.ToString('yyyy-MM-dd'), $_.Exception.Message)
        return @()
    }
}

function Load-ThresholdMap {
    Add-Type -AssemblyName System.Data
    $conn = New-Object System.Data.SqlClient.SqlConnection $ServiceConnectionString
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
SELECT ValveCategory, ProductionLine, CurrentThreshold
FROM dbo.WZ_ProductionOutputThreshold;
"@
    $cmd.CommandTimeout = 60
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $table = New-Object System.Data.DataTable
    $conn.Open()
    [void]$adapter.Fill($table)
    $conn.Close()

    $map = @{}
    foreach ($row in $table.Rows) {
        $cat = ([string]$row.ValveCategory).Trim()
        $line = ([string]$row.ProductionLine).Trim()
        if ($cat -and $line) {
            $map["$cat|$line"] = [decimal]$row.CurrentThreshold
        }
    }
    return $map
}

$thresholdMap = Load-ThresholdMap
$rawRows = New-Object System.Collections.Generic.List[object]

Write-ProgressLine ("start syncWindow={0}~{1}, targetProductionDate={2}~{3}" -f $SyncStart.ToString('yyyy-MM-dd'), $SyncEnd.ToString('yyyy-MM-dd'), $TargetStart.ToString('yyyy-MM-dd'), $TargetEnd.ToString('yyyy-MM-dd'))

$cur = $SyncStart.Date
$chunkIndex = 0
$totalChunks = [math]::Ceiling((($SyncEnd.Date - $SyncStart.Date).Days + 1) / [double]$ChunkDays)
while ($cur -le $SyncEnd.Date) {
    $chunkIndex++
    $chunkEnd = $cur.AddDays($ChunkDays - 1)
    if ($chunkEnd -gt $SyncEnd.Date) { $chunkEnd = $SyncEnd.Date }

    $chunkRows = Invoke-Cxcnsj $cur $chunkEnd
    $before = $rawRows.Count

    foreach ($row in $chunkRows) {
        $productionDate = Convert-DateOnly $row.F_ORA_DATE1
        if ($null -eq $productionDate -or $productionDate -lt $TargetStart.Date -or $productionDate -gt $TargetEnd.Date) {
            continue
        }

        $valveCategory = (Convert-MojibakeText $row.F_ORA_FMLB).Trim()
        $productionLine = (Convert-MojibakeText $row.F_ORA_SCX).Trim()
        if ([string]::IsNullOrWhiteSpace($valveCategory) -or [string]::IsNullOrWhiteSpace($productionLine)) {
            continue
        }

        $qtyValue = $row.FQTY
        if ($null -eq $qtyValue) { $qtyValue = 0 }
        $qty = [decimal]$qtyValue

        $rawRows.Add([pscustomobject]@{
            ProductionDate = $productionDate.ToString('yyyy-MM-dd')
            ValveCategory = $valveCategory
            ProductionLine = $productionLine
            Qty = $qty
            BillNo = [string]$row.FBILLNO
            PlanTrackingNo = [string]$row.FMTONO
            EntryId = [string]$row.FENTRYID
            Seq = [string]$row.FSEQ
            MaterialId = [string]$row.FMATERIALID
            OrderDate = (Convert-DateOnly $row.FDATE)
            RequestedDeliveryDate = (Convert-DateOnly $row.F_ORA_DATETIME)
            DeliveryDate = (Convert-DateOnly $row.FDELIVERYDATE)
            ReplyDeliveryDate = (Convert-DateOnly $row.F_BLN_HFJHRQ)
            Status = Convert-MojibakeText $row.FSTATUS
            EntryStatus = Convert-MojibakeText $row.FENTRYSTATUS
            FixedCycleDays = Convert-MojibakeText $row.F_ORA_GDZQ
            BodyMaterial = Convert-MojibakeText $row.F_BLN_FLJCZ
            SealFaceForm = Convert-MojibakeText $row.F_BLN_FLMFMXS
        })
    }

    $added = $rawRows.Count - $before
    Write-ProgressLine ("chunk {0}/{1} {2}~{3}: apiRows={4}, matchedRows={5}, totalMatched={6}" -f $chunkIndex, $totalChunks, $cur.ToString('yyyy-MM-dd'), $chunkEnd.ToString('yyyy-MM-dd'), $chunkRows.Count, $added, $rawRows.Count)
    $cur = $chunkEnd.AddDays(1)
}

$groups = $rawRows | Group-Object ProductionDate, ValveCategory, ProductionLine, EntryId, PlanTrackingNo, Seq
$dedupRows = New-Object System.Collections.Generic.List[object]
foreach ($group in $groups) {
    $first = $group.Group[0]
    $thresholdKey = "$($first.ValveCategory)|$($first.ProductionLine)"
    $threshold = $null
    if ($thresholdMap.ContainsKey($thresholdKey)) {
        $threshold = $thresholdMap[$thresholdKey]
    }
    $rawQtySum = ($group.Group | Measure-Object Qty -Sum).Sum
    $duplicateRows = $group.Count - 1

    $dedupRows.Add([pscustomobject]@{
        ProductionDate = $first.ProductionDate
        ValveCategory = $first.ValveCategory
        ProductionLine = $first.ProductionLine
        Qty = $first.Qty
        CurrentThreshold = $threshold
        BillNo = $first.BillNo
        PlanTrackingNo = $first.PlanTrackingNo
        EntryId = $first.EntryId
        Seq = $first.Seq
        MaterialId = $first.MaterialId
        OrderDate = if ($first.OrderDate) { $first.OrderDate.ToString('yyyy-MM-dd') } else { '' }
        RequestedDeliveryDate = if ($first.RequestedDeliveryDate) { $first.RequestedDeliveryDate.ToString('yyyy-MM-dd') } else { '' }
        DeliveryDate = if ($first.DeliveryDate) { $first.DeliveryDate.ToString('yyyy-MM-dd') } else { '' }
        ReplyDeliveryDate = if ($first.ReplyDeliveryDate) { $first.ReplyDeliveryDate.ToString('yyyy-MM-dd') } else { '' }
        Status = $first.Status
        EntryStatus = $first.EntryStatus
        FixedCycleDays = $first.FixedCycleDays
        BodyMaterial = $first.BodyMaterial
        SealFaceForm = $first.SealFaceForm
        RawRowCount = $group.Count
        RemovedDuplicateRows = $duplicateRows
        RawQtySum = $rawQtySum
    })
}

$summaryRows = $dedupRows |
    Group-Object ProductionDate, ValveCategory, ProductionLine |
    ForEach-Object {
        $first = $_.Group[0]
        $qty = ($_.Group | Measure-Object Qty -Sum).Sum
        $threshold = $first.CurrentThreshold
        $rawRowsCount = ($_.Group | Measure-Object RawRowCount -Sum).Sum
        $removedRows = ($_.Group | Measure-Object RemovedDuplicateRows -Sum).Sum
        [pscustomobject]@{
            ProductionDate = $first.ProductionDate
            ValveCategory = $first.ValveCategory
            ProductionLine = $first.ProductionLine
            DedupDetailCount = $_.Count
            DedupQty = $qty
            CurrentThreshold = $threshold
            IsOverThreshold = if ($threshold -ne $null -and $qty -gt $threshold) { 'Y' } else { 'N' }
            ExceedQty = if ($threshold -ne $null) { $qty - $threshold } else { $null }
            Ratio = if ($threshold -ne $null -and $threshold -ne 0) { [math]::Round([double]($qty / $threshold), 2) } else { $null }
            RawApiRows = $rawRowsCount
            RemovedDuplicateRows = $removedRows
        }
    } |
    Sort-Object @{ Expression = 'ProductionDate'; Ascending = $true }, @{ Expression = 'ValveCategory'; Ascending = $true }, @{ Expression = 'ProductionLine'; Ascending = $true }

$dedupRows |
    Sort-Object ProductionDate, ValveCategory, ProductionLine, BillNo, PlanTrackingNo, Seq |
    Export-Csv -Path $detailPath -Encoding UTF8 -NoTypeInformation

$summaryRows |
    Export-Csv -Path $summaryPath -Encoding UTF8 -NoTypeInformation

Write-ProgressLine ("done rawMatchedRows={0}, dedupRows={1}, summaryRows={2}" -f $rawRows.Count, $dedupRows.Count, @($summaryRows).Count)
Write-ProgressLine ("detailPath={0}" -f $detailPath)
Write-ProgressLine ("summaryPath={0}" -f $summaryPath)

[pscustomobject]@{
    DetailPath = $detailPath
    SummaryPath = $summaryPath
    ProgressPath = $progressPath
    RawMatchedRows = $rawRows.Count
    DedupRows = $dedupRows.Count
    SummaryRows = @($summaryRows).Count
} | ConvertTo-Json -Depth 3
