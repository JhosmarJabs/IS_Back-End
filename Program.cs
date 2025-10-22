using MiWebService.Data;
using MiWebService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("ConexionServidor")!;

builder.Services.AddScoped<EmpresasDatos>(_ => new EmpresasDatos(connectionString));
builder.Services.AddScoped<PersonaDatos>(_ => new PersonaDatos(connectionString));
builder.Services.AddSingleton<MemoriaPersonas>(provider => 
    new MemoriaPersonas(new PersonaDatos(connectionString)));

builder.Services.AddHostedService<Principal>(provider => 
    new Principal(connectionString));

/* builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(origin =>
            origin.StartsWith("http://localhost") ||
            origin.StartsWith("http://127.0.0.1") ||
            origin.StartsWith("http://192.168.15.135") ||
            origin.StartsWith("file://"))
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.WebHost.UseUrls(); */

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()); 
});

builder.WebHost.UseUrls("http://*:5075");

builder.WebHost.ConfigureKestrel(serverOptions =>
    serverOptions.ListenAnyIP(5075));

var app = builder.Build();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();