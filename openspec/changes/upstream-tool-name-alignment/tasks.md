## 1. Infrastructure — Artifact Link Types

- [ ] 1.1 Add `ArtifactLinkType` enum (`Branch`, `Commit`, `Hyperlink`) to the Application layer (e.g., `dotnet/src/G5e.AzureDevOpsServerMCP.Application/Services/`)
- [ ] 1.2 Add `LinkArtifactToWorkItemAsync(string collection, string project, string repository, ArtifactLinkType type, string linkTarget, int workItemId, CancellationToken)` to `IRepositoryService`
- [ ] 1.3 Implement `LinkArtifactToWorkItemAsync` in `AzureDevOpsRepositoryService`:
  - `Branch`: `vstfs:///Git/Ref/{projectId}/{repoId}/GB{Uri.EscapeDataString(branchName)}`, `rel = "ArtifactLink"`, `name = "Branch"`
  - `Commit`: `vstfs:///Git/Commit/{projectId}/{repoId}/{commitId}`, `rel = "ArtifactLink"`, `name = "Fixed in Changeset"`
  - `Hyperlink`: `rel = "Hyperlink"`, `url = raw URL` (no vstfs)

## 2. Tools Layer — WorkItemTools.cs

- [ ] 2.1 Rename `[McpServerTool(Name = "wit_get_work_item")]` → `[McpServerTool(Name = "wit_work_item")]`; add `string action` parameter (validated as `"get"`); update `[Description]`
- [ ] 2.2 Rename `[McpServerTool(Name = "wit_work_item_write_create")]` → `[McpServerTool(Name = "wit_work_item_write")]`; add `string action` parameter (validated as `"create"`); update `[Description]`
- [ ] 2.3 Merge `AddWorkItemComment` and `UpdateWorkItemComment` into a single `CommentWrite` method registered as `[McpServerTool(Name = "wit_work_item_comment_write")]` with `string action` (`"add"` | `"update"`); dispatch to the appropriate service call based on `action`; update `[Description]`

## 3. Tools Layer — RepositoryTools.cs

- [ ] 3.1 Rename `[McpServerTool(Name = "wit_add_artifact_link")]` → `[McpServerTool(Name = "wit_work_item_link_write")]`; add `string action` (validated as `"add_artifact_link"`), `string type` (`"branch"` | `"commit"` | `"hyperlink"`), and `string linkTarget` (branch name, commit SHA, or URL); delegate to `LinkArtifactToWorkItemAsync`; update `[Description]`
- [ ] 3.2 Rename `[McpServerTool(Name = "repo_create_pull_request")]` → `[McpServerTool(Name = "repo_pull_request_write")]`; add `string action` parameter (validated as `"create"`); update `[Description]`

## 4. Tests — Integration Tests

- [ ] 4.1 Update `WorkItemToolsFixtureTests.cs`: replace all assertions on `"wit_get_work_item"` with `"wit_work_item"`
- [ ] 4.2 Update `WorkItemToolsFixtureTests.cs`: replace all assertions on `"wit_work_item_write_create"` with `"wit_work_item_write"`
- [ ] 4.3 Update `WorkItemToolsFixtureTests.cs`: replace all assertions on `"wit_add_work_item_comment"` and `"wit_update_work_item_comment"` with `"wit_work_item_comment_write"`
- [ ] 4.4 Update `RepositoryToolsFixtureTests.cs`: replace all assertions on `"wit_add_artifact_link"` with `"wit_work_item_link_write"`
- [ ] 4.5 Update `RepositoryToolsFixtureTests.cs`: replace all assertions on `"repo_create_pull_request"` with `"repo_pull_request_write"`
- [ ] 4.6 Add fixture test covering `wit_work_item_link_write` with `type=commit` and `type=hyperlink`

## 5. Documentation

- [ ] 5.1 Update `FORK_MATRIX.md`: replace all upstream capability column values with new tool names and annotate which `action` values the fork implements for each row
