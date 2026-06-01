## ADDED Requirements

### Requirement: Each connected client is assigned a network role

The system SHALL assign one network role per connected client from a defined set including at minimum FPS operator, RTS commander, and spectator.

#### Scenario: Role assigned on join

- **WHEN** a client successfully joins a session
- **THEN** the server assigns a network role before any role-scoped entities are spawned
- **AND** a role-assigned event fires on the server and the joining client

#### Scenario: Role change by server

- **WHEN** the server changes a client's network role during an active session
- **THEN** the new role takes effect on the next interest evaluation tick
- **AND** a role-changed event fires with the previous and new role

### Requirement: Roles define input authority scopes

The system SHALL enforce that clients can only send input commands permitted by their assigned role's authority scope.

#### Scenario: FPS operator sends movement input

- **WHEN** an FPS operator client sends movement input for their owned character
- **THEN** the server accepts and processes the command

#### Scenario: FPS operator sends unit order command

- **WHEN** an FPS operator client attempts to send an RTS unit order command
- **THEN** the server rejects the command
- **AND** no unit state changes occur

#### Scenario: RTS commander sends unit order command

- **WHEN** an RTS commander client sends a unit order command for a unit they control
- **THEN** the server accepts and processes the command

### Requirement: Role context is queryable by gameplay systems

The system SHALL expose the current role, owning client identifier, and authority scope through a queryable role context interface available on server and owning client.

#### Scenario: Gameplay checks authority before action

- **WHEN** a gameplay system queries whether the local client may issue build commands
- **THEN** the role context interface returns a boolean based on the assigned role and authority scope

### Requirement: Client perspective routing responds to role assignment

The system SHALL provide a client-side hook that activates the appropriate camera and input rig based on the assigned network role without duplicating entity prefabs per role.

#### Scenario: FPS role activates first-person rig

- **WHEN** a client receives an FPS operator role assignment
- **THEN** the first-person camera and input rig are activated
- **AND** the RTS command input rig remains inactive

#### Scenario: RTS role activates command rig

- **WHEN** a client receives an RTS commander role assignment
- **THEN** the RTS command camera and input rig are activated
- **AND** the first-person input rig remains inactive unless the role also owns an FPS character
