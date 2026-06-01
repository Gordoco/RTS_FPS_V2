## ADDED Requirements

### Requirement: Play Mode tests validate host-client replication

The system SHALL include automated Play Mode tests that start a host and at least one client in-process and verify basic entity replication.

#### Scenario: Two-player spawn sync test

- **WHEN** the replication smoke test starts a host and one client
- **AND** the server spawns a test entity
- **THEN** the client observes the entity within a bounded wait time
- **AND** the entity's replicated transform matches the server position within tolerance

### Requirement: Play Mode tests validate disconnect cleanup

The system SHALL include automated Play Mode tests that verify session and entity cleanup when a client disconnects.

#### Scenario: Client disconnect cleanup test

- **WHEN** a connected client disconnects during an active test session
- **THEN** the server fires a player-left event
- **AND** entities owned exclusively by the disconnected client are cleaned up according to configured despawn policy

### Requirement: Edit Mode tests cover pure networking logic

The system SHALL include Edit Mode tests for networking logic that does not require a live transport, including spatial grid relevance and role authority scope evaluation.

#### Scenario: Spatial grid relevance unit test

- **WHEN** an entity is placed in a spatial grid cell adjacent to a client's tracked cell
- **THEN** the relevance query returns true for that entity-client pair

#### Scenario: Role authority scope unit test

- **WHEN** the authority scope for an RTS commander role is queried for unit order commands
- **THEN** the scope permits unit order commands
- **AND** the scope denies first-person weapon fire commands

### Requirement: Manual multiplayer test scene is provided

The system SHALL include a dedicated test scene with UI controls to start host, join as client, spawn test entities, and display connection status for manual verification.

#### Scenario: Manual host and client test

- **WHEN** a developer opens the multiplayer test scene and starts a host
- **AND** a second editor instance joins as client using the displayed address
- **THEN** both instances show connected status
- **AND** spawning a test entity from the host UI makes it appear on the client
