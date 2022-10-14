using dotnet.mongodb.utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace dotnet.mongodb.core
{
    public class MongoDbHelper
    {
        private readonly string connectionString = null;
        private readonly string databaseName = null;
        private MongoDB.Driver.IMongoDatabase database = null;
        private readonly bool autoCreateDb = false;
        private readonly bool autoCreateCollection = false;

        static MongoDbHelper()
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        public MongoDbHelper(string dbName, bool autoCreateDb = false, bool autoCreateCollection = false)
        {
            this.connectionString = ConfigHelper.Instance.GetConnectionString("mongodb");
            this.databaseName = dbName;
            this.autoCreateDb = autoCreateDb;
            this.autoCreateCollection = autoCreateCollection;
        }

        #region Private method

        private MongoClient CreateMongoClient()
        {
            return new MongoClient(connectionString);
        }


        private MongoDB.Driver.IMongoDatabase GetMongoDatabase()
        {
            if (database == null)
            {
                var client = CreateMongoClient();
                if (!DatabaseExists(client, databaseName) && !autoCreateDb)
                {
                    throw new KeyNotFoundException("This MongoDB name does not exist：" + databaseName);
                }

                database = CreateMongoClient().GetDatabase(databaseName);
            }

            return database;
        }

        private bool DatabaseExists(MongoClient client, string dbName)
        {
            try
            {
                var dbNames = client.ListDatabases().ToList().Select(db => db.GetValue("name").AsString);
                return dbNames.Contains(dbName);
            }
            catch
            {
                return true;
            }

        }

        private bool CollectionExists(IMongoDatabase database, string collectionName)
        {
            var options = new ListCollectionsOptions
            {
                Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName)
            };

            return database.ListCollections(options).ToEnumerable().Any();
        }


        private MongoDB.Driver.IMongoCollection<TDoc> GetMongoCollection<TDoc>(string name, MongoCollectionSettings settings = null)
        {
            var mongoDatabase = GetMongoDatabase();

            if (!CollectionExists(mongoDatabase, name) && !autoCreateCollection)
            {
                throw new KeyNotFoundException("This Collection name does not exist：" + name);
            }

            return mongoDatabase.GetCollection<TDoc>(name, settings);
        }

        private List<UpdateDefinition<TDoc>> BuildUpdateDefinition<TDoc>(object doc, string parent)
        {
            var updateList = new List<UpdateDefinition<TDoc>>();
            foreach (var property in typeof(TDoc).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var key = parent == null ? property.Name : string.Format("{0}.{1}", parent, property.Name);
                //non-null complex type
                if ((property.PropertyType.IsClass || property.PropertyType.IsInterface) && property.PropertyType != typeof(string) && property.GetValue(doc) != null)
                {
                    if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        #region Collection type
                        int i = 0;
                        var subObj = property.GetValue(doc);
                        foreach (var item in subObj as IList)
                        {
                            if (item.GetType().IsClass || item.GetType().IsInterface)
                            {
                                updateList.AddRange(BuildUpdateDefinition<TDoc>(doc, string.Format("{0}.{1}", key, i)));
                            }
                            else
                            {
                                updateList.Add(Builders<TDoc>.Update.Set(string.Format("{0}.{1}", key, i), item));
                            }
                            i++;
                        }
                        #endregion
                    }
                    else
                    {
                        #region entity type
                        //complex types, navigation properties, class objects, and collection objects
                        var subObj = property.GetValue(doc);
                        foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        {
                            updateList.Add(Builders<TDoc>.Update.Set(string.Format("{0}.{1}", key, sub.Name), sub.GetValue(subObj)));
                        }
                        #endregion
                    }
                }
                else //simple type
                {
                    updateList.Add(Builders<TDoc>.Update.Set(key, property.GetValue(doc)));
                }
            }

            return updateList;
        }


        private void CreateIndex<TDoc>(IMongoCollection<TDoc> col, string[] indexFields, CreateOneIndexOptions options = null)
        {
            if (indexFields == null)
            {
                return;
            }
            var indexKeys = Builders<TDoc>.IndexKeys;
            IndexKeysDefinition<TDoc> keys = null;
            if (indexFields.Length > 0)
            {
                keys = indexKeys.Descending(indexFields[0]);
            }
            for (var i = 1; i < indexFields.Length; i++)
            {
                var strIndex = indexFields[i];
                keys = keys.Descending(strIndex);
            }

            if (keys != null)
            {
                col.Indexes.CreateOne(new CreateIndexModel<TDoc>(keys), options);
            }

        }

        #endregion

        public void CreateCollectionIndex<TDoc>(string collectionName, string[] indexFields, CreateOneIndexOptions options = null)
        {
            CreateIndex(GetMongoCollection<TDoc>(collectionName), indexFields, options);
        }

        public void CreateCollection<TDoc>(string[] indexFields = null, CreateOneIndexOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            CreateCollection<TDoc>(collectionName, indexFields, options);
        }

        public void CreateCollection<TDoc>(string collectionName, string[] indexFields = null, CreateOneIndexOptions options = null)
        {
            var mongoDatabase = GetMongoDatabase();
            mongoDatabase.CreateCollection(collectionName);
            CreateIndex(GetMongoCollection<TDoc>(collectionName), indexFields, options);
        }

        public long Count<TDoc>(string collectionName,EstimatedDocumentCountOptions options=null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            return collection.EstimatedDocumentCount(options);
        }


        public List<TDoc> Find<TDoc>(Expression<Func<TDoc, bool>> filter, FindOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            return Find<TDoc>(collectionName, filter, options);
        }

        public List<TDoc> Find<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, FindOptions options = null)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            return colleciton.Find(filter, options).ToList();
        }


        public List<TDoc> FindByPage<TDoc, TResult>(Expression<Func<TDoc, bool>> filter, Expression<Func<TDoc, TResult>> keySelector, int pageIndex, int pageSize, out int rsCount)
        {
            string collectionName = typeof(TDoc).Name;
            return FindByPage<TDoc, TResult>(collectionName, filter, keySelector, pageIndex, pageSize, out rsCount);
        }

        public List<TDoc> FindByPage<TDoc, TResult>(string collectionName, Expression<Func<TDoc, bool>> filter, Expression<Func<TDoc, TResult>> keySelector, int pageIndex, int pageSize, out int rsCount)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            rsCount = colleciton.AsQueryable().Where(filter).Count();

            int pageCount = rsCount / pageSize + ((rsCount % pageSize) > 0 ? 1 : 0);
            if (pageIndex > pageCount) pageIndex = pageCount;
            if (pageIndex <= 0) pageIndex = 1;

            return colleciton.AsQueryable(new AggregateOptions { AllowDiskUse = true }).Where(filter).OrderByDescending(keySelector).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public void Insert<TDoc>(TDoc doc, InsertOneOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            Insert<TDoc>(collectionName, doc, options);
        }

        public void Insert<TDoc>(string collectionName, TDoc doc, InsertOneOptions options = null)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            colleciton.InsertOne(doc, options);
        }


        public void InsertMany<TDoc>(IEnumerable<TDoc> docs, InsertManyOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            InsertMany<TDoc>(collectionName, docs, options);
        }

        public void InsertMany<TDoc>(string collectionName, IEnumerable<TDoc> docs, InsertManyOptions options = null)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            colleciton.InsertMany(docs, options);
        }

        public bool Update<TDoc>(TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            List<UpdateDefinition<TDoc>> updateList = BuildUpdateDefinition<TDoc>(doc, null);
            return colleciton.UpdateOne(filter, Builders<TDoc>.Update.Combine(updateList), options).IsAcknowledged;
        }

        public bool Update<TDoc>(string collectionName, TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            List<UpdateDefinition<TDoc>> updateList = BuildUpdateDefinition<TDoc>(doc, null);
            return colleciton.UpdateOne(filter, Builders<TDoc>.Update.Combine(updateList), options).IsAcknowledged;
        }


        public bool Update<TDoc>(Expression<Func<TDoc, bool>> filter, UpdateDefinition<TDoc> updateFields, UpdateOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            return Update<TDoc>(collectionName, filter, updateFields, options);
        }

        public bool Update<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, UpdateDefinition<TDoc> updateFields, UpdateOptions options = null)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            return colleciton.UpdateOne(filter, updateFields, options).IsAcknowledged;
        }


        public void UpdateMany<TDoc>(TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            UpdateMany<TDoc>(collectionName, doc, filter, options);
        }


        public void UpdateMany<TDoc>(string collectionName, TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            List<UpdateDefinition<TDoc>> updateList = BuildUpdateDefinition<TDoc>(doc, null);
            colleciton.UpdateMany(filter, Builders<TDoc>.Update.Combine(updateList), options);
        }


        public bool Delete<TDoc>(Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            return Delete<TDoc>(collectionName, filter, options);
        }

        public bool Delete<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            return colleciton.DeleteOne(filter, options).IsAcknowledged;
        }


        public bool DeleteMany<TDoc>(Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            return DeleteMany<TDoc>(collectionName, filter, options);
        }


        public bool DeleteMany<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            return colleciton.DeleteMany(filter, options).IsAcknowledged;
        }

        public void ClearCollection<TDoc>(string collectionName)
        {
            var colleciton = GetMongoCollection<TDoc>(collectionName);
            var inddexs = colleciton.Indexes.List();
            List<IEnumerable<BsonDocument>> docIndexs = new List<IEnumerable<BsonDocument>>();
            while (inddexs.MoveNext())
            {
                docIndexs.Add(inddexs.Current);
            }
            var mongoDatabase = GetMongoDatabase();
            mongoDatabase.DropCollection(collectionName);

            if (!CollectionExists(mongoDatabase, collectionName))
            {
                CreateCollection<TDoc>(collectionName);
            }

            if (docIndexs.Count > 0)
            {
                colleciton = mongoDatabase.GetCollection<TDoc>(collectionName);
                foreach (var index in docIndexs)
                {
                    foreach (IndexKeysDefinition<TDoc> indexItem in index)
                    {
                        try
                        {
                            colleciton.Indexes.CreateOne(new CreateIndexModel<TDoc>(indexItem));
                        }
                        catch
                        { }
                    }
                }
            }

        }
    }

}
