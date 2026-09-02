using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;
using VisitManagement.Api.OpenApi;
using VisitManagement.Api.Auth;
using VisitManagement.Api.Middleware;
using VisitManagement.Application.Abstractions;
using VisitManagement.Application.Usecases;
using VisitManagement.Application.Validators;
using VisitManagement.Infrastructure.Persistence;
using VisitManagement.Infrastructure.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<List<AuthClientOptions>>(builder.Configuration.GetSection("AuthClients"));
builder.Services.AddSingleton<TokenIssuer>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any())
            return Task.CompletedTask;
        if (!metadata.OfType<IAuthorizeData>().Any())
            return Task.CompletedTask;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
        });
        return Task.CompletedTask;
    });
});

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters (user-secrets or appsettings).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ScopeClaims.ReadPolicy, policy =>
        policy.RequireAuthenticatedUser()
            .RequireAssertion(ctx => ScopeClaims.HasScope(ctx.User, ScopeClaims.Read)));
    options.AddPolicy(ScopeClaims.WritePolicy, policy =>
        policy.RequireAuthenticatedUser()
            .RequireAssertion(ctx => ScopeClaims.HasScope(ctx.User, ScopeClaims.Write)));
});

var permitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 60);
var windowSeconds = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var key = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            QueueLimit = 0
        });
    });
});

var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (origins.Length == 0)
            policy.SetIsOriginAllowed(_ => false);
        else
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateVisitRequestValidator>(ServiceLifetime.Singleton);

builder.Services.AddScoped<CreateVisit>();
builder.Services.AddScoped<GetAllVisits>();
builder.Services.AddScoped<GetVisitById>();
builder.Services.AddScoped<UpdateVisit>();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton<IVisitRepository, InMemoryVisitRepository>();
}
else
{
    var cs = builder.Configuration.GetConnectionString("Visits");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("ConnectionStrings:Visits is required.");
    builder.Services.AddDbContext<VisitManagementDbContext>(options =>
        options.UseMySql(cs, ServerVersion.AutoDetect(cs)));
    builder.Services.AddScoped<IVisitRepository, EfVisitRepository>();
}

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
}
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
