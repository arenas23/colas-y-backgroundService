using Application.Services;
using Domain.Interfaces;
using Infrastructure.DependencyInjection;
using Infrastructure.MongoDB.Config;
using Infrastructure.RabbitMqUtil;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ISenderService,SenderService>();
builder.Services.AddInfrastructureServices(builder.Configuration);




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
