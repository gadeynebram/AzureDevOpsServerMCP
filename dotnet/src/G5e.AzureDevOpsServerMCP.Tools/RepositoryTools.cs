using System.Text.Json;
using System.ComponentModel;
using G5e.AzureDevOpsServerMCP.Application.Services;
using ModelContextProtocol.Server;

namespace G5e.AzureDevOpsServerMCP.Tools;

/// <summary>
/// Repository tools for Azure DevOps MCP server.
/// </summary>
[McpServerToolType]
public class RepositoryTools
{
    private readonly IRepositoryService _repositoryService;

    public RepositoryTools(IRepositoryService repositoryService)
    {
        _repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
    }

    /// <summary>
    /// Creates a new feature branch from a source branch.
    /// </summary>
    /// <param name="collection">The Azure DevOps collection name</param>
    /// <param name="project">The Azure DevOps project name or ID</param>
    /// <param name="repository">The repository name or ID</param>
    /// <param name="branchName">The name of the new branch (e.g., "feature/TASK-123")</param>
    /// <param name="fromBranch">The source branch to create from (e.g., "develop")</param>
    /// <returns>JSON object with branch creation details</returns>
    [McpServerTool(Name = "repo_create_branch")]
    [Description("Creates a new feature branch in a Git repository from an existing branch.")]
    public async Task<string> CreateFeatureBranch(string collection, string project, string repository, string branchName, string fromBranch)
    {
        try
        {
            var result = await _repositoryService.CreateBranchAsync(collection, project, repository, branchName, fromBranch);

            var response = new
            {
                branchName = result.BranchName,
                objectId = result.ObjectId,
                url = result.Url,
                success = true
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name });
        }
    }

    /// <summary>
    /// Adds an artifact link to a work item.
    /// </summary>
    /// <param name="action">The action to perform. Supported value: add_artifact_link</param>
    /// <param name="type">The artifact type: branch, commit, or hyperlink</param>
    /// <param name="linkTarget">The branch name, commit SHA, or URL to link</param>
    /// <param name="collection">The Azure DevOps collection name</param>
    /// <param name="project">The Azure DevOps project name</param>
    /// <param name="workItemId">The work item ID to link to</param>
    /// <param name="repository">The repository name; required for branch and commit types</param>
    /// <returns>JSON object confirming the link was created</returns>
    [McpServerTool(Name = "wit_work_item_link_write")]
    [Description("Add an artifact link to a work item. Supported action: add_artifact_link. Supported types: branch (link a Git branch, requires repository), commit (link a commit SHA, requires repository), hyperlink (link any URL).")]
    public async Task<string> LinkArtifactToWorkItem(string action, string type, string linkTarget, string collection, string project, int workItemId, string? repository = null)
    {
        if (action != "add_artifact_link")
            return JsonSerializer.Serialize(new { error = $"Unsupported action '{action}'. Use 'add_artifact_link'." });
        if (!Enum.TryParse<ArtifactLinkType>(type, ignoreCase: true, out var linkType))
            return JsonSerializer.Serialize(new { error = $"Unsupported type '{type}'. Use 'branch', 'commit', or 'hyperlink'." });
        try
        {
            var result = await _repositoryService.LinkArtifactToWorkItemAsync(collection, project, repository, linkType, linkTarget, workItemId);

            return JsonSerializer.Serialize(new
            {
                workItemId = result.WorkItemId,
                linkType = result.LinkType,
                linkTarget = result.LinkTarget,
                success = true
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name });
        }
    }

    /// <summary>
    /// Creates a pull request and links it to a work item.
    /// </summary>
    /// <param name="action">The action to perform. Supported value: create</param>
    /// <param name="collection">The Azure DevOps collection name</param>
    /// <param name="project">The Azure DevOps project name</param>
    /// <param name="repository">The repository name</param>
    /// <param name="sourceBranch">The source branch (e.g., "feature/TASK-123")</param>
    /// <param name="targetBranch">The target branch (e.g., "main" or "develop")</param>
    /// <param name="title">The pull request title</param>
    /// <param name="workItemId">The work item ID to link to</param>
    /// <param name="description">Optional pull request description in Markdown format</param>
    /// <returns>JSON object with pull request details</returns>
    [McpServerTool(Name = "repo_pull_request_write")]
    [Description("Write pull request data. Supported action: create (creates a pull request and links it to a work item).")]
    public async Task<string> CreatePullRequestForWorkItem(string action, string collection, string project, string repository, string sourceBranch, string targetBranch, string title, int workItemId, string? description = null)
    {
        if (action != "create")
            return JsonSerializer.Serialize(new { error = $"Unsupported action '{action}'. Use 'create'." });
        try
        {
            var result = await _repositoryService.CreatePullRequestAsync(collection, project, repository, sourceBranch, targetBranch, title, description, workItemId);

            return JsonSerializer.Serialize(new
            {
                pullRequestId = result.PullRequestId,
                title = result.Title,
                url = result.Url,
                status = result.Status,
                success = true
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name });
        }
    }
}
