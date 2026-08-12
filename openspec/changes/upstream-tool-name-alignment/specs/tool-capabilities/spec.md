## MODIFIED Requirements

### Requirement: Existing tool contracts are compatibility-sensitive
Changes to supported capabilities SHALL preserve existing MCP tool contracts unless a proposal explicitly describes a contract change.

This change is an explicitly proposed contract change. The normative tool name contracts for the fork's implemented capabilities are updated below.

#### Scenario: Revising an existing tool behavior
- **WHEN** a change modifies an existing fork tool in a supported capability area
- **THEN** the proposal SHALL describe compatibility expectations for tool name, input shape, and observable behavior

### Requirement: Fork MCP tool names align with the upstream consolidated registry
The fork SHALL register its MCP tools under the post-consolidation tool names established by the upstream `microsoft/azure-devops-mcp` in the v2.9.0 release window.

#### Scenario: Client resolves work item retrieval tool
- **WHEN** a client queries the fork's MCP tool registry for work item retrieval
- **THEN** the tool SHALL be registered as `wit_work_item` with an `action` parameter
- **AND** the implemented action SHALL be `get`

#### Scenario: Client resolves work item write tool
- **WHEN** a client queries the fork's MCP tool registry for work item creation
- **THEN** the tool SHALL be registered as `wit_work_item_write` with an `action` parameter
- **AND** the implemented action SHALL be `create`

#### Scenario: Client resolves work item comment write tool
- **WHEN** a client queries the fork's MCP tool registry for comment operations
- **THEN** the tool SHALL be registered as `wit_work_item_comment_write` with an `action` parameter
- **AND** the implemented actions SHALL be `add` and `update`

#### Scenario: Client resolves work item link write tool
- **WHEN** a client queries the fork's MCP tool registry for artifact linking
- **THEN** the tool SHALL be registered as `wit_work_item_link_write` with an `action` parameter
- **AND** the implemented action SHALL be `add_artifact_link`
- **AND** the supported link types SHALL be `branch`, `commit`, and `hyperlink`

#### Scenario: Client resolves pull request write tool
- **WHEN** a client queries the fork's MCP tool registry for pull request creation
- **THEN** the tool SHALL be registered as `repo_pull_request_write` with an `action` parameter
- **AND** the implemented action SHALL be `create`

#### Scenario: Branch creation tool is unchanged
- **WHEN** a client queries the fork's MCP tool registry for branch creation
- **THEN** the tool SHALL remain registered as `repo_create_branch` with no `action` parameter

### Requirement: Capability references stay aligned
Changes that alter supported fork capabilities SHALL keep the normative spec and the fork capability reference documentation aligned.

#### Scenario: Updating supported capabilities
- **WHEN** this change is applied
- **THEN** `FORK_MATRIX.md` SHALL be updated to reference the new tool names and the actions implemented by the fork for each capability row
