using System.Text.Json;
using G5e.AzureDevOpsServerMCP.Application.Services;
using G5e.AzureDevOpsServerMCP.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace G5e.AzureDevOpsServerMCP.IntegrationTests;

[TestClass]
public class RepositoryToolsFixtureTests
{
    [TestMethod]
    public async Task CreateFeatureBranch_UsesFixtureBackedService_AndSerializesExpectedShape()
    {
        // Arrange
        var service = new FakeRepositoryService();
        var sut = new RepositoryTools(service);

        // Act
        var json = await sut.CreateFeatureBranch("DefaultCollection", "TestProject", "TestRepo", "feature/TEST-123", "main");

        // Assert
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual("feature/TEST-123", root.GetProperty("branchName").GetString());
        Assert.IsFalse(string.IsNullOrEmpty(root.GetProperty("objectId").GetString()));
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task CreateFeatureBranch_WhenServiceThrows_ReturnsSerializedError()
    {
        // Arrange
        var service = new ThrowingRepositoryService(new InvalidOperationException("Branch already exists"));
        var sut = new RepositoryTools(service);

        // Act
        var json = await sut.CreateFeatureBranch("DefaultCollection", "TestProject", "TestRepo", "feature/TEST-123", "main");

        // Assert
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual("Branch already exists", root.GetProperty("error").GetString());
        Assert.AreEqual("InvalidOperationException", root.GetProperty("type").GetString());
    }

    [TestMethod]
    public async Task CreatePullRequestForWorkItem_ReturnsSerializedResult()
    {
        var sut = new RepositoryTools(new FakeRepositoryService());

        var json = await sut.CreatePullRequestForWorkItem(
            "create",
            "DefaultCollection",
            "TestProject",
            "TestRepo",
            "feature/TEST-123",
            "main",
            "Implement TEST-123",
            42,
            "## Summary\n- Implemented TEST-123");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual(99, root.GetProperty("pullRequestId").GetInt32());
        Assert.AreEqual("Implement TEST-123", root.GetProperty("title").GetString());
        Assert.AreEqual("active", root.GetProperty("status").GetString());
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task CreatePullRequestForWorkItem_WhenServiceThrows_ReturnsSerializedError()
    {
        var sut = new RepositoryTools(new ThrowingRepositoryService(new InvalidOperationException("source branch not found")));

        var json = await sut.CreatePullRequestForWorkItem(
            "create",
            "DefaultCollection",
            "TestProject",
            "TestRepo",
            "feature/TEST-123",
            "main",
            "Implement TEST-123",
            42,
            "## Summary\n- Failing case");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual("source branch not found", root.GetProperty("error").GetString());
        Assert.AreEqual("InvalidOperationException", root.GetProperty("type").GetString());
    }

    [TestMethod]
    public async Task LinkArtifactToWorkItem_Branch_ReturnsSerializedResult()
    {
        var sut = new RepositoryTools(new FakeRepositoryService());

        var json = await sut.LinkArtifactToWorkItem("add_artifact_link", "branch", "feature/TEST-123", "DefaultCollection", "TestProject", 42, "TestRepo");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual(42, root.GetProperty("workItemId").GetInt32());
        Assert.AreEqual("branch", root.GetProperty("linkType").GetString());
        Assert.AreEqual("feature/TEST-123", root.GetProperty("linkTarget").GetString());
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task LinkArtifactToWorkItem_WhenServiceThrows_ReturnsSerializedError()
    {
        var sut = new RepositoryTools(new ThrowingRepositoryService(new InvalidOperationException("repository not found")));

        var json = await sut.LinkArtifactToWorkItem("add_artifact_link", "branch", "feature/TEST-123", "DefaultCollection", "TestProject", 42, "TestRepo");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual("repository not found", root.GetProperty("error").GetString());
        Assert.AreEqual("InvalidOperationException", root.GetProperty("type").GetString());
    }

    [TestMethod]
    public async Task LinkArtifactToWorkItem_Commit_ReturnsSerializedResult()
    {
        var sut = new RepositoryTools(new FakeRepositoryService());

        var json = await sut.LinkArtifactToWorkItem("add_artifact_link", "commit", "abc123def456", "DefaultCollection", "TestProject", 42, "TestRepo");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual(42, root.GetProperty("workItemId").GetInt32());
        Assert.AreEqual("commit", root.GetProperty("linkType").GetString());
        Assert.AreEqual("abc123def456", root.GetProperty("linkTarget").GetString());
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    [TestMethod]
    public async Task LinkArtifactToWorkItem_Hyperlink_ReturnsSerializedResult()
    {
        var sut = new RepositoryTools(new FakeRepositoryService());

        var json = await sut.LinkArtifactToWorkItem("add_artifact_link", "hyperlink", "https://example.invalid/docs/spec", "DefaultCollection", "TestProject", 42);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual(42, root.GetProperty("workItemId").GetInt32());
        Assert.AreEqual("hyperlink", root.GetProperty("linkType").GetString());
        Assert.AreEqual("https://example.invalid/docs/spec", root.GetProperty("linkTarget").GetString());
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
    }

    private sealed class FakeRepositoryService : IRepositoryService
    {
        public Task<CreateBranchResult> CreateBranchAsync(
            string collection,
            string project,
            string repository,
            string branchName,
            string fromBranch,
            CancellationToken cancellationToken = default)
        {
            var result = new CreateBranchResult
            {
                BranchName = branchName,
                ObjectId = "abc123def456abc123def456abc123def456abc1",
                Url = $"https://example.invalid/{project}/_git/{repository}/refs/heads/{branchName}"
            };

            return Task.FromResult(result);
        }

        public Task<LinkBranchResult> LinkBranchToWorkItemAsync(
            string collection,
            string project,
            string repository,
            string branchName,
            int workItemId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new LinkBranchResult
            {
                WorkItemId = workItemId,
                BranchName = branchName,
                Repository = repository
            });

        public Task<LinkArtifactResult> LinkArtifactToWorkItemAsync(
            string collection,
            string project,
            string? repository,
            ArtifactLinkType type,
            string linkTarget,
            int workItemId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new LinkArtifactResult
            {
                WorkItemId = workItemId,
                LinkType = type.ToString().ToLowerInvariant(),
                LinkTarget = linkTarget
            });

        public Task<CreatePullRequestResult> CreatePullRequestAsync(
            string collection,
            string project,
            string repository,
            string sourceBranch,
            string targetBranch,
            string title,
            string? description,
            int workItemId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new CreatePullRequestResult
            {
                PullRequestId = 99,
                Title = title,
                Url = $"https://example.invalid/{project}/_git/{repository}/pullrequest/99",
                Status = "active"
            });
    }

    private sealed class ThrowingRepositoryService : IRepositoryService
    {
        private readonly Exception _exception;

        public ThrowingRepositoryService(Exception exception)
        {
            _exception = exception;
        }

        public Task<CreateBranchResult> CreateBranchAsync(
            string collection,
            string project,
            string repository,
            string branchName,
            string fromBranch,
            CancellationToken cancellationToken = default)
            => Task.FromException<CreateBranchResult>(_exception);

        public Task<LinkBranchResult> LinkBranchToWorkItemAsync(
            string collection,
            string project,
            string repository,
            string branchName,
            int workItemId,
            CancellationToken cancellationToken = default)
            => Task.FromException<LinkBranchResult>(_exception);

        public Task<LinkArtifactResult> LinkArtifactToWorkItemAsync(
            string collection,
            string project,
            string? repository,
            ArtifactLinkType type,
            string linkTarget,
            int workItemId,
            CancellationToken cancellationToken = default)
            => Task.FromException<LinkArtifactResult>(_exception);

        public Task<CreatePullRequestResult> CreatePullRequestAsync(
            string collection,
            string project,
            string repository,
            string sourceBranch,
            string targetBranch,
            string title,
            string? description,
            int workItemId,
            CancellationToken cancellationToken = default)
            => Task.FromException<CreatePullRequestResult>(_exception);
    }
}
