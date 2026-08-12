## Why

The upstream `microsoft/azure-devops-mcp` performed a major tool consolidation (v2.9.0 window) that renamed and merged the MCP tool surface from individual per-operation tools to action-parameter-based consolidated tools. The fork's dotnet implementation still registers the pre-consolidation tool names, creating a systematic naming divergence that breaks compatibility with any client following the upstream tool registry. This change aligns the fork's registered tool names and parameter contracts with the upstream consolidated model.

## What Changes

- **BREAKING** Rename `wit_get_work_item` → `wit_work_item` (add `action` parameter, `action=get`)
- **BREAKING** Rename `wit_work_item_write_create` → `wit_work_item_write` (add `action` parameter, `action=create`)
- **BREAKING** Merge `wit_add_work_item_comment` + `wit_update_work_item_comment` → `wit_work_item_comment_write` (add `action` parameter, `action=add|update`)
- **BREAKING** Rename `wit_add_artifact_link` → `wit_work_item_link_write` (add `action` parameter, `action=add_artifact_link`; expand link type support to `branch | commit | hyperlink`)
- **BREAKING** Rename `repo_create_pull_request` → `repo_pull_request_write` (add `action` parameter, `action=create`)
- `repo_create_branch` is unchanged — upstream retained this name
- Update `FORK_MATRIX.md` to reflect new tool names and action-based capability tracking

This change affects the .NET fork surface under `dotnet/` exclusively. The root TypeScript files are upstream-tracking and are not modified.

## Capabilities

### New Capabilities

_(none — this change reshapes existing capabilities only)_

### Modified Capabilities

- `tool-capabilities`: Tool name contracts for all five implemented work item and repository write tools are changing. The observable MCP surface (registered tool names and their input schemas) changes for all tools except `repo_create_branch`.

## Impact

- `dotnet/src/G5e.AzureDevOpsServerMCP.Tools/WorkItemTools.cs` — rename tool attributes, merge comment tools, add `action` dispatch
- `dotnet/src/G5e.AzureDevOpsServerMCP.Tools/RepositoryTools.cs` — rename tool attributes, add `action` dispatch, expand artifact link types (commit, hyperlink)
- `dotnet/src/G5e.AzureDevOpsServerMCP.Infrastructure.AzureDevOps/Services/AzureDevOpsRepositoryService.cs` — extend `LinkBranchToWorkItemAsync` or add new method for commit and hyperlink artifact types
- `dotnet/src/G5e.AzureDevOpsServerMCP.Application/Services/IRepositoryService.cs` — update interface if a new method is added
- `dotnet/tests/G5e.AzureDevOpsServerMCP.IntegrationTests/WorkItemToolsFixtureTests.cs` — update all tool name assertions
- `dotnet/tests/G5e.AzureDevOpsServerMCP.IntegrationTests/RepositoryToolsFixtureTests.cs` — update all tool name assertions
- `FORK_MATRIX.md` — update capability rows to reference new tool names and action parameters
- Any MCP client configuration pointing to old tool names must be updated manually (out of scope for this change)
