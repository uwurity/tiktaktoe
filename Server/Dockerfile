FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["tiktaktoe.Server/tiktaktoe.Server.csproj", "tiktaktoe.Server/"]
RUN dotnet restore "tiktaktoe.Server/tiktaktoe.Server.csproj"
COPY . .
WORKDIR "/src/tiktaktoe.Server"
RUN dotnet build "tiktaktoe.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "tiktaktoe.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "tiktaktoe.Server.dll"]
