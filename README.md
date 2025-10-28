⚡ NAP-Manager
A customizable Windows desktop app built with WPF (.NET Framework 4.8) that gives users full control over hidden and standard power settings — including Modern Standby, Hibernate, Sleep, Display, and USB Suspend options.

🖥️ Features

✅ Status Dashboard
View current power settings including:

Modern Standby state
Hibernate status
Sleep & Display timeouts (AC/DC)
USB Suspend status

💤 Sleep Settings Tab

Set sleep timeout for AC and battery modes

🌙 Display Settings Tab

Set display timeout for AC and battery modes

🛌 Hibernate Settings Tab

Toggle Hibernate ON/OFF
Set Hibernate timeout for AC and battery

🧠 Advanced Settings Tab

Toggle Modern Standby (S0/S3)
Toggle USB Selective Suspend (AC/DC)


📦 Requirements
Windows 10/11
.NET Framework 4.8
Administrator privileges (for registry and powercfg changes)
Visual Studio 2022 or newer (for building)


🛠️ How It Works
Registry Edits
Modern Standby Toggle:
HKLM\SYSTEM\CurrentControlSet\Control\Power\PlatformAoAcOverride

PowerShell Commands:
powercfg /hibernate on|off
powercfg /change standby-timeout-ac|dc
powercfg /change monitor-timeout-ac|dc
powercfg /setacvalueindex /setdcvalueindex for USB suspend
