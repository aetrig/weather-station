using weatherStationWeb.Components;
using weatherStationWeb.Hubs;
using weatherStationWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();



builder.Services.AddHostedService<MqttInfluxService>();
// builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttInfluxService>());
builder.Services.AddSignalR();
builder.Services.AddSingleton<InfluxQueryService>();

var app = builder.Build();

// app.MapHub<mqttHub>("/Hub");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<mqttHub>("/mqttHub");

app.Run();
