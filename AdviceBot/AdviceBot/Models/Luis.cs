using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdviceBot.Models
{

    public class AdviceLUIS
    {
        public string query { get; set; }
        public aIntent[] intents { get; set; }
        public aEntity[] entities { get; set; }
    }

    public class aIntent
    {
        public string intent { get; set; }
        public float score { get; set; }
        public aAction[] actions { get; set; }
    }

    public class aAction
    {
        public bool triggered { get; set; }
        public string name { get; set; }
        public aParameter[] parameters { get; set; }
    }

    public class aParameter
    {
        public string name { get; set; }
        public bool required { get; set; }
        public aValue[] value { get; set; }
    }

    public class aValue
    {
        public string entity { get; set; }
        public string type { get; set; }
        public float score { get; set; }
    }

    public class aEntity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
    }

}