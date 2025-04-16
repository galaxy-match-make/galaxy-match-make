namespace galaxy_match_make.Services;

public class MiddlewareService
{
    private readonly RequestDelegate _next;

    public MiddlewareService(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, GoogleAuthService googleAuthService)
    {
        try
        {

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader?.StartsWith("Bearer ") == true)
            {
                var token = authHeader["Bearer ".Length..].Trim();

                try
                {
                    var principal = await googleAuthService.ValidateGoogleJwtAndCreatePrincipal(token);
                    context.User = principal;
                }
                catch
                {
                }
            }

            await _next(context);
        }
        catch
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("You are not authorized to access this resource.");
        }
    }
}