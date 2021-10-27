using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace yardillocore.Models
{
    public class Vault
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Name { get; set; }
        public string Macroname { get; set; }

        public string Encryptwithkey { get; set; }
        public string Safekeeptext { get; set; }
        public string Tenantid { get; set; }

    }

    public class VaultResponse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Name { get; set; }
        public string Macroname { get; set; }

        public string Encryptwithkey { get; set; }
        public string Safekeeptext { get; set; }
        public MessageResponse Message { get; set; }
    }
}
