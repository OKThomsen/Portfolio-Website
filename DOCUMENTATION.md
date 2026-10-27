# Portfolio Website — Code Documentation

Technical reference for the current state of the codebase. For product/content
direction see `CLAUDE.md`; this file covers how the code is put together.

## Stack

- **ASP.NET Core 10** (Razor Pages, server-rendered — no Blazor, no SPA framework)
- **Entity Framework Core 10** (`Microsoft.EntityFrameworkCore.SqlServer`)
- **SQL Server 2022** (Express edition), run in Docker
- **Docker Compose** runs the app and SQL Server together; `dotnet run` still works for development
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
    SeedData.cs              All site content; synced into the database on startup
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
Dockerfile / .dockerignore     App image (multi-stage: SDK build, ASP.NET runtime)
docker-compose.yml             The app and SQL Server 2022 containers
.env / .env.example            SA password and the app's published port
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
quick links (Skills and Projects anchors on the home page; the pages are
already in the header), email/phone, and the same social links.

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

`SeedData.cs` is the **source of truth** for all content, and seeding is safe
to run repeatedly:

- Every item is matched **by key**: a skill category by name, a skill by name
  within its category, a project or blog post by slug, and a project
  technology or post tag by name within its parent.
- On every startup the database is made to match the file (`Sync`): changed
  items are updated, new ones are added, and items removed from the file are
  deleted. EF Core only writes the values that actually changed, so a startup
  with no content changes writes nothing.
- So editing a post, a project or a sort order in `SeedData.cs` and restarting
  (or redeploying) is all it takes. Never edit content directly in the
  database: the next startup puts it back.
- Renaming a key (a slug or a skill name) counts as deleting the old item and
  adding a new one.
- Blog posts are listed in `BlogPosts()` with the slug of their related
  project; projects are synced first, so a post can point at a project added in
  the same run.

The page models query EF Core directly and pass the entity types to the
view. There is no caching, view-model or mapping layer:

- `IndexModel.OnGetAsync()` loads all skill categories (with skills, ordered),
  all `Featured` projects (with technologies, ordered), and the slug of the
  newest blog post per project, in three independent queries. A project card
  shows a "Blog post" link next to "Source code" when it has a post.
- `Blog/IndexModel` loads all posts with tags, ordered by `PublishedOn`
  descending.
- `Blog/PostModel` loads one post by slug with tags and related project.

`About` and `Contact` are static Razor Pages with empty page models. Their
content (about copy, education cards, contact cards) is hardcoded in the
`.cshtml` files, not taken from the database.

## Presentation notes

- **Layout rule — one shared left edge:** the header nav, every section and
  the footer use the same container (`--container`, 72rem, plus `--gutter`
  on each side), so all content starts on one vertical line at every
  viewport width. There are only two widths inside it: full container width
  for grids and cards, and `--reading-width` (42rem) for long-form text —
  which stays anchored to the left edge, never centered. `html` sets
  `scrollbar-gutter: stable` so pages without a scrollbar don't shift the
  edge sideways. When adding a section, use `.section-inner` and don't give
  it its own max-width.
- **Section headers:** every section (except the hero) opens with
  `<header class="section-header">` containing the `h1`/`h2` title (all
  titles share `--font-size-title`) and an optional `.lede` intro. Only the
  hero has an `.eyebrow` label above its title. Headings are left-aligned;
  nothing is text-centered.
- **Card grids:** Featured Projects and Education use a 2-column grid of
  bordered surface cards (1 column below 52rem). Contact uses the same card
  style in an auto-fit row.
- **Full-height pages:** Home and About use `body.full-sections` (each
  section is one screen tall, content vertically centered). Contact uses
  `body.fill-viewport` instead: the single section fills the space between
  header and footer, so the footer sits at the bottom of the window.
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

## Content notes

- **Hero and footer copy name no technologies.** The hero positions Oskar as a
  software engineer who builds complete systems, and leads with the pool
  tournament platform (in production), then the master's thesis for the Danish
  EPA case. The TV 2 bachelor project is deliberately left out of the hero.
  Specific technologies are left to the Technical Skills section and the
  project cards, so they aren't repeated. Every claim in the hero should map
  to a seeded project.
- **Target stack vs. experience.** ASP.NET Core, EF Core and Azure are the
  stack Oskar is targeting, and this site is his first project in it. Copy
  should not present them as established expertise.

## Configuration & secrets

- For `dotnet run`, `ConnectionStrings:PortfolioDb` lives in
  `appsettings.Development.json`, which is **tracked in git** (not
  gitignored). It points at `localhost,14330`, matching the port mapping in
  `docker-compose.yml`.
- `docker-compose.yml` takes the SQL `sa` password from `.env`
  (`MSSQL_SA_PASSWORD`), which is gitignored. `.env.example` documents the
  expected shape with a placeholder value. The `app` container gets its
  connection string from compose as the `ConnectionStrings__PortfolioDb`
  environment variable (pointing at `sqlserver,1433` on the Docker network),
  and runs in the Production environment, so it never reads
  `appsettings.Development.json`.
- The app container sets `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, so
  behind a reverse proxy it trusts `X-Forwarded-For`/`X-Forwarded-Proto` and
  knows a request came in over HTTPS.
- Both the compose password and the connection string use the same
  hardcoded dev-only password (`DevOnly_ChangeMe!123`). This is fine for local
  dev but must not be reused anywhere real credentials matter. A production
  connection string should come from environment variables or a secret store,
  not from a committed file.
- `.gitignore` ignores all `*.md` files except `README.md` and
  `DOCUMENTATION.md`, so internal working notes (including `CLAUDE.md`) stay
  out of the repo.
- The root-level `/CV.pdf` (the personal source copy) is gitignored. The
  served copy at `wwwroot/files/CV.pdf` is tracked.

## Running locally

Docker Desktop must be running first. Copy `.env.example` to `.env` once.

**Everything in Docker:** `docker compose up -d --build` builds the app image
(`Dockerfile`: SDK image to publish, ASP.NET runtime image to run, as a non-root
user) and starts it with SQL Server. The site is at `http://localhost:8090`
(`APP_PORT` in `.env`). The app waits for SQL Server's health check before
starting. Run the same command again after changing code or content.
`docker compose logs -f app` follows the app's log.

**App via `dotnet run`** (faster while developing):

1. `docker compose up -d sqlserver` — starts only SQL Server on
   `localhost:14330`.
2. `dotnet run --launch-profile http` from `src/Portfolio.Web` — applies
   migrations, seeds content, serves at `http://localhost:5101`
   (`https://localhost:7233` under the `https` profile).

Both use the same database. No manual `dotnet ef database update` step is
needed — migrations run automatically on startup (see `Program.cs`).

SQL Server runs as the **Express** edition (`MSSQL_PID`), which is free for
production use; the image's default Developer edition isn't. Its memory is
capped at 2 GB (`MSSQL_MEMORY_LIMIT_MB`).

To add a migration after changing a model:
`dotnet ef migrations add <Name>` from `src/Portfolio.Web` (requires the
`dotnet-ef` global tool).

## Production (oskar.kjaer-thomsen.dk)

The site runs on the same Linux VM as the pool tournament platform
(`turneringsportal.kjaer-thomsen.dk`), from a clone of this repo's
`production` branch:

```
~/Projects/
├── caddy/                 the shared reverse proxy (not in any repo, see below)
├── pool-tournament-app/   the pool app, its `production` branch
└── Portfolio-Website/     this repo, `production` branch
```

**This site's production override** (`docker-compose.prod.yml`, on top of
`docker-compose.yml`): no ports are published on the host, the app joins the
shared `web` network as `portfolio-app`, and `MSSQL_SA_PASSWORD` must be set.
The server's `.env`:

```sh
COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml
MSSQL_SA_PASSWORD=<random>
```

It was created with `MSSQL_SA_PASSWORD=$(openssl rand -hex 16)Aa1!`. SQL
Server needs upper case, lower case, digits and a symbol, and `rand -hex` gives
only lower case and digits, hence the suffix. Without a `.env`, SQL Server gets
a blank password and keeps restarting. SQL Server only reads the password when
it creates its data, so to change it, recreate the volume
(`docker compose down -v`, then `up`), which is safe (see Backups).

**Backups:** none needed. All content comes from `SeedData.cs` and is synced
into the database on every startup, so an empty database is rebuilt from the
repo at the next start.

**Updating this site:** `git pull && docker compose up -d --build` in
`~/Projects/Portfolio-Website`. Migrations and the content sync run on
startup.

**Branches:** `main` is where work happens; `production` is what the server
runs. Merge `main` into `production` to deploy.

### The shared Caddy

Only one program can listen on ports 80 and 443, so one Caddy sits in front of
both sites. It lives on the server in `~/Projects/caddy`, outside both repos,
as its own Compose project (`name: caddy`), so rebuilding either app never
touches it.

- It gets and renews each site's HTTPS certificate from Let's Encrypt by
  itself, and redirects http to https.
- Caddy and the apps meet on an external Docker network called `web`, created
  once with `docker network create web`. Caddy won't start until it exists.
- Both apps' Compose services are called `app`, so on `web` each has an alias
  of its own: `portfolio-app` (this repo's `docker-compose.prod.yml`) and
  `pool-app` (the pool repo's `docker-compose.prod.yml`).

After editing its `Caddyfile`, in `~/Projects/caddy`:
`docker compose exec caddy caddy reload --config /etc/caddy/Caddyfile`.
Logs: `docker compose logs -f caddy`.

Its two files, to recreate it on a new server:

`~/Projects/caddy/docker-compose.yml`:

```yaml
name: caddy

services:
  caddy:
    image: caddy:2.10-alpine
    restart: unless-stopped
    ports:
      - '80:80'
      - '443:443'
      - '443:443/udp'
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile:ro
      - caddy-data:/data
      - caddy-config:/config
    networks:
      - web

networks:
  web:
    external: true

volumes:
  caddy-data:
  caddy-config:
```

`~/Projects/caddy/Caddyfile`:

```
turneringsportal.kjaer-thomsen.dk {
	reverse_proxy pool-app:3000
}

oskar.kjaer-thomsen.dk {
	reverse_proxy portfolio-app:8080
}
```

**Setting up a new server from scratch:**

1. DNS: A records for both domains pointing at the server's public IP.
2. `docker network create web`
3. Start both apps (`docker compose up -d --build` in each repo, with its
   `.env`).
4. In `~/Projects/caddy`: `docker compose up -d`, then
   `docker compose logs -f caddy` until both certificates are obtained.

## Current gaps / known TODOs

- `Project.Contribution` has a `TODO` placeholder on the TV2 project, pending
  Oskar's wording for his individual contribution.
- The thesis post's `PublishedOn` (2025-06-15) is a placeholder for the real
  submission/defense date.
- The thesis repository lives under a teammate's GitHub account
  (`anton4d`), and the TV2 repository is a fork of a teammate's original.
- `HasDetailPage` is set on some projects, but no `/projects/<slug>` route or
  page exists yet.
- `wwwroot/files/CV.pdf` is served, but no page links to it yet (`CLAUDE.md`
  wants a CV link in the hero).
- No automated tests exist yet.
- No CI/CD pipeline: deploying is a manual `git pull` and rebuild on the
  server (see "Production").
