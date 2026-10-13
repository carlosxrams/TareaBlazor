using Microsoft.AspNetCore.Components.Web;
using WebEmpresa.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

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


app.Use(async (context, next) =>
{
    // Generar un nonce aleatorio por petición
    var bytes = new byte[32];
    using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
    {
        rng.GetBytes(bytes);
    }
    var nonce = Convert.ToBase64String(bytes);
    context.Items["CSPNonce"] = nonce;

    // Eliminar cabeceras CSP previas para evitar duplicados en el análisis de ZAP
    context.Response.Headers.Remove("Content-Security-Policy");

    // Aplicar la cabecera nosniff a TODAS las peticiones (incluyendo CSS, JS e imágenes)
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Política CSP con style-src 'self' (eliminado 'unsafe-inline')
    context.Response.Headers["Content-Security-Policy"] =
        $"default-src 'self'; " +
        $"script-src 'self' 'nonce-{nonce}' 'wasm-unsafe-eval'; " +
        $"style-src 'self'; " +
        $"img-src 'self' data:; " +
        $"connect-src 'self' ws://localhost:5230 wss://localhost:5230; " +
        $"form-action 'self'; " +
        $"frame-ancestors 'self'; " +
        $"object-src 'none'; " +
        $"base-uri 'self';";

    await next();
});


app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();