using Portfolio.Web.Models;

namespace Portfolio.Web.Data;

// Content here is Oskar's real, verified work only — see CLAUDE.md: never fabricate
// projects, skills, or experience. Update this file as new projects/skills are confirmed.
public static class SeedData
{
    public static async Task SeedAsync(PortfolioDbContext db)
    {
        if (!db.SkillCategories.Any())
        {
            db.SkillCategories.AddRange(
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
                        new Skill { Name = "Docker", SortOrder = 1 },
                        new Skill { Name = "Kubernetes", SortOrder = 2 },
                        new Skill { Name = "Git", SortOrder = 3 },
                        new Skill { Name = "CI/CD", SortOrder = 4 },
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
                }
            );
        }

        if (!db.Projects.Any())
        {
            db.Projects.AddRange(
                new Project
                {
                    Slug = "satellite-pesticide-detection",
                    Title = "Satellite-Based Detection of Pesticide Overuse",
                    Summary = "Analyzed Sentinel-2 satellite imagery to flag likely overuse of pre-harvest desiccant on Danish potato fields, for a case brought by the Danish Environmental Protection Agency.",
                    Purpose = "The Danish EPA (Miljøstyrelsen) wanted a way to detect when farmers were applying excessive pre-harvest desiccant to potato crops. This master's thesis project used satellite data to calculate vegetation indices over the desiccation period, where an unusually steep drop in vegetation index can indicate overuse.",
                    Contribution = "TODO — Oskar to confirm individual contribution details for the site copy.",
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
                    RepositoryUrl = null, // TODO: publish this repo to GitHub and link it here
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
                }
            );
        }

        await db.SaveChangesAsync();
    }
}
