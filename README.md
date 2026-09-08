# CoffeeNChill - Canteen Management System

**Module:** CLDV6212 — Cloud Development B
**Institution:** The Independent Institute of Education (Emeris University)
**Assessment:** Portfolio of Evidence - Part 1
**Group:** G1

A cloud-enabled microservices system replacing CoffeeNChill's paper menus, handwritten order slips and filing-cabinet documents with Azure Table Storage, Azure File Shares and containerised HTTP-triggered Azure Functions.

---

## 🎥 Video Demonstration

> _To be added before submission._

---

## Team

| Member | Student No. | Role | Focus |
|---|---|---|---|
| Kyle | ST10473747 | A | Domain model & Table Storage repository |
| Evan | ST10482786 | B | Menu HTTP functions & error handling |
| Saveer |  ST10487403 | C | Azure File Share & document endpoints |
| Daniel | ST10491642 | D | Containerisation, testing, documentation, integration |

---

<!-- ═══════════ OWNER: A — architecture & data model ═══════════ -->
## Architecture Overview

> _Owned by A. Diagram to be added at `/docs/architecture.png`._

## Data Model

> _Owned by A._

<!-- ═══════════ END A ═══════════ -->

<!-- ═══════════ OWNER: B — API endpoints ═══════════ -->
## API Endpoint Reference

> _Owned by B._

| Method | Route | Purpose | Success | Errors |
|---|---|---|---|---|
| POST | `/api/menu` | Create a menu item | 201 | 400, 409 |
| GET | `/api/menu` | List all menu items | 200 | — |
| GET | `/api/menu/category/{category}` | Filter by category | 200 | 404 |
| PUT | `/api/menu/{category}/{id}` | Update price/availability | 200 | 400, 404 |
| DELETE | `/api/menu/{category}/{id}` | Remove an item | 204 | 404 |
| POST | `/api/documents/upload` | Upload a staff document | 201 | 400 |
| GET | `/api/documents` | List staff documents | 200 | — |
| GET | `/api/documents/download/{fileName}` | Download a document | 200 | 404 |

<!-- ═══════════ END B ═══════════ -->

<!-- ═══════════ OWNER: C — file storage & deviations ═══════════ -->
## Staff Document Storage

> _Owned by C._

## Known Deviations from the Brief

> _Owned by C. Must cover, with IEEE citations:_
> 1. _Azurite does not emulate Azure Files_
> 2. _Azurite port mapping in the brief is incorrect_
> 3. _`coffeenchill-Azurite` is an invalid Docker Hub tag_
> 4. _Table Storage has no decimal type_

<!-- ═══════════ END C ═══════════ -->

<!-- ═══════════ OWNER: D — setup, docker, testing ═══════════ -->
## Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 8.0 (LTS) | Build and run the Functions project |
| Azure Functions Core Tools | v4 | Local Functions host (`func start`) |
| Docker Desktop | Latest | Azurite emulator and container builds |
| Azure Storage Explorer | Latest | Inspect tables and file shares |
| Postman | Latest | Run the API test collection |
| Git | 2.40+ | Version control |

Verify your setup:

```bash
dotnet --version     # expect 8.0.x
func --version       # expect 4.x
docker --version
```

## Local Setup

> _Owned by D. To be completed._

## Standalone Docker Execution

> _Owned by D. To be completed. Part 1 uses `docker run` only — no Docker Compose._

## Docker Hub Images

> _Owned by D. Links to be added once published._

## Postman Collection

The exported collection and environment live in [`/docs`](./docs).

1. Import both JSON files into Postman
2. Select the **CoffeeNChill — Local (Azurite)** environment
3. Confirm `baseUrl` points at your running host
4. Collection → **Run**

<!-- ═══════════ END D ═══════════ -->

<!-- ═══════════ ALL — one row each, do not edit other rows ═══════════ -->
## Team Contributions

> _Each member completes their own row. Commit counts from `git shortlog -sn --all`._

| Member | Student No. | Responsibilities | Commits | Video segment |
|---|---|---|---|---|
| Daniel Button | ST10491642 | Docker, Docker Hub, Postman suite, README, integration | _TBC_ | _TBC_ |

## AI Usage Declaration

> _To be completed before submission. See [`/docs/ai-usage-log.md`](./docs/ai-usage-log.md) for the running record._

## References

> _IEEE style. To be completed._

<!-- ═══════════ END ALL ═══════════ -->

---

## Repository Structure

```
.
├── src/
│   └── CoffeeNChill.Functions/     # Azure Functions project
├── CONTRIBUTING.md             # Shared-file protocol and Git workflow
├── .gitignore
├── .gitattributes
├── .dockerignore
└── README.md
```

## Contributing

See [`docs/CONTRIBUTING.md`](./docs/CONTRIBUTING.md) for branch naming, commit conventions, the shared-file protocol and the review process. **Read it before your first commit.**
