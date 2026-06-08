# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# restore (com cache de camada usando só os .csproj)
COPY ToaToa.sln ./
COPY ToaToa/ToaToa.csproj ToaToa/
COPY ToaToa.Client/ToaToa.Client.csproj ToaToa.Client/
RUN dotnet restore ToaToa.sln

# publish
COPY . .
RUN dotnet publish ToaToa/ToaToa.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN mkdir -p /app/Data
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ToaToa.dll"]
