﻿using MongoDB.Driver;
using Application.Services;
using Infrastructure.RabbitMqUtil;
using Microsoft.Extensions.Configuration;
using Infrastructure.MongoDB.Config;
using Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Domain.Interfaces.Repositories;
using Infrastructure.MongoDB.Repositories;
using Infrastructure.RabbitMqUtil.Config;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMongoDBServices(configuration);
            services.AddRabbitMQServices(configuration);
            return services;
        }

        private static IServiceCollection AddMongoDBServices( this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDBSettings>( configuration.GetSection(nameof(MongoDBSettings)));

            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });

            return services;
        }

        private static IServiceCollection AddRabbitMQServices( this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqSettings>(configuration.GetSection(nameof(RabbitMqSettings)));

            services.AddSingleton<ChannelManager>();
            services.AddSingleton<IMessageProcessor, MessageProcessor>();
            services.AddSingleton<IRabbitMqSettingsRepository, RabbitMqSettingsRepository>();
            services.AddHostedService<MessageProccesingService>();
            services.AddHostedService<MessageRetryProcessingService>();
            return services;
        }
    }
}