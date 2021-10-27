using System;
using System.Collections.Generic;
using System.Text;

namespace yardillocore.Models
{
    
    public class DatabaseSettings : IDatabaseSettings
    {
        public string CasesCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string MessagesCollectionName { get; set; }
        public string CaseTypesCollectionName { get; set; }
        public string Caseactivityhistorycollection { get; set; }
        public string ActionAuthLogscollection { get; set; }

        public string Adapterscollection { get; set; }
        public string Vaultcollection { get; set; }

        public string AdapterLogscollection { get; set; }
        public string SchedulesCollectionName { get; set; }
        public string SpeechToTextCollection { get; set; }
        public string SpeechToTextMAPCollection { get; set; }
        public string CommonnamesCollection { get; set; }

        public string SpeechToTextAttrCollection { get; set; }
        public string CommonaispeechidCollection { get; set; }
        public string SpeechWebHook { get; set; }

        public string AIServiceBaseURL { get; set; }
    }

    public interface IDatabaseSettings
    {
        string CasesCollectionName { get; set; }
        string CaseTypesCollectionName { get; set; }
        string MessagesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        public string Caseactivityhistorycollection { get; set; }
        public string ActionAuthLogscollection { get; set; }
        public string Adapterscollection { get; set; }
        public string Vaultcollection { get; set; }
        public string AdapterLogscollection { get; set; }
        public string SchedulesCollectionName { get; set; }

        public string SpeechToTextCollection { get; set; }
        public string SpeechToTextMAPCollection { get; set; }

        public string CommonnamesCollection { get; set; }


        public string CommonaispeechidCollection { get; set; }


        public string SpeechToTextAttrCollection { get; set; }

        public string SpeechWebHook { get; set; }

        public string AIServiceBaseURL { get; set; }


    }
}
