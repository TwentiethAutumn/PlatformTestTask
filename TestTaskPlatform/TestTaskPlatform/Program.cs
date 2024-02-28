using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using TestTaskPlatform;
using TestTaskPlatform.Context;
using TestTaskPlatform.Hubs;
using TestTaskPlatform.Middleware;
using TestTaskPlatform.Repositories;
using TestTaskPlatform.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(opt =>  
{  
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "TicTacToe", Version = "v1" });

    opt.AddSignalRSwaggerGen();
    
    opt.AddSecurityDefinition("basic", new OpenApiSecurityScheme  
    {  
        Name = "Authorization",  
        Type = SecuritySchemeType.Http,  
        Scheme = "basic",  
        In = ParameterLocation.Header,  
        Description = "Basic Authorization header using the Bearer scheme."  
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement  
    {  
        {  
            new OpenApiSecurityScheme  
            {  
                Reference = new OpenApiReference  
                {  
                    Type = ReferenceType.SecurityScheme,  
                    Id = "basic"  
                }  
            },  
            new string[] {}  
        }  
    });
});
builder.Services.AddCors();
builder.Services.AddDbContext<ApiContext>(
    options =>
    {
        options.UseInMemoryDatabase("ticTacToe");
    }, ServiceLifetime.Scoped);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR().AddNewtonsoftJsonProtocol(options => {
    options.PayloadSerializerSettings.TypeNameHandling = TypeNameHandling.All;
});
;
builder.Services.AddSingleton<GameState<Lobby>>();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Lobby>("/lobby");

app.Run();