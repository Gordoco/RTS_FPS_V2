/*
 * RTS_FPS_V2 Networking — Public API (Abstractions)
 *
 * Extension points for gameplay systems:
 * - INetworkTransport  — connection seam; swap NgoUnityTransportAdapter for Steam later
 * - INetworkSession      — host/client lifecycle and player events
 * - INetworkEntitySpawner — server-side spawn/despawn
 * - INetworkTransformSync / INetworkStateSync — replicated state reads
 * - IRoleContext         — asymmetrical role and authority queries
 *
 * Local multiplayer testing: use Unity Multiplayer Play Mode (com.unity.multiplayer.playmode)
 * with MultiplayerPlayModeAutoConnect — no second editor required.
 *
 * Gameplay assemblies MUST reference this assembly only, never NGO or transport types.
 */
