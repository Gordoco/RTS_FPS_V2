## 1. Package and Assembly Setup

- [x] 1.1 Add `com.unity.netcode.gameobjects` and `com.unity.transport` to `Packages/manifest.json`
- [x] 1.2 Create `Assets/Scripts/Networking/` folder structure (Abstractions, Core, Entities, Sync, Interest, Roles, Transports/Ngo, Transports/Steam placeholder)
- [x] 1.3 Create `RTS_FPS_V2.Networking.Abstractions.asmdef` (no NGO refs) and `RTS_FPS_V2.Networking.Ngo.asmdef` (references NGO + Abstractions); update main `RTS_FPS_V2.asmdef` to reference Abstractions only
- [x] 1.4 Verify the project compiles with no errors after package and assembly changes

## 2. Transport Abstraction and Network Session (network-session)

- [x] 2.1 Define `INetworkTransport`, `ConnectionEndpoint` (DirectIp + reserved SteamLobby shape), `TransportType` enum, and transport lifecycle events in Abstractions
- [x] 2.2 Implement `NgoUnityTransportAdapter` in `Transports/Ngo/` wrapping UTP direct-IP host/client start/stop
- [x] 2.3 Define `INetworkSession`, `SessionConfig`, and session lifecycle event types in Abstractions (transport-agnostic)
- [x] 2.4 Implement `NetworkSessionManager` in Core that delegates connection operations to `INetworkTransport` and replication to NGO internally
- [x] 2.5 Implement connection lifecycle events (player join, leave, session start, shutdown, connection failed/rejected)
- [x] 2.6 Enforce max-player limit and port-in-use error handling per spec
- [x] 2.7 Create a `NetworkBootstrap` prefab with session manager, injectable `NgoUnityTransportAdapter`, and default `SessionConfig` asset

## 3. Networked Entities (networked-entities)

- [x] 3.1 Define `INetworkEntity`, `INetworkEntitySpawner`, and entity lifecycle events in Abstractions
- [x] 3.2 Implement `NetworkEntityRegistry` with string-key → prefab registration (ScriptableObject or inspector-driven)
- [x] 3.3 Implement server-only `NetworkEntitySpawner` using registry lookup, ownership assignment, and NGO spawn
- [x] 3.4 Implement server-only despawn with lifecycle event firing
- [x] 3.5 Implement entity lookup by network identifier on server and clients
- [x] 3.6 Create two test prefabs: `TestCharacter` (owner-writable) and `TestUnit` (server-authoritative)

## 4. State Replication (state-replication)

- [x] 4.1 Define `INetworkTransformSync` and `INetworkStateSync<T>` interfaces in Abstractions
- [x] 4.2 Implement `NetworkTransformSync` wrapper around NGO `NetworkTransform` with server authority
- [x] 4.3 Implement `NetworkStateSync` generic component using `NetworkVariable<T>` with configurable write permissions
- [x] 4.4 Implement server RPC command channel pattern (`SubmitCommand`) for owner-client input forwarding
- [x] 4.5 Attach sync components to test prefabs and verify composability (transform + custom state on same entity)

## 5. Interest Management (interest-management)

- [x] 5.1 Implement server-side `SpatialGrid` pure logic class with unit-testable cell queries
- [x] 5.2 Implement `InterestManager` server component with configurable tick interval
- [x] 5.3 Integrate role-based entity category filters into relevance evaluation
- [x] 5.4 Drive NGO `NetworkShow`/`NetworkHide` from interest evaluation results
- [x] 5.5 Implement distant-entity sync rate throttling on transform sync with configurable distance thresholds

## 6. Network Roles (network-roles)

- [x] 6.1 Define `NetworkRole` enum, `IRoleContext`, and authority scope definitions in Abstractions
- [x] 6.2 Implement server-side role assignment on client join with role-assigned/changed events
- [x] 6.3 Implement authority scope enforcement on server RPC command validation
- [x] 6.4 Implement client-side `PerspectiveRouter` that activates FPS or RTS camera/input rig based on role
- [x] 6.5 Wire role context queries into interest manager category filters

## 7. Multiplayer Test Scene and Manual Verification (network-testing)

- [x] 7.1 Create `MultiplayerTestScene` with host/join UI, connection status display, and spawn test buttons
- [x] 7.2 Add scene to build settings and document two-editor manual test workflow in scene README comment
- [x] 7.3 Verify manual host + client test: spawn entity on host appears on client with correct transform

## 8. Automated Tests (network-testing)

- [x] 8.1 Create `Assets/Tests/PlayMode/Networking/` with asmdef referencing networking assembly and NGO test utilities
- [x] 8.2 Write Edit Mode tests for `SpatialGrid` relevance queries
- [x] 8.3 Write Edit Mode tests for role authority scope evaluation
- [x] 8.4 Write Play Mode smoke test: host + client spawn sync with transform tolerance check
- [x] 8.5 Write Play Mode test: client disconnect triggers player-left event and owned entity cleanup

## 9. Integration and Documentation

- [x] 9.1 Add public API summary in Abstractions documenting extension points and noting `INetworkTransport` as the Steam migration seam
- [x] 9.2 Run full Edit Mode and Play Mode test suites and fix any failures
- [x] 9.3 Review that no gameplay assembly code references NGO or transport types directly (abstraction boundary check)
- [x] 9.4 Add `Transports/Steam/README.md` stub describing future Steam adapter responsibilities (no implementation in v1)
