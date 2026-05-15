param(
    [string]$OutputDir = "E:\order_platform\exports",
    [string]$StartDate = "2026-07-01",
    [string]$EndDateExclusive = "2026-08-01",
    [string]$ConnectionString = $env:OCP_SERVICE_CONNECTION_STRING
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw "请先设置环境变量 OCP_SERVICE_CONNECTION_STRING，或通过 -ConnectionString 传入 OCP_Service 数据库连接串。"
}

if (-not (Test-Path -LiteralPath $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outputPath = Join-Path $OutputDir "OCP_OrderTracking_${StartDate}_to_$($EndDateExclusive)_$stamp.csv"

$query = @"
SELECT
    SOBillNo AS BillNo,
    MtoNo AS PlanTrackingNo,
    CONVERT(varchar(10), PrdScheduleDate, 120) AS ProductionDate,
    CAST(ISNULL(OrderQty, 0) AS decimal(18, 6)) AS OrderQty,
    CONVERT(varchar(10), OrderCreateDate, 120) AS OrderCreateDate,
    CAST(SOEntryID AS nvarchar(100)) AS SOEntryID,
    MaterialNumber,
    MaterialName,
    BillStatus,
    CONVERT(varchar(19), ESBModifyDate, 120) AS ESBModifyDate
FROM dbo.OCP_OrderTracking WITH (NOLOCK)
WHERE PrdScheduleDate >= CAST(@StartDate AS date)
  AND PrdScheduleDate < CAST(@EndDateExclusive AS date)
ORDER BY PrdScheduleDate, SOBillNo, MtoNo, SOEntryID;
"@

$conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
$cmd = $conn.CreateCommand()
$cmd.CommandText = $query
$cmd.CommandTimeout = 120
$null = $cmd.Parameters.Add("@StartDate", [System.Data.SqlDbType]::Date)
$cmd.Parameters["@StartDate"].Value = [DateTime]::Parse($StartDate)
$null = $cmd.Parameters.Add("@EndDateExclusive", [System.Data.SqlDbType]::Date)
$cmd.Parameters["@EndDateExclusive"].Value = [DateTime]::Parse($EndDateExclusive)

$dt = New-Object System.Data.DataTable
$conn.Open()
try {
    $reader = $cmd.ExecuteReader()
    $dt.Load($reader)
}
finally {
    $conn.Close()
}

$dt | Export-Csv -Path $outputPath -NoTypeInformation -Encoding UTF8

$qty = [decimal]0
foreach ($row in $dt.Rows) {
    $qty += [decimal]$row.OrderQty
}

[pscustomobject]@{
    OutputPath = $outputPath
    Rows = $dt.Rows.Count
    Qty = $qty
}
