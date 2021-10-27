using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace yardillocore.Models
{
    public class Adapter
    {
        public Adapter()
        {

        }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Macroname { get; set; }
        public bool Isdisabled { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public List<string> Headers { get; set; }
        public string Body { get; set; }
        public string Name { get; set; }
        public MessageResponse Message { get; set; }


    }

    public class Adapterresponse
    {
        public Adapterresponse()
        {

            Fields = new List<SetCasetypefield>();
        }
        public string Response { get; set; }
        public List<SetCasetypefield> Fields { get; set; }
        public string Actionresponse { get; set; }

        public Adapterresponsemap Adapterresponsemaps { get; set; }
        public string Adapterresponseattr { get; set; }

    }
    public class Adapterresponsemap
    {
        public Adapterresponsemap()
        {
            Fields = new List<SetCasetypefield>();
        }
        public string Adapterid { get; set; }
        public List<SetCasetypefield> Fields { get; set; }
    }
}
