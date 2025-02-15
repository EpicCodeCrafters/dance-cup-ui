namespace ECC.DanceCup.UI.Middlewares;

public class TokenWritingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = context.Session.GetString("token");
        
        if (string.IsNullOrWhiteSpace(token) is false)
        {
            context.Request.Headers.Append("Authorization", "Bearer " + token);
        }
        
        await next(context);
    }
}