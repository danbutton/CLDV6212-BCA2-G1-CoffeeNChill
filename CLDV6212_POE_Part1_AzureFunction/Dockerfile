# =============================================================================
#  CoffeeNChill Azure Functions — multi-stage container build
#
#  NOTE: the Azure Functions base images are published for linux/amd64 only,
#  so on Apple Silicon this must be built with --platform linux/amd64.
#  Docker Desktop runs it under emulation.
# =============================================================================

# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file alone first so the restore layer caches and only
# re-runs when dependencies change, not on every source edit.
COPY ["CLDV6212_POE_Part1_AzureFunction.csproj", "./"]
RUN dotnet restore "CLDV6212_POE_Part1_AzureFunction.csproj"

COPY . .
RUN dotnet publish "CLDV6212_POE_Part1_AzureFunction.csproj" \
    --configuration Release \
    --output /home/site/wwwroot \
    --no-restore

# ---------- Stage 2: runtime ----------
# Functions host only — no SDK, no source. Roughly a quarter the size.
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS final

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_URLS=http://+:80

EXPOSE 80

COPY --from=build /home/site/wwwroot /home/site/wwwroot
