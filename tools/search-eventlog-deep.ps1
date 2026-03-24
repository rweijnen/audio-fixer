# Search events from today and yesterday, focused on audio/device issues

Write-Host "=== System log last 2 days (INTELAUDIO/audio/SoundWire related) ==="
Get-WinEvent -FilterHashtable @{LogName='System'; StartTime=(Get-Date).AddDays(-2)} -ErrorAction SilentlyContinue |
    Where-Object { $_.Message -match 'INTELAUDIO|CS35L56|SoundWire|SDCA|amp|audio|stopped this device' -or $_.ProviderName -match 'audio' } |
    Format-Table TimeCreated, Id, ProviderName, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(200,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== ALL System errors/warnings last 2 days ==="
Get-WinEvent -FilterHashtable @{LogName='System'; Level=1,2,3; StartTime=(Get-Date).AddDays(-2)} -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, ProviderName, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(200,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== Available audio/soundwire/cirrus logs ==="
Get-WinEvent -ListLog '*audio*','*soundwire*','*cirrus*','*CS35*','*IntelSST*','*smart*sound*','*SDCA*' -ErrorAction SilentlyContinue | Format-Table LogName, RecordCount, IsEnabled

Write-Host "`n=== DeviceSetupManager last 2 days ==="
Get-WinEvent -FilterHashtable @{LogName='Microsoft-Windows-DeviceSetupManager/Admin'; StartTime=(Get-Date).AddDays(-2)} -MaxEvents 50 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(200,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== Power events last 2 days ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-Kernel-Power'; StartTime=(Get-Date).AddDays(-2)} -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(150,$_.Message.Length))}} -AutoSize -Wrap
