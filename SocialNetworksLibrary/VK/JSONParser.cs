using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworksLibrary
{
    public static class JSONParser
    {
        public static dynamic JsonParsing(string sresponse)
        {
            string result = sresponse;
            dynamic obj;
            try
            {
                obj = JObject.Parse(result);
            }
            catch
            {
                obj = JArray.Parse(result);
            }
            var elem = RecursiveParse(obj);
            return elem;
        }

        public static dynamic RecursiveParse(JToken obj)
        {
            dynamic unadaptedJson;
            dynamic adaptedJson;
            try
            {
                unadaptedJson = obj.ToObject<Dictionary<string, object>>();
                adaptedJson = new Dictionary<string, object>();
                foreach (var key in unadaptedJson.Keys)
                {
                    if (unadaptedJson[key] is JToken)
                    {
                        var answer = RecursiveParse(unadaptedJson[key]);
                        adaptedJson.Add(key, answer);
                    }
                    else
                    {
                        adaptedJson.Add(key, unadaptedJson[key]);
                    }
                }
            }
            catch
            {
                unadaptedJson = obj.ToObject<List<object>>();
                adaptedJson = new List<object>();
                foreach (var elem in unadaptedJson)
                {
                    if (elem is JToken)
                    {
                        var answer = RecursiveParse(elem);
                        adaptedJson.Add(answer);
                    }
                    else
                    {
                        adaptedJson.Add(elem);
                    }
                }
            }
            return adaptedJson;
        }
    }
}
