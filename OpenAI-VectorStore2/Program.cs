﻿var builder = WebApplication.CreateBuilder(args);

// Obtener la clave API de OpenAI desde appsettings.json
var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];

if (string.IsNullOrEmpty(openAiApiKey))
{
    throw new Exception("La clave de API de OpenAI no está configurada.");
}

builder.Services.AddSingleton(openAiApiKey);


// Agregar controladores
builder.Services.AddControllers();

// Habilitar CORS (opcional, pero necesario para el frontend)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Habilitar CORS y servir archivos estáticos
app.UseCors();
app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Mapea los controladores de API
    endpoints.MapFallbackToFile("index.html"); // Sirve un archivo HTLM como página de inicio si existe

});

//app.MapControllers();

app.Run();