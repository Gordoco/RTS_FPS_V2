# Steam Network Transport Adapter (Future)

This folder is reserved for the v2 Steam Networking transport adapter.

## Responsibilities (not implemented in v1)

- Implement `INetworkTransport` using Steamworks networking (relay, NAT traversal).
- Map `ConnectionEndpoint.SteamLobby` to Steam lobby create/join flows.
- Wire NGO `NetworkManager` to a Steam-compatible `NetworkTransport` implementation.
- Keep session, entity, sync, interest, and role APIs unchanged.

## Migration seam

Gameplay and session code depend on `INetworkTransport` and `ConnectionEndpoint` only.
Replacing `NgoUnityTransportAdapter` with `SteamNetworkTransportAdapter` should not
require changes outside `Assets/Scripts/Networking/Transports/Steam/`.
