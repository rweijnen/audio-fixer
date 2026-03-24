Write-Host "=== IntelAudioServiceLog (all entries) ==="
Get-WinEvent -LogName 'IntelAudioServiceLog' -MaxEvents 100 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, ProviderName, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(250,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== Microsoft-Windows-Audio/Operational (errors/warnings, last 50) ==="
Get-WinEvent -FilterHashtable @{LogName='Microsoft-Windows-Audio/Operational'; Level=1,2,3} -MaxEvents 50 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, ProviderName, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(250,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== Microsoft-Windows-Audio/PlaybackManager (last 30) ==="
Get-WinEvent -LogName 'Microsoft-Windows-Audio/PlaybackManager' -MaxEvents 30 -ErrorAction SilentlyContinue |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(250,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== Power resume events last 2 days ==="
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-Kernel-Power'; StartTime=(Get-Date).AddDays(-2)} -ErrorAction SilentlyContinue |
    Where-Object { $_.Id -in @(107, 507, 42) } |
    Format-Table TimeCreated, Id, LevelDisplayName, @{N='Msg';E={$_.Message.Substring(0,[Math]::Min(200,$_.Message.Length))}} -AutoSize -Wrap

Write-Host "`n=== Enable Microsoft-Windows-Audio/GlitchDetection and Informational ==="
Write-Host "(These logs exist but are disabled - could be useful for deeper debugging)"
Get-WinEvent -ListLog 'Microsoft-Windows-Audio/*' | Format-Table LogName, RecordCount, IsEnabled, MaximumSizeInBytes
