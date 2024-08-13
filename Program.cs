using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TranskriptTest.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true; // Optional: for better readability in debugging
}).AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
var connectionString = builder.Configuration.GetConnectionString("Dev");
builder.Services.AddDbContext<VideoDbContext>(opt =>
{
    opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    opt.EnableSensitiveDataLogging(true);
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.PhysicalPath;
        if (path.EndsWith(".vtt"))
        {
            ctx.Context.Response.ContentType = "text/vtt";
        }
    }
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "subtitles",
    pattern: "Subtitles/{fileName}",
    defaults: new { controller = "Home", action = "GetSubtitle" }
);

app.Run();
