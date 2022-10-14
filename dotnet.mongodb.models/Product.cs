using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet.mongodb.models
{
    public class Product
    {
        [BsonElement("_id")]
        public ObjectId Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("quantity")]
        public int Quantity { get; set; }
        [BsonElement("brand")]
        public string Brand { get; set; }
        [BsonElement("MFGD")]
        public DateTime MFGD { get; set; }
        [BsonElement("details")]
        public string Details { get; set; }
    }
}
