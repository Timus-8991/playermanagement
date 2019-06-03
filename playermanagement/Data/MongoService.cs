using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace playermanagement.Data
{
    public class MongoService
    {
        protected IMongoDatabase db;
        protected List<string> collectionNames;
        private readonly MongoContext _context = null;

        public MongoService(IOptions<MongoSettings> settings)
        {
            collectionNames = new List<string>();
            _context = new MongoContext(settings);
            db = _context.GetDatabase();
        }


        /// <summary>
        /// creates a new collection
        /// </summary>
        /// <param name="collectionName"></param>
        protected async Task CreateACollection(string collectionName)
        {
            if (collectionNames.Count == 0)
            {
                collectionNames = db.ListCollectionNames().ToList();
            }

            // create the collection of do not exists
            if (!collectionNames.Where(collection => collection == collectionName).Any())
            {

                db.CreateCollection(collectionName);

                // add it to local cache element
                // this local cache element should point to a redis instance
                collectionNames.Add(collectionName);
            }
            await Task.CompletedTask;
        }

        //Generic Type T extensions
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public async Task CreateAsync<T>(string collectionName, T document)
        {
            try
            {
                await CreateACollection(collectionName);
                var collection = db.GetCollection<T>(collectionName);
                await collection.InsertOneAsync(document);
                return;
            }
            catch (Exception ex)
            {
                string ecp = ex.Message;
                return;
            }
        }
        public async Task<T> GetDocumentsByIdAsync<T>(Guid id, string collectionName)
        {
            await CreateACollection(collectionName);
            var collection = db.GetCollection<T>(collectionName);

            var filter = Builders<T>.Filter.Eq("_id", id);
            var result = await collection.Find(filter).ToListAsync();

            return result.FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public async Task<List<T>> GetAllDocumentsAsync<T>(string collectionName)
        {
            await CreateACollection(collectionName);
            var collection = db.GetCollection<T>(collectionName);
            var docs = await collection.Find(new BsonDocument()).ToListAsync();
            return docs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public async Task<List<T>> GetDocumentsByFieldAsync<T>(string collectionName, string fieldName, object fieldValue)
        {
            await CreateACollection(collectionName);
            var collection = db.GetCollection<T>(collectionName);

            var filter = Builders<T>.Filter.Eq(fieldName, fieldValue);
            var result = await collection.Find(filter).ToListAsync();

            return result;
        }

        public async Task<List<T>> GetDocumentsByFilterAsync<T>(string collectionName, JObject filterDictionary)
        {
            await CreateACollection(collectionName);
            var collection = db.GetCollection<T>(collectionName);
            var filter = BsonSerializer.Deserialize<BsonDocument>(JsonConvert.SerializeObject(filterDictionary));
            var result = await collection.Find(filter).ToListAsync();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="collectionName"></param>
        /// <param name="updateFieldName"></param>
        /// <param name="updateFieldValue"></param>
        /// <returns></returns>
        public async Task<bool> UpdateDocumentFieldByIdAsync<T>(Guid id, string collectionName, string updateFieldName, object updateFieldValue)
        {
            await CreateACollection(collectionName);
            var collection = db.GetCollection<T>(collectionName);

            var filter = Builders<T>.Filter.Eq("_id", id);
            var update = Builders<T>.Update.Set(updateFieldName, updateFieldValue);

            var result = await collection.UpdateOneAsync(filter, update);

            return result.ModifiedCount != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="collectionName"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public async Task<bool> ReplaceDocumentByIdAsync<T>(Guid id, string collectionName, T document)
        {
            await CreateACollection(collectionName);
            var collection = db.GetCollection<T>(collectionName);

            var filter = Builders<T>.Filter.Eq("_id", id);

            var result = await collection.ReplaceOneAsync(filter, document);

            return result.ModifiedCount != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDocumentByIdAsync<T>(Guid id, string collectionName)
        {
            try
            {
                await CreateACollection(collectionName);
                var collection = db.GetCollection<T>(collectionName);
                var filter = Builders<T>.Filter.Eq("_id", id);
                var result = await collection.DeleteOneAsync(filter);
                return result.DeletedCount != 0;
            }

            catch (Exception ex)
            {
                string ecp = ex.Message;
                return false;
            }
        }
    }

}
