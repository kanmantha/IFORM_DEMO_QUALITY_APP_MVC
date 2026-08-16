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
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
ENTRYPOINT ["dotnet", "IFormQualityApp.dll"]
