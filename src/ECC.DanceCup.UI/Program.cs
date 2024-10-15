using ECC.DanceCup.UI.Extensions;
using ECC.DanceCup.UI.ExternalServices.DanceCupApi;
using ECC.DanceCup.UI.ExternalServices.DanceCupAuth;

var webApplicationBuilder = WebApplication.CreateBuilder(args);

webApplicationBuilder.Services.AddDanceCupApiClient(webApplicationBuilder.Configuration);
webApplicationBuilder.Services.AddDanceCupAuthClient(webApplicationBuilder.Configuration);

webApplicationBuilder.Services.AddControllersWithViews();

var webApplication = webApplicationBuilder.Build();

if (!webApplication.Environment.IsDevelopment())
{
    webApplication.UseExceptionHandler("/Home/Error");
    webApplication.UseHsts();
}

webApplication.UseHttpsRedirection();
webApplication.UseStaticFiles();

webApplication.UseRouting();

webApplication.UseAuthorization();

webApplication.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

await webApplication.CheckExternalServicesHealthAsync();

await webApplication.RunAsync();
