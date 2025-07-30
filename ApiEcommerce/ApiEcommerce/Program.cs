using System.Text;
using ApiEcommerce.Constants;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));

// Para habilitar caché.
builder.Services.AddResponseCaching(options =>
{
  options.MaximumBodySize = 1024 * 1024;
  options.UseCaseSensitivePaths = true;
});

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if (string.IsNullOrEmpty(secretKey)) throw new InvalidOperationException("La SecretKey no está definida.");

builder.Services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(options =>
    {
      options.RequireHttpsMetadata = false;
      options.SaveToken = true;
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false
      };
    }
);

builder.Services.AddControllers(options =>
{
  options.CacheProfiles.Add(CacheProfiles.Default10, CacheProfiles.Profile10);
  options.CacheProfiles.Add(CacheProfiles.Default20, CacheProfiles.Profile20);
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
  {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                    "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                    "Ejemplo: \"12345abcdef\"",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header
        },
        new List<string>()
      }
    });

    options.SwaggerDoc("v1", new OpenApiInfo
    {
      Version = "v1",
      Title = "API Ecommerce",
      Description = "API para gestionar productos y usuarios.",
      TermsOfService = new Uri("http://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "FSociety",
        Url = new Uri("http://github.com/cjavat")
      },
      License = new OpenApiLicense
      {
        Name = "Licencia de uso",
        Url = new Uri("http://example.com/license")
      }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
      Version = "v2",
      Title = "API Ecommerce v2",
      Description = "API para gestionar productos y usuarios.",
      TermsOfService = new Uri("http://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "FSociety",
        Url = new Uri("http://github.com/cjavat")
      },
      License = new OpenApiLicense
      {
        Name = "Licencia de uso",
        Url = new Uri("http://example.com/license")
      }
    });
  }

);

// Habilitar versionamiento de API
var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
  option.AssumeDefaultVersionWhenUnspecified = true;
  option.DefaultApiVersion = new ApiVersion(1.0);
  option.ReportApiVersions = true;
  // option.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version")); // /?api-version 
});
apiVersioningBuilder.AddApiExplorer(option =>
{
  option.GroupNameFormat = "'v'VVV";// v1, v2, v3...
  option.SubstituteApiVersionInUrl = true; // api/v{version}/"products"
});

// Habilitar CORS
builder.Services.AddCors(options =>
    {
      options.AddPolicy(
          PolicyNames.AllowSpecificOrigin,
          builder => { builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader(); }
      );
    }
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.EnableSwaggerDocumentUrlsEndpoint();
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
  });

}

app.UseHttpsRedirection();

app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
