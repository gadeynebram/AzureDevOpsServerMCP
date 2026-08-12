# Fork Capability Matrix

Project: G5e.AzureDevOpsServerMCP

This matrix compares capabilities from the original Azure DevOps MCP Server with the current .NET fork.

- Upstream source: [docs/TOOLSET.md](docs/TOOLSET.md)
- Fork implementation: [dotnet](dotnet)

| Capability | Upstream tool (post-consolidation) | Fork tool | Implemented actions | Status |
| --- | --- | --- | --- | --- |
| Get work item context (details + comments) | `wit_work_item` | `wit_work_item` | `action=get` | Implemented |
| Create work item | `wit_work_item_write` | `wit_work_item_write` | `action=create` | Implemented |
| Add comment to work item | `wit_work_item_comment_write` | `wit_work_item_comment_write` | `action=add` | Implemented |
| Update comment on work item | `wit_work_item_comment_write` | `wit_work_item_comment_write` | `action=update` | Implemented |
| Create feature branch | `repo_create_branch` | `repo_create_branch` | — | Implemented |
| Link artifact to work item (branch, commit, hyperlink) | `wit_work_item_link_write` | `wit_work_item_link_write` | `action=add_artifact_link`, types: `branch`, `commit`, `hyperlink` | Implemented |
| Create pull request and link to work item | `repo_pull_request_write` | `repo_pull_request_write` | `action=create` | Implemented |
| Pipelines | `pipelines_*` | - | - | Not implemented |
| Wiki | `wiki_*` | - | - | Not implemented |
| Search | `search_*` | - | - | Not implemented |
| Test Plans | `testplan_*` | - | - | Not implemented |
| Advanced Security | `advsec_*` | - | - | Not implemented |
| Core (projects/teams/identity) | `core_*` | - | - | Not implemented |
| Work (iterations/capacity/settings) | `work_*` | - | - | Not implemented |

## Notes

- This fork currently focuses on a limited, pragmatic subset in the [dotnet](dotnet) implementation.
- The current scope mainly covers repository + work item collaboration workflows for feature delivery.
- `repo_create_pull_request` in this fork also links the pull request to a work item and supports an optional Markdown description for the pull request body.
- The implemented fork capabilities were validated with fixture-backed integration tests and manual smoke tests.

## NuGet Feed Example (GitHub Packages)

If you publish this fork's packages to GitHub Packages, consumers can configure a `NuGet.config` like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<packageSources>
		<add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
		<add key="github-g5e" value="https://nuget.pkg.github.com/gadeynebram/index.json" />
	</packageSources>
	<packageSourceCredentials>
		<github-g5e>
			<add key="Username" value="GITHUB_USERNAME" />
			<add key="ClearTextPassword" value="GITHUB_PAT" />
		</github-g5e>
	</packageSourceCredentials>
</configuration>
```

Notes:

- Replace `OWNER` with your GitHub organization or user name.
- The PAT must at least include `read:packages` for restore (and `write:packages` when publishing).
- Prefer CI/CD secrets or environment-based credential injection over committing plaintext tokens.
