using System.Text.Json;
using System.ComponentModel;
using G5e.AzureDevOpsServerMCP.Application.Services;
using ModelContextProtocol.Server;

namespace G5e.AzureDevOpsServerMCP.Tools;

/// <summary>
/// Work item tools for Azure DevOps MCP server.
/// </summary>
[McpServerToolType]
public class WorkItemTools
{
    private readonly IWorkItemContextService _workItemContextService;

    public WorkItemTools(IWorkItemContextService workItemContextService)
    {
        _workItemContextService = workItemContextService ?? throw new ArgumentNullException(nameof(workItemContextService));
    }

    /// <summary>
    /// Gets the context of a work item including its details and comments.
    /// </summary>
    /// <param name="action">The action to perform. Supported value: get</param>
    /// <param name="collection">The Azure DevOps collection name</param>
    /// <param name="project">The Azure DevOps project name or ID</param>
    /// <param name="workItemId">The numeric work item ID</param>
    /// <returns>JSON object with work item details and comments</returns>
    [McpServerTool(Name = "wit_work_item")]
    [Description("Retrieve work item data. Supported action: get (retrieves a work item's context including title, state, description, assigned user, and all comments).")]
    public async Task<string> GetWorkItemContext(string action, string collection, string project, int workItemId)
    {
        if (action != "get")
            return JsonSerializer.Serialize(new { error = $"Unsupported action '{action}'. Use 'get'." });
        try
        {
            var ctx = await _workItemContextService.GetWorkItemContextAsync(collection, project, workItemId);

            var result = new
            {
                workItem = new
                {
                    id = ctx.Id,
                    title = ctx.Title,
                    type = ctx.Type,
                    state = ctx.State,
                    description = ctx.Description,
                    assignedTo = ctx.AssignedTo,
                    url = ctx.Url
                },
                comments = ctx.Comments.Select(c => new
                {
                    id = c.Id,
                    author = c.Author,
                    content = c.Content,
                    createdDate = c.CreatedDate
                }),
                commentCount = ctx.Comments.Count
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name });
        }
    }

    /// <summary>
    /// Adds or updates a comment on a work item.
    /// </summary>
    /// <param name="action">The action to perform: add or update</param>
    /// <param name="collection">The Azure DevOps collection name</param>
    /// <param name="project">The Azure DevOps project name or ID</param>
    /// <param name="workItemId">The numeric work item ID</param>
    /// <param name="comment">The comment content. Azure DevOps stores comments as rich text and supports both plain text and basic HTML formatting (div, br, nbsp, span, lists, etc.).</param>
    /// <param name="commentId">The comment ID to update; required when action is update</param>
    [McpServerTool(Name = "wit_work_item_comment_write")]
    [Description("Write comments on a work item. Supported actions: add (adds a new comment, requires comment), update (updates an existing comment, requires comment and commentId).")]
    public async Task<string> CommentWrite(string action, string collection, string project, int workItemId, string comment, int commentId = 0)
    {
        if (action == "add")
        {
            try
            {
                var result = await _workItemContextService.AddCommentAsync(collection, project, workItemId, comment);
                return JsonSerializer.Serialize(new
                {
                    commentId = result.CommentId,
                    url = result.Url,
                    success = true
                }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name });
            }
        }
        else if (action == "update")
        {
            if (commentId <= 0)
                return JsonSerializer.Serialize(new { error = "commentId is required for action 'update'." });
            try
            {
                var result = await _workItemContextService.UpdateCommentAsync(collection, project, workItemId, commentId, comment);
                return JsonSerializer.Serialize(new
                {
                    commentId = result.CommentId,
                    workItemId = result.WorkItemId,
                    text = result.Text,
                    version = result.Version,
                    url = result.Url,
                    success = true
                }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name });
            }
        }
        else
        {
            return JsonSerializer.Serialize(new { error = $"Unsupported action '{action}'. Use 'add' or 'update'." });
        }
    }

    /// <summary>
    /// Creates a new work item in a project.
    /// </summary>
    /// <param name="action">The action to perform. Supported value: create</param>
    /// <param name="collection">The Azure DevOps collection name</param>
    /// <param name="project">The Azure DevOps project name or ID</param>
    /// <param name="workItemType">The work item type (e.g., "Task", "Bug", "User Story")</param>
    /// <param name="title">The work item title</param>
    /// <param name="description">The work item description (optional)</param>
    /// <returns>JSON object with the created work item ID, title, type, and URL</returns>
    [McpServerTool(Name = "wit_work_item_write")]
    [Description("Write work item data. Supported action: create (creates a new work item with a specified type, title, and optional description).")]
    public async Task<string> CreateWorkItem(string action, string collection, string project, string workItemType, string title, string? description = null)
    {
        if (action != "create")
            return JsonSerializer.Serialize(new { error = $"Unsupported action '{action}'. Use 'create'." });
        try
        {
            var result = await _workItemContextService.CreateWorkItemAsync(collection, project, workItemType, title, description);

            return JsonSerializer.Serialize(new
            {
                workItemId = result.WorkItemId,
                title = result.Title,
                type = result.Type,
                url = result.Url,
                success = true
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name });
        }
    }
}
