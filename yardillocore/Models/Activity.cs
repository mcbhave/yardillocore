using System;
using System.Collections.Generic;
using System.Text;

namespace yardillocore.Models
{
    public class Activity
    {
        public Activity()
        {
            Actions = new List<Action>();
            Isdisabled = false;
            Activityseq = 0;
        }
        public string Activityid { get; set; }
        public bool Activitycomplete { get; set; }
        public bool Isdisabled { get; set; }
        public int Activityseq { get; set; }
        public string Activityname { get; set; }
        public string Activitydesc { get; set; }
        public List<Action> Actions { get; set; }
        public int CompareTo(Activity compareSeq)
        {
            // A null value means that this object is greater.
            if (compareSeq == null)
                return 1;
            else
                return this.Activityseq.CompareTo(compareSeq.Activityseq);
        }
    }
}
