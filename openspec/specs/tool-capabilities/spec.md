# Capability: Tool Capabilities

## Purpose
Define the intended capability scope of the fork and how capability changes are governed.
## Requirements
### Requirement: Supported capability scope is intentionally limited
The fork SHALL support a pragmatic subset of Azure DevOps MCP capabilities centered on work item and repository collaboration workflows unless expanded by an explicit change.

#### Scenario: Assessing current fork scope
- GIVEN the current fork implementation is reviewed
- WHEN supported capability areas are described
- THEN work item and repository collaboration workflows SHALL be treated as the primary in-scope capability set
- AND unsupported upstream areas SHALL remain out of scope unless introduced by an explicit change proposal

### Requirement: Capability expansion is explicit
New capability areas SHALL require an explicit proposal before implementation.

#### Scenario: Adding a new upstream capability area
- GIVEN a change proposes support for a capability area not currently implemented in the fork
- WHEN the change is planned
- THEN the proposal SHALL identify the new capability area explicitly
- AND it SHALL describe how the scope affects fork priorities, contracts, and validation

### Requirement: Capability references stay aligned
Changes that alter supported fork capabilities SHALL keep the normative spec and the fork capability reference documentation aligned.

#### Scenario: Updating supported capabilities
- GIVEN a change adds, removes, or materially reshapes a supported capability
- WHEN the change artifacts are completed
- THEN the relevant OpenSpec capability specification SHALL be updated
- AND FORK_MATRIX.md SHALL be reviewed and updated if the capability snapshot has changed

### Requirement: Existing tool contracts are compatibility-sensitive
Changes to supported capabilities SHALL preserve existing MCP tool contracts unless a proposal explicitly describes a contract change.

#### Scenario: Revising an existing tool behavior
- GIVEN a change modifies an existing fork tool in a supported capability area
- WHEN the change is proposed
- THEN the proposal SHALL describe compatibility expectations for tool name, input shape, and observable behavior

### Requirement: Upstream synchronization is audit-first
The repository workflow SHALL provide an upstream synchronization audit phase that runs before any merge phase.

#### Scenario: Running upstream sync workflow
- GIVEN upstream updates may affect fork-specific behavior
- WHEN a sync operation is initiated
- THEN an upstream impact audit SHALL run first
- AND the audit SHALL produce a severity-classified report

### Requirement: Implemented fork capabilities receive targeted impact checks
The upstream audit SHALL explicitly evaluate impact on currently implemented fork collaboration capabilities.

#### Scenario: Upstream changes include work item or repository tools
- GIVEN upstream changes are fetched for comparison
- WHEN audit classification is performed
- THEN changes affecting implemented fork capabilities SHALL be identified explicitly
- AND findings SHALL be classified by severity (High, Medium, Low)

### Requirement: Merge phase targets a sync branch by default
The upstream merge phase SHALL merge into a sync branch derived from fork main, not directly into main by default.

#### Scenario: Executing approved merge phase
- GIVEN the audit phase is complete and user confirmation is provided
- WHEN merge execution starts
- THEN the merge target SHALL be a sync branch
- AND direct merge into main SHALL be blocked unless explicitly overridden by policy

### Requirement: High-impact findings gate merge execution
The merge phase SHALL be blocked when unresolved High-impact audit findings are present.

#### Scenario: Audit reports unresolved High findings
- GIVEN the upstream audit report contains unresolved High-impact findings
- WHEN merge execution is requested
- THEN merge execution SHALL be blocked
- AND the workflow SHALL require explicit resolution or acknowledgment path before proceeding

