# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["IFormQualityApp.csproj", "."]
RUN dotnet restore "IFormQualityApp.csproj"
COPY . .
RUN dotnet publish "IFormQualityApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render free-tier instances have limited memory and a tiny /dev/shm.
# Server GC can segfault (exit 139) under those conditions, so force
# workstation GC and cap the heap to fit within the 512MB limit.
ENV DOTNET_gcServer=0
ENV DOTNET_GCHeapHardLimit=0x1C000000
ENV DOTNET_GCHeapCount=1
ENV DOTNET_EnableDiagnostics=0

ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
ENTRYPOINT ["dotnet", "IFormQualityApp.dll"]