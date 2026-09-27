namespace AgencyHub.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantId =
                context.User
                    .FindFirst("tenantId")
                    ?.Value;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                context.Response.StatusCode = 401;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        message =
                            "Tenant information is missing."
                    });

                return;
            }

            if (!Guid.TryParse(
                tenantId,
                out _))
            {
                context.Response.StatusCode = 401;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        message =
                            "Invalid tenant information."
                    });

                return;
            }

            context.Items["TenantId"] =
                tenantId;
        }

        await _next(context);
    }
}