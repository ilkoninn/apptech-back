using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppTech.Business.Helpers
{
    public class RedirectFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId == "Auth_Login")
            {
                operation.Responses.Add("302", new OpenApiResponse { Description = "Redirect" });
                operation.Extensions.Add("x-redirect-url", new OpenApiString("/api/Auth/google-response"));
            }
        }
    }
}
