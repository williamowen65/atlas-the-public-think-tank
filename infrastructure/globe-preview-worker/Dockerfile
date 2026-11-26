FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /source
COPY src ./src
RUN dotnet publish src/hosts/Atlas.Api/Atlas.Api.csproj -c Release -r linux-x64 --self-contained -o /worker

# Keep this version aligned with the Microsoft.Playwright package in Atlas.Api.
FROM mcr.microsoft.com/playwright/dotnet:v1.58.0-noble
RUN apt-get update \
    && apt-get install -y --no-install-recommends gifsicle \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /worker
COPY --from=build /worker .
ENTRYPOINT ["./Atlas.Api", "--publish-globe-previews"]
