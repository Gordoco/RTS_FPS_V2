## ADDED Requirements

### Requirement: Transform state replicates from server to clients

The system SHALL provide a reusable transform sync component that replicates position and rotation from the server to all relevant clients.

#### Scenario: Server moves entity

- **WHEN** the server updates an entity's transform
- **THEN** connected clients observe the updated position within two network ticks

#### Scenario: Client cannot authoritatively move server-owned entity

- **WHEN** a non-owning client attempts to directly set the transform of a server-authoritative entity
- **THEN** the change is not applied on the server
- **AND** the client transform is corrected to match server state on the next sync

### Requirement: Custom gameplay state replicates via typed sync channels

The system SHALL provide a generic custom-state sync pattern that replicates named gameplay variables (e.g., health, order queue, build progress) with configurable write permissions.

#### Scenario: Server updates gameplay variable

- **WHEN** the server modifies a server-authoritative gameplay variable on an entity
- **THEN** all clients with visibility to that entity receive the updated value

#### Scenario: Owner client sends input via command channel

- **WHEN** an owning client sends an input command through the designated command channel
- **THEN** the server receives the command via a server RPC
- **AND** the server validates and applies the command before replicating resulting state

### Requirement: Sync components are composable on any registered entity

The system SHALL allow multiple sync components (transform, physics, custom state) to be attached to a single networked entity prefab without conflicting network identifiers.

#### Scenario: Entity with transform and custom state

- **WHEN** an entity prefab includes both transform sync and custom state sync components
- **THEN** both state domains replicate independently and correctly on spawn

### Requirement: Replication abstractions hide transport details

Gameplay assemblies SHALL consume sync state through public interfaces and MUST NOT reference underlying transport sync types directly.

#### Scenario: Gameplay reads replicated health

- **WHEN** a gameplay system reads an entity's health through the public sync interface
- **THEN** the value reflects the last server-authoritative update without requiring transport API calls
