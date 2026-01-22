using ECC.DanceCup.UI.Auth;
using ECC.DanceCup.UI.Extensions;
using ECC.DanceCup.UI.ExternalServices.DanceCupApi;
using ECC.DanceCup.UI.ExternalServices.DanceCupAuth;
using ECC.DanceCup.UI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;

var webApplicationBuilder = WebApplication.CreateBuilder(args);

webApplicationBuilder.Services.AddDanceCupApiClient(webApplicationBuilder.Configuration);
webApplicationBuilder.Services.AddDanceCupAuthClient(webApplicationBuilder.Configuration);

webApplicationBuilder.Services.AddControllersWithViews();

webApplicationBuilder.Services.AddScoped<ICurrentUser, CurrentUser>();
webApplicationBuilder.Services.AddTransient<TokenWritingMiddleware>();
webApplicationBuilder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = webApplicationBuilder.Configuration["AuthOptions:ValidIssuer"],
            ValidateAudience = true,
            ValidAudience = webApplicationBuilder.Configuration["AuthOptions:ValidAudience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(webApplicationBuilder.Configuration["AuthOptions:Secret"] ?? string.Empty))
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

                if (context.Principal is not null)
                {
                    await currentUser.SignInAsync(context.Principal, context.HttpContext.RequestAborted);
                }
            }
        };
    });

webApplicationBuilder.Services.AddSession();

var webApplication = webApplicationBuilder.Build();

webApplication.UseHttpMetrics();

if (!webApplication.Environment.IsDevelopment())
{
    webApplication.UseExceptionHandler("/Home/Error");
    webApplication.UseHsts();
}

webApplication.UseSession();

webApplication.UseHttpsRedirection();
webApplication.UseStaticFiles();

webApplication.UseRouting();

webApplication.UseMiddleware<TokenWritingMiddleware>();

webApplication.UseAuthentication();
webApplication.UseAuthorization();

webApplication.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

webApplication.MapMetrics();

await webApplication.CheckExternalServicesHealthAsync();

await webApplication.RunAsync();
