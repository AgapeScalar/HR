var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<WebApplication3.Services.IBucketService, WebApplication3.Services.LocalBucketService>();
builder.Services.AddSingleton<WebApplication3.Services.ICandidateRepository, WebApplication3.Services.CandidateRepository>();
builder.Services.AddSingleton<WebApplication3.Services.IAnalysisService, WebApplication3.Services.LocalAnalysisService>();
builder.Services.AddSingleton<WebApplication3.Services.LocalAnalysisService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/ui/index.html");
});


app.Run();
