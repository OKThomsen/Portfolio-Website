# Portfolio Website — Project Instructions

## Project Overview

This repository contains the personal portfolio website for Oskar Thomsen, a Master's graduate in Software Engineering.

The website's purpose is to:
- Present Oskar professionally to recruiters and hiring managers.
- Highlight software engineering skills and technical breadth.
- Showcase selected GitHub projects with clear technical context.
- Demonstrate engineering ability through the quality of the portfolio itself.
- Support applications for software engineering roles, particularly roles involving C#, .NET, Microsoft Azure, SQL, cloud, backend development, and related technologies.

The portfolio should feel like a **professional software engineer's portfolio**, not a generic developer template.

## Primary Audience

1. Recruiters
2. Hiring managers
3. Software engineers / technical interviewers
4. Potential employers

A visitor should understand who Oskar is, what he works with, and what he has built within roughly 30–60 seconds.

## Design Direction

Use the provided reference website as inspiration:

https://praestholm.dk

The desired aesthetic is:
- Minimal
- Technical
- Clean
- Professional
- Personal
- Content-focused
- Generous whitespace
- Subtle technical/developer visual language

Avoid:
- Generic "passionate developer" language
- Excessive animations
- Overly flashy gradients
- Excessive technology logos
- Artificial skill ratings such as stars
- Generic portfolio-template appearance
- Unnecessary complexity

The website should prioritize readability and credibility.

## Core Information Architecture

The initial website should be a single-page portfolio with sections approximately in this order:

1. Hero
2. About
3. Technical Skills
4. Featured Projects
5. Education
6. Contact

Important projects may have dedicated project-detail pages.

Possible project URL structure:

`/projects/<project-name>`

## Hero Section

The hero should immediately communicate:
- Oskar's name
- Software Engineer title
- Master's degree / software engineering background
- Short professional positioning
- GitHub link
- LinkedIn link
- CV link
- Clear CTA to projects

The exact wording should be based on the actual content collected during the project. Do not invent professional experience.

## Skills

Skills should be grouped rather than presented as a giant list.

Potential categories:

### Languages
- C#
- Python
- Java
- SQL
- JavaScript / TypeScript

### Backend
- .NET
- ASP.NET Core
- REST APIs
- Entity Framework Core

### Cloud & DevOps
- Microsoft Azure
- Docker
- Kubernetes
- Terraform
- CI/CD

### Databases
- MySQL
- SQL Server
- PostgreSQL

### Frontend
- React
- Angular
- HTML
- CSS

### Data & Distributed Systems
- Apache Kafka
- Apache Spark
- Hadoop
- RabbitMQ

Only include technologies that can be supported by Oskar's actual education/projects/experience.

Do not fabricate expertise.

## Projects

Projects are the centerpiece of the portfolio.

Target approximately 4–6 featured projects.

Each featured project should communicate:

- What the project does
- Why it exists
- Oskar's contribution
- Technologies used
- Important engineering decisions/challenges
- GitHub repository
- Optional demo
- Optional detailed project page

Projects should collectively demonstrate breadth, for example:
- C#/.NET/backend
- Cloud/Azure
- Data processing/distributed systems
- Python/data/GIS
- Full-stack development
- AI/automation

Do not include every GitHub repository. Curate the strongest projects.

## Project Detail Pages

Where appropriate, project pages should include:

### Overview
What the project does and why it exists.

### Architecture
A clear diagram or explanation of the system.

### Technologies
A concise list of relevant technologies.

### Engineering Challenges
Interesting technical problems and how they were approached.

### Results / Outcome
What was achieved.

### What I Learned
Short reflection where useful.

### Source Code
GitHub link.

The purpose is to show engineering thinking, not just the final result.

## .NET / Microsoft Positioning

Because many target roles involve Microsoft technologies, the portfolio should make relevant experience easy to find.

C#, .NET, ASP.NET Core, SQL Server, Azure, Docker, CI/CD, and related technologies should be represented where truthful.

If the current project portfolio lacks a strong modern C#/.NET showcase, identify this as a potential future project rather than pretending existing work uses technologies it does not.

A possible future showcase project could involve:

ASP.NET Core
→ Entity Framework Core
→ SQL Server
→ Docker
→ Azure
→ CI/CD

Do not implement this merely for keyword stuffing. It should be a technically meaningful project.

## Content Principles

- Be concise.
- Be technically specific.
- Prefer evidence over claims.
- Explain what was actually built.
- Do not exaggerate skill level.
- Do not invent work experience.
- Do not claim production experience unless supported.
- Use concrete technologies and engineering decisions where relevant.
- Avoid buzzword-heavy copy.

## Technical Principles

Keep the architecture as simple as practical.

The website will eventually be deployed to a server controlled by Oskar.

Potential deployment architecture:

GitHub
→ build/deployment
→ server
→ web server/reverse proxy
→ HTTPS
→ domain

The exact hosting architecture has not yet been decided.

Do not introduce infrastructure complexity before it is necessary.

## Development Process

Work in these phases:

### Phase 1 — Content
- Inventory existing projects.
- Collect GitHub URLs.
- Select featured projects.
- Gather education information.
- Gather CV, LinkedIn, and contact information.
- Establish accurate technical skill inventory.

### Phase 2 — Information Architecture
- Finalize sections.
- Decide which projects get dedicated pages.
- Define navigation.

### Phase 3 — Visual Design
- Establish typography.
- Establish color palette.
- Establish spacing.
- Establish cards/buttons/navigation.
- Establish responsive behavior.
- Establish animation principles.

### Phase 4 — Technology Selection
Choose the frontend/framework based on:
- Quality of the resulting website
- Maintainability
- Deployment simplicity
- Performance
- Ability to demonstrate relevant engineering skills

Do not select a technology purely because it is fashionable.

### Phase 5 — Implementation
Build the website incrementally:
1. Project setup
2. Global layout
3. Navigation
4. Hero
5. About
6. Skills
7. Projects
8. Project pages
9. Education
10. Contact
11. Responsive design
12. Accessibility
13. Performance
14. Final polish

### Phase 6 — Deployment
- Configure server
- Configure web server/reverse proxy
- Configure HTTPS
- Configure domain
- Configure deployment
- Verify production build

### Phase 7 — Polish
Review the website from:
- Recruiter perspective
- Hiring-manager perspective
- Software-engineer perspective
- Mobile perspective
- Accessibility perspective

## Coding Guidelines

Before implementing a major feature:
1. Understand the existing architecture.
2. Prefer simple solutions.
3. Avoid unnecessary dependencies.
4. Keep components modular.
5. Keep content separate from presentation where practical.
6. Do not rewrite working code without a reason.
7. Verify builds after significant changes.
8. Keep the UI responsive.
9. Maintain accessibility.
10. Optimize for clarity over cleverness.

## GitHub / External Links

GitHub projects should be linked directly and clearly.

Do not create fake repository URLs.

Do not assume repository names or project descriptions without verifying them.

## Images and Visual Assets

Use visuals when they meaningfully improve understanding:
- Architecture diagrams
- Project screenshots
- Relevant project imagery

Avoid decorative stock imagery that does not contribute to the portfolio.

## Current Project Status

The project is currently in the **planning / discovery stage**.

The first task is NOT to immediately build the complete website.

First:
1. Inspect the repository.
2. Identify the current state.
3. Propose the initial implementation plan.
4. Identify missing information.
5. Start with the smallest useful foundation.

## Important Constraint

Do not fabricate information about Oskar.

If content is missing, use a placeholder and clearly identify it as content that needs to be supplied.

The portfolio must represent the real person, education, skills, and projects accurately.
