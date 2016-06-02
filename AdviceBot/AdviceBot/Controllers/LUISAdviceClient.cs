using AdviceBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AdviceBot.Controllers
{
    public class LUISAdviceClient
    {

        
        public static async Task<AdviceLUIS> ParseUserInput(string input)
            {
            string response = string.Empty;
            input = Uri.EscapeDataString(input);

            using (var client = new HttpClient())
            {
                string LUISurl = "https://api.projectoxford.ai/luis/v1/application?id=38a7af3c-7b63-4016-92ba-b9ed03ffc8b2&subscription-key=d3f0150e09d242a0a5360e6f6c58d24f&q=" + input;
                HttpResponseMessage msg = await client.GetAsync(LUISurl);

                if (msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    var _Data = JsonConvert.DeserializeObject<AdviceLUIS>(jsonResponse);
                    return _Data;
                }

                return null;
            }


        }
    }
}