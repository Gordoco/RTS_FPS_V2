## Why

RTS_FPS_V2 is an asymmetrical game that mixes first-person character control with real-time strategy unit management on a shared world. Every future gameplay system—combat, economy, AI, building placement—depends on a single, authoritative client-server replication layer that can scale from a handful of FPS characters to hundreds of RTS units and environmental state. The project currently has no networking stack; establishing a reusable framework now prevents each feature team from inventing incompatible replication patterns later.

## What Changes

- Add Unity Netcode for GameObjects as the **initial** transport and RPC foundation, wrapped behind project-specific abstractions so gameplay code never couples directly to Netcode APIs.
- Design a **transport abstraction** (`INetworkTransport`) so a future version can swap Unity Transport for **Steam Networking** (Steamworks relay/P2P) without rewriting session, entity, or replication logic.
- Introduce a **network session** layer: host/client bootstrap, connection lifecycle, player join/leave, and session configuration (tick rate, max players, role assignment hooks).
- Introduce a **networked entity** layer: spawn/despawn registry, stable network IDs, ownership/authority rules, and prefab registration so units, characters, and world objects share one replication path.
- Introduce **state replication** components and patterns: transform sync, physics state, and a generic custom-state channel for gameplay variables (health, orders, build progress) with server authority and client prediction hooks where appropriate.
- Introduce **interest management** primitives: spatial relevance and role-based visibility so RTS-scale entity counts do not flood every client with full world state.
- Introduce **network roles** for asymmetrical play: per-connection role type (e.g., FPS operator, RTS commander), role-specific input authority, and camera/perspective routing without duplicating entity definitions.
- Add a **multiplayer test harness**: headless or multi-editor play-mode tests that validate spawn, sync, authority transfer, and disconnect cleanup.
- Organize all networking code under `Assets/Scripts/Networking/` with a dedicated assembly definition, keeping it isolated from gameplay assemblies that consume it via interfaces.

## Capabilities

### New Capabilities

- `network-session`: Host/client lifecycle, transport configuration, player connection events, and session settings.
- `networked-entities`: Entity registry, spawn/despawn pipeline, network ID assignment, ownership, and prefab registration.
- `state-replication`: Reusable sync components and patterns for transform, physics, and arbitrary gameplay state with server authority.
- `interest-management`: Spatial and role-based relevance filtering to limit which entities replicate to which clients.
- `network-roles`: Asymmetrical role assignment, per-role authority scopes, and perspective routing for FPS vs RTS players.
- `network-testing`: Automated and manual multiplayer test utilities for validating replication correctness.

### Modified Capabilities

<!-- No existing specs in openspec/specs/ -->

## Impact

- **Dependencies**: Adds `com.unity.netcode.gameobjects` (and Unity Transport) to `Packages/manifest.json` for v1. Steamworks / Steam Networking packages are **not** added now but the transport interface is shaped to accommodate them later.
- **Code**: New `Assets/Scripts/Networking/` assemblies split into transport-agnostic abstractions and an NGO transport adapter; gameplay assemblies reference abstractions only.
- **Scenes**: A bootstrap/network manager prefab and a multiplayer test scene will be required.
- **Tests**: New Play Mode tests under `Assets/Tests/PlayMode/Networking/` for session and replication smoke tests.
- **Systems**: All future gameplay features (units, weapons, buildings, environment destruction) will build on these abstractions rather than calling Netcode directly.
