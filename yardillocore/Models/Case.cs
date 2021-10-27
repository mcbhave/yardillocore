using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace yardillocore.Models
{
    public class Case
    {
        public Case()
        {
            Fields = new List<Casefield>();
            Activities = new List<CaseActivity>();
        }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public int Casenumber { get; set; }
        public string Casetitle { get; set; }
        public string Casetype { get; set; }
        public string Casestatus { get; set; }
        public string Currentactivityid { get; set; }
        public string Currentactionid { get; set; }

        public string Casedescription { get; set; }
        public string Createdate { get; set; }
        public string Createuser { get; set; }

        public string Updatedate { get; set; }
        public string Updateuser { get; set; }

        public List<Casefield> Fields { get; set; }
        public List<CaseActivity> Activities { get; set; }
        public MessageResponse Message { get; set; }
        public string itemId { get; set; }

        public string Blob { get; set; }
    }

    public class CaseActivity
    {
        public CaseActivity()
        {
            Actions = new List<CaseAction>();
        }
        public string Activityid { get; set; }
        public bool Activitycomplete { get; set; }
        public int Activityseq { get; set; }
        public string Activityname { get; set; }
        public string Activitycompletedate { get; set; }
        public bool Isdisabled { get; set; }
        public List<CaseAction> Actions { get; set; }
        public int CompareTo(Activity compareSeq)
        {
            // A null value means that this object is greater.
            if (compareSeq == null)
                return 1;

            else
                return this.Activityseq.CompareTo(compareSeq.Activityseq);
        }

    }
    public class CaseAction
    {

        public string Actionid { get; set; }
        public string Actionname { get; set; }
        public string Actiontype { get; set; }
        public int Actionseq { get; set; }
        public bool Actioncomplete { get; set; }
        public string Actioncompletedate { get; set; }
        public string Actionstatus { get; set; }
        public bool Isdisabled { get; set; }
        public string Adapterid { get; set; }
        public string Adapterresponse { get; set; }

        // Default comparer for Part type.
        public int CompareTo(Action compareSeq)
        {
            // A null value means that this object is greater.
            if (compareSeq == null)
                return 1;

            else
                return this.Actionseq.CompareTo(compareSeq.Actionseq);
        }
    }
     

    public class Casefield
    {
        public Casefield()
        {
            Options = new List<Option>();
            Type = "TEXT";
        }
        public string Fieldid { get; set; }
        public string Fieldname { get; set; }
        public int Seq { get; set; }
        public bool Required { get; set; }
        public string Value { get; set; }

        public string Type { get; set; }
        public List<Option> Options { get; set; }
        public int CompareTo(Casefield compareSeq)
        {
            // A null value means that this object is greater.
            if (compareSeq == null)
                return 1;
            else
                return this.Seq.CompareTo(compareSeq.Seq);
        }
    }
}
