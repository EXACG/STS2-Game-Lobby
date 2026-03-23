# CLIENT MOD

## OVERVIEW
Godot 4.5 + .NET 9 Slay the Spire 2 mod. This subtree owns mod startup, config persistence, menu patching, lobby integration, packaging inputs, and installer-facing assets.

## STRUCTURE
```text
sts2-lan-connect/
|- Scripts/                    # C# source
|  |- Lobby/                   # lobby runtime, UI, control channel, diagnostics
|  `- Patches.*.cs            # menu/button injection and official-flow interception
|- tools/build_pck.gd         # packs the .pck asset
|- sts2_lan_connect.csproj    # .NET / STS2 path setup
|- project.godot              # Godot project config
`- lobby-defaults.json        # packaged default endpoint settings
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Mod startup or install timing | `Scripts/Entry.cs` | `[ModInitializer]` entrypoint |
| Persisted config, overrides, bindings | `Scripts/LanConnectConfig.cs` | lock-based config store |
| Menu injection and safe button cloning | `Scripts/Patches.MultiplayerSubmenu.cs` | deferred UI hooks into multiplayer submenu |
| Host-side room publication | `Scripts/LanConnectHostFlow.cs` | ENet host start + lobby registration |
| Lobby runtime and sessions | `Scripts/Lobby/` | read child AGENTS first for this area |
| Build output and packaged files | `../scripts/build-sts2-lan-connect.sh`, `../scripts/package-sts2-lan-connect.sh` | release scripts define final package contents |

## CONVENTIONS
- Keep the `LanConnect*` naming family and `Sts2LanConnect.Scripts` namespace.
- Use `Callable.From(...).CallDeferred()` when touching SceneTree-dependent UI setup.
- Use `TaskHelper.RunSafely(...)` for async work kicked off from Godot signals or UI events.
- Preserve `_camelCase` private fields and one-main-type-per-file organization.
- Config writes go through `LanConnectConfig`; avoid ad hoc JSON or parallel persistence paths.

## ANTI-PATTERNS
- Do not touch `.godot/`, `build/`, or `release/` outputs by hand.
- Do not access menu nodes too early; patching code must tolerate retry/deferred attach.
- Do not replace STS2 logging/util helpers with arbitrary console-style output.
- Do not edit packaged installers under `../releases/`; edit `../scripts/` and repackage.

## COMMANDS
```bash
../scripts/build-sts2-lan-connect.sh
../scripts/build-sts2-lan-connect.sh --install
../scripts/package-sts2-lan-connect.sh
```

## NOTES
- `sts2_lan_connect.csproj` encodes platform-specific STS2 paths for Windows and macOS.
- `lobby-defaults.json` can be copied through as-is or regenerated from env variables during packaging.
- The densest subsystem is `Scripts/Lobby/`; use its child guidance for most gameplay-facing changes.
