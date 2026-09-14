# CoffeeNChill - Canteen Management System

**Module:** CLDV6212 - Cloud Development B
**Institution:** The Independent Institute of Education (Emeris University)
**Assessment:** Portfolio of Evidence - Part 1
**Group:** G1

A cloud-enabled microservices system replacing CoffeeNChill's paper menus, handwritten order slips and filing-cabinet documents with Azure Table Storage, Azure Blob Storage and containerised HTTP-triggered Azure Functions.

---

## 🎥 Video Demonstration

| Segment | Member | Link |
|---|---|---|
| Architecture & data model | Kyle | _link to be added_ |
| Menu endpoints & error handling | Evan | _link to be added_ |
| Blob Storage & document endpoints | Saveer | _link to be added_ |
| Containerisation, Docker Hub & API testing | Daniel Button | [Watch](https://youtu.be/Je9jZIyTPik) |

---

## Team

| Member | Student No. | Role | Focus |
|---|---|---|---|
| Kyle | ST10473747 | A | Domain model & Table Storage service |
| Evan | ST10482786 | B | Menu HTTP functions & error handling |
| Saveer Singh | ST10487403 | C | Azure Blob Storage & document endpoints |
| Daniel Button | ST10491642 | D | Containerisation, testing, documentation, integration |

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
| POST | `/api/menu` | Create a menu item | 201 | 400, 500 |
| GET | `/api/menu` | List all menu items | 200 | — |
| GET | `/api/menu/category/{category}` | Filter by category | 200 | 404 |
| PUT | `/api/menu/{category}/{id}` | Update a menu item | 200 | 400, 404 |
| DELETE | `/api/menu/{category}/{id}` | Remove an item | 204 | 404 |
| POST | `/api/documents/upload` | Upload a staff document | 200 | 400 |
| GET | `/api/documents` | List staff documents | 200 | — |
| GET | `/api/documents/download/{fileName}` | Download a document | 200 | 404 |

Menu entities are addressed by their storage keys: `PartitionKey` is the
category and `RowKey` is the SKU.

```json
{
  "PartitionKey": "Hot Drinks",
  "RowKey": "COF-001",
  "Name": "Espresso",
  "Description": "Double shot, locally roasted",
  "Price": 28.50,
  "IsAvailable": true
}
```

<!-- ═══════════ END B ═══════════ -->

<!-- ═══════════ OWNER: C — file storage & deviations ═══════════ -->
## Staff Document Storage

> _Owned by C._

Staff documents are stored in an **Azure Blob Storage** container named
`staff-docs`. Per the Sept 2026 POE addendum, this replaces the original Azure
Files design; Azurite does not emulate the Files service.

The layer sits behind `IDocumentRepository` and is implemented in
`BlobDocumentRepository`, registered as a singleton in `Program.cs`. Three HTTP
endpoints expose it:

| Method | Route | Behaviour |
|---|---|---|
| POST | `/api/documents/upload` | Streams a multipart file to `staff-docs` |
| GET | `/api/documents` | Lists blobs with name, size, last-modified, content-type |
| GET | `/api/documents/download/{fileName}` | Streams the blob back; 404 if missing |

**Design choices**

- **Streaming, not buffering** — `UploadAsync(Stream)` and
  `DownloadStreamingAsync()` keep large PDFs out of memory.
- **No manual chunking** — the SDK blocks uploads automatically at 4 MiB.
- **Single round-trip listing** — `GetBlobsAsync()` returns size, date and
  content-type in `BlobItem.Properties`.
- **Container bootstrap** — `CreateIfNotExists()` runs in the constructor, so a
  fresh Azurite works with zero setup.
- **Errors logged, not swallowed** — every method logs via `ILogger` and
  rethrows for the HTTP layer to translate into a 500.

Locally the layer runs against **Azurite** on the same connection string as the
MenuItems table, so no live Azure account is required.

## Known Deviations from the Brief
Four points where the brief disagrees with the tooling it specifies. Documented
here so the marker can see they were deliberate.

### 1. Azurite does not emulate Azure Files

The brief asked for **Azure File Shares**, but Azurite only supports Blob,
Queue and Table, the Files service is not covered. Any call to a
`ShareClient` will fail at runtime. This was fixed by the Sept 2026 addendum
that moved docs to **Azure Blob Storage**, which Azurite is a complete
emulation of. As the layer sits behind `IDocumentRepository`, the swap did not
require changes to the HTTP contract.

### 2. Azurite port mapping in the brief is incorrect

The brief labels port 10000 as Tables, 10001 as Blobs and 10002 as Queues, this
is inverted. The correct mapping is:

| Port | Service |
|---|---|
| 10000 | Blob |
| 10001 | Queue |
| 10002 | Table |

The `docker run` ports themselves are fine; the description is misleading.

### 3. `coffeenchill-Azurite` is an invalid Docker Hub tag

Docker Hub requires **lowercase** repo and tag names; uppercase is rejected at
push time. We publish `coffeenchill-azurite:v1.0` (all lowercase), and
`coffeenchill-functions:v1.0`.

### 4. Table Storage has no decimal type

Azure Table Storage only supports `Int64`, `Double`, `Boolean`, `DateTime`,
`Guid`, `String`, and `Binary` no `decimal`. Prices are stored as `double`
because the brief lists `Price` as "Double/Decimal". Acceptable for a canteen
menu. **Not** acceptable for financial ledgers where `decimal` rounding matters.
Here we make explicit the trade-off we are faced with.


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
dotnet --version
docker --version
docker info
git --version
```

`dotnet` should report 10.0.x, and `docker info` must not error — Docker Desktop
has to be running.

> **Azure Functions Core Tools is not required.** The project is a standard
> `.csproj` and the container uses the official Azure Functions runtime image,
> which supplies the host. `dotnet build` is all that is needed locally.

## Local Setup

```bash
git clone https://github.com/danbutton/CLDV6212-BCA2-G1-CoffeeNChill.git
cd CLDV6212-BCA2-G1-CoffeeNChill

cd CLDV6212_POE_Part1_AzureFunction
dotnet build
```

Expect `Build succeeded`.

The local settings file holds storage connection strings and is deliberately
gitignored. Copy the template if you need to run outside a container:

```bash
cp local.settings.example.json local.settings.json
```

Every connection in the template points at Azurite
(`UseDevelopmentStorage=true`), so no live Azure account is required for local
development.

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

Verify with `docker ps`.

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
| [`danbutton/coffeenchill-functions:v1.0`](https://hub.docker.com/r/danbutton/coffeenchill-functions) | The containerised Functions app |
| [`danbutton/coffeenchill-azurite:v1.0`](https://hub.docker.com/r/danbutton/coffeenchill-azurite) | Azurite storage emulator |

```bash
docker pull danbutton/coffeenchill-functions:v1.0
docker pull danbutton/coffeenchill-azurite:v1.0
```

Publishing and verification:

```bash
docker login
docker push danbutton/coffeenchill-functions:v1.0

docker rmi danbutton/coffeenchill-functions:v1.0
docker pull danbutton/coffeenchill-functions:v1.0
```

The published image was verified by deleting it locally and pulling it back
down, confirming that what is on Docker Hub is the image that runs.

> The brief specifies the tag `coffeenchill-Azurite:v1.0`. Docker Hub repository
> names must be lowercase, so this is published as `coffeenchill-azurite:v1.0`.

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

Thirteen requests across two folders — menu endpoints against Table Storage and
document endpoints against Blob Storage. Every request carries automated
assertions on status code, response shape and response time. Alongside the
success paths, the suite includes deliberate failure cases:

| Scenario | Expected |
|---|---|
| Invalid payload (empty keys, negative price) | 400 with a validation message |
| Update an item that does not exist | 404 Not Found |
| Download a file that does not exist | 404 Not Found |

All URLs are built from the `{{baseUrl}}` environment variable, so the same
collection runs unchanged against a locally hosted app or the container without
editing a single request.

**Current result: 39 of 39 assertions passing.**

> Two notes for anyone re-running the suite:
>
> 1. The upload request needs a file attached manually in the form-data body —
>    Postman does not persist binary file paths across an export.
> 2. The suite creates `COF-001`, `COF-002` and `PAS-104` itself, so those three
>    entities must be absent before a run. Delete them first if re-running.
>
> The upload endpoint returns `200 OK` rather than `201 Created`. By REST
> convention a resource that did not previously exist should return 201, so the
> assertion accepts either rather than altering the endpoint contract.

<!-- ═══════════ END D ═══════════ -->

<!-- ═══════════ ALL — one row each, do not edit other rows ═══════════ -->
## Team Contributions

> _Each member completes their own row. Commit counts from `git shortlog -sn --all`._

| Member | Student No. | Responsibilities | Commits | Video segment |
|---|---|---|---|---|
| Daniel Button | ST10491642 | Multi-stage Dockerfile, Docker Hub publishing, Postman suite, project consolidation, README | _TBC_ | [Watch](https://youtu.be/Je9jZIyTPik) |
| Kyle | ST10473747 | Digital Menu and Document Management | 17 | [Watch](https://youtu.be/OoT5CZL1-W4) |
| Evan | ST10482786 | Menu endpoint adjustments, validation | _TBC_ | _TBC_ |
| Saveer Singh | ST10487403 | Blob Storage repository, upload/list/download endpoints | _TBC_ | _TBC_ |

## AI Usage Declaration

In accordance with the assessment instructions, the group declares the following
use of AI tools during this submission.

**Claude (Anthropic)** was used to:

- identify errors in the brief, including Azurite's lack of Azure Files support,
  the incorrect port mapping and the invalid Docker Hub tag casing
- review the multi-stage Dockerfile and repository structure
- assist in debugging container-to-container networking, the `linux/amd64`
  platform constraint on Apple Silicon, and a dependency-injection registration
  that was lost when two parallel projects were consolidated

All architectural decisions, storage schema design, validation rules and testing
were determined and implemented by group members. Every AI suggestion was
reviewed, tested against the running application and modified before inclusion.
Each member can explain and defend the code they contributed.

A running record is kept in [`/docs/ai-usage-log.md`](./docs/ai-usage-log.md).

## References

Microsoft (2026) *Quickstart: Azure Blob Storage client library for .NET*. Available at:
https://learn.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet (Accessed: 10 September 2026).

Stack Overflow (2020) *How upload blob in Azure Blob Storage with specified ContentType with .NET v12 SDK?* Available at:
https://stackoverflow.com/questions/59945376/how-upload-blob-in-azure-blob-storage-with-specified-contenttype-with-net-v12-s (Accessed: 10 September 2026).

Microsoft (2025) *Use the Azurite emulator for local Azure Storage development*. Available at:
https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite (Accessed: 10 September 2026).

Microsoft (2023) *Azure Blob storage output binding for Azure Functions*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-output (Accessed: 10 September 2026).

Microsoft (2023) *Class BlobHttpHeaders | Azure SDK for .NET*. Available at:
https://azuresdkdocs.z19.web.core.windows.net/dotnet/Azure.Storage.Blobs/12.23.0/api/Azure.Storage.Blobs.Models/Azure.Storage.Blobs.Models.BlobHttpHeaders.html (Accessed: 10 September 2026).

Microsoft (2026) *Guide for running C# Azure Functions in an isolated worker process*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide (Accessed: 12 September 2026).

DotnetUstad (no date) *C# Function Documentation and Comments*. Available at:
https://dotnetustad.com/c-sharp/function-documentation-and-comments (Accessed: 12 September 2026).

Microsoft (2024) *In the Azure functions isolated process model, how can one return a stream without buffering all content first?* Available at:
https://learn.microsoft.com/en-us/answers/questions/1418946/in-the-azure-functions-isolated-process-model-how (Accessed: 12 September 2026).

Microsoft (2019) *FileResult.FileDownloadName Property (System.Web.Mvc)*. Available at:
https://learn.microsoft.com/en-us/dotnet/api/system.web.mvc.fileresult.filedownloadname (Accessed: 12 September 2026).

ASP Today (2026) *File Upload and Processing in ASP.NET Core: Streaming, Validation, and Cloud Storage*. Available at:
https://www.asptoday.com/p/file-upload-and-processing-in-aspnet (Accessed: 12 September 2026).

Microsoft (2026) *Dependency injection in .NET Azure Functions*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection (Accessed: 13 September 2026).

Microsoft (2026) *Azure Table storage design guidelines*. Available at:
https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-design-guidelines (Accessed: 13 September 2026).

Microsoft (2026) *azure-functions/dotnet-isolated*. Microsoft Artifact Registry. Available at:
https://mcr.microsoft.com/en-us/artifact/mar/azure-functions/dotnet-isolated (Accessed: 13 September 2026).

Docker Inc. (2026) *Multi-stage builds*. Docker Documentation. Available at:
https://docs.docker.com/build/building/multi-stage/ (Accessed: 13 September 2026).

Docker Inc. (2026) *Networking overview*. Docker Documentation. Available at:
https://docs.docker.com/engine/network/ (Accessed: 13 September 2026).

Docker Inc. (2026) *Multi-platform builds*. Docker Documentation. Available at:
https://docs.docker.com/build/building/multi-platform/ (Accessed: 13 September 2026).

Postman Inc. (2026) *Writing tests in Postman*. Postman Learning Center. Available at:
https://learning.postman.com/docs/writing-scripts/test-scripts/ (Accessed: 13 September 2026).

The Independent Institute of Education (2026) *Addendum: POE — CLDV6212/w*. School of Computer Science.

<!-- ═══════════ END ALL ═══════════ -->

---

## Repository Structure

```
.
├── CLDV6212_POE_Part1_AzureFunction/   # Azure Functions project
│   ├── Functions/                      # HTTP-triggered functions
│   ├── Models/                         # MenuItem entity
│   ├── Repositories/                   # Blob document repository
│   ├── Services/                       # Table storage service
│   ├── Dockerfile                      # Multi-stage container build
│   └── Program.cs                      # Host and DI registration
├── docs/
│   ├── CoffeeNChill.postman_collection.json
│   ├── CoffeeNChill.postman_environment.json
│   ├── Addendum_-_CLDV6212_POE.pdf
│   ├── ai-usage-log.md
│   └── meeting-minutes.md
├── CONTRIBUTING.md                     # Git workflow and shared-file protocol
├── .gitignore
├── .gitattributes
├── .dockerignore
└── README.md
```

## Contributing

See [`CONTRIBUTING.md`](./CONTRIBUTING.md) for branch naming, commit conventions,
the shared-file protocol and the review process.
