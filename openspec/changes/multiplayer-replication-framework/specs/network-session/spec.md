## ADDED Requirements

### Requirement: Host can start a network session

The system SHALL provide a host bootstrap path that starts a listen server, binds a configurable port, and transitions the local machine to an in-session state.

#### Scenario: Successful host start

- **WHEN** the host bootstrap is invoked with valid session configuration
- **THEN** a network session starts listening for client connections
- **AND** the host client is connected to the session as player index 0

#### Scenario: Host start with invalid port

- **WHEN** the host bootstrap is invoked with a port that is already in use
- **THEN** the session does not enter an in-session state
- **AND** a descriptive error is surfaced through the session error event

### Requirement: Client can join an existing session

The system SHALL provide a client bootstrap path that connects to a host using a transport-agnostic `ConnectionEndpoint`. v1 SHALL support direct IP and port; the endpoint model SHALL reserve fields for future Steam lobby-based join without API changes.

#### Scenario: Successful client join via direct IP

- **WHEN** a client bootstrap is invoked with a reachable direct-IP endpoint
- **THEN** the client connects to the session
- **AND** a player-joined event fires on both host and client

#### Scenario: Client join to unreachable host

- **WHEN** a client bootstrap is invoked with an unreachable address
- **THEN** the client remains out of session
- **AND** a connection-failed event fires with a timeout or unreachable reason

### Requirement: Session bootstrap is transport-pluggable

The system SHALL route all host and client connection operations through an `INetworkTransport` interface so a future Steam Networking adapter can replace the v1 Unity Transport adapter without changing session or gameplay APIs.

#### Scenario: Session uses injected transport adapter

- **WHEN** the network session manager starts a host or client
- **THEN** it delegates connection start/stop to the configured `INetworkTransport` implementation
- **AND** gameplay code interacts only with `INetworkSession`, not transport-specific types

#### Scenario: Transport adapter swap does not break session events

- **WHEN** a different `INetworkTransport` implementation is configured at bootstrap
- **THEN** session lifecycle events (player join, leave, shutdown) continue to fire through the same `INetworkSession` interface

### Requirement: Session exposes connection lifecycle events

The system SHALL raise events for player join, player leave, session start, and session shutdown that gameplay systems can subscribe to without referencing transport APIs.

#### Scenario: Remote player disconnect

- **WHEN** a connected client disconnects unexpectedly
- **THEN** a player-left event fires on the server with the disconnected client identifier
- **AND** the server cleans up session state associated with that client

### Requirement: Session configuration is centralized

The system SHALL expose session settings including maximum players, server tick rate, and transport port through a single configuration object applied at bootstrap time.

#### Scenario: Session respects max player limit

- **WHEN** the number of connected clients reaches the configured maximum
- **THEN** additional connection attempts are rejected
- **AND** a connection-rejected event fires on the server
