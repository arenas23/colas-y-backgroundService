using Application.Services;
using Domain.Interfaces.Services;
using Infrastructure.DependencyInjection;
using Infrastructure.MongoDB.Config;
using Infrastructure.RabbitMqUtil;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<IQueueService,QueueService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => 
{
    options.FormatterName = ConsoleFormatterNames.Simple;
});

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "[HH:mm:ss] ";
    options.UseUtcTimestamp = false;
    options.IncludeScopes = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
