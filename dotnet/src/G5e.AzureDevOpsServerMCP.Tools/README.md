# G5e.AzureDevOpsServerMCP.Tools

MCP tool implementations for the **G5e Azure DevOps Server MCP** packages.

This package contains the `[McpServerTool]`-annotated tool classes that expose Azure DevOps operations (work items, repositories, pipelines, etc.) as MCP tools via the `ModelContextProtocol` runtime.

## Part of the G5e.AzureDevOpsServerMCP family

| Package | Description |
|---|---|
| `G5e.AzureDevOpsServerMCP.Domain` | Core domain abstractions |
| `G5e.AzureDevOpsServerMCP.Application` | Application contracts and result models |
| `G5e.AzureDevOpsServerMCP.Infrastructure.Configuration` | Configuration types |
| `G5e.AzureDevOpsServerMCP.Infrastructure.AzureDevOps` | Azure DevOps client implementations |
| `G5e.AzureDevOpsServerMCP.Tools` | MCP tool implementations |
| `G5e.AzureDevOpsServerMCP.AspNetCore` | ASP.NET Core integration package |

## Background

This project is part of a fork of [microsoft/azure-devops-mcp](https://github.com/microsoft/azure-devops-mcp), focused on making Azure DevOps MCP Server functionality available as reusable .NET/NuGet packages, with support for Azure DevOps Server (on-prem) scenarios.

For more information, see the [fork notes](https://github.com/gadeynebram/AzureDevOpsServerMCP/blob/main/README_FORK.md).
