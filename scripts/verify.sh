#!/usr/bin/env bash
# ============================================================================
#  CoffeeNChill — structure verification
#  Run from inside your repo folder at any time: bash verify.sh
#  Tells you exactly what is present, missing, or wrong.
# ============================================================================

PASS=0; FAIL=0; WARN=0
ok ()   { echo "  ✅ $1"; PASS=$((PASS+1)); }
bad ()  { echo "  ❌ $1"; FAIL=$((FAIL+1)); }
warn () { echo "  ⚠️  $1"; WARN=$((WARN+1)); }

echo "=============================================="
echo " CoffeeNChill — verification"
echo " $(date '+%Y-%m-%d %H:%M')"
echo "=============================================="
echo

# ---------------------------------------------------------------- tools
echo "--- 1. Tools ---"
if command -v dotnet >/dev/null 2>&1; then ok "dotnet $(dotnet --version)"
else bad "dotnet NOT installed — get .NET 8 SDK from dotnet.microsoft.com"; fi

if command -v func >/dev/null 2>&1; then ok "func $(func --version 2>/dev/null | head -1)"
else warn "func NOT installed (optional — see Step 1 for the workaround)"; fi

if command -v docker >/dev/null 2>&1; then
  if docker info >/dev/null 2>&1; then ok "docker running"
  else warn "docker installed but NOT running — open Docker Desktop"; fi
else bad "docker NOT installed"; fi

if command -v git >/dev/null 2>&1; then ok "git $(git --version | awk '{print $3}')"
else bad "git NOT installed"; fi
echo

# ---------------------------------------------------------------- repo
echo "--- 2. Repository ---"
if [ -d .git ]; then ok "in a git repo: $(pwd)"
else bad "NOT a git repo — cd into your repo folder"; echo; exit 1; fi

BR=$(git branch --show-current 2>/dev/null)
ok "current branch: $BR"

if git remote -v | grep -q origin; then ok "remote 'origin' configured"
else bad "no remote — git remote add origin <url>"; fi
echo

# ---------------------------------------------------------------- dotfiles
echo "--- 3. Root files ---"
for f in .gitignore .gitattributes .dockerignore README.md CONTRIBUTING.md; do
  [ -f "$f" ] && ok "$f" || bad "$f MISSING"
done
for f in gitignore gitattributes dockerignore; do
  [ -f "$f" ] && bad "$f exists WITHOUT its dot — rename it"
done
echo

# ---------------------------------------------------------------- ignore rules
echo "--- 4. Ignore rules working ---"
if grep -q "local.settings.json" .gitignore 2>/dev/null; then ok ".gitignore covers local.settings.json"
else bad ".gitignore does NOT cover local.settings.json"; fi

if grep -q "DS_Store" .gitignore 2>/dev/null; then ok ".gitignore covers .DS_Store"
else warn ".gitignore does not mention .DS_Store"; fi

if git ls-files | grep -q "local.settings.json$"; then bad "local.settings.json IS TRACKED — remove it"
else ok "local.settings.json not tracked"; fi

if git ls-files | grep -q "DS_Store"; then bad ".DS_Store IS TRACKED — git rm --cached"
else ok ".DS_Store not tracked"; fi
echo

# ---------------------------------------------------------------- folders
echo "--- 5. Folders ---"
for d in docs src; do
  [ -d "$d" ] && ok "$d/" || bad "$d/ MISSING"
done
for d in Models DTOs Repositories Functions Middleware Validation; do
  [ -d "src/$d" ] && bad "src/$d exists — WRONG LEVEL, should be src/CoffeeNChill.Functions/$d"
done
echo

# ---------------------------------------------------------------- project
echo "--- 6. Functions project ---"
FN="src/CoffeeNChill.Functions"
if [ -d "$FN" ]; then
  ok "$FN/"
  for f in CoffeeNChill.Functions.csproj Program.cs host.json Dockerfile local.settings.example.json; do
    [ -f "$FN/$f" ] && ok "$f" || bad "$f MISSING"
  done
  [ -f "$FN/local.settings.json" ] && ok "local.settings.json (local only)" \
    || warn "local.settings.json missing — cp from the example"

  echo "  -- code folders --"
  for d in Models DTOs Repositories Functions Middleware Validation; do
    [ -d "$FN/$d" ] && ok "$d/" || bad "$d/ MISSING"
  done

  echo "  -- skeleton files --"
  for f in Models/MenuItemEntity.cs \
           DTOs/CreateMenuItemRequest.cs DTOs/UpdateMenuItemRequest.cs DTOs/MenuItemResponse.cs \
           Repositories/IMenuRepository.cs Repositories/IDocumentRepository.cs \
           Repositories/MenuRepository.cs Repositories/BlobDocumentRepository.cs \
           Validation/MenuItemValidator.cs Middleware/ExceptionHandlingMiddleware.cs; do
    [ -f "$FN/$f" ] && ok "$(basename $f)" || bad "$f MISSING"
  done
else
  bad "$FN/ MISSING — the project has not been created yet"
fi
echo

# ---------------------------------------------------------------- docs
echo "--- 7. /docs ---"
echo "  -- REQUIRED by the brief --"
for f in CoffeeNChill.postman_collection.json CoffeeNChill.postman_environment.json; do
  [ -f "docs/$f" ] && ok "$f" || bad "$f MISSING"
done
echo "  -- optional --"
ls docs/*.pdf >/dev/null 2>&1 && ok "addendum PDF" || warn "addendum PDF (useful evidence, not required)"
ls docs/*.png >/dev/null 2>&1 && ok "architecture diagram" || warn "architecture diagram (A's task)"
[ -f "docs/meeting-minutes.md" ] && ok "meeting-minutes.md" || warn "meeting-minutes.md (optional)"
[ -f "docs/ai-usage-log.md" ] && ok "ai-usage-log.md" || warn "ai-usage-log.md (optional working file)"

echo "  -- README required sections --"
if grep -qi "AI Usage" README.md 2>/dev/null; then ok "AI Usage Declaration section present"
else bad "README has NO AI Usage Declaration — required by instruction 3"; fi
echo

# ---------------------------------------------------------------- build
echo "--- 8. Build ---"
if [ -f "$FN/CoffeeNChill.Functions.csproj" ] && command -v dotnet >/dev/null 2>&1; then
  if (cd "$FN" && dotnet build -v quiet --nologo >/tmp/build.log 2>&1); then
    ok "dotnet build SUCCEEDED"
  else
    bad "dotnet build FAILED — last lines:"
    tail -12 /tmp/build.log | sed 's/^/       /'
  fi
else
  warn "skipped (no project or no dotnet)"
fi
echo

# ---------------------------------------------------------------- commits
echo "--- 9. Commits ---"
if git rev-parse HEAD >/dev/null 2>&1; then
  echo "  Per author:"
  git shortlog -sn --all 2>/dev/null | sed 's/^/    /'
  GEN=$(git log --all --pretty=%s 2>/dev/null | grep -icE "^(fix|update|test|changes|asdf|wip)$")
  [ "$GEN" -gt 0 ] && warn "$GEN generic commit message(s) — these do not count" \
                   || ok "no generic commit messages"
else
  warn "no commits yet"
fi
echo

# ---------------------------------------------------------------- summary
echo "=============================================="
echo " Passed: $PASS   Failed: $FAIL   Warnings: $WARN"
echo "=============================================="
[ "$FAIL" -eq 0 ] && echo "🎉 Structure is correct." \
                  || echo "Fix the ❌ items above, then re-run."
