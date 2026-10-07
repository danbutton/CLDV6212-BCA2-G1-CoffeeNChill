# CoffeeNChill — Canteen Management System

**CLDV6212 Cloud Development B · Portfolio of Evidence Part 1 · Group G1**
The Independent Institute of Education (Emeris University)

---

## About

CoffeeNChill canteen is upgrading to Azure cloud services. Real-time menu data
will use Azure Table Storage, while staff documents live on Azure Blob Storage for
remote access. These features run as HTTP-triggered Azure Functions in Docker
containers, integrated with a local storage emulator for development.

---

## Demo

**[Watch the demonstration](https://youtu.be/U1IrvKKaiac)**

Covers the architecture, the data model, every endpoint, the containerised
application, the published Docker images, and the automated test suite.

---

## Setup

### 1. Install

| Tool | Purpose |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | Builds the project |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | Runs the app and the storage emulator |
| [Postman](https://www.postman.com/downloads/) | Runs the API tests |
| [Azure Storage Explorer](https://azure.microsoft.com/features/storage-explorer/) | Inspects the stored data |
| Git | Clones the repository |

Check they work:

```bash
dotnet --version
docker info
git --version
```

`dotnet` should report 10.0.x, and `docker info` must not error — Docker Desktop
needs to be running.

Azure Functions Core Tools is **not** required. The container supplies the
Functions host.

### 2. Clone and build

```bash
git clone https://github.com/EMWCCN/cldv6212-2026-g1-part-1-danbutton.git
cd cldv6212-2026-g1-part-1-danbutton/CLDV6212_POE_Part1_AzureFunction
dotnet build
```

You should see `Build succeeded`.

---

## Running the system

### 1. Create the network

```bash
docker network create coffeenchill-net
```

Two containers need to find each other and Part 1 doesn't permit Docker Compose.
A user-defined network gives them name resolution through Docker's embedded DNS.

### 2. Start the storage emulator

```bash
docker run -d \
  --name azurite \
  --network coffeenchill-net \
  -p 10000:10000 -p 10001:10001 -p 10002:10002 \
  -v azurite-data:/data \
  mcr.microsoft.com/azure-storage/azurite \
  azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0 --location /data
```

The `0.0.0.0` host flags are required — by default Azurite binds only to its own
container loopback and nothing outside can reach it.

Ports are **10000 Blob, 10001 Queue, 10002 Table**.

### 3. Build the application image

```bash
cd CLDV6212_POE_Part1_AzureFunction
docker build --platform linux/amd64 -t danbutton/coffeenchill-functions:v1.0 .
```

The Dockerfile builds in two stages. The .NET SDK image compiles and publishes,
then only the published output is copied into the Azure Functions runtime image,
so the SDK never ships. The project file is restored before the source is copied,
which lets Docker cache the dependency layer and only recompile when code
actually changes.

`--platform linux/amd64` is required on Apple Silicon — the Azure Functions base
images are published for Intel only, so Docker runs them under emulation.

### 4. Run the application

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

Confirm the routes loaded:

```bash
docker logs coffeenchill-functions | grep "Mapped function route"
```

Eight routes should be listed.

The connection string is written out in full rather than using
`UseDevelopmentStorage=true`, because that shorthand resolves to `127.0.0.1` —
which inside the Functions container refers to that container, not Azurite.
Pointing at the hostname `azurite` works because both containers share the
network. The account key shown is Azurite's published development key and is not
a secret.

### Stopping

```bash
docker rm -f coffeenchill-functions azurite
docker volume rm azurite-data
docker network rm coffeenchill-net
```

---

## API

| Method | Route | Returns |
|---|---|---|
| POST | `/api/menu` | 201 · 400 on invalid input |
| GET | `/api/menu` | 200 — all items |
| GET | `/api/menu/category/{category}` | 200 · 404 if the category is empty |
| PUT | `/api/menu/{category}/{id}` | 200 · 400 on invalid input · 404 if missing |
| DELETE | `/api/menu/{category}/{id}` | 204 · 404 if missing |
| POST | `/api/documents/upload` | 200 · 400 if no file supplied |
| GET | `/api/documents` | 200 — all documents |
| GET | `/api/documents/download/{fileName}` | 200 · 404 if missing |

Menu entities use their storage keys directly: `PartitionKey` is the category
and `RowKey` is the SKU. Partitioning by category means filtering a whole
category is a single-partition query, which is the cheapest read Table Storage
offers.

### Try it

**Add a menu item**

```bash
curl -X POST http://localhost:7071/api/menu \
  -H "Content-Type: application/json" \
  -d '{"PartitionKey":"Hot Drinks","RowKey":"COF-001","Name":"Espresso","Description":"Double shot, locally roasted","Price":28.50,"IsAvailable":true}'
```

**List everything**

```bash
curl http://localhost:7071/api/menu
```

**Filter by category**

```bash
curl "http://localhost:7071/api/menu/category/Hot%20Drinks"
```

**Update an item**

```bash
curl -X PUT "http://localhost:7071/api/menu/Hot%20Drinks/COF-001" \
  -H "Content-Type: application/json" \
  -d '{"Name":"Espresso","Description":"Double shot, locally roasted","Price":32.00,"IsAvailable":false}'
```

**Delete an item**

```bash
curl -X DELETE "http://localhost:7071/api/menu/Hot%20Drinks/COF-001"
```

**Upload a document**

```bash
echo "Barista recipe sheet" > recipe.txt
curl -X POST http://localhost:7071/api/documents/upload -F "file=@recipe.txt"
```

**List documents**

```bash
curl http://localhost:7071/api/documents
```

**Download a document**

```bash
curl -O -J http://localhost:7071/api/documents/download/recipe.txt
```

---

## Testing

The Postman collection and environment are in [`/docs`](./docs). Import both,
select the **CoffeeNChill — Local (Azurite)** environment, and run the collection.

From the command line:

```bash
npm install -g newman
newman run docs/CoffeeNChill.postman_collection.json \
  -e docs/CoffeeNChill.postman_environment.json
```

Thirteen requests across two folders, with **39 assertions, all passing**. Every
request checks status code, response shape and response time. Alongside the
success paths, the suite tests failure cases explicitly: an invalid payload
returns 400 with a validation message, updating a missing item returns 404, and
requesting a document that doesn't exist returns 404.

All URLs are built from a `{{baseUrl}}` environment variable, so the same
collection runs unchanged against a locally hosted app or the container.

Two notes when re-running: the upload request needs a file attached manually in
the form-data body, since Postman does not persist file paths across an export;
and the suite creates `COF-001`, `COF-002` and `PAS-104` itself, so those three
must be absent before a run.

---

## Docker images

Both images are published publicly with semantic version tags.

| Image | |
|---|---|
| [`danbutton/coffeenchill-functions:v1.0`](https://hub.docker.com/r/danbutton/coffeenchill-functions) | The application |
| [`danbutton/coffeenchill-azurite:v1.0`](https://hub.docker.com/r/danbutton/coffeenchill-azurite) | Storage emulator |

```bash
docker pull danbutton/coffeenchill-functions:v1.0
docker pull danbutton/coffeenchill-azurite:v1.0
```

Each published image was verified by deleting it locally and pulling it back
down, confirming that what is on Docker Hub is the image that runs.

---

## Project status

**Working now**

- Menu items stored in Azure Table Storage, partitioned by category
- Create, list, filter by category, update and delete
- Staff documents stored in Azure Blob Storage with MIME validation
- Upload, list and download endpoints
- All eight endpoints running in a Docker container against Azurite
- Automated Postman suite — 39 assertions, all passing
- Both images published publicly to Docker Hub

**Where we are**

Part 1 of 3 complete. The storage layer and HTTP API work end to end,
containerised and published.

**Planned**

- Docker Compose orchestration
- Queue-based order processing
- A front end consuming the API
- Deployment to a hosted environment
- CI/CD pipeline

---

## Deviations from the brief

**Azurite does not emulate Azure Files.** The emulator supports Blob, Queue and
Table only. Staff documents are therefore stored in Azure Blob Storage, which the
module addendum subsequently confirmed.

**The Azurite port mapping in the brief is incorrect.** The actual assignment is
10000 Blob, 10001 Queue, 10002 Table.

**`coffeenchill-Azurite` is not a valid Docker Hub tag.** Repository names must
be lowercase, so the emulator image is published as `coffeenchill-azurite:v1.0`.

**Azure Table Storage has no decimal type.** Prices are stored as `double`.

**Azure Functions base images are published for linux/amd64 only.** Builds on ARM
hardware require `--platform linux/amd64`, which Docker satisfies through
emulation.

---

## Team

| Member | Student No. | Contribution |
|---|---|---|
| Kyle | ST10473747 | Menu item model, Table Storage service, five menu endpoints, MIME validation and error logging |
| Evan | ST10482786 | Menu endpoint adjustments, GET classes, validation |
| Saveer Singh | ST10487403 | Blob Storage repository, upload, list and download endpoints |
| Daniel Button | ST10491642 | Multi-stage Dockerfile, Docker Hub publishing, Postman test suite, project consolidation, documentation |

Individual contributions are visible in the commit history and in each member's
branch.

---

## AI usage

AI tools were used during this project and are disclosed here as required by the
assessment instructions.

**Claude (Anthropic)** was used to review the Dockerfile and repository
structure, to help identify errors in the brief — Azurite's lack of Azure Files
support, the incorrect port mapping and the invalid Docker Hub tag — and to
assist in debugging container-to-container networking, the Intel-only platform
constraint on Apple Silicon, and a dependency-injection registration that was
lost when two parallel projects were merged into one.

All design decisions, the storage schema and the validation logic were determined
and implemented by group members. Every suggestion was tested against the running
application before being kept.

Chat transcript: ADD_YOUR_CLAUDE_SHARE_LINK_HERE

---

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

---

## Repository layout

```
CLDV6212_POE_Part1_AzureFunction/   Azure Functions project
  Functions/                        HTTP-triggered endpoints
  Models/                           MenuItem entity
  Repositories/                     Blob document repository
  Services/                         Table storage service
  Dockerfile                        Multi-stage container build
  Program.cs                        Host and dependency injection
docs/                               Postman collection, environment, addendum
```
