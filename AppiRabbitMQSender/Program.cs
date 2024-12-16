using Application.Services;
using Domain.Interfaces;
using Domain.Interfaces.RabbitMqUtil;
using Infrastructure.RabbitMqUtil;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ISenderService,SenderService>();
builder.Services.AddSingleton<IRabbitMqUtil, RabbitMqUtil>()
    .AddHostedService<RabbitMqService>();

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
