# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Star-Do is a Stardew Valley mod (C# / .NET 6.0) that adds a full-screen tabbed gameplay planner with task management, quest tracking, and templates. It runs on SMAPI 4.0.0+.

## Build

```bash
dotnet build
```

The `Pathoschild.Stardew.ModBuildConfig` NuGet package auto-detects the game install path and deploys the built mod to the `Stardew Valley/Mods` folder. If detection fails, set `GamePath` in `StarDo.csproj` or create a `stardew.targets` file. There is no test suite or linter configured.

## Architecture

**Entry point:** `ModEntry.cs` — Registers SMAPI event handlers (ButtonPressed, SaveLoaded, DayStarted). Opens `PlannerMenu` on configurable hotkey (default F2). Triggers daily task resets via `TaskManager.ProcessDayStart()`.

**Three-layer structure:**

- **Models/** — Data structures: `PlannerTask` (with subtasks, category, priority, recurring flag, completion count), `SubTask`, `PlannerData` (versioned container), and enums (`TaskCategory`: Farm/Processing/Social/Goals/Quests; `TaskPriority`: Daily/Weekly/Monthly).

- **Services/** — Business logic:
  - `TaskManager` — Task CRUD, completion toggling, period-based reset logic (daily=every morning, weekly=Monday, monthly=season start), JSON persistence via SMAPI helper to `data/{SaveFolderName}.json`.
  - `TemplateProvider` — 15 pre-built task templates in 3 groups (Daily Routines, Seasonal, Long-term Goals) with season-aware filtering.
  - `DataMigrator` — Handles v1→v2→v3 data format migrations.

- **UI/** — XNA/Stardew Valley rendering:
  - `PlannerMenu` — Main fullscreen modal (80% viewport, clamped 800x600–1600x1000) with 3 tabs and gamepad support (LB/RB).
  - **Pages/**: `TaskListPage` (task list with category filters, priority grouping, quick-add), `TaskDetailPage` (rich editor modal), `QuestPage` (active quests/special orders with progress bars), `TemplatePage` (enable/disable templates).
  - **Components/**: `TabBarComponent`, `TaskRowComponent` (badges, progress bars, checkboxes), `TextInputComponent` (custom TextBox wrapper), `ScrollableListComponent`.

## Key Conventions

- UI uses `SpriteBatch` drawing, `ClickableComponent`/`Rectangle` hit detection, `Game1.smallFont`/`dialogueFont` for text
- Category colors: green (Farm), amber (Processing), pink (Social), blue (Goals), purple (Quests)
- Priority colors: red (Daily), gold (Weekly), gray (Monthly)
- Data is stored as one JSON file per save game with a `DataVersion` field (currently 3)
- Private fields accessed with `this.` prefix
- Task IDs are 8-character GUIDs
