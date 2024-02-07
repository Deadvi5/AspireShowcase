using WeatherApp.Web;
using WeatherApp.Web.Components;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddRedisOutputCache("outputCache");

builder.Services.AddHttpClient<WeatherApiClient>(
    static client => client.BaseAddress = new Uri("http://weatherapi"));

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseOutputCache();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
