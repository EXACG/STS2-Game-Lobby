# PROJECT KNOWLEDGE BASE

**Generated:** 2026-03-23
**Commit:** `9b37a8a`
**Branch:** `main`

## OVERVIEW
`STS2 LAN Connect` is a two-runtime public repo: the public lobby server in `lobby-service/` and the Godot/.NET client mod in `sts2-lan-connect/`. The official registry/admin service is private and intentionally not tracked here. The real editing root is this directory; the outer wrapper only holds tarballs, and `releases/` only mirrors packaged output.

## PRECEDENCE
- Nearest `AGENTS.md` wins.
- Use this file for cross-repo routing and shared gotchas.
- Use child files for stack-specific rules in `lobby-service/`, `sts2-lan-connect/`, and `sts2-lan-connect/Scripts/Lobby/`.

## STRUCTURE
```text
STS2-Game-Lobby/
|- lobby-service/                # Node 20 + TypeScript lobby server
|- sts2-lan-connect/            # Godot 4.5 + .NET 9 client mod
|- scripts/                     # build, package, install, sync automation
|- docs/                        # user/deployment docs in Chinese
|- research/                    # reconstructed notes and reverse-engineering context
`- releases/                    # mirrored package output; do not edit directly
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Lobby API, tickets, room lifecycle | `lobby-service/src/server.ts` | HTTP, WebSocket, relay wiring |
| Room rules and compatibility checks | `lobby-service/src/store.ts` | single source of truth for room state |
| Relay fallback behavior | `lobby-service/src/relay.ts` | UDP relay manager and sessions |
| Client startup and install path | `sts2-lan-connect/Scripts/Entry.cs` | mod entrypoint installs runtime and monitor |
| Client config and persisted overrides | `sts2-lan-connect/Scripts/LanConnectConfig.cs` | lock-based config store |
| Lobby UI and join/create flows | `sts2-lan-connect/Scripts/Lobby/` | dense hotspot; read child AGENTS first |
| Packaging and release sync | `scripts/` | source of truth for release contents |
| Deployment and player docs | `docs/` | reference material, not primary source for code edits |
| Historical context | `research/` | reconstructed notes; treat as read-only context |

## CODE MAP
| Symbol / File | Type | Location | Role |
|---------------|------|----------|------|
| `LobbyStore` | class | `lobby-service/src/store.ts` | room state, tickets, compatibility, saved-run slots |
| `RoomRelayManager` | class | `lobby-service/src/relay.ts` | UDP relay allocation and forwarding |
| `server.ts` | module | `lobby-service/src/server.ts` | API routes, WS `/control`, request logging |
| `Entry.Init` | entrypoint | `sts2-lan-connect/Scripts/Entry.cs` | installs config, runtime, monitor |
| `LanConnectLobbyRuntime` | class | `sts2-lan-connect/Scripts/Lobby/LanConnectLobbyRuntime.cs` | hosted/joined session lifecycle |
| `LanConnectLobbyOverlay` | class | `sts2-lan-connect/Scripts/Lobby/LanConnectLobbyOverlay.cs` | main lobby UI and dialogs |

## CONVENTIONS
- TypeScript service is strict ESM: keep `node:` imports for built-ins and include `.js` extensions in local imports.
- Service tests live beside source as `*.test.ts` and run through Node's built-in test runner after compilation.
- The public Node service uses the standard package script shape (`build`, `check`, `start`, `test`) and runs as an in-memory + WS/UDP service.
- C# mod code keeps the `LanConnect*` naming family and uses `Sts2LanConnect.Scripts` namespace.
- Godot/UI hooks prefer deferred installation (`Callable.From(...).CallDeferred()`) and `TaskHelper.RunSafely(...)` for fire-and-forget async work.
- User-facing docs and many runtime messages are Chinese; preserve existing language unless the surrounding file uses English.

## ANTI-PATTERNS (THIS PROJECT)
- Do not edit `releases/` copies directly; update source and rerun the package/sync scripts.
- Do not treat relay as the primary game protocol; it is a room-level fallback after direct connection fails.
- Do not add separate guidance files for `docs/`, `research/`, or `releases/` unless those areas become active editing domains.
- Do not bypass the existing validation/error-code flow in the service just to simplify handlers.
- Do not reintroduce private registry/admin service source into this public repo.

## UNIQUE STYLES
- The repo is intentionally split by runtime, not by shared library abstractions.
- `scripts/package-*.sh` and `scripts/sync-release-repo.sh` define release contents; mirrored trees under `releases/` are disposable outputs.
- `research/` files are reconstructed notes, not authoritative specs.

## COMMANDS
```bash
# lobby service
cd lobby-service && npm ci
cd lobby-service && npm run check
cd lobby-service && npm run test

# client mod
./scripts/build-sts2-lan-connect.sh
./scripts/package-sts2-lan-connect.sh

# service package / deploy helpers
./scripts/package-lobby-service.sh
./scripts/sync-release-repo.sh --repo-dir <public-repo-clone>
```

## NOTES
- There is no CI workflow in `.github/workflows/`; local verification matters.
- `scripts/` is cross-cutting and stays governed here unless it grows into an independent workstream.
- `releases/sts2_lobby_service/lobby-service/` looks source-like but is still mirrored output.
