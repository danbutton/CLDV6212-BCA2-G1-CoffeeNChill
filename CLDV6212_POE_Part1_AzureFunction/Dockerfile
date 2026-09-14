# =============================================================================
#  CoffeeNChill Azure Functions — multi-stage container build
#
#  NOTE: the Azure Functions base images are published for linux/amd64 only,
#  so on Apple Silicon this must be built with --platform linux/amd64.
#  Docker Desktop runs it under emulation.
# =============================================================================

# Multi-stage build: the SDK image compiles and publishes, then only the
# published output is copied into the Functions runtime image. The SDK layer
# is discarded, substantially reducing the shipped image size [7].
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# The project file is copied and restored before the source so Docker caches
# the dependency layer and only re-restores when packages change [7].
COPY ["CLDV6212_POE_Part1_AzureFunction.csproj", "./"]
RUN dotnet restore "CLDV6212_POE_Part1_AzureFunction.csproj"

COPY . .
RUN dotnet publish "CLDV6212_POE_Part1_AzureFunction.csproj" \
    --configuration Release \
    --output /home/site/wwwroot \
    --no-restore

# The Azure Functions base images are published for linux/amd64 only, so
# builds on ARM hosts require --platform linux/amd64 [5], [9].
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS final

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_URLS=http://+:80

EXPOSE 80

COPY --from=build /home/site/wwwroot /home/site/wwwroot
