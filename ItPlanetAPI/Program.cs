using ItPlanetAPI;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddScoped<AuthorizedUser>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo {Title = "Demo API", Version = "v1"});
    option.OperationFilter<OpenApiParameterIgnoreFilter>();
    option.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Basic HTTP Authentication",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<DatabaseContext>(options =>
    {
        const string defaultStr = "Server=localhost,1433;Database=chipping;Persist Security Info=True; Encrypt=False;User ID=sa;Password=YourStrong@Passw0rd;Trust Server Certificate=True;";
        
        var str = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONNECTION_STRING"))
            ? defaultStr
            :Environment.GetEnvironmentVariable("CONNECTION_STRING");
        
        using var ctx = new DatabaseContext(options.UseSqlServer(str).Options);
        ctx.Database.EnsureCreated();
        
        //ctx.Database.EnsureDeleted();
    }
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();