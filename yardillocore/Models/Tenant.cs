using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace yardillocore.Models
{
    public class Tenant  
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Tenantname { get; set; }
        public string Tenantdesc { get; set; }

        public string _owner { get; set; }
        public string Createdate { get; set; }
        public string Createuser { get; set; }

        public string Dbconnection { get; set; }
        public string Rapidapikey { get; set; }

        public string Rapidhost { get; set; }

        public string Rapidsubscription { get; set; }
        public MessageResponse Message { get; set; }
        public string YAuthSource { get; set; }

    }
    public class TenantUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string Tenantname { get; set; }
        public string Userid { get; set; }

        public string Createdate { get; set; }

        public string Createuserid { get; set; }

        public string Source { get; set; }

        public string Role { get; set; }

        public string Type { get; set; }

        public string YAuthSource { get; set; }

        public string RapidAPIkey { get; set; }

        public string Installationid { get; set; }
    }
}
