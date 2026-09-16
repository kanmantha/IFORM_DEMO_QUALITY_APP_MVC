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

# Render free-tier instances have limited memory (512MB) and a tiny /dev/shm.
# Server GC can segfault (exit 139) under those conditions, so force
# workstation GC, cap the heap well below the instance limit, and disable
# tiered compilation to keep JIT memory and peak working set small.
ENV DOTNET_gcServer=0
ENV DOTNET_GCHeapHardLimit=0x10000000
ENV DOTNET_GCHeapCount=1
ENV DOTNET_TieredCompilation=0
ENV DOTNET_EnableDiagnostics=0

ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
ENTRYPOINT ["dotnet", "IFormQualityApp.dll"]