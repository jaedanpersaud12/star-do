# 🌾 Star-Do — A Stardew Valley Planner Mod

> Organize your farm life like a proper Valley legend.

Star-Do is an in-game planner for [Stardew Valley](http://stardewvalley.net/) that gives you a clean, tabbed menu for daily chores, long-term goals, templates, and quest tracking.

---

## ✨ Why Star-Do?

- Keep your **daily routine** tight (animals, machines, greenhouse, etc.)
- Track **sub-tasks** and completion history
- Split work by **category** and **priority**
- Reuse **seasonal and routine templates** with one click
- View **quests + special orders** without leaving your planner flow

---

## 🧺 Feature Highlights

### ✅ Task System
- Rich tasks: title, notes, checklist, category, priority
- Recurring tasks with day-start reset behavior
- Quick add + full detail editor
- Per-save JSON storage and versioned data migration

### 📚 Planner Pages
- **Tasks** — active/completed task management
- **Quests** — quest + special order overview
- **Templates** — prebuilt Stardew routine templates

### 🎮 Quality of Life
- Full-screen in-game menu (SMAPI style)
- Gamepad tab switching (LB/RB)
- Configurable hotkey

---

## 📦 Install

1. Install [SMAPI](https://smapi.io/) (4.0.0+).
2. Copy the `StarDo` mod folder into `Stardew Valley/Mods/`.
3. Launch Stardew Valley through SMAPI.

---

## 🛠️ Build from Source

Requires [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or newer.

```bash
git clone https://github.com/your-username/star-do.git
cd star-do
dotnet build
```

Build artifacts are copied to:

```text
artifacts/StarDo/
```

(includes mod DLL + `manifest.json`).

---

## 🔁 Cross-Machine Dev Workflow (macOS dev → Windows test)

If your dev machine does **not** have Stardew + SMAPI installed, use local reference DLLs.

### 1) Create the refs folder

```text
.game-refs/
```

### 2) Copy required references into `.game-refs/`

- `Stardew Valley.dll`
- `StardewValley.GameData.dll`
- `MonoGame.Framework.dll`
- `xTile.dll`
- `StardewModdingAPI.dll`
- `smapi-internal/SMAPI.Toolkit.CoreInterfaces.dll`

### 3) Build locally

```bash
dotnet build
```

### 4) Move output to your Windows test machine

Copy `artifacts/StarDo/` into your mod folder and test in-client.

---

## 🧭 Usage

- Press **F2** (default) to open Star-Do.
- **Tasks tab**: quick add or create detailed tasks.
- **Quests tab**: inspect active quests and special orders.
- **Templates tab**: enable reusable routine checklists.

---

## ⚙️ Config

Edit `config.json` in your mod folder:

| Setting | Default | Description |
|---|---:|---|
| `OpenListKey` | `F2` | Key/button to open Star-Do. Uses SMAPI [SButton values](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings). |
| `OpenAtStartup` | `false` | Opens the planner automatically when save loads. |
| `ShowHudOverlay` | `true` | Shows the compact top-left gameplay task HUD. |

---

## 🧱 Project Layout

```text
Models/      # data models + enums
Services/    # migration, task logic, templates
UI/          # menu, pages, reusable UI components
ModEntry.cs  # SMAPI entry point + event wiring
```

---

## 🗂️ Version Notes

### 2.0.0
- Full rewrite into tabbed planner UI
- Rich tasks + recurrence + templates + quests
- Modernized for .NET 6 / SMAPI 4+

### 1.0.0
- Initial simple to-do list release
