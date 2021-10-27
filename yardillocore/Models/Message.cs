using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace yardillocore.Models
{
    public class Message
    {
        public Message()
        {
            Messagedate = DateTime.UtcNow.ToString();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Callertype { get; set; }
        public string Tenantid { get; set; }
        public string Callerid { get; set; }
        public string Messageype { get; set; }
        public string Messagecode { get; set; }
        public string Callerrequest { get; set; }
        public string Callresponse { get; set; }
        public string Headerrequest { get; set; }
        public string Callerrequesttype { get; set; }
        public string MessageDesc { get; set; }
        public string Userid { get; set; }
        public string YAuthSource { get; set; }
        public string Messagedate { get; set; }
    }
    public static class ICallerType
    {
        public const string CASE = "CASE";
        public const string TENANT = "TENANT";
        public const string CASETYPE = "CASETYPE";
        public const string ADAPTER = "ADAPTER";
        public const string VAULT = "VAULT";
        public const string CASE_SEARCH = "CASE_SEARCH";
    }
    public class MessageResponse
    {
        public string _id { get; set; }
        public string Messageype { get; set; }
        public string Messagecode { get; set; }
        public string Messagedesc { get; set; }
        //public string Messagedate { get; set; }
    }

}
