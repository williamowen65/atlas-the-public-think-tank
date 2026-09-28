# Atlas — The Public Think Tank

[![Atlas Rewrite Tests](https://github.com/williamowen65/atlas-the-public-think-tank/actions/workflows/AtlasRewriteTests.yml/badge.svg)](https://github.com/williamowen65/atlas-the-public-think-tank/actions/workflows/AtlasRewriteTests.yml)
[![License](https://img.shields.io/github/license/williamowen65/atlas-the-public-think-tank)](LICENSE)

Atlas is an open-source platform for thinking through complex public problems together. It is designed to help people develop ideas, examine possible solutions, connect related contributions, and make the structure of a discussion easier to understand.

This repository contains a new version of Atlas. The application is being rebuilt around explicit **bounded contexts**: focused parts of the system that each own a particular set of concepts and rules. This keeps the growing platform understandable while allowing its features to work together through documented boundaries.

> **Project status:** Atlas is under active development. The current Console application is an executable reference for exercising and reviewing the domain workflows before they are exposed through a web API and user interface.

## Transparency by design

Atlas treats transparency as a product requirement. People should be able to inspect and question the rules, rankings, and algorithms that shape their experience—not merely view the source files. The [Atlas transparency and algorithmic accountability principles](docs/TRANSPARENCY.md) describe the project's commitments to understandable specifications, explainable results, versioned decision behavior, auditability, reproducible verification, privacy, and an open change process.

## How Atlas is organized

| Boundary | Responsibility |
|---|---|
| **Graph** | Nodes, node types, relationships, and lifecycle |
| **Content** | Structured descriptions and content blocks |
| **Participants** | Participant identity and domain-level permissions |
| **Voting** | Node importance ratings and reaction votes |
| **Communities** | Communities, membership, and node association |
| **Comments** | Threaded conversations and comment lifecycle |
| **Discovery** | Searching, filtering, ranking, and pagination |
| **Moderation** | Reports, moderator review, decisions, and enforced actions |
| **Contracts** | Shared messages used across boundaries |
| **Console** | Composition root and interactive workflow harness |

These boundaries currently run together as a modular application. Separating their responsibilities now keeps deployment options open without adding the operational complexity of distributed services prematurely.

## Run the current application

Atlas currently targets [.NET 10](https://dotnet.microsoft.com/).

```bash
git clone https://github.com/williamowen65/atlas-the-public-think-tank.git
cd atlas-the-public-think-tank
dotnet restore Atlas.sln
dotnet run --project src/Atlas.Console/Atlas.Console.csproj
```

Run the automated tests with:

```bash
dotnet test Atlas.sln
```

## Repository guide

- [`src/`](src/) — current Atlas boundaries and the Console host
- [`tests/`](tests/) — automated tests organized by boundary
- [`docs/`](docs/README.md) — requirements, architecture, contracts, workflows, testing guidance, glossary, and blackboards
- [`legacy/`](legacy/README.md) — the earlier web application and its original README
- [`infrastructure/`](infrastructure/README.md) — planned infrastructure as code
- [`data/`](data/) — file-system data used by the Console host

For a deeper technical introduction, start with the [documentation index](docs/README.md). The [requirements baseline](docs/requirements/README.md) and [traceability matrix](docs/requirements/TRACEABILITY.md) connect intended behavior to implementation and verification.

## Contributing

Atlas welcomes thoughtful contributions to its code, tests, documentation, architecture, accessibility, and product design.

Read [CONTRIBUTING.md](CONTRIBUTING.md) for the complete contribution path, including where ideas and bugs belong, how work becomes `contribution-ready`, testing and documentation expectations, the relationship between GitHub and Jira, and contribution licensing.

GitHub Issues are the public proposal and contribution backlog. Jira separately tracks work Atlas has accepted and committed to deliver; the two systems are intentionally not synchronized.

## Earlier version

The previous Atlas web application is preserved under [`legacy/`](legacy/). Its [archived README](legacy/README.md) records the earlier project description and feature direction.

## License

Atlas is licensed under the [GNU Affero General Public License version 3](LICENSE). Copyright and trademark information is recorded in [NOTICE.md](NOTICE.md).
