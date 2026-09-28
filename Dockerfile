FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SQLQueryBuilder.Api/SQLQueryBuilder.Api.csproj SQLQueryBuilder.Api/
RUN dotnet restore SQLQueryBuilder.Api/SQLQueryBuilder.Api.csproj

COPY SQLQueryBuilder.Api/ SQLQueryBuilder.Api/
RUN dotnet publish SQLQueryBuilder.Api/SQLQueryBuilder.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "SQLQueryBuilder.Api.dll"]
