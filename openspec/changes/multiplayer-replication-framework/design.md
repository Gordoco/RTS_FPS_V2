## Context

RTS_FPS_V2 is a greenfield Unity project (Input System, Test Framework, no existing netcode). The game combines FPS character control with RTS unit command on a shared map. Clients must see a consistent world state while the server remains authoritative. Entity counts will range from a few player characters to hundreds of AI-controlled units plus environmental objects.

Unity Netcode for GameObjects (NGO) is the chosen **v1 implementation**—it is officially supported, integrates with Unity's Multiplayer Center, and provides the RPC/NetworkVariable/NetworkObject primitives needed for client-server replication. A future version will adopt **Steam Networking** (Steamworks transport: relay, NAT traversal, lobby-based join) for production PC multiplayer. Gameplay code must not call NGO or Steam APIs directly; instead it consumes transport-agnostic abstractions so only the transport adapter layer changes when Steam is introduced.

## Goals / Non-Goals

**Goals:**

- Provide a reusable, testable networking foundation that all future gameplay systems build on.
- Support asymmetrical roles (FPS operator vs RTS commander) on the same session with distinct authority and visibility rules.
- Replicate units, characters, and environment objects through a unified entity pipeline with server authority.
- Scale RTS entity counts via interest management so clients receive only relevant state.
- Expose clear extension points (interfaces, events, base components) for gameplay teams.
- Include automated Play Mode tests for core replication flows.
- Keep the session and connection layer **transport-pluggable** so Steam Networking can replace Unity Transport later with minimal churn outside the adapter assembly.

**Non-Goals:**

- Steam Networking / Steamworks integration in v1 (design and interfaces only; no Steam packages or lobby UI yet).
- Dedicated server build pipeline and cloud deployment (future work; design leaves hooks only).
- Full game features: combat, economy, AI pathfinding, building placement logic.
- Client-side prediction and lag compensation for FPS shooting (interfaces reserved, not implemented).
- Relay/NAT punch-through production setup in v1 (use direct IP / localhost; Steam relay deferred to Steam transport adapter).
- Snapshot interpolation tuning beyond baseline transform sync.

## Decisions

### 1. NGO for v1 replication; Steam Networking as planned v2 transport

**Choice:** NGO with Unity Transport (UTP) over direct IP for the initial foundation. Steam Networking (via Steamworks / Facepunch.Steamworks or Unity's Steam transport adapter for NGO) is the planned production transport for PC release.

**Rationale:** NGO gives a working client-server stack immediately for development and automated tests. Steam provides relay, NAT traversal, and lobby/matchmaking that UTP direct-IP does not. Separating transport from session/entity logic avoids a costly rewrite when Steam is adopted.

**Alternatives considered:**
- *Start with Steam now*: Adds Steam client dependency, complicates CI/automated tests, and slows foundation work.
- *Mirror*: Mature but third-party; less integrated with Unity 6 tooling.
- *Custom UDP stack*: Maximum control but high cost; premature for foundation phase.

### 2. Transport abstraction (`INetworkTransport`) separate from replication abstractions

**Choice:** Two layers of abstraction:
1. **Transport layer** — `INetworkTransport`, `ConnectionEndpoint`, host/client start/stop, connection events. v1 implemented by `NgoUnityTransportAdapter`; future `SteamNetworkTransportAdapter` plugs in here.
2. **Replication layer** — `INetworkSession`, `INetworkEntity`, `INetworkSync` wrapping NGO's NetworkObject/NetworkVariable/RPC in v1. Replication stays on NGO even after Steam transport is added (NGO supports custom transports).

**Rationale:** Steam replaces *how clients connect*, not necessarily *how state replicates*. NGO's pluggable transport interface (`NetworkTransport`) allows swapping UTP for Steam's socket layer while keeping entity/sync code stable. Gameplay never references either transport.

**Alternatives considered:**
- *Direct NGO usage everywhere*: Faster initially but blocks clean Steam migration and couples gameplay to NGO connection APIs.
- *Full stack rewrite for Steam later*: Replacing NGO entirely would discard replication components; keeping NGO + swapping transport is lower risk.

### 3. Abstraction layer over NGO replication (`INetworkSession`, `INetworkEntity`, `INetworkSync`)

**Choice:** Thin wrapper interfaces in `RTS_FPS_V2.Networking.Abstractions` assembly; NGO types never leak into gameplay assemblies. NGO-specific implementations live in `RTS_FPS_V2.Networking.Ngo`.

**Rationale:** Gameplay systems depend on stable contracts. Transport swaps, NGO API churn, or dedicated-server paths affect only adapter assemblies.

**Alternatives considered:**
- *Single monolithic networking assembly*: Simpler initially but makes Steam adapter isolation harder and increases gameplay coupling risk.

### 4. Server-authoritative replication with optional owner-write NetworkVariables

**Choice:** Server owns all gameplay state mutations. Clients send input/commands via ServerRpc; server validates and applies. Transform sync uses NGO's `NetworkTransform` wrapped behind `NetworkTransformSync`.

**Rationale:** RTS/FPS hybrid needs cheat resistance and consistent unit state. Server authority is the standard pattern for both genres.

**Alternatives considered:**
- *Client authority for FPS characters*: Lower latency but complicates RTS unit consistency; deferred behind `INetworkPredictable` interface for future work.

### 5. Unified entity registry with typed spawn requests

**Choice:** `NetworkEntityRegistry` maps string prefab keys → registered prefabs. Spawning goes through `INetworkEntitySpawner.Spawn(key, position, ownerClientId, roleContext)` which assigns network ID, ownership, and fires lifecycle events.

**Rationale:** Units, characters, and environment props share spawn/despawn/lookup semantics. A registry avoids scattered `Instantiate` + manual `NetworkObject.Spawn` calls.

### 6. Layered interest management

**Choice:** Two-phase relevance:
1. **Role filter** — clients only subscribe to entity categories their role cares about (e.g., RTS commander gets all friendly units + fog-of-war slice; FPS operator gets nearby entities + owned character).
2. **Spatial grid** — within role-permitted entities, a server-side grid buckets entities; clients receive full state for nearby cells and throttled/dormant state for distant ones.

**Implementation:** Phase 1 uses NGO's `NetworkShow`/`NetworkHide` driven by a server-side `InterestManager` tick. Phase 2 adds distance-based sync rate scaling via custom sync components (not NGO's built-in interest management, which is scene-based and insufficient for dynamic RTS swarms).

**Alternatives considered:**
- *Replicate everything to everyone*: Simple but fails at RTS scale.
- *Separate scenes per role*: Breaks shared-world asymmetrical design.

### 7. Role system as session metadata + authority scopes

**Choice:** Each connected client receives a `NetworkRole` enum (`FpsOperator`, `RtsCommander`, `Spectator`) stored in session state. Authority scopes define which ServerRpc categories each role may invoke. Perspective routing (which camera rig activates) is a client-side concern driven by role assignment events.

**Rationale:** Keeps role logic centralized; gameplay systems query `IRoleContext` rather than hardcoding per-player checks.

### 8. Assembly and folder structure

```
Assets/Scripts/Networking/
  Abstractions/         — INetworkTransport, INetworkSession, INetworkEntity, INetworkSync (no NGO/Steam refs)
  Core/                 — session manager, bootstrap, events (depends on Abstractions)
  Entities/             — registry, spawner, lifecycle
  Sync/                 — transform, physics, custom state wrappers
  Interest/             — relevance manager, spatial grid
  Roles/                — role assignment, authority scopes
  Transports/
    Ngo/                — NgoUnityTransportAdapter, NGO replication implementations
    Steam/              — (future) SteamNetworkTransportAdapter — stub folder only in v1
Assets/Scripts/Networking/
  RTS_FPS_V2.Networking.Abstractions.asmdef   — no engine netcode refs
  RTS_FPS_V2.Networking.Ngo.asmdef            — references NGO + Abstractions
```

Gameplay assembly (`RTS_FPS_V2`) references `RTS_FPS_V2.Networking.Abstractions` only. NGO adapter references NGO. Future Steam adapter references Steamworks + Abstractions (and likely NGO for replication).

`ConnectionEndpoint` is a transport-agnostic struct supporting:
- `DirectIp` (host address + port) — v1 default
- `SteamLobby` (Steam lobby/match ID) — reserved for Steam adapter; unused in v1

### 9. Testing strategy

**Choice:** Play Mode tests using NGO's `NetworkManager` with host + client in-process (`MultiplayerPlayMode` or manual dual-instance). Edit Mode tests for pure-logic types (spatial grid, role scopes).

**Rationale:** Replication bugs only surface with actual network ticks; Edit Mode alone is insufficient.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| NGO API churn across Unity versions | Abstraction layer isolates gameplay; pin package version in manifest |
| RTS scale exceeds single-server capacity | Interest management + sync rate scaling; document horizontal sharding as future work |
| Asymmetrical visibility creates desync edge cases | Server remains sole authority; clients render fog/unknown state locally without affecting server truth |
| Wrapper layer adds indirection overhead | Keep wrappers thin (delegate to NGO); profile in test scene before optimizing |
| Multi-instance Play Mode tests are flaky | Use deterministic tick waits; retry hooks; separate fast smoke tests from full integration |
| No relay service → hard to test over internet | Acceptable for v1; Steam relay addresses this in transport adapter |
| Steam migration rewrites connection but not replication | Transport abstraction + NGO custom transport path limits scope of future change |
| Steam dependency breaks headless CI | Keep NGO/UTP as default transport in test configuration; Steam adapter tested separately |

## Migration Plan

1. Add NGO package to manifest; verify project compiles.
2. Land networking assembly with session bootstrap (no gameplay entities yet).
3. Add test scene with two spawnable prefabs (character + unit) to validate replication.
4. Wire interest management and roles incrementally behind feature flags (`NETWORK_INTEREST_MGMT`, `NETWORK_ROLES`).
5. Gameplay teams adopt interfaces as new systems are built; no existing gameplay code to migrate.

**Future Steam migration path (post-v1):**
1. Add Steamworks package and implement `SteamNetworkTransportAdapter` implementing `INetworkTransport`.
2. Wire NGO `NetworkManager` to use Steam's transport (Facepunch transport or Unity SteamNetworkingSockets adapter).
3. Extend bootstrap UI for Steam lobby create/join using `ConnectionEndpoint.SteamLobby`.
4. Run existing replication tests unchanged (still NGO replication; only connection path differs).
5. Keep UTP direct-IP adapter for local dev and CI.

Rollback: Remove networking assembly and NGO package; project returns to offline-only state.

## Open Questions

- **Steamworks package choice**: Facepunch.Steamworks vs official Steamworks.NET vs Unity's Steam transport package? (Decide when starting Steam adapter.)
- **Dedicated server headless build**: Required before production? (Leaning yes, but out of scope for initial foundation.)
- **Maximum target entity count**: Design assumes ~500 replicated entities with interest management; confirm with performance budget.
- **Physics authority**: Unity Physics on server only, or kinematic on clients with server correction? (Default: server physics for units, kinematic client display until perf testing.)
- **Save/reconnect**: Should entity registry persist across brief disconnects? (Deferred; document as non-goal for v1.)
