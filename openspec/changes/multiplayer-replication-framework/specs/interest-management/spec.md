## ADDED Requirements

### Requirement: Server determines entity visibility per client

The system SHALL run a server-side interest manager that decides which entities are visible to each connected client based on configured relevance rules.

#### Scenario: Entity enters client relevance zone

- **WHEN** an entity moves into a client's spatial relevance zone
- **THEN** the client begins receiving full replication for that entity

#### Scenario: Entity leaves client relevance zone

- **WHEN** an entity moves outside a client's spatial relevance zone
- **THEN** the client stops receiving updates for that entity
- **AND** the entity is hidden or placed in a dormant state on the client

### Requirement: Role-based entity categories filter relevance

The system SHALL support role-based filters so clients only receive entity categories permitted by their assigned network role.

#### Scenario: RTS commander receives unit updates

- **WHEN** a client is assigned the RTS commander role
- **THEN** the interest manager includes friendly and visible-enemy unit entities in that client's relevance set

#### Scenario: FPS operator receives nearby entities

- **WHEN** a client is assigned the FPS operator role
- **THEN** the interest manager includes the client's owned character and entities within a configurable proximity radius

### Requirement: Interest manager runs on a configurable tick interval

The system SHALL evaluate relevance rules on a server tick interval that is independent of the render frame rate and configurable via session settings.

#### Scenario: Relevance update after entity spawn

- **WHEN** a new entity is spawned on the server
- **THEN** the interest manager evaluates visibility for all connected clients on the next interest tick

### Requirement: Distant entities use reduced sync frequency

The system SHALL support throttled replication for entities that are role-relevant but spatially distant, reducing bandwidth while maintaining positional accuracy within a configurable tolerance.

#### Scenario: Distant unit sync throttle

- **WHEN** a unit is role-relevant to a client but beyond the full-fidelity distance threshold
- **THEN** the entity's transform updates at a reduced tick rate
- **AND** the client's displayed position remains within the configured tolerance of server truth
