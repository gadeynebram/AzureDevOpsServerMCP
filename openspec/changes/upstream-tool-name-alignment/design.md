## Context

The fork's dotnet implementation registered MCP tools under pre-consolidation names that existed in upstream before the v2.9.0 tool consolidation. Upstream merged multiple single-purpose tools into action-parameter-based consolidated tools (e.g., `wit_add_work_item_comment` + `wit_update_work_item_comment` → `wit_work_item_comment_write` with `action=add|update`). The fork must adopt the same registered tool names and parameter shapes to stay compatible with clients following the upstream tool registry.

The fork runs exclusively on the dotnet surface. The root TypeScript files are not modified.

## Goals / Non-Goals

**Goals:**
- Align all five affected MCP tool names with upstream's post-consolidation registry
- Match upstream's `action` parameter pattern for each consolidated tool
- Expand `wit_work_item_link_write` artifact link support to `branch | commit | hyperlink` (NuGet client does not constrain this — it is pure vstfs URL construction)
- Update `FORK_MATRIX.md` to track tool name + action for each implemented capability

**Non-Goals:**
- Implementing upstream actions the fork does not support (e.g., `wit_work_item` `action=update`, `action=update_batch`)
- Email mention resolution (`@<email>` → identity GUID lookup) — not advertised, not broken, deferred
- Wiki, build, or pull-request artifact link types in `wit_work_item_link_write`
- Any changes to the root TypeScript files

## Decisions

### Decision 1: Merge comment tools into a single C# method

**Choice:** Collapse `AddWorkItemComment` and `UpdateWorkItemComment` into a single `CommentWrite` method on `WorkItemTools` with an `action` string parameter (`"add"` | `"update"`).

**Alternatives considered:**
- Keep two separate C# methods, register both under the new single tool name — not possible; MCP SDK requires one method per `[McpServerTool]` registration.
- Use two separate registrations with the same `Name` — would cause a runtime conflict.

**Rationale:** One `[McpServerTool]` attribute per registered tool name is the SDK contract. Merging is the only clean option.

### Decision 2: Keep separate C# methods for other consolidated tools

**Choice:** For `wit_work_item` (action=get), `wit_work_item_write` (action=create), `wit_work_item_link_write` (action=add_artifact_link), and `repo_pull_request_write` (action=create) — keep one C# method each and advertise only the implemented action in the `[Description]` annotation.

**Alternatives considered:**
- Add full action dispatch with "not implemented" errors for other actions — adds noise and may confuse LLM clients.

**Rationale:** MCP clients use the `Description` to decide what actions to invoke. Advertising only `action=get` (for example) is cleaner and prevents clients from calling unimplemented actions.

### Decision 3: Extend artifact link support via a new infrastructure method

**Choice:** Add a new `LinkArtifactToWorkItemAsync(ArtifactLinkType type, ...)` method to `IRepositoryService` and `AzureDevOpsRepositoryService`, where `ArtifactLinkType` is an enum covering `Branch`, `Commit`, and `Hyperlink`. The existing `LinkBranchToWorkItemAsync` can be kept for internal use or delegated to.

**Alternatives considered:**
- Extend `LinkBranchToWorkItemAsync` with optional parameters — makes the method signature unwieldy and parameter combinations hard to validate.
- Inline all vstfs URL building in the tool method — moves infrastructure logic into the tools layer, violating layering.

**Rationale:** A typed enum + dedicated method keeps the infrastructure layer clean and testable.

**vstfs URL patterns per type:**
```
Branch:    vstfs:///Git/Ref/{projectId}/{repoId}/GB{Uri.EscapeDataString(branchName)}
Commit:    vstfs:///Git/Commit/{projectId}/{repoId}/{commitId}
Hyperlink: rel="Hyperlink" (not ArtifactLink), url = raw URL, no vstfs
```

### Decision 4: FORK_MATRIX.md updated to tool-name + action format

**Choice:** Replace upstream column values with new consolidated tool names and annotate which `action` values the fork implements.

**Rationale:** The upstream tool name is now the primary contract identifier. Action qualifies it.

## Risks / Trade-offs

- **Breaking change for existing clients** → Mitigation: clearly documented as breaking in proposal; clients must update their MCP tool configuration manually.
- **Partial action coverage** (fork only implements a subset of actions per tool) → Mitigation: `[Description]` annotations explicitly list only the implemented actions, preventing LLM clients from attempting unsupported ones.
- **Hyperlink `rel` difference** (`"Hyperlink"` vs `"ArtifactLink"`) → Mitigation: infrastructure method handles rel selection per type; tool layer does not need to know.

## Migration Plan

1. Deploy the updated dotnet server (new tool names take effect immediately on restart)
2. Update any MCP client configurations that reference the old tool names:
   - `wit_get_work_item` → `wit_work_item`
   - `wit_work_item_write_create` → `wit_work_item_write`
   - `wit_add_work_item_comment` / `wit_update_work_item_comment` → `wit_work_item_comment_write`
   - `wit_add_artifact_link` → `wit_work_item_link_write`
   - `repo_create_pull_request` → `repo_pull_request_write`
3. No data migration needed — tool renames have no persistence side effects.

## Open Questions

_(none — all decisions are resolved)_
