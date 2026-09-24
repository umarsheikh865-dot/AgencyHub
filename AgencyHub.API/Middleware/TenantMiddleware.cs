namespace AgencyHub.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            const string tenantHeaderName = "X-Tenant-Id";
            string tenantId = "default-tenant";

            if (context.Request.Headers.TryGetValue(tenantHeaderName, out var extractedTenantId) && !string.IsNullOrEmpty(extractedTenantId))
            {
                tenantId = extractedTenantId!;
            }

            // Store TenantId in HttpContext items for EF Core DbContext to read
            context.Items["TenantId"] = tenantId;

            await _next(context);
        }
    }
}