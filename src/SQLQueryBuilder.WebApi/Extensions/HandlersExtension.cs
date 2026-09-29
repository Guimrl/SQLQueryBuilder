using SQLQueryBuilder.Application.Columns;
using SQLQueryBuilder.Application.Queries;
using SQLQueryBuilder.Application.Tables;

namespace SQLQueryBuilder.WebApi.Extensions;

public static class HandlersExtension
{
    public static void AddHandlers(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IGetTablesHandler, GetTablesHandler>();
        builder.Services.AddScoped<IGetColumnsHandler, GetColumnsHandler>();
        builder.Services.AddScoped<IGenerateQueryHandler, GenerateQueryHandler>();
    }
}
