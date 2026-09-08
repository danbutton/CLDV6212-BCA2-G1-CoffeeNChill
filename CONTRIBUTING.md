# Shared Files & Merge Protocol
### `/docs/CONTRIBUTING.md` 

---

## The core idea

> **Fill the shared files in completely during the contract sprint — including registrations, packages and config keys for code that doesn't exist yet.**

Commit a skeleton that *compiles*, with every class stubbed out to throw `NotImplementedException`. After that, each person only ever replaces the body of a file they own. The shared files are already finished and nobody has to touch them again.

---

## The eight shared files

| File | Who needs it | Risk | Protocol |
|---|---|---|---|
| `Program.cs` | A, B, C | 🔴 High | Pre-fill all DI registrations day one |
| `CoffeeNChill.Functions.csproj` | A, C, D | 🔴 High | Pre-add all NuGet packages day one |
| `local.settings.example.json` | A, C, D | 🟡 Medium | Pre-add all config keys day one |
| `README.md` | Everyone | 🟡 Medium | Section ownership markers |
| `CoffeeNChill.postman_collection.json` | D only | 🔴 High | **Single owner. Never co-edit.** |
| `host.json` | D | 🟢 Low | D owns, rarely changes |
| `.gitignore` / `.dockerignore` | D | 🟢 Low | D owns |
| `/docs/meeting-minutes.md` + `ai-usage-log.md` | Everyone | 🟡 Medium | Append-only + union merge |
| **PoE submission document (Word)** | Everyone | 🔴 High | **Keep it out of git entirely** |

---

## 1. `Program.cs` — commit this exact file on day one

Every registration present from the start. Nobody touches it again.

```csharp
using CoffeeNChill.Functions.Middleware;
using CoffeeNChill.Functions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ── SHARED FILE ──────────────────────────────────────────────────────────
// All DI registrations were added during the contract sprint. Do NOT edit
// this file without announcing it in the group chat first.
// ─────────────────────────────────────────────────────────────────────────

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<ExceptionHandlingMiddleware>();   // owner: B
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<IMenuRepository, MenuRepository>();               // owner: A
        services.AddSingleton<IDocumentRepository, FileShareDocumentRepository>(); // owner: C
    })
    .Build();

host.Run();
```

For this to compile before anyone has written anything, also commit **stub classes** during the sprint:

```csharp
// Repositories/MenuRepository.cs — STUB, owner: A
public class MenuRepository : IMenuRepository
{
    public Task<MenuItemEntity?> GetAsync(string category, string sku, CancellationToken ct = default)
        => throw new NotImplementedException();

    // ... one stub per interface method
}
```

Now: `dotnet build` succeeds, `func start` runs, and A/B/C each fill in only their own file. `Program.cs` is never edited again.

---

## 2. `.csproj` — pre-add every package

XML merge conflicts are the ugliest kind. Add all packages on day one and **keep them alphabetised** — if a conflict does happen, alphabetical order makes "keep both lines" obviously correct.

```xml
<ItemGroup>
  <PackageReference Include="Azure.Data.Tables" Version="12.9.1" />
  <PackageReference Include="Azure.Storage.Files.Shares" Version="12.20.0" />
  <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="1.23.0" />
  <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http" Version="3.2.0" />
  <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore" Version="1.3.2" />
  <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="1.18.1" />
</ItemGroup>
```

**Rule:** if you genuinely need a new package later, announce it, add it alone in a one-line PR, merge it immediately, everyone pulls. Never bundle a package addition into a feature PR.

---

## 3. `local.settings.example.json` — every key, day one

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "TableStorageConnection": "UseDevelopmentStorage=true",
    "FileShareConnection": "<paste-your-azure-storage-connection-string>",
    "MenuTableName": "MenuItems",
    "StaffDocsShareName": "staff-docs"
  }
}
```

Commit this. Each person copies it to `local.settings.json` (gitignored) and fills in their own secrets.

🔴 **`local.settings.json` must never be committed.** A real `AccountKey` in git history is a live credential leak and a conduct problem. Confirm it's in `.gitignore` before the first push.

---

## 4. `README.md` — section ownership markers

Markdown merges cleanly *if people stay in their own region*. Commit the skeleton with HTML-comment markers on day one:

```markdown
<!-- ═══ OWNER: A — architecture & data model ═══ -->
## Architecture Overview
<!-- ═══ END A ═══ -->

<!-- ═══ OWNER: B — API endpoints ═══ -->
## API Endpoint Reference
<!-- ═══ END B ═══ -->

<!-- ═══ OWNER: C — file storage & deviations ═══ -->
## Staff Document Storage
## Known Deviations from the Brief
<!-- ═══ END C ═══ -->

<!-- ═══ OWNER: D — setup, docker, testing ═══ -->
## Prerequisites
## Local Setup
## Standalone Docker Execution
## Docker Hub Images
## Postman Collection
<!-- ═══ END D ═══ -->

<!-- ═══ ALL — one row each, do not touch other rows ═══ -->
## Team Contributions
## AI Usage Declaration
## References
<!-- ═══ END ALL ═══ -->
```

Git merges non-overlapping regions of a text file automatically. Stay inside your markers and you will never conflict.

For the contributions table: **edit only your own row.** Four people each adding one row to different lines merges fine; two people rewriting the whole table does not.

---

## 5. Postman collection — single owner, no exceptions

**D is the only person who ever opens this file.**

A Postman export is machine-generated JSON with internal GUIDs, ordering and escaped script strings. Two people editing it produces a conflict that is realistically unresolvable — you will end up discarding someone's work.

**How the others contribute:** post the request spec in the group chat, D adds it.

```
@D new endpoint ready:
PUT /api/menu/{category}/{id}
Body: { "price": 32.00, "isAvailable": false }
Expected: 200 with updated item, 404 if SKU missing, 400 if price <= 0
```

If you must parallelise: each person exports their own folder as a *separate* collection, D imports and merges once at the end. Slower, but safe.

---

## 6. Meeting minutes & AI log — union merge

These are append-only, so tell git to keep both sides instead of conflicting. Create `.gitattributes` in the repo root:

```gitattributes
docs/meeting-minutes.md   merge=union
docs/ai-usage-log.md      merge=union
```

Union merge keeps both versions of a conflicting region rather than stopping. Perfect for logs. **Never apply it to code** — it would silently keep both sides of a real conflict.

Also: **add new entries at the top, not the bottom.** Four people appending to the last line of a file all conflict on that same line; prepending spreads them out.

---

## 7. The PoE Word document — keep it out of git

`.docx` is a binary zip. Git cannot merge it. Two people editing it means one person's work is destroyed.

**Use a Google Doc.** Real multi-author editing, full revision history showing who wrote what, comments for review. Export to `.docx` once at the very end and attach that to the LMS submission.

Put the link in the pinned WhatsApp message, not in the repo.

Same applies to the architecture diagram: use draw.io (`.drawio` files are XML and diff reasonably) or Figma, then export a PNG to `/docs/architecture.png`. Only the exported PNG goes in the repo.

---

## 8. The lock protocol — for the rare later edit

When someone genuinely must change a shared file after the sprint:

```
🔒 Taking Program.cs — adding a registration. Nobody touch it.
```

→ make the change alone, in a PR containing nothing else
→ get it reviewed and merged within the hour
→ then:

```
🔓 Program.cs released and merged. Everyone `git pull` now.
```

Small scope, fast merge, immediate broadcast. The danger isn't the edit — it's the edit sitting unmerged for two days while three people build on stale code.

---

## Conflict cheat sheet (for when it happens anyway)

**Golden rule: pull before you start work, every single day.** Most conflicts are just stale branches.

```bash
git checkout main && git pull
git checkout my-branch
git merge main          # resolve now, on your branch, not in the PR
```

**`.csproj` conflict** → almost always keep both `<PackageReference>` lines, re-alphabetise, `dotnet restore`.

**`Program.cs` conflict** → keep both registration lines. The order of `AddSingleton` calls doesn't matter.

**`README.md` conflict** → you edited outside your markers. Keep both sections, put your text back inside your own region.

**Postman collection conflict** → this shouldn't have happened. `git checkout --theirs` to take D's version, then D re-adds the lost requests from the chat spec.

**Genuinely stuck?**
```bash
git merge --abort
```
Post the conflict in the group chat. Do not force-push over someone's work to make the error go away — it's recoverable until someone does that.

---

## Day-one checklist

Before anyone leaves the kickoff call:

- [ ] `Program.cs` committed with all three registrations
- [ ] Stub classes for `MenuRepository`, `FileShareDocumentRepository`, `ExceptionHandlingMiddleware` — all throwing `NotImplementedException`
- [ ] `.csproj` has all six packages, alphabetised
- [ ] `local.settings.example.json` has all six keys
- [ ] `local.settings.json` confirmed in `.gitignore`
- [ ] `README.md` skeleton with ownership markers
- [ ] `.gitattributes` with union-merge rules
- [ ] Google Doc created for the PoE document, link pinned in WhatsApp
- [ ] **`dotnet build` succeeds and `func start` runs** on all four machines

That last line is the real test. If the skeleton compiles for everyone before anyone writes a feature, the rest of the project is four people working in parallel instead of four people waiting on each other.
