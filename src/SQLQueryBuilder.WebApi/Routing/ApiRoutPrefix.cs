using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SQLQueryBuilder.WebApi.Routing;

public sealed class ApiRoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                var prefixRoute = new AttributeRouteModel(new RouteAttribute(prefix));
                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                    prefixRoute,
                    selector.AttributeRouteModel);
            }
        }
    }
}
