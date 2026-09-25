# Portfolio Website — Code Documentation

Technical reference for the current state of the codebase. For product/content
direction see `CLAUDE.md`; this file covers how the code is put together.

## Stack

- **ASP.NET Core 10** (Razor Pages, server-rendered — no Blazor, no SPA framework)
- **Entity Framework Core 10** (`Microsoft.EntityFrameworkCore.SqlServer`)
- **SQL Server 2022**, run locally via Docker
- **Docker Compose** for local infrastructure only (the app itself runs via `dotnet run`, not containerized yet)
- Plain CSS and a small vanilla JS file — no frontend build step or npm dependencies

Chosen deliberately over a JS framework so the site itself functions as a
Microsoft-stack showcase project — see `CLAUDE.md` → ".NET / Microsoft
Positioning".

## Project layout

```
src/Portfolio.Web/
  Program.cs                 Composition root: DI, migration + seed on startup, middleware pipeline
  Data/
    PortfolioDbContext.cs    EF Core DbContext + entity configuration
    SeedData.cs              Idempotent seed of real project/skill/blog content
  Models/
    Project.cs                Featured project entity
    ProjectTechnology.cs      Technology tag belonging to a Project
    SkillCategory.cs          Grouping for skills (e.g. "Languages")
    Skill.cs                  Individual skill belonging to a SkillCategory
    BlogPost.cs               Blog article, optionally linked to a Project
    BlogPostTag.cs            Tag belonging to a BlogPost
  Migrations/                 EF Core migrations (source of truth for schema)
    ..._InitialCreate          Projects, technologies, skill categories, skills
    ..._AddBlogPosts           Blog posts + tags
  Pages/
    Index.cshtml(.cs)         Home page: hero + skills + featured projects
    About.cshtml(.cs)         About copy + education (static, no page model logic)
    Contact.cshtml(.cs)       Contact details (static, no page model logic)
    Blog/Index.cshtml(.cs)    /blog — list of posts, newest first
    Blog/Post.cshtml(.cs)     /blog/{slug} — single post
    Error.cshtml(.cs)         Default error page
    Shared/_Layout.cshtml     Shared HTML shell: animated SVG background, header nav, footer
  wwwroot/
    css/site.css              All styling
    js/site.js                Background/section tracking + footer height (see below)
    img/tech/<slug>.svg       Vendored skill icons
    files/CV.pdf              Served copy of the CV
  appsettings.json             Base config (no secrets)
  appsettings.Development.json Dev-only config, including the local DB connection string
docker-compose.yml             Local SQL Server 2022 container
.env / .env.example            SA password for the compose SQL Server container
```

## Routes

| Route          | Page              | Data source                         |
|----------------|-------------------|-------------------------------------|
| `/`            | `Index`           | DB: skill categories, featured projects |
| `/About`       | `About`           | Hardcoded in `.cshtml`              |
| `/Contact`     | `Contact`         | Hardcoded in `.cshtml`              |
| `/blog`        | `Blog/Index`      | DB: all blog posts                  |
| `/blog/{slug}` | `Blog/Post`       | DB: one post + tags + related project |

`Blog/Post.cshtml` declares `@page "/blog/{slug}"` to override the default
file-based route (`/Blog/Post/{slug}`). An unknown slug returns 404.

Header nav: About, Blog, Contact, plus GitHub/LinkedIn icon links. Footer:
quick links (Skills and Projects anchors on the home page, About, Blog,
Contact), email/phone, and the same social links.

## Data model

Three aggregates, all read-only from the app's perspective (all writes
currently happen through `SeedData`):

- **Project** (1) → **ProjectTechnology** (many), cascade delete. `Slug` is
  unique — intended for the future `/projects/<slug>` detail route. `Featured`
  and `SortOrder` control homepage inclusion/ordering. `HasDetailPage` flags
  projects that should eventually get a dedicated page (not yet implemented,
  and not yet read by any view).
- **SkillCategory** (1) → **Skill** (many), cascade delete. `SortOrder` on
  both levels controls display order.
- **BlogPost** (1) → **BlogPostTag** (many), cascade delete. `Slug` is unique.
  `Body` is stored as HTML. An optional `RelatedProjectId` FK points to a
  `Project` (`SetNull` on delete), which renders as a "Related project" box on
  the post page.

Blog posts are their own entity rather than an extension of `Project`. By
convention a post's tags reuse its related project's `Technologies` list
instead of inventing separate blog tags.

## Request flow

`Program.cs` registers `PortfolioDbContext` with SQL Server, and on every
startup it runs `db.Database.MigrateAsync()` then `SeedData.SeedAsync(db)`.

Seeding is safe to run repeatedly and never overwrites existing rows:

- Skill categories and projects are only inserted if their table is empty.
  Editing them in `SeedData.cs` will **not** update an existing database.
- Blog posts are seeded **per slug**: each post is inserted only if its slug
  isn't already in the database. A new post added to `SeedData.cs` therefore
  reaches databases that already hold the earlier posts. `FindProjectAsync`
  resolves `RelatedProject` from the change tracker first, because on a fresh
  database the project may have been added in the same run but not saved yet.

The page models query EF Core directly and pass the entity types to the
view. There is no caching, view-model or mapping layer:

- `IndexModel.OnGetAsync()` loads all skill categories (with skills, ordered)
  and all `Featured` projects (with technologies, ordered) in two independent
  queries.
- `Blog/IndexModel` loads all posts with tags, ordered by `PublishedOn`
  descending.
- `Blog/PostModel` loads one post by slug with tags and related project.

`About` and `Contact` are static Razor Pages with empty page models. Their
content (about copy, education entries, contact links) is hardcoded in the
`.cshtml` files, not taken from the database.

## Presentation notes

- **Skill icons:** each skill renders as an icon tile (brand SVG + label, with
  a hover lift). Icons are vendored SVGs in `wwwroot/img/tech/<slug>.svg`. The
  skill-name → slug mapping lives in `IndexModel.IconBySkill`, since it's a
  presentation concern and stays out of the DB. Skills with no mapping fall
  back to a neutral inline glyph.
- **Animated background:** `_Layout.cshtml` contains an SVG wave pattern.
  `site.js` uses an `IntersectionObserver` to set `body[data-section]` to
  whichever `main section[id]` is crossing the middle of the viewport. The CSS
  keys the wave's drift, fade and tint off that attribute. Transitions are
  enabled only after first paint (`bg-ready` class) to avoid a flash on load.
- **Footer height:** `site.js` publishes the footer's rendered height as the
  `--footer-height` CSS variable, so the last full-height section can size
  itself to the space above the footer.
- **Blog bodies:** rendered with `@Html.Raw(Model.Post.Body)`. This is safe
  only because bodies come from `SeedData.cs`. If posts ever become editable
  through the UI or come from another source, they must be sanitized first.
  `.blog-post__body` styles `h2` and `ul` for longer articles.

## Configuration & secrets

- `ConnectionStrings:PortfolioDb` lives in `appsettings.Development.json`,
  which is **tracked in git** (not gitignored). It points at
  `localhost,14330`, matching the port mapping in `docker-compose.yml`.
- `docker-compose.yml` takes the SQL `sa` password from `.env`
  (`MSSQL_SA_PASSWORD`), which is gitignored. `.env.example` documents the
  expected shape with a placeholder value.
- Both the compose password and the connection string use the same
  hardcoded dev-only password (`DevOnly_ChangeMe!123`). This is fine for local
  dev but must not be reused anywhere real credentials matter. A production
  connection string should come from environment variables or a secret store,
  not from a committed file.
- `.gitignore` ignores all `*.md` files except `README.md` and
  `DOCUMENTATION.md`, so internal working notes stay out of the repo.

## Running locally

1. `docker compose up -d` — starts SQL Server 2022 on `localhost:14330`.
   (Docker Desktop must be running first.)
2. `dotnet run --launch-profile http` from `src/Portfolio.Web` — applies
   migrations, seeds content, serves at `http://localhost:5101`
   (`https://localhost:7233` under the `https` profile).

No manual `dotnet ef database update` step is needed — migrations run
automatically on startup (see `Program.cs`).

To add a migration after changing a model:
`dotnet ef migrations add <Name>` from `src/Portfolio.Web` (requires the
`dotnet-ef` global tool).

## Current gaps / known TODOs

- `Project.Contribution` has two `TODO` placeholders (thesis project, TV2
  project), pending Oskar's wording for his individual contribution.
- Both blog posts are **drafts**. The bodies were built only from verified
  project data and repo facts, and still need Oskar's own write-up. The thesis
  post's `PublishedOn` (2025-06-15) is a placeholder for the real
  submission/defense date.
- The thesis repository lives under a teammate's GitHub account
  (`anton4d`), and the TV2 repository is a fork of a teammate's original.
- `HasDetailPage` is set on some projects, but no `/projects/<slug>` route or
  page exists yet.
- `wwwroot/files/CV.pdf` is served, but no page links to it yet (`CLAUDE.md`
  wants a CV link in the hero).
- Content edits to existing skills/projects in `SeedData.cs` don't reach an
  already-seeded database (see "Request flow").
- No automated tests exist yet.
- Not yet deployed. No Dockerfile for the app, no CI/CD pipeline and no
  hosting setup.
