# Audio Fixer

A system tray utility that automatically detects and fixes a common audio driver issue on Dell XPS laptops equipped with Cirrus Logic CS35L56 amplifiers.

## The Problem

After resuming from standby/sleep, the CS35L56 amp devices sometimes fail to reinitialize properly, resulting in **Code 43** errors ("Windows has stopped this device because it has reported problems").

This affects up to 4 devices:
- CS35L56 amp (left tweeter)
- CS35L56 amp (right tweeter)
- CS35L56 amp (left woofer)
- CS35L56 amp (right woofer)

When this happens, Windows reports "No audio device is installed" and the speaker icon in the system tray shows a red cross:

![Traybar showing no audio device](assets/traybar-error.png)

Device Manager shows the affected CS35L56 amp devices with warning indicators:

![Device Manager showing CS35L56 errors](assets/device-manager.png)

## The Fix

The only way to restore audio without rebooting is to **disable and re-enable** the parent Intel Smart Sound Technology OED device in Device Manager. This causes the entire SoundWire audio bus to reinitialize, which clears the Code 43 errors on the child CS35L56 amp devices.

Audio Fixer automates this process.

## Features

- **System tray icon** with green (OK) or red (error) status indicator
- **Automatic monitoring** every 30 seconds for Code 43 errors
- **Resume detection** automatically checks device status after waking from standby
- **Toast notifications** with a "Fix Now" action button when a problem is detected
- **One-click fix** via tray icon context menu or toast notification
- **Verification** confirms all devices are healthy after applying the fix

## How It Works

1. Enumerates audio devices using the Windows SetupAPI and checks their status via CfgMgr32
2. Matches devices by friendly name ("CS35L56" for amps, "Smart Sound Technology OED" for the parent device) so it works across different hardware revisions
3. When a problem is detected, disables and re-enables the Intel SST OED device using `SetupDiCallClassInstaller` with `DIF_PROPERTYCHANGE`
4. Verifies the fix by re-checking all CS35L56 device statuses

No command-line tools (devcon, pnputil, etc.) are used. All device operations go through the Windows SetupAPI and Configuration Manager native APIs.

## Requirements

- Windows 10/11
- .NET 9.0 Runtime
- Administrator privileges (required for device disable/enable)
- Dell XPS laptop with CS35L56 audio amplifiers (developed/tested on Dell XPS 16 9640)

## Building

```
dotnet build src/AudioFixer/AudioFixer.csproj
```

## Usage

Run `AudioFixer.exe` — it will appear in the system tray. The app requires administrator elevation and will prompt via UAC on launch.

- **Double-click** the tray icon to check status (or fix if a problem is detected)
- **Right-click** for the context menu: Check Now, Fix Now, Exit
- When a problem is detected, a **toast notification** appears with a "Fix Now" button

## License

MIT
