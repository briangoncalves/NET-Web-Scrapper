using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebCrawlerPOC.Domain;
using WebCrawlerPOC.Infrastructure.Mongo.Interfaces;

namespace WebCrawlerPOC.Infrastructure.Mongo
{
    public class MongoRepository<TDocument> : IMongoRepository<TDocument>
         where TDocument : IDocument
    {
        private readonly IMongoCollection<TDocument> _collection;
        private readonly IConnectionThrottlingPipeline _pipeline;

        public MongoRepository(IMongoDbSettings settings, IConnectionThrottlingPipeline pipeline)
        {
            var _client = settings.CreateClient();
            var database = _client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
            _pipeline = pipeline;
        }

        private protected static string GetCollectionName(Type documentType)
        {
            return ((BsonCollectionAttribute)documentType.GetCustomAttributes(
                    typeof(BsonCollectionAttribute),
                    true)
                .FirstOrDefault())?.CollectionName;
        }

        public virtual IQueryable<TDocument> AsQueryable()
        {
            return _collection.AsQueryable();
        }
        public virtual IEnumerable<TDocument> FilterBy(
            Expression<Func<TDocument, bool>> filterExpression)
        {
            return _collection.Find(filterExpression).ToEnumerable();
        }
        public virtual IEnumerable<TProjected> FilterBy<TProjected>(
            Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression)
        {
            return _collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
        }
        public virtual TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _collection.Find(filterExpression).FirstOrDefault();
        }
        public virtual Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _pipeline.AddRequest(_collection.Find(filterExpression).FirstOrDefaultAsync());
        }
        public virtual TDocument FindById(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            return _collection.Find(filter).SingleOrDefault();
        }
        public virtual Task<TDocument> FindByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            return _pipeline.AddRequest(_collection.Find(filter).SingleOrDefaultAsync());
        }
        public virtual void InsertOne(TDocument document)
        {
            _collection.InsertOne(document);
        }

        public virtual Task InsertOneAsync(TDocument document)
        {
            return _pipeline.AddRequest(_collection.InsertOneAsync(document));
        }
        public void InsertMany(ICollection<TDocument> documents)
        {
            _collection.InsertMany(documents);
        }
        public virtual Task InsertManyAsync(ICollection<TDocument> documents)
        {
            return _pipeline.AddRequest(_collection.InsertManyAsync(documents));
        }
        public void ReplaceOne(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            _collection.FindOneAndReplace(filter, document);
        }
        public virtual Task ReplaceOneAsync(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            return _pipeline.AddRequest(_collection.FindOneAndReplaceAsync(filter, document));
        }
        public void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            _collection.FindOneAndDelete(filterExpression);
        }
        public Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _pipeline.AddRequest(_collection.FindOneAndDeleteAsync(filterExpression));
        }
        public void DeleteById(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            _collection.FindOneAndDelete(filter);
        }
        public Task DeleteByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            return _pipeline.AddRequest(_collection.FindOneAndDeleteAsync(filter));
        }
        public void DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
        {
            _collection.DeleteMany(filterExpression);
        }
        public Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _pipeline.AddRequest(_collection.DeleteManyAsync(filterExpression));
        }
    }
}