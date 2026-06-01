## ADDED Requirements

### Requirement: Entities are registered by prefab key

The system SHALL maintain a registry mapping string prefab keys to network-ready prefabs that can be resolved at spawn time on the server.

#### Scenario: Spawn with registered key

- **WHEN** the server requests a spawn using a registered prefab key
- **THEN** the corresponding prefab is instantiated and assigned a stable network identifier

#### Scenario: Spawn with unregistered key

- **WHEN** the server requests a spawn using an unknown prefab key
- **THEN** the spawn request fails
- **AND** an error is logged without affecting existing entities

### Requirement: Server controls entity spawn and despawn

The system SHALL only allow the server to spawn and despawn networked entities. Clients MUST NOT directly instantiate networked entities.

#### Scenario: Server spawns entity visible to clients

- **WHEN** the server spawns a registered entity
- **THEN** all connected clients receive the entity within one network tick
- **AND** the entity appears at the specified world position

#### Scenario: Server despawns entity

- **WHEN** the server despawns a networked entity
- **THEN** the entity is destroyed on the server and all clients
- **AND** a despawn event fires with the entity's network identifier

### Requirement: Entities support ownership assignment

The system SHALL assign an owning client identifier to each spawned entity when specified, and expose ownership queries to gameplay systems.

#### Scenario: Player-owned character spawn

- **WHEN** the server spawns a character entity with an owner client identifier
- **THEN** only the owning client receives owner-write authority for designated owner-writable state
- **AND** non-owning clients receive read-only replicated state

### Requirement: Entity lookup by network identifier

The system SHALL provide lookup of live entities by network identifier on both server and clients.

#### Scenario: Lookup existing entity

- **WHEN** gameplay code queries an entity by a valid network identifier
- **THEN** the corresponding entity reference is returned

#### Scenario: Lookup despawned entity

- **WHEN** gameplay code queries an entity by a network identifier that has been despawned
- **THEN** the lookup returns no entity without throwing
