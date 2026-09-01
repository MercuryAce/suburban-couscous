using System.Text.Json.Serialization;
using FluentValidation;
using VisitManagement.Api.Middleware;
using VisitManagement.Application.Abstractions;
using VisitManagement.Application.Usecases;
using VisitManagement.Application.Validators;
using VisitManagement.Infrastructure.Persistence;
using VisitManagement.Infrastructure.Time;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IVisitRepository, InMemoryVisitRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateVisitRequestValidator>(ServiceLifetime.Singleton);

builder.Services.AddScoped<CreateVisit>();
builder.Services.AddScoped<GetAllVisits>();
builder.Services.AddScoped<GetVisitById>();


var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
//app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;