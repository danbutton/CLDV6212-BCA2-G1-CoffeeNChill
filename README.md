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
> 5. _Azure Functions base images are published for linux/amd64 only_

<!-- ═══════════ END C ═══════════ -->

<!-- ═══════════ OWNER: D — setup, docker, testing ═══════════ -->
## Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 10.0 | Build the Functions project |
| Docker Desktop | Latest | Azurite emulator and container builds |
| Azure Storage Explorer | Latest | Inspect tables and blob containers |
| Postman | Latest | Run the API test collection |
| Newman | Latest | Optional CLI runner for the same collection |
| Git | 2.40+ | Version control |

Verify your setup:

```bash
dotnet --version     # expect 10.0.x
docker --version
docker info          # must not error — Docker Desktop has to be running
git --version
```

> **Azure Functions Core Tools is not required.** The project is a standard
> `.csproj` and the container uses the official Azure Functions runtime image,
> which supplies the host. `dotnet build` is all that is needed locally.

## Local Setup

```bash
# 1. Clone
git clone https://github.com/danbutton/CLDV6212-BCA2-G1-CoffeeNChill.git
cd CLDV6212-BCA2-G1-CoffeeNChill

# 2. Build
cd CLDV6212_POE_Part1_AzureFunction
dotnet build
```

Expect `Build succeeded`.

The local settings file holds storage connection strings and is deliberately
gitignored. Copy the template and fill in your own values if you need to run
outside a container:

```bash
cp local.settings.example.json local.settings.json
```

Every connection in the template points at Azurite
(`UseDevelopmentStorage=true`), so no live Azure account is required for local
development.

A verification script is provided to confirm your environment matches the repo:

```bash
bash scripts/verify.sh
```

It checks the toolchain, folder layout, required files, ignore rules and build
status, and reports each as pass, fail or warning.

## Standalone Docker Execution

Part 1 uses `docker run` only — no Docker Compose.

### 1. Create a user-defined network

```bash
docker network create coffeenchill-net
```

Containers can only resolve one another by name on a user-defined network.
This is how the Functions container reaches Azurite without orchestration.

### 2. Start Azurite

```bash
docker run -d \
  --name azurite \
  --network coffeenchill-net \
  -p 10000:10000 -p 10001:10001 -p 10002:10002 \
  -v azurite-data:/data \
  mcr.microsoft.com/azure-storage/azurite \
  azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0 --location /data
```

Verify:

```bash
docker ps
```

> The `--blobHost/--queueHost/--tableHost 0.0.0.0` flags are required. Without
> them Azurite binds only to its own container loopback and cannot be reached
> from another container.
>
> Note also that the port mapping differs from the brief: the actual assignment
> is **10000 = Blob, 10001 = Queue, 10002 = Table**. All three are published.

### 3. Build the Functions image

```bash
cd CLDV6212_POE_Part1_AzureFunction
docker build --platform linux/amd64 -t danbutton/coffeenchill-functions:v1.0 .
```

> **`--platform linux/amd64` is required on Apple Silicon.** The Azure Functions
> base images are published for `linux/amd64` only, so an ARM host must build
> and run them under emulation. Omitting the flag fails with
> `no match for platform in manifest`.

The Dockerfile is multi-stage: the .NET 10 SDK image compiles and publishes the
project, then only the published output is copied into the Azure Functions
runtime image. The SDK layer is discarded, so it never ships.

The `.csproj` is copied and restored before the source is added, so Docker
caches the dependency layer and only re-runs the restore when packages actually
change rather than on every code edit.

Check the built image:

```bash
docker images | grep coffeenchill
```

### 4. Run the Functions container

```bash
AZ="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite:10000/devstoreaccount1;QueueEndpoint=http://azurite:10001/devstoreaccount1;TableEndpoint=http://azurite:10002/devstoreaccount1;"

docker run -d \
  --platform linux/amd64 \
  --name coffeenchill-functions \
  --network coffeenchill-net \
  -p 7071:80 \
  -e AzureWebJobsStorage="$AZ" \
  -e TableStorageConnection="$AZ" \
  -e BlobStorageConnection="$AZ" \
  -e MenuTableName="MenuItems" \
  -e StaffDocsContainerName="staff-docs" \
  danbutton/coffeenchill-functions:v1.0
```

Watch the host start and list its routes:

```bash
docker logs -f coffeenchill-functions
```

`Ctrl+C` leaves the log view; the container keeps running.

> **Why the connection strings are written out in full.**
> `UseDevelopmentStorage=true` expands to `127.0.0.1`. Inside the Functions
> container that address means *the Functions container itself*, not Azurite.
> Because both containers share `coffeenchill-net`, Docker's embedded DNS
> resolves the hostname `azurite`, so the Blob, Queue and Table endpoints are
> given explicitly against that name.
>
> `Eby8vdM02...` is Azurite's published public development key, not a secret.

### Teardown

```bash
docker rm -f coffeenchill-functions azurite
docker volume rm azurite-data
docker network rm coffeenchill-net
```

## Docker Hub Images

Both images are published publicly with semantic version tags.

| Image | Purpose |
|---|---|
| `danbutton/coffeenchill-functions:v1.0` | The containerised Functions app |
| `danbutton/coffeenchill-azurite:v1.0` | Azurite storage emulator |

```bash
docker pull danbutton/coffeenchill-functions:v1.0
docker pull danbutton/coffeenchill-azurite:v1.0
```

Publishing and verification:

```bash
docker login
docker push danbutton/coffeenchill-functions:v1.0

# Confirm the published image works from a clean state
docker rmi danbutton/coffeenchill-functions:v1.0
docker pull danbutton/coffeenchill-functions:v1.0
```

> The brief specifies the tag `coffeenchill-Azurite:v1.0`. Docker Hub repository
> names must be lowercase, so this is published as
> `coffeenchill-azurite:v1.0`.

## Postman Collection

The exported collection and environment live in [`/docs`](./docs).

### Desktop app

1. Import both JSON files into Postman
2. Select the **CoffeeNChill — Local (Azurite)** environment
3. Confirm `baseUrl` points at your running host (`http://localhost:7071/api`)
4. Collection → **Run**

### Command line

The same collection runs headlessly via Newman:

```bash
npm install -g newman

newman run docs/CoffeeNChill.postman_collection.json \
  -e docs/CoffeeNChill.postman_environment.json
```

### What the collection covers

Requests are organised into two folders — menu endpoints and document
endpoints — and every request carries automated assertions on status code,
response shape and response time. Alongside the success paths, the suite
includes deliberate failure cases:

| Scenario | Expected |
|---|---|
| Duplicate SKU on create | 409 Conflict |
| Invalid payload (bad category, empty SKU, negative price) | 400 with all failures listed |
| Category with no items | 404 Not Found |
| Update or delete a missing item | 404 Not Found |
| Upload with no file attached | 400 Bad Request |
| Download a file that does not exist | 404 Not Found |

All URLs are built from the `{{baseUrl}}` environment variable, so the same
collection runs unchanged against a locally hosted app or the container without
editing a single request.

> The upload request needs a file attached manually in the form-data body —
> Postman does not persist binary file paths across an export.

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
