using Microsoft.EntityFrameworkCore;
using Portfolio.Web.Models;

namespace Portfolio.Web.Data;

// Content here is Oskar's real, verified work only — see CLAUDE.md: never fabricate
// projects, skills, or experience. Update this file as new projects/skills are confirmed.
public static class SeedData
{
    public static async Task SeedAsync(PortfolioDbContext db)
    {
        // Everything is seeded per key (skill name, project slug, post slug) rather than "only
        // if the table is empty", so items added here later still reach databases that were
        // seeded earlier. Existing rows are never changed, so a new skill should go at the end
        // of its category to keep the same order in old and new databases.
        var existingCategories = await db.SkillCategories.Include(c => c.Skills).ToListAsync();

        foreach (var category in SkillCategories())
        {
            var existing = existingCategories.FirstOrDefault(c => c.Name == category.Name);
            if (existing is null)
            {
                db.SkillCategories.Add(category);
                continue;
            }

            existing.Skills.AddRange(category.Skills.Where(s => existing.Skills.All(e => e.Name != s.Name)));
        }

        var existingProjectSlugs = await db.Projects.Select(p => p.Slug).ToListAsync();
        db.Projects.AddRange(Projects().Where(p => !existingProjectSlugs.Contains(p.Slug)));

        var existingPostSlugs = await db.BlogPosts.Select(p => p.Slug).ToListAsync();

        if (!existingPostSlugs.Contains("masters-thesis-satellite-pesticide-detection"))
        {
            db.BlogPosts.Add(new BlogPost
            {
                Slug = "masters-thesis-satellite-pesticide-detection",
                Title = "My Master's Thesis: Satellite-Based Detection of Pesticide Overuse",
                Excerpt = "A summary of my master's thesis on using Sentinel-2 satellite imagery to flag likely overuse of pre-harvest desiccant on Danish potato fields, for a case brought by the Danish Environmental Protection Agency.",
                // Every fact, number and figure is taken from the thesis report
                // (Satellite_Based_Detection_of_Pesticide_Overuse_Final.pdf). The system was built
                // jointly, so the post says "we" and doesn't split the work per author.
                Body = """
                    <p>
                        For our master's thesis in Software Engineering at the University of
                        Southern Denmark, Anton Christensen and I worked on a case from the Danish
                        Environmental Protection Agency (Miljøstyrelsen): could satellite imagery
                        help the agency decide which potato fields to inspect for overuse of
                        pre-harvest desiccants?
                    </p>
                    <h2>The problem</h2>
                    <p>
                        Before harvest, seed potato farmers spray a desiccant that kills the plant
                        above ground, so the potatoes stop growing and the skin hardens. The
                        chemicals can leach into groundwater and harm nearby wildlife, so the
                        agency regulates how much and how often they may be applied. Compliance is
                        checked with soil samples, but there are fewer than a thousand inspections a
                        year (746 in 2019), and the sites are picked at random.
                    </p>
                    <p>
                        The agency's idea was to use the vegetation index NDVI, which can be
                        calculated from Sentinel-2 satellite data. A desiccant makes a field's NDVI
                        drop, and a field whose NDVI drops faster than normal might have been
                        sprayed too heavily. A list of suspicious fields would be a better basis for
                        choosing inspections than chance.
                    </p>
                    <h2>What we built</h2>
                    <p>
                        A Python system that collects satellite data for every field of a given
                        crop type in Denmark and turns it into vegetation index time series:
                    </p>
                    <ul>
                        <li>A database populator reads the agency's public GIS shapefile of all Danish fields, filters it by crop type, and stores each field's ID, crop type and polygon in MySQL</li>
                        <li>Download workers ask the Sentinel Hub Catalog API which dates have imagery for a field, including cloud cover, and download the raw spectral bands as GeoTIFFs from the Process API</li>
                        <li>Processing workers read the GeoTIFFs with Rasterio, check the data mask for invalid pixels, calculate NDVI, NDRE, GNDVI and EVI per pixel, and store the results or render heatmaps and KDE plots</li>
                        <li>RabbitMQ distributes the work. A work order (for example "download field data for August") is split into tasks of at most ten days, which are spread across the running workers</li>
                    </ul>
                    <h2>Engineering decisions</h2>
                    <p>
                        <strong>Raw bands instead of ready-made statistics.</strong> Sentinel Hub
                        can compute an index or an average for a field on its own servers, which
                        is cheaper. We downloaded the raw reflectance bands instead, because that
                        let us check the data per pixel, draw heatmaps, and calculate new indices
                        later without downloading everything again. We chose Sentinel Hub over
                        openEO because it is a plain REST API and doesn't tie the project to
                        Python.
                    </p>
                    <p>
                        <strong>A relational database.</strong> The agency suggested a time series
                        database. We chose MySQL because the data is relational (fields and their
                        readings per date), and because during exploration we kept updating data
                        and changing the schema, which time series databases handle poorly.
                    </p>
                    <p>
                        <strong>Parallel workers.</strong> The workload is I/O-bound: most of the
                        time goes to waiting for the API and the database. Python's Global
                        Interpreter Lock rules out simple multithreading, so we ran several worker
                        processes fed by RabbitMQ. In a test with ten fields and 30 days of data,
                        one worker took 119 seconds and four workers took 32 seconds. The time per
                        day of data stayed at about 4.3 seconds, so adding workers did not slow
                        each one down.
                    </p>
                    <p>
                        <strong>Long-running tasks in RabbitMQ.</strong> A download task could run
                        for over an hour, but RabbitMQ drops a consumer that runs too long, and our
                        worker blocked the heartbeat while waiting for a task to finish. We moved
                        task execution to a separate thread with a callback, raised the timeout to
                        two hours, and capped each task at ten days of data to stay within it.
                    </p>
                    <p>
                        <strong>Bounding boxes or individual fields.</strong> Instead of one
                        request per field, the system can download large bounding boxes and cut
                        the fields out afterwards. On Funen this was about 29 times faster (163
                        seconds for 40 boxes against 4,824 seconds for 1,993 fields), but the
                        per-field downloads followed the field borders more closely and gave more
                        accurate readings. We kept both modes, so speed can be traded for accuracy.
                    </p>
                    <h2>Results</h2>
                    <p>
                        The data told a clear story. On a typical seed potato field, NDVI starts
                        rising in mid-May, holds just below 0.8 through June and July, falls to
                        about 0.6 between 7 and 15 August, which matches the timing of a
                        desiccant, and drops to near zero at harvest in October.
                    </p>
                    <figure>
                        <img src="/img/blog/thesis/ndvi-season-field-2443.png" width="1000" height="500" loading="lazy"
                             alt="Line chart of minimum, maximum, average and median NDVI for field 2443 through 2020. NDVI is low until May, peaks around 0.8 in June and July, falls steeply in August, and reaches near zero in mid-October.">
                        <figcaption>NDVI through 2020 for one certified seed potato field (field 2443), from days with at most 25% cloud cover.</figcaption>
                    </figure>
                    <p>
                        The heatmaps show the same drop across the whole field, pixel by pixel:
                    </p>
                    <figure>
                        <div class="figure-row">
                            <img src="/img/blog/thesis/ndvi-heatmap-2020-08-07.png" width="640" height="480" loading="lazy"
                                 alt="NDVI heatmap of field 2443 on 7 August 2020, almost entirely green at around 0.8.">
                            <img src="/img/blog/thesis/ndvi-heatmap-2020-08-15.png" width="640" height="480" loading="lazy"
                                 alt="NDVI heatmap of field 2443 on 15 August 2020, pale green and yellow at around 0.6.">
                            <img src="/img/blog/thesis/ndvi-heatmap-2020-08-30.png" width="640" height="480" loading="lazy"
                                 alt="NDVI heatmap of field 2443 on 30 August 2020, orange and red at around 0.2.">
                        </div>
                        <figcaption>Field 2443 on 7, 15 and 30 August 2020.</figcaption>
                    </figure>
                    <p>
                        Seed potatoes could also be told apart from starch potatoes, sugar beets
                        and apple orchards by their NDVI curves alone. Starch potatoes are usually
                        left to wilt naturally, so their NDVI falls later and more gradually.
                    </p>
                    <figure>
                        <div class="figure-row">
                            <img src="/img/blog/thesis/ndvi-seed-potatoes.png" width="1000" height="500" loading="lazy"
                                 alt="Average NDVI from August to October 2020 for ten seed potato fields. Several fall from around 0.8 to below 0.4 by mid-August, and the rest are already low in early August.">
                            <img src="/img/blog/thesis/ndvi-starch-potatoes.png" width="1000" height="500" loading="lazy"
                                 alt="Average NDVI from August to October 2020 for ten starch potato fields. All stay above 0.6 through mid-August, then decline gradually through September and October.">
                        </div>
                        <figcaption>Average NDVI from August to October 2020 for ten seed potato fields (left) and ten starch potato fields (right).</figcaption>
                    </figure>
                    <p>
                        We developed three methods for scoring possible overuse: comparing the
                        decline of NDVI with that of NDRE (which measures deeper into the plant than
                        the leaf canopy), finding each field's steepest NDVI drop, and comparing the
                        NDVI and NDRE gradients. Each method produces a ranked list of fields.
                    </p>
                    <figure>
                        <img src="/img/blog/thesis/ndvi-ndre-field-2443.png" width="1000" height="500" loading="lazy"
                             alt="Line chart of average NDVI and average NDRE for field 2443 through 2020. Both follow the same shape, with NDRE lower than NDVI, and NDVI falling more steeply in August.">
                        <figcaption>Average NDVI and NDRE for field 2443. NDVI falls more steeply than NDRE after the desiccant is applied.</figcaption>
                    </figure>
                    <h2>What didn't work</h2>
                    <p>
                        To validate the scores, we needed ground truth: fields where the agency
                        knew whether desiccants had been used correctly or overused. The agency
                        could not share this data because of GDPR, and the farmers we contacted
                        declined. Our contact at the agency also left partway through the project,
                        so the requirements were never validated with the stakeholder.
                    </p>
                    <p>
                        Clouds were the other problem. Readings with more than 25% cloud cover were
                        too noisy to use, so some fields had gaps of weeks, often around the
                        desiccation date. The scoring methods ranked the same fields very
                        differently, and without ground truth we could not tell which one was
                        right. The steepest-drop NDVI ranking was the most reliable of the three,
                        and we argued it is still a better starting point for inspections than
                        random selection. But the question of whether overuse can be detected
                        remains open.
                    </p>
                    <h2>Next steps</h2>
                    <ul>
                        <li>Get ground truth, either from the agency's internal data or from test data from desiccant manufacturers</li>
                        <li>Compare NDVI and NDRE over the whole desiccation period instead of only the steepest drop</li>
                        <li>With enough ground truth, train a machine learning model that also includes weather and cloud cover data</li>
                    </ul>
                    """,
                // TODO: replace with the actual thesis defense date (the report was handed in 2025-06-01).
                PublishedOn = new DateOnly(2025, 6, 15),
                RelatedProject = await FindProjectAsync(db, "satellite-pesticide-detection"),
                Tags =
                [
                    new BlogPostTag { Name = "GIS", SortOrder = 1 },
                    new BlogPostTag { Name = "Sentinel-2 API", SortOrder = 2 },
                    new BlogPostTag { Name = "MySQL", SortOrder = 3 },
                    new BlogPostTag { Name = "RabbitMQ", SortOrder = 4 },
                    new BlogPostTag { Name = "Docker", SortOrder = 5 },
                ]
            });
        }

        if (!existingPostSlugs.Contains("building-this-portfolio-website"))
        {
            db.BlogPosts.Add(new BlogPost
            {
                Slug = "building-this-portfolio-website",
                Title = "Building This Portfolio Website in ASP.NET Core",
                Excerpt = "Why this site is a server-rendered ASP.NET Core application backed by EF Core and SQL Server rather than a static page or JavaScript framework, and how it is put together.",
                // DRAFT article body for Oskar to review/expand — built only from what the
                // repository itself shows (stack, data model, startup flow), no invented results.
                Body = """
                    <p>
                        A portfolio site could be a single static HTML page. I chose to build this
                        one as a server-rendered ASP.NET Core application instead, because the
                        roles I am applying for work with C#, .NET, SQL Server and Azure. Rather
                        than only listing those technologies, I wanted the portfolio itself to be a
                        project built with them.
                    </p>
                    <h2>Stack</h2>
                    <ul>
                        <li>ASP.NET Core 10 with Razor Pages, so every page is rendered on the server and there is no SPA framework</li>
                        <li>Entity Framework Core 10 as the data layer</li>
                        <li>SQL Server 2022, run locally in a Docker container via Docker Compose</li>
                    </ul>
                    <h2>Content as data</h2>
                    <p>
                        Projects, skills and blog posts, including this one, are modeled as EF Core
                        entities and stored in SQL Server rather than hardcoded in the markup. Each
                        project has its technologies as child rows, skills are grouped into
                        categories, and a blog post can optionally reference a project, which is
                        how the "Related project" box below is filled in. Presentation-only
                        concerns, like which icon belongs to which skill, are kept in the page code
                        rather than in the database.
                    </p>
                    <h2>Startup, migrations and seeding</h2>
                    <p>
                        On startup the application applies any pending EF Core migrations and then
                        runs an idempotent seed step, so a fresh database is created and populated
                        without any manual commands. Blog posts are seeded per slug, which means a
                        new post added to the seed reaches a database that already contains the
                        earlier ones, without duplicating or overwriting existing content.
                    </p>
                    <h2>What's next</h2>
                    <p>
                        Remaining work includes dedicated detail pages for each featured project,
                        automated tests, and deployment to a server with HTTPS and a domain.
                    </p>
                    <p>
                        <em>
                            This is a draft, and I'll add more about the design decisions and what I
                            learned building the site.
                        </em>
                    </p>
                    """,
                PublishedOn = new DateOnly(2026, 9, 23),
                RelatedProject = await FindProjectAsync(db, "portfolio-website"),
                Tags =
                [
                    new BlogPostTag { Name = "ASP.NET Core", SortOrder = 1 },
                    new BlogPostTag { Name = "Entity Framework Core", SortOrder = 2 },
                    new BlogPostTag { Name = "SQL Server", SortOrder = 3 },
                    new BlogPostTag { Name = "Docker", SortOrder = 4 },
                ]
            });
        }

        if (!existingPostSlugs.Contains("building-a-pool-tournament-platform"))
        {
            db.BlogPosts.Add(new BlogPost
            {
                Slug = "building-a-pool-tournament-platform",
                Title = "Building a Tournament Platform for My Local Pool Club",
                Excerpt = "How I replaced a pool club's Excel sheets with a TypeScript web app that runs its weekly tournaments, updates live on players' phones and the club's TV, and is self-hosted with Docker and Caddy.",
                // DRAFT article body for Oskar to review/expand — built only from what the
                // pool-tournament-app repository shows (README, DOCUMENTATION.md, code, config).
                Body = """
                    <p>
                        My local pool hall ran its weekly tournaments and season standings from
                        Excel sheets. I built a web app to replace them, and it now runs in
                        production on a server I host myself.
                    </p>
                    <h2>What it does</h2>
                    <ul>
                        <li>An admin creates a tournament, draws the players into pools and can rearrange them by drag and drop</li>
                        <li>Players enter their own match results from their phones using a join code</li>
                        <li>Pool tables, the knockout bracket and the season leaderboard update live, including on a TV screen in the club</li>
                        <li>Season points and player handicaps are tracked across tournaments</li>
                    </ul>
                    <h2>Architecture</h2>
                    <p>
                        The app is a TypeScript monorepo with three packages. The tournament rules
                        (pool scheduling, standings, qualification, knockout seeding and handicaps)
                        live in a pure domain package with no database or HTTP code, so the same
                        logic is shared by the backend and the frontend and can be unit tested on
                        its own. The backend is a Node.js API built with Fastify on top of
                        PostgreSQL, and the frontend is a React app built with Vite.
                    </p>
                    <h2>Live updates</h2>
                    <p>
                        When a result is entered, the server tells every open page that the
                        tournament changed, using Server-Sent Events. The event carries no data;
                        each page refetches what it shows. That keeps the server side simple, which
                        is enough for an app running as a single server process.
                    </p>
                    <h2>Testing</h2>
                    <p>
                        The domain rules have unit tests, and the API is covered by integration
                        tests that run against a real PostgreSQL database rather than mocks: around
                        270 test cases in total, run with Vitest.
                    </p>
                    <h2>Deployment</h2>
                    <p>
                        The app is built into a single Docker image with a multi-stage build, where
                        the API also serves the built frontend. On a Linux server, Docker Compose
                        runs the app, PostgreSQL, Caddy as a reverse proxy with automatic HTTPS, and
                        a backup container that takes a nightly database dump. Only Caddy is
                        reachable from the internet.
                    </p>
                    <p>
                        <em>
                            This is a draft, and I'll add more about the design decisions and what I
                            learned building the platform.
                        </em>
                    </p>
                    """,
                PublishedOn = new DateOnly(2026, 10, 25),
                RelatedProject = await FindProjectAsync(db, "pool-tournament-app"),
                Tags =
                [
                    new BlogPostTag { Name = "TypeScript", SortOrder = 1 },
                    new BlogPostTag { Name = "React", SortOrder = 2 },
                    new BlogPostTag { Name = "Node.js", SortOrder = 3 },
                    new BlogPostTag { Name = "PostgreSQL", SortOrder = 4 },
                    new BlogPostTag { Name = "Vitest", SortOrder = 5 },
                    new BlogPostTag { Name = "Docker", SortOrder = 6 },
                    new BlogPostTag { Name = "Caddy", SortOrder = 7 },
                ]
            });
        }

        await db.SaveChangesAsync();
    }

    private static SkillCategory[] SkillCategories() =>
    [
        new SkillCategory
        {
            Name = "Languages",
            SortOrder = 1,
            Skills =
            [
                new Skill { Name = "C#", SortOrder = 1 },
                new Skill { Name = "Python", SortOrder = 2 },
                new Skill { Name = "Java", SortOrder = 3 },
                new Skill { Name = "SQL", SortOrder = 4 },
                new Skill { Name = "HTML/CSS", SortOrder = 5 },
                new Skill { Name = "TypeScript", SortOrder = 6 },
            ]
        },
        new SkillCategory
        {
            Name = "Backend & Web",
            SortOrder = 2,
            Skills =
            [
                new Skill { Name = "ASP.NET Core", SortOrder = 1 },
                new Skill { Name = "Entity Framework Core", SortOrder = 2 },
                new Skill { Name = "REST APIs", SortOrder = 3 },
                new Skill { Name = "Node.js", SortOrder = 4 },
                new Skill { Name = "React", SortOrder = 5 },
                new Skill { Name = "Vitest", SortOrder = 6 },
            ]
        },
        new SkillCategory
        {
            Name = "Databases",
            SortOrder = 3,
            Skills =
            [
                new Skill { Name = "SQL Server", SortOrder = 1 },
                new Skill { Name = "MySQL", SortOrder = 2 },
                new Skill { Name = "PostgreSQL", SortOrder = 3 },
                new Skill { Name = "MongoDB", SortOrder = 4 },
                new Skill { Name = "Redis", SortOrder = 5 },
            ]
        },
        new SkillCategory
        {
            Name = "Cloud & DevOps",
            SortOrder = 4,
            Skills =
            [
                new Skill { Name = "Linux", SortOrder = 1 },
                new Skill { Name = "Docker", SortOrder = 2 },
                new Skill { Name = "Kubernetes", SortOrder = 3 },
                new Skill { Name = "Azure", SortOrder = 4 },
                new Skill { Name = "Google Cloud", SortOrder = 5 },
                new Skill { Name = "CI/CD", SortOrder = 6 },
                new Skill { Name = "Git", SortOrder = 7 },
                new Skill { Name = "Caddy", SortOrder = 8 },
            ]
        },
        new SkillCategory
        {
            Name = "Data & Messaging",
            SortOrder = 5,
            Skills =
            [
                new Skill { Name = "Apache Kafka", SortOrder = 1 },
                new Skill { Name = "RabbitMQ", SortOrder = 2 },
                new Skill { Name = "MQTT", SortOrder = 3 },
            ]
        },
    ];

    private static Project[] Projects() =>
    [
        new Project
        {
            Slug = "pool-tournament-app",
            Title = "Pool Tournament Platform",
            Summary = "A web platform that runs the weekly tournaments at my local pool hall, from pool play and knockout brackets to season standings and handicaps, with live updates on players' phones and the club's TV screen.",
            Purpose = "The club tracked its tournaments and season standings in Excel sheets. The app replaces them: an admin creates the tournament and draws the pools, players enter their own match results with a join code, and the tables, bracket and season leaderboard update live.",
            Contribution = "Built and deployed solo, from the data model and tournament rules to the production server.",
            EngineeringNotes = "A TypeScript monorepo where the tournament rules live in a pure domain package shared by a Fastify API and a React frontend. Changes reach open pages through Server-Sent Events, PostgreSQL schema changes go through versioned migrations, and the API is covered by integration tests against a real database.",
            Outcome = "Running in production on a self-hosted Linux server with Docker Compose, Caddy for automatic HTTPS, and nightly database backups.",
            RepositoryUrl = "https://github.com/OKThomsen/pool-tournament-app",
            DemoUrl = "https://turneringsportal.kjaer-thomsen.dk",
            HasDetailPage = true,
            Featured = true,
            // 0 so it sorts first in databases seeded before it was added, without renumbering.
            SortOrder = 0,
            Technologies =
            [
                new ProjectTechnology { Name = "TypeScript", SortOrder = 1 },
                new ProjectTechnology { Name = "React", SortOrder = 2 },
                new ProjectTechnology { Name = "Node.js", SortOrder = 3 },
                new ProjectTechnology { Name = "PostgreSQL", SortOrder = 4 },
                new ProjectTechnology { Name = "Vitest", SortOrder = 5 },
                new ProjectTechnology { Name = "Docker", SortOrder = 6 },
                new ProjectTechnology { Name = "Caddy", SortOrder = 7 },
            ]
        },
        new Project
        {
            Slug = "satellite-pesticide-detection",
            Title = "Satellite-Based Detection of Pesticide Overuse",
            Summary = "Analyzed Sentinel-2 satellite imagery to flag likely overuse of pre-harvest desiccant on Danish potato fields, for a case brought by the Danish Environmental Protection Agency.",
            Purpose = "The Danish EPA (Miljøstyrelsen) wanted a way to detect when farmers were applying excessive pre-harvest desiccant to potato crops. This master's thesis project used satellite data to calculate vegetation indices over the desiccation period, where an unusually steep drop in vegetation index can indicate overuse.",
            Contribution = "Designed and built jointly with Anton Christensen as a two-person master's thesis.",
            EngineeringNotes = "Pulled and processed Sentinel-2 API data, computed vegetation indices across the desiccation window, and used RabbitMQ and Docker to structure the data processing pipeline, with results stored in MySQL.",
            RepositoryUrl = "https://github.com/anton4d/Satellite-Based_Detection_of_Pesticide_Overuse",
            HasDetailPage = true,
            Featured = true,
            SortOrder = 1,
            Technologies =
            [
                new ProjectTechnology { Name = "GIS", SortOrder = 1 },
                new ProjectTechnology { Name = "Sentinel-2 API", SortOrder = 2 },
                new ProjectTechnology { Name = "MySQL", SortOrder = 3 },
                new ProjectTechnology { Name = "RabbitMQ", SortOrder = 4 },
                new ProjectTechnology { Name = "Docker", SortOrder = 5 },
            ]
        },
        new Project
        {
            Slug = "tv2-flexible-architecture",
            Title = "Flexible Software Architectures — TV 2 Play Case",
            Summary = "A microservice-based mockup of TV 2's streaming platform, TV 2 Play, built to explore independent scalability and availability under a publish/subscribe architecture.",
            Purpose = "Bachelor project built around a case supplied by Danish broadcaster TV 2: model their video platform as a set of independently scalable microservices.",
            Contribution = "TODO — Oskar to confirm individual contribution; repository is a fork of a teammate's original, group repository.",
            EngineeringNotes = "Three services were implemented and containerized independently, communicating via a publish/subscribe pattern, with the project focused specifically on availability and load balancing under scale.",
            RepositoryUrl = "https://github.com/okthomsen/Video-platform-TV2",
            HasDetailPage = true,
            Featured = true,
            SortOrder = 2,
            Technologies =
            [
                new ProjectTechnology { Name = "Microservices", SortOrder = 1 },
                new ProjectTechnology { Name = "Apache Kafka", SortOrder = 2 },
                new ProjectTechnology { Name = "Docker", SortOrder = 3 },
            ]
        },
        new Project
        {
            Slug = "wildlife-camera",
            Title = "Wildlife Camera Monitoring System",
            Summary = "A simulated wildlife camera network built on ESP32 and Raspberry Pi, with wireless drone-based image offloading and local LLM image annotation.",
            Purpose = "Embedded systems project modeling a remote wildlife monitoring setup: cameras collect images in the field with no permanent network connection, and a drone periodically flies over to collect the data.",
            Contribution = "Designed and built the full system individually, including the offload workflow and image annotation step.",
            EngineeringNotes = "Implemented wireless image offloading triggered when a drone connects to each camera's local Wi-Fi, plus a return-to-base workflow that transfers collected images to a server for post-processing. Offloaded images are annotated automatically by a locally-run large language model.",
            RepositoryUrl = "https://github.com/okthomsen/Wildlife-Camera",
            HasDetailPage = true,
            Featured = true,
            SortOrder = 3,
            Technologies =
            [
                new ProjectTechnology { Name = "Embedded Linux", SortOrder = 1 },
                new ProjectTechnology { Name = "Python", SortOrder = 2 },
                new ProjectTechnology { Name = "Bash/Shell", SortOrder = 3 },
                new ProjectTechnology { Name = "Wi-Fi Networking", SortOrder = 4 },
                new ProjectTechnology { Name = "Local LLM", SortOrder = 5 },
            ]
        },
        new Project
        {
            Slug = "portfolio-website",
            Title = "This Portfolio Website",
            Summary = "This site itself — a server-rendered ASP.NET Core application backed by EF Core and SQL Server, built specifically to demonstrate the Microsoft development stack.",
            Purpose = "Built as a deliberate showcase: rather than just listing .NET/Azure as target technologies, the portfolio site itself is implemented in ASP.NET Core with EF Core and SQL Server, containerized with Docker.",
            Contribution = "Designed and built solo, including the data model, seed content pipeline, and layout.",
            EngineeringNotes = "Razor Pages over a SQL Server database via EF Core; project and skill content is modeled as real data rather than hardcoded markup.",
            RepositoryUrl = "https://github.com/OKThomsen/Portfolio-Website",
            HasDetailPage = false,
            Featured = true,
            SortOrder = 4,
            Technologies =
            [
                new ProjectTechnology { Name = "ASP.NET Core", SortOrder = 1 },
                new ProjectTechnology { Name = "Entity Framework Core", SortOrder = 2 },
                new ProjectTechnology { Name = "SQL Server", SortOrder = 3 },
                new ProjectTechnology { Name = "Docker", SortOrder = 4 },
            ]
        },
    ];

    // The Projects block above may have just added the entity in-memory without saving
    // yet, so check the change tracker before falling back to a DB query.
    private static async Task<Project?> FindProjectAsync(PortfolioDbContext db, string slug) =>
        db.ChangeTracker.Entries<Project>()
            .Select(e => e.Entity)
            .FirstOrDefault(p => p.Slug == slug)
        ?? await db.Projects.FirstOrDefaultAsync(p => p.Slug == slug);
}
