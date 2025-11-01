param(
    [Parameter(Mandatory=$true)]
    [string]$Name
)

$oldEncoding = [Console]::OutputEncoding 
[Console]::OutputEncoding = [System.Text.Encoding]::Utf8

$Name = $Name.Split([IO.Path]::GetInvalidFileNameChars()) -join '_'
$Name = [DateTime]::Now.ToString("yyyyMMddTHHmmss") + "_$Name.sql"

$filename = Join-Path $PSScriptRoot "up" $Name
Out-File $filename -Encoding utf8
Start-Process $filename

[Console]::OutputEncoding = $oldEncoding