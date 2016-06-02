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
                string LUISurl = "YourLUISPublishedAppURLHERE&q=" + input;
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
