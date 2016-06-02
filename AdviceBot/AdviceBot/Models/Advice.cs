using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdviceBot.Models
{
    public class Advice
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string AdviceTitle { get; set; }
        public string AdviceTags { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}