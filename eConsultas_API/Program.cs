using DataAccessLayer.Data;
using Microsoft.Extensions.Configuration;
using DataAccessLayer.Filters;
using DataAccessLayer.Interface;
using DataAccessLayer.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using DataAccessLayer.Middlewares; 
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using DataAccessLayer.Filters;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Registre o ClinicaDbContext no contêiner de injeção de dependência.
builder.Services.AddDbContext<ClinicaDbContext>();
builder.Services.AddScoped<IUserInterface, UserRepository>();
builder.Services.AddScoped<IGenTokenFilter, GenTokenFilter>();
builder.Services.AddScoped<IDecToken, DecToken>();
builder.Services.AddScoped<IAppointInterface, AppointRepository>();
builder.Services.AddScoped<ILoginInterface, LoginRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();



//Testar o endereço IP do localhost que se no caso fosse um projecto real pegariamos o endereço IP remoto
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
});


//DeleteLogTableIfExist.CheckAndDeleteLogTableIfExist("Data Source=LAPTOP-D8T7SBRN;Initial Catalog=DB_eClinica;Trusted_Connection=True;TrustServerCertificate=True");


// Add services Logger
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true; // Opcional: formata o JSON de maneira legível
    });
#if USE_MY_TOKEN
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endif
// nao eh necessario a validação esta no servidor
//#if USE_MY_TOKEN
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = false;
//    options.SaveToken = true;
//    options.TokenValidationParameters = new TokenValidationParameters()
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };
//});
//#endif

// add cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsProfile",
    policy =>
    {
        policy
       .AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("MyCorsProfile");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




//HTTP Strict Transport Security (HSTS): Aqui  o HSTS é uma política de segurança que ajuda a proteger sites contra ataques man-in-the-middle.
app.UseHttpsRedirection();
app.UseXXssProtection();  // Use o middleware personalizado
if (app.Environment.IsProduction())
{
    app.UseHsts();
}
//Adiciona cabeçalho para tentativa de proteção contra XSS em navegadores antigos.
app.Use(async (context, next) =>
{
    if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
    {
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    }
    await next();
});
// Adiciona cabeçalho para tentativa de proteção contra XSS em navegadores antigos
//Recomenda-se sem  adotar práticas mais modernas como CSP para defesas robustas, porque essa política de Segurança de Conteúdo
//(Content Security Policy - CSP) é uma ferramenta poderosa para prevenir ataques XSS e outros tipos de ataques de injeção. 
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';");
    await next();
});
//Cabeçalho X-Frame-Options: Protege contra ataques clickjacking impedindo o conteúdo de ser exibido em um iframe.
app.Use((context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY"; // ou "SAMEORIGIN" se precisar usar iframes do mesmo domínio
    return next();
});

//Cabeçalho X-Content-Type-Options: Impede que o navegador tente adivinhar ("mime sniffing") o tipo de conteúdo e força a interpretação dos cabeçalhos conforme declarado.
app.Use((context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    return next();
});


//Referrer-Policy: Controla quais informações do referenciador (referrer) são incluídas com as solicitações.
app.Use((context, next) =>
{
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    return next();
});


//Feature-Policy: Permite que você controle quais APIs da Web e recursos podem ser usados pelo seu aplicativo.
app.Use((context, next) =>
{
    context.Response.Headers["Feature-Policy"] = "camera 'none'; microphone 'none'; geolocation 'self'";
    return next();
});



app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
