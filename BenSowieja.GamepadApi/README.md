\# 🎮 GamepadApi



\[!\[NuGet](https://img.shields.io/nuget/v/GamepadApi.svg)](https://www.nuget.org/packages/GamepadApi/)

\[!\[NuGet Downloads](https://img.shields.io/nuget/dt/GamepadApi.svg)](https://www.nuget.org/packages/GamepadApi/)



A simple, lightweight \*\*.NET Gamepad API\*\* for Windows using \*\*XInput\*\*.  

Supports up to 4 controllers with normalized input, deadzones, vibration, and event-based state updates.



---



\## ✨ Features



\- 🎮 Supports \*\*up to 4 XInput gamepads\*\*

\- 🕹️ Normalized \*\*sticks\*\* (`Vector2`) and \*\*triggers\*\* (`float`)

\- 🛑 Configurable \*\*deadzones\*\* (stick radial, trigger threshold)

\- 🔔 Events: `DeviceConnected`, `DeviceDisconnected`, `StateChanged`

\- 💥 \*\*Vibration support\*\* (left + right motor)

\- 🪶 Lightweight, no dependencies (except `System.Numerics` for `Vector2`)



---



\## 📦 Installation



Install via NuGet:



```bash

dotnet add package GamepadApi



