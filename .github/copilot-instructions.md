# Project Guidelines

## Code Style
- Follow formatting and language rules from [.editorconfig](../.editorconfig) and compiler/analyzer settings in [Directory.Build.props](../Directory.Build.props).
- Keep changes minimal and localized. Preserve public API shape unless the task explicitly requires API changes.
- Prefer existing project patterns in [src/](../src/) and mirrored tests in [test/](../test/).

## Architecture
- This repository is the source of truth for runtime helpers that enable incremental migration from ASP.NET Framework to ASP.NET Core.
- Main boundaries:
  - Adapter surface and runtime behavior: [src/Microsoft.AspNetCore.SystemWebAdapters/](../src/Microsoft.AspNetCore.SystemWebAdapters/)
  - Shared contracts: [src/Microsoft.AspNetCore.SystemWebAdapters.Abstractions/](../src/Microsoft.AspNetCore.SystemWebAdapters.Abstractions/)
  - ASP.NET Core-side migration services: [src/Microsoft.AspNetCore.SystemWebAdapters.CoreServices/](../src/Microsoft.AspNetCore.SystemWebAdapters.CoreServices/)
  - ASP.NET Framework-side migration services: [src/Microsoft.AspNetCore.SystemWebAdapters.FrameworkServices/](../src/Microsoft.AspNetCore.SystemWebAdapters.FrameworkServices/)
  - OWIN integration: [src/Microsoft.AspNetCore.SystemWebAdapters.Owin/](../src/Microsoft.AspNetCore.SystemWebAdapters.Owin/)
  - Analyzer projects: [src/Microsoft.AspNetCore.SystemWebAdapters.Analyzers/](../src/Microsoft.AspNetCore.SystemWebAdapters.Analyzers/) and [src/Microsoft.AspNetCore.SystemWebAdapters.Analyzers.CSharp/](../src/Microsoft.AspNetCore.SystemWebAdapters.Analyzers.CSharp/)
- Tests are organized by component under [test/](../test/). Add or update tests in the nearest matching test project.

## Build and Test
- Before doing any other work in this repository, run `./restore.cmd` and then `. ./activate.ps1` from the repo root.
- Use repository scripts first (they match CI behavior):
  - Windows full restore/build/pack/test: [build.cmd](../build.cmd)
  - Cross-platform full restore/build/pack/test: [build.sh](../build.sh)
  - Restore only (Windows): [restore.cmd](../restore.cmd)
- CI invokes Arcade build orchestration via `eng/common/cibuild.cmd` from pipeline YAML files. Do not create alternate build flows unless requested.
- Use the SDK/runtimes pinned in [global.json](../global.json).
- For targeted validation, run `dotnet test` on the closest project under [test/](../test/) after making changes.

## Conventions
- Prioritize migration safety and behavioral parity with `System.Web` semantics over framework rewrites.
- Do not introduce alternate web stacks or replacement migration libraries. Implement using ASP.NET Core / ASP.NET Framework primitives and existing adapters in this repo.
- When implementing missing behavior, extend existing adapter/runtime services instead of adding new third-party dependency chains.
- Preserve cross-target compatibility assumptions documented in [README.md](../README.md).

## Aspire Samples and E2E
- This repo uses .NET Aspire to orchestrate incremental migration scenarios, including ASP.NET Framework apps launched via IIS Express.
- IIS Express scenarios are Windows-only. Treat failures on non-Windows hosts as environment constraints, not product regressions.
- For sample/AppHost work, follow existing patterns in [src/Aspire.Hosting.IncrementalMigration/](../src/Aspire.Hosting.IncrementalMigration/) and sample hosts like [samples/SessionRemote/SessionRemoteAppHost/](../samples/SessionRemote/SessionRemoteAppHost/).
- For E2E changes, follow the fixture/lifecycle pattern in [test/Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests/](../test/Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests/) and keep tests non-concurrent where the shared distributed app host requires it.
- Prefer AppHost-defined topology and environment wiring over ad-hoc process orchestration. Keep endpoint/API key/session/auth wiring aligned with existing incremental migration extensions.
- When an AI workflow needs Aspire dashboard orchestration/telemetry context, use Aspire agent/MCP guidance rather than inventing custom scripts.

### Working on samples or E2E tests
1. Ensure you are on Windows (IIS Express is Windows-only).
2. Run `./restore.cmd` then `. ./activate.ps1` from the repo root if not done yet.
3. Build the solution: `./build.cmd` (or `dotnet build` on the relevant AppHost/test project).
4. To run a sample locally, launch its AppHost — e.g. `dotnet run --project samples/SessionRemote/SessionRemoteAppHost`.
5. To run E2E tests: `dotnet test test/Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests`.
6. Check Aspire dashboard output for resource health and logs; use `aspire agent init` to wire the MCP server to your AI assistant for live telemetry queries.

## Pitfalls and Gotchas
- Files in [eng/common/](../eng/common/) are managed by Arcade automation; do not edit them directly.
- Build outputs and generated files under [artifacts/](../artifacts/) are not source-of-truth.
- For nuanced behavior, consult design docs before changing pipeline/session behavior:
  - [designs/http-modules.md](../designs/http-modules.md)
  - [designs/remote-session.md](../designs/remote-session.md)

## Documentation Links
- Project overview and supported targets: [README.md](../README.md)
- Contribution and security process: [CONTRIBUTING.md](../CONTRIBUTING.md), [SECURITY.md](../SECURITY.md)
- Migration samples: [samples/README.md](../samples/README.md)
- Aspire Incremental Migration host package: [src/Aspire.Hosting.IncrementalMigration/README.md](../src/Aspire.Hosting.IncrementalMigration/README.md)
- Aspire MCP server setup: [aspire.dev/dashboard/mcp-server](https://aspire.dev/dashboard/mcp-server/)
- Aspire agentic workflow overview: [devblogs.microsoft.com/aspire/agentic-dev-aspirations](https://devblogs.microsoft.com/aspire/agentic-dev-aspirations/)
