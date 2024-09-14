var builder = WebApplication.CreateBuilder(args);

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
app.MapControllers();
app.Run();