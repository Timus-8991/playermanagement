using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace playermanagement.Data
{
    public class MongoContext
    {
        private IMongoDatabase _database = null;
        private readonly IOptions<MongoSettings> _options;

        public MongoContext(IOptions<MongoSettings> options)
        {
            _options = options;
        }

        private void SetContext()
        {
            try
            {
                var client = new MongoClient(_options.Value.ConnectionString);

                if (client != null)
                {
                    _database = client.GetDatabase(_options.Value.DatabaseName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database connection failed with error - {ex.Message}");
            }
        }

        public IMongoDatabase GetDatabase()
        {
            SetContext();

            return _database;
        }
    }
}
