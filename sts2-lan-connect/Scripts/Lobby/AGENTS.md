# LOBBY SUBSYSTEM

## OVERVIEW
This hotspot handles lobby runtime state, HTTP/WS clients, UI overlay, join/host flows, save binding, relay fallback, and diagnostics.

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Session lifecycle and heartbeat loop | `LanConnectLobbyRuntime.cs` | hosted room + joined client control state |
| Main lobby UI, filters, dialogs, pagination | `LanConnectLobbyOverlay.cs` | very large file; keep edits surgical |
| REST calls to lobby service | `LanConnectLobbyApiClient.cs` | maps client actions to service routes |
| Control-channel WebSocket traffic | `LanConnectLobbyControlClient.cs` | host/client messaging over `/control` |
| Join ordering and fallback behavior | `LanConnectLobbyJoinFlow.cs` | direct vs relay sequencing |
| Host relay tunnel | `LanConnectLobbyRelayHostTunnel.cs` | host registration on relay side |
| Save binding / continue-run republish | `LanConnectMultiplayerSaveRoomBinding.cs`, `LanConnectContinueRunLobbyAutoPublisher.cs` | keep save-room invariants aligned |
| Diagnostics and debug export | `LanConnectDebugReport.cs`, `LanConnectSaveDiagnostics.cs` | useful when tracing field failures |

## CONVENTIONS
- Prefer extending existing runtime/session objects over creating parallel singleton state.
- Keep room metadata, save binding, and player-name directory updates synchronized with session attach/close points.
- Reuse the existing staged status/progress messaging style instead of adding raw popup spam.
- Large UI edits in `LanConnectLobbyOverlay.cs` should stay localized and preserve current filter/pagination/search behavior.
- When client/server contracts change, update both `LanConnectLobbyApiClient.cs` and `lobby-service/src/server.ts` / `lobby-service/src/store.ts` together.

## ANTI-PATTERNS
- Do not split connection-strategy behavior across multiple ad hoc branches; funnel it through existing join/runtime flow objects.
- Do not bypass `LanConnectLobbyRuntime` when wiring heartbeats, room close, or control-channel teardown.
- Do not hand-edit the overlay in ways that break deferred build/setup assumptions.
- Do not change save-slot semantics in client code without matching service-side validation.

## NOTES
- `LanConnectLobbyOverlay.cs` is intentionally large because it owns the whole in-game lobby surface; prefer helper extraction only when it reduces coupling.
- This subtree is the main integration boundary with the service: API shape drift shows up here first.
- Debug-report and diagnostics files are first stop for user-reported room/join mismatch issues.
