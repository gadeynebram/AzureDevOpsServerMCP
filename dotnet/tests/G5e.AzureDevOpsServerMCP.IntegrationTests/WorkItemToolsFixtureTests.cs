using System.Text.Json;
using G5e.AzureDevOpsServerMCP.Application.Services;
using G5e.AzureDevOpsServerMCP.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace G5e.AzureDevOpsServerMCP.IntegrationTests;

[TestClass]
public class WorkItemToolsFixtureTests
{
    [TestMethod]
    public async Task GetWorkItemContext_UsesFixtureBackedService_AndSerializesExpectedShape()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "work-item-context-result.json");
        var service = new FixtureBackedWorkItemContextService(fixturePath);
        var sut = new WorkItemTools(service);

        var json = await sut.GetWorkItemContext("DefaultCollection", "UZG.IZ.PrestIZ", 1);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var workItem = root.GetProperty("workItem");
        var comments = root.GetProperty("comments");

        Assert.AreEqual(1, workItem.GetProperty("id").GetInt32());
        Assert.AreEqual("Als ontwikkelaar wil ik work items ophalen via een MCP server zodat mijn AI-assistent context heeft over mijn taken", workItem.GetProperty("title").GetString());
        Assert.AreEqual("User Story", workItem.GetProperty("type").GetString());
        Assert.AreEqual("New", workItem.GetProperty("state").GetString());
        Assert.AreEqual("Gadeyne Bram", workItem.GetProperty("assignedTo").GetString());
        Assert.AreEqual(1, root.GetProperty("commentCount").GetInt32());
        Assert.AreEqual(1, comments.GetArrayLength());
        StringAssert.Contains(comments[0].GetProperty("content").GetString(), "Spike afgerond", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public async Task GetWorkItemContext_WhenServiceThrows_ReturnsSerializedError()
    {
        var sut = new WorkItemTools(new ThrowingWorkItemContextService(new InvalidOperationException("fixture failure")));

        var json = await sut.GetWorkItemContext("DefaultCollection", "UZG.IZ.PrestIZ", 1);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual("fixture failure", root.GetProperty("error").GetString());
        Assert.AreEqual("InvalidOperationException", root.GetProperty("type").GetString());
    }

    private sealed class FixtureBackedWorkItemContextService : IWorkItemContextService
    {
        private readonly string _fixturePath;

        public FixtureBackedWorkItemContextService(string fixturePath)
        {
            _fixturePath = fixturePath;
        }

        public async Task<WorkItemContextResult> GetWorkItemContextAsync(string collection, string project, int workItemId, CancellationToken cancellationToken = default)
        {
            var json = await File.ReadAllTextAsync(_fixturePath, cancellationToken);
            var result = JsonSerializer.Deserialize<WorkItemContextResult>(json);

            if (result is null)
            {
                throw new InvalidOperationException("Fixture could not be deserialized.");
            }

            return result;
        }

        public Task<AddCommentResult> AddCommentAsync(string collection, string project, int workItemId, string comment, CancellationToken cancellationToken = default)
            => Task.FromResult(new AddCommentResult { CommentId = 1, Url = string.Empty });

        public Task<UpdateCommentResult> UpdateCommentAsync(string collection, string project, int workItemId, int commentId, string text, CancellationToken cancellationToken = default)
            => Task.FromResult(new UpdateCommentResult { CommentId = commentId, WorkItemId = workItemId, Text = text, Version = 2, Url = string.Empty });

        public Task<CreateWorkItemResult> CreateWorkItemAsync(string collection, string project, string workItemType, string title, string? description = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new CreateWorkItemResult { WorkItemId = 2, Title = title, Type = workItemType, Url = string.Empty });
    }

    private sealed class ThrowingWorkItemContextService : IWorkItemContextService
    {
        private readonly Exception _exception;

        public ThrowingWorkItemContextService(Exception exception)
        {
            _exception = exception;
        }

        public Task<WorkItemContextResult> GetWorkItemContextAsync(string collection, string project, int workItemId, CancellationToken cancellationToken = default)
            => Task.FromException<WorkItemContextResult>(_exception);

        public Task<AddCommentResult> AddCommentAsync(string collection, string project, int workItemId, string comment, CancellationToken cancellationToken = default)
            => Task.FromException<AddCommentResult>(_exception);

        public Task<UpdateCommentResult> UpdateCommentAsync(string collection, string project, int workItemId, int commentId, string text, CancellationToken cancellationToken = default)
            => Task.FromException<UpdateCommentResult>(_exception);

        public Task<CreateWorkItemResult> CreateWorkItemAsync(string collection, string project, string workItemType, string title, string? description = null, CancellationToken cancellationToken = default)
            => Task.FromException<CreateWorkItemResult>(_exception);
    }

    [TestMethod]
    public async Task AddWorkItemComment_ReturnsSerializedCommentId()
    {
        var sut = new WorkItemTools(new FakeAddCommentWorkItemContextService());

        var json = await sut.AddWorkItemComment("DefaultCollection", "UZG.IZ.PrestIZ", 1, "Test comment via MCP");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual(42, root.GetProperty("commentId").GetInt32());
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task AddWorkItemComment_WhenServiceThrows_ReturnsSerializedError()
    {
        var sut = new WorkItemTools(new ThrowingWorkItemContextService(new InvalidOperationException("comment failed")));

        var json = await sut.AddWorkItemComment("DefaultCollection", "UZG.IZ.PrestIZ", 1, "Test comment");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual("comment failed", root.GetProperty("error").GetString());
        Assert.AreEqual("InvalidOperationException", root.GetProperty("type").GetString());
    }

    private sealed class FakeAddCommentWorkItemContextService : IWorkItemContextService
    {
        public Task<WorkItemContextResult> GetWorkItemContextAsync(string collection, string project, int workItemId, CancellationToken cancellationToken = default)
            => Task.FromResult(new WorkItemContextResult());

        public Task<AddCommentResult> AddCommentAsync(string collection, string project, int workItemId, string comment, CancellationToken cancellationToken = default)
            => Task.FromResult(new AddCommentResult { CommentId = 42, Url = "https://example.invalid/comment/42" });

        public Task<UpdateCommentResult> UpdateCommentAsync(string collection, string project, int workItemId, int commentId, string text, CancellationToken cancellationToken = default)
            => Task.FromResult(new UpdateCommentResult { CommentId = commentId, WorkItemId = workItemId, Text = text, Version = 2, Url = "https://example.invalid/comment/" + commentId });

        public Task<CreateWorkItemResult> CreateWorkItemAsync(string collection, string project, string workItemType, string title, string? description = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new CreateWorkItemResult { WorkItemId = 3, Title = title, Type = workItemType, Url = string.Empty });
    }

    [TestMethod]
    public async Task UpdateWorkItemComment_ReturnsSerializedCommentDetails()
    {
        var sut = new WorkItemTools(new FakeAddCommentWorkItemContextService());

        var json = await sut.UpdateWorkItemComment("DefaultCollection", "UZG.IZ.PrestIZ", 1, 100, "Updated comment text via MCP");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual(100, root.GetProperty("commentId").GetInt32());
        Assert.AreEqual(1, root.GetProperty("workItemId").GetInt32());
        Assert.AreEqual("Updated comment text via MCP", root.GetProperty("text").GetString());
        Assert.AreEqual(2, root.GetProperty("version").GetInt32());
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task UpdateWorkItemComment_WhenServiceThrows_ReturnsSerializedError()
    {
        var sut = new WorkItemTools(new ThrowingWorkItemContextService(new InvalidOperationException("update failed")));

        var json = await sut.UpdateWorkItemComment("DefaultCollection", "UZG.IZ.PrestIZ", 1, 100, "Updated text");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual("update failed", root.GetProperty("error").GetString());
        Assert.AreEqual("InvalidOperationException", root.GetProperty("type").GetString());
    }

    [TestMethod]
    public async Task CreateWorkItem_ReturnsSerializedWorkItemDetails()
    {
        var sut = new WorkItemTools(new FakeCreateWorkItemService());
        var json = await sut.CreateWorkItem("DefaultCollection", "UZG.IZ.PrestIZ", "Task", "New task via MCP");
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.AreEqual(99, root.GetProperty("workItemId").GetInt32());
        Assert.AreEqual("New task via MCP", root.GetProperty("title").GetString());
        Assert.AreEqual("Task", root.GetProperty("type").GetString());
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task CreateWorkItem_WithDescription_ReturnsSerializedWorkItem()
    {
        var sut = new WorkItemTools(new FakeCreateWorkItemService());
        var json = await sut.CreateWorkItem("DefaultCollection", "UZG.IZ.PrestIZ", "Bug", "Critical bug", "This is a critical issue that needs fixing");
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.AreEqual(99, root.GetProperty("workItemId").GetInt32());
        Assert.AreEqual("Critical bug", root.GetProperty("title").GetString());
        Assert.AreEqual("Bug", root.GetProperty("type").GetString());
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task CreateWorkItem_WhenServiceThrows_ReturnsSerializedError()
    {
        var sut = new WorkItemTools(new ThrowingWorkItemContextService(new InvalidOperationException("Invalid work item type")));
        var json = await sut.CreateWorkItem("DefaultCollection", "UZG.IZ.PrestIZ", "InvalidType", "Test");
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.AreEqual("Invalid work item type", root.GetProperty("error").GetString());
        Assert.AreEqual("InvalidOperationException", root.GetProperty("type").GetString());
    }

    private sealed class FakeCreateWorkItemService : IWorkItemContextService
    {
        public Task<WorkItemContextResult> GetWorkItemContextAsync(string collection, string project, int workItemId, CancellationToken cancellationToken = default)
            => Task.FromResult(new WorkItemContextResult());
        public Task<AddCommentResult> AddCommentAsync(string collection, string project, int workItemId, string comment, CancellationToken cancellationToken = default)
            => Task.FromResult(new AddCommentResult { CommentId = 42, Url = "https://example.invalid/comment/42" });
        public Task<UpdateCommentResult> UpdateCommentAsync(string collection, string project, int workItemId, int commentId, string text, CancellationToken cancellationToken = default)
            => Task.FromResult(new UpdateCommentResult { CommentId = commentId, WorkItemId = workItemId, Text = text, Version = 2, Url = "https://example.invalid/comment/" + commentId });
        public Task<CreateWorkItemResult> CreateWorkItemAsync(string collection, string project, string workItemType, string title, string? description = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new CreateWorkItemResult { WorkItemId = 99, Title = title, Type = workItemType, Url = "https://example.invalid/work-item/99" });
    }
}
