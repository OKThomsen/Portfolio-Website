# Portfolio Website — Code Documentation

Technical reference for the current state of the codebase. For product/content
direction see `CLAUDE.md`; this file covers how the code is put together.

## Stack

- **ASP.NET Core 10** (Razor Pages, server-rendered — no Blazor, no SPA framework)
- **Entity Framework Core 10** (`Microsoft.EntityFrameworkCore.SqlServer`)
- **SQL Server 2022**, run locally via Docker
- **Docker Compose** for local infrastructure only (the app itself runs via `dotnet run`, not containerized yet)

Chosen deliberately over a JS framework so the site itself functions as a
Microsoft-stack showcase project — see `CLAUDE.md` → ".NET / Microsoft
Positioning".

## Project layout

```
src/Portfolio.Web/
  Program.cs                 Composition root: DI, migration + seed on startup, middleware pipeline
  Data/
    PortfolioDbContext.cs    EF Core DbContext + entity configuration
    SeedData.cs              Idempotent seed of real project/skill content
  Models/
    Project.cs                Featured project entity
    ProjectTechnology.cs      Technology tag belonging to a Project
    SkillCategory.cs          Grouping for skills (e.g. "Languages")
    Skill.cs                  Individual skill belonging to a SkillCategory
  Migrations/                 EF Core migrations (source of truth for schema)
  Pages/
    Index.cshtml(.cs)         Single-page portfolio (hero/about/skills/projects/etc.)
    Error.cshtml(.cs)         Default error page
    Shared/_Layout.cshtml     Shared HTML shell
  appsettings.json             Base config (no secrets)
  appsettings.Development.json Dev-only config, including the local DB connection string
docker-compose.yml             Local SQL Server 2022 container
.env / .env.example            SA password for the compose SQL Server container
```

## Data model

Two independent aggregates, both read-only from the app's perspective (all
writes currently happen through `SeedData`):

- **Project** (1) → **ProjectTechnology** (many), cascade delete. `Slug` is
  unique — intended for the future `/projects/<slug>` detail route. `Featured`
  and `SortOrder` control homepage inclusion/ordering. `HasDetailPage` flags
  projects that should eventually get a dedicated page (not yet implemented).
- **SkillCategory** (1) → **Skill** (many), cascade delete. `SortOrder` on
  both levels controls display order.

No relationship exists between projects and skills — they're independent
content trees, matching how they're presented on the page (separate
sections).

## Request flow

`Program.cs` registers `PortfolioDbContext` with SQL Server, and on every
startup: runs `db.Database.MigrateAsync()` then `SeedData.SeedAsync(db)`.
`SeedData` only inserts if the respective table is empty, so it's safe to run
repeatedly — it will not duplicate or overwrite content.

The only page with real logic is `Index`: `IndexModel.OnGetAsync()` loads all
skill categories (with skills, ordered) and all `Featured` projects (with
technologies, ordered) via two independent EF Core queries, and the Razor
page renders them directly. There is no caching, view models, or mapping
layer — the entity types are used directly by the view.

## Configuration & secrets

- `ConnectionStrings:PortfolioDb` lives in `appsettings.Development.json`
  (gitignored is *not* currently applied to this file — see note below) and
  points at `localhost,14330`, matching the port mapping in
  `docker-compose.yml`.
- `docker-compose.yml` takes the SQL `sa` password from `.env`
  (`MSSQL_SA_PASSWORD`), which is gitignored; `.env.example` documents the
  expected shape with a placeholder value.
- Both the compose password and the connection string currently use the same
  hardcoded dev-only password (`DevOnly_ChangeMe!123`). Fine for local dev;
  must not be reused anywhere real credentials matter.

## Running locally

1. `docker compose up -d` — starts SQL Server 2022 on `localhost:14330`.
2. `dotnet run --launch-profile http` from `src/Portfolio.Web` — applies
   migrations, seeds content, serves at `http://localhost:5101`
   (`https://localhost:7233` under the `https` profile).

No manual `dotnet ef database update` step is needed — migrations run
automatically on startup (see `Program.cs`).

## Current gaps / known TODOs (from the code itself)

- `Project.RepositoryUrl` is `null` for the thesis project (repo currently
  private) and for this portfolio repo itself (not yet published) —
  see `SeedData.cs` comments.
- `Project.Contribution` has two `TODO` placeholders (thesis project, TV2
  project) pending Oskar confirming individual-contribution wording — see
  `[[repo_inventory]]`-equivalent context in `CLAUDE.md`.
- `HasDetailPage` is set on three projects but no `/projects/<slug>` route or
  page exists yet — homepage links to these would currently 404.
- No automated tests exist yet.
