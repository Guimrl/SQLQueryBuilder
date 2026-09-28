FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/SQLQueryBuilder.Application/SQLQueryBuilder.Application.csproj SQLQueryBuilder.Application/
COPY src/SQLQueryBuilder.Infra/SQLQueryBuilder.Infra.csproj SQLQueryBuilder.Infra/
COPY src/SQLQueryBuilder.WebApi/SQLQueryBuilder.WebApi.csproj SQLQueryBuilder.WebApi/
RUN dotnet restore SQLQueryBuilder.WebApi/SQLQueryBuilder.WebApi.csproj

COPY src/SQLQueryBuilder.Application/ SQLQueryBuilder.Application/
COPY src/SQLQueryBuilder.Infra/ SQLQueryBuilder.Infra/
COPY src/SQLQueryBuilder.WebApi/ SQLQueryBuilder.WebApi/
RUN dotnet publish SQLQueryBuilder.WebApi/SQLQueryBuilder.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "SQLQueryBuilder.WebApi.dll"]
