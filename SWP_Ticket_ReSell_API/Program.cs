using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Swashbuckle.AspNetCore.Filters;

using SWP_Ticket_ReSell_DAO.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// config swagger
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                             builder.Configuration.GetSection("AppSettings:SerectKey").Value!)),
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };
                    options.RequireHttpsMetadata = false;
                });
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new()
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>(JwtBearerDefaults.AuthenticationScheme);
});
builder.Services.AddScoped(typeof(ServiceBase<>));
builder.Services.AddScoped(typeof(GenericRepository<>));

builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
//Khỏi tạo FireBase
builder.Services.AddScoped<FirebaseStorageService>();
//Lấy cấu hình tử appsetting
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//--
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
        options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
        options.BackchannelTimeout = TimeSpan.FromSeconds(120);
    });
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDbContext<swp1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDB"))
           .UseLazyLoadingProxies()
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());

// Thêm UserService
var app = builder.Build();
app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
// enable authentication
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
