Write-Host "=== All CirrusLogic events (last 180 days) ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='CirrusLogic-Drv-XuCsMe'; StartTime=(Get-Date).AddDays(-180)} -MaxEvents 200 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Data';E={($_.Properties | ForEach-Object { $_.Value }) -join ', '}} -AutoSize -Wrap

Write-Host "`n=== CirrusLogic events grouped by EventID ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='CirrusLogic-Drv-XuCsMe'; StartTime=(Get-Date).AddDays(-180)} -MaxEvents 500 -ErrorAction SilentlyContinue |
    Group-Object Id | ForEach-Object {
        Write-Host "EventID $($_.Name): $($_.Count) occurrences, Levels: $(($_.Group | Select-Object -Unique LevelDisplayName | ForEach-Object { $_.LevelDisplayName }) -join ', ')"
        $_.Group | Select-Object -First 3 | ForEach-Object {
            Write-Host "  $($_.TimeCreated) Level=$($_.LevelDisplayName) Data=$(($_.Properties | ForEach-Object { $_.Value }) -join ', ')"
        }
    }

Write-Host "`n=== CirrusLogic error events only (Level 1,2 = Critical,Error) ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='CirrusLogic-Drv-XuCsMe'; Level=1,2; StartTime=(Get-Date).AddDays(-180)} -MaxEvents 200 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Data';E={($_.Properties | ForEach-Object { $_.Value }) -join ', '}}, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(200,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== CirrusLogic events around the screenshot date (Feb 5 2026) ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='CirrusLogic-Drv-XuCsMe'; StartTime='2026-02-04'; EndTime='2026-02-06'} -MaxEvents 200 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Data';E={($_.Properties | ForEach-Object { $_.Value }) -join ', '}} -AutoSize -Wrap

Write-Host "`n=== Correlate: CirrusLogic errors with nearby Power resume events ==="
$cirrusErrors = Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='CirrusLogic-Drv-XuCsMe'; Level=1,2; StartTime=(Get-Date).AddDays(-180)} -MaxEvents 50 -ErrorAction SilentlyContinue
foreach ($err in $cirrusErrors | Select-Object -First 10) {
    $before = $err.TimeCreated.AddMinutes(-5)
    $after = $err.TimeCreated.AddMinutes(1)
    $power = Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-Kernel-Power'; StartTime=$before; EndTime=$after} -ErrorAction SilentlyContinue |
        Where-Object { $_.Id -in @(107, 507) }
    Write-Host "Cirrus Error at $($err.TimeCreated) EventID=$($err.Id) Data=$(($err.Properties | ForEach-Object { $_.Value }) -join ', ')"
    foreach ($p in $power) {
        Write-Host "  -> Power event $($p.Id) at $($p.TimeCreated): $($p.Message.Substring(0,[Math]::Min(100,$p.Message.Length)))"
    }
}
