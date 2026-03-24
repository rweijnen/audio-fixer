# Search for audio-related error events in the System log
$events = Get-WinEvent -FilterHashtable @{LogName='System'; Level=1,2,3; StartTime=(Get-Date).AddDays(-90)} -MaxEvents 500 |
    Where-Object { $_.Message -match 'CS35L56|Code 43|SoundWire|smart sound|INTELAUDIO|stopped this device' }

Write-Host "=== Audio-related error events ==="
$events | Format-Table TimeCreated, Id, ProviderName, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(150,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== Kernel-PnP events (last 50) ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-Kernel-PnP'} -MaxEvents 50 |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(150,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== UserPnP events (last 50) ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-UserPnp'} -MaxEvents 50 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(150,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== DriverFrameworks events (last 50) ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-DriverFrameworks-UserMode'} -MaxEvents 50 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(150,$_.Message.Length))}} -AutoSize -Wrap
