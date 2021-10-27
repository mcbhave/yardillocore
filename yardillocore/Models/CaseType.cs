using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace yardillocore.Models
{
    public class CaseType
    {
        public CaseType()
        {
            Activities = new List<Activity>();
        }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Casetype { get; set; }

        public string Casestypedesc { get; set; }
        public string Createdate { get; set; }
        public string Createuser { get; set; }
        public string Updateuser { get; set; }
        public string Updatedate { get; set; }
        public string Defaultactivityid { get; set; }
        public List<Activity> Activities { get; set; }
        public List<Casetypefield> Fields { get; set; }
        public MessageResponse Message { get; set; }
    }
    public class Casetypefield
    {
        public Casetypefield()
        {
            Options = new List<Option>();
            Type = "TEXT";
        }
        public string Fieldid { get; set; }
        public string Fieldname { get; set; }
        public int Seq { get; set; }
        public bool Required { get; set; }
        public string message { get; set; }
        public string Value { get; set; }
        public List<Option> Options { get; set; }
        public string Type { get; set; }
        public int CompareTo(Casetypefield compareSeq)
        {
            // A null value means that this object is greater.
            if (compareSeq == null)
                return 1;
            else
                return this.Seq.CompareTo(compareSeq.Seq);
        }
    }
    public class SetCasetypefield
    {
        public string Fieldid { get; set; }
        public string Value { get; set; }
    }
    public class FieldOption
    {
        public string Optionid { get; set; }
    }
    public class Option
    {

        public string Optionid { get; set; }
        public string Name { get; set; }
        public int Seq { get; set; }
        public string Value { get; set; }
    }
}
