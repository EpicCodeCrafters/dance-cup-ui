using ECC.DanceCup.UI.Auth;
using ECC.DanceCup.UI.Extensions;
using ECC.DanceCup.UI.ExternalServices.DanceCupApi;
using ECC.DanceCup.UI.ExternalServices.DanceCupAuth;
using ECC.DanceCup.UI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;
using Serilog.Sinks.Kafka;

const string serviceName = "dance-cup-ui";

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

webApplicationBuilder.Services.AddSerilog(loggerConfiguration => loggerConfiguration
    .ReadFrom.Configuration(webApplicationBuilder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", serviceName)
    .Enrich.WithProperty("Environment", webApplicationBuilder.Environment.EnvironmentName)
    .WriteTo.Kafka(
        bootstrapServers: webApplicationBuilder.Configuration["KafkaOptions:BootstrapServers"],
        topic: webApplicationBuilder.Configuration["KafkaOptions:Topics:DanceCupLogs:Name"]
    )
    .WriteTo.Console()
);

var webApplication = webApplicationBuilder.Build();

if (!webApplication.Environment.IsDevelopment())
{
    webApplication.UseExceptionHandler("/Home/Error");
    webApplication.UseHsts();
}

webApplication.UseSerilogRequestLogging();

webApplication.UseSession();

webApplication.UseHttpsRedirection();
webApplication.UseStaticFiles();

webApplication.UseRouting();

webApplication.UseHttpMetrics();

webApplication.UseMiddleware<TokenWritingMiddleware>();

webApplication.UseAuthentication();
webApplication.UseAuthorization();

webApplication.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

webApplication.MapMetrics();

await webApplication.CheckExternalServicesHealthAsync();

try
{
    await webApplication.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}