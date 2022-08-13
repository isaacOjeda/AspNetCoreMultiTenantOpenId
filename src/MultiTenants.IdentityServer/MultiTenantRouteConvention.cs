using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MultiTenants.IdentityServer;
public class MultiTenantRouteConvention : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        foreach (var selector in model.Selectors.ToList())
        {
            selector.AttributeRouteModel.Template = "{tenant}/" + selector.AttributeRouteModel.Template;
        }
    }
}