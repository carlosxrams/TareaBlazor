using Microsoft.AspNetCore.Components.Web;
using WebEmpresa.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios de componentes Razor e Interactive Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Registrar HttpClient mediante AddHttpClient y registrar la instancia Scoped
builder.Services.AddHttpClient("WebAPI", client =>
{
    // Reemplaza con la URL base exacta donde corre tu API (asegúrate de incluir el puerto correcto)
    client.BaseAddress = new Uri("https://localhost:7084/");
});

// Permite inyectar @inject HttpClient directamente en la página Cliente.razor
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("WebAPI"));

var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();