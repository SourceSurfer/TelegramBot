using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp;

namespace TelegramB
{
    internal class Weather
    {
        private readonly RestClient client;
        //Key: 6a81939f30034d66a75135404232109
        public Weather(RestClient client)
        {
            this.client = client;
        }

        internal async Task<string> GetWeather()
        {
            var request = new RestRequest("/current.json", Method.Get);
            request.AddParameter("key", "6a81939f30034d66a75135404232109");
            request.AddParameter("q", "Kudrovo");
            RestResponse data = await client.ExecuteAsync(request);
            string Temp = string.Empty;
            if (data.Content == null) return Temp;
            dynamic array = JsonConvert.DeserializeObject(data.Content);
#if DEBUG // TODO доделать погоду
            Console.WriteLine(array is IJEnumerable<JToken>);
            Console.WriteLine(array.GetType());
            Console.WriteLine(array is JProperty);

            var enumerable = (JObject)array;
            IEnumerator enumerator = enumerable.GetEnumerator();

            JProperty property = enumerator.Current as JProperty;
            Console.WriteLine(property.Name, property.Value, property.Type);
#endif

            foreach (Newtonsoft.Json.Linq.JProperty prop in array)
            {
                if (prop.Name == "current")
                {
                    var current_Data = prop.Value.Children();
                    foreach (Newtonsoft.Json.Linq.JProperty item in current_Data)
                    {
                        switch (item.Name)
                        {
                            case "temp_c":
                                Temp = item.Value.ToString();
                                break;
                        }
                    }
                }
            }
            return Temp;
        }
    }
}
