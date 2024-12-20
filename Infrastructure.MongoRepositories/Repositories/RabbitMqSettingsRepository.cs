using Domain.Entities.Dto;
using Domain.Interfaces.Repositories;
using Infrastructure.MongoDB.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MongoDB.Repositories
{
    public class RabbitMqSettingsRepository(IMongoDatabase database) : MongoRepository<RabbitMqSettings>(database), IRabbitMqSettingsRepository
    {
        public async Task<MessagesConcurrentDto> RetrieveRabbitMqSettings()
        {
            var filter = Builders<RabbitMqSettings>.Filter.Empty;
            var projection = Builders<RabbitMqSettings>.Projection
                .Include(x=>x.TransactionConcurrentMessages)
                .Include(x=>x.RetryConcurrentMessages)
                .Exclude("_id");
            var result = await _collection.Find(filter)
                .Project<MessagesConcurrentDto>(projection)
                .FirstOrDefaultAsync();
            return result;
        }
    }
}
