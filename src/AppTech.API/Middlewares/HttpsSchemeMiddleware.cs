namespace AppTech.API.Middlewares
{
    public class HttpsSchemeMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpsSchemeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Scheme != "https")
            {
                context.Request.Scheme = "https";
            }

            await _next(context);
        }
    }
}
