using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VK
{
    public static class Methods
    {
        #region Methods for PGPI-N

        //Возвращает расширенную информацию о пользователях. 
        public static List<Dictionary<string, object>> UsersGet(string[] userIDs, string[] fields, string nameCase = "nom")
        {
            string Parameters = string.Format("user_ids={0}&fields={1}&name_case={2}",
                string.Join(",", userIDs), string.Join(",", fields), nameCase);
            return JsonParsing("users.get", Parameters);
        }

        //Возвращает список идентификаторов пользователей и публичных страниц, которые входят в список подписок пользователя. 
        public static List<Dictionary<string, object>> UsersGetSubscriptions(string userID, string[] fields)
        {
            string Parameters = string.Format("user_id={0}&fields={1}",
                userID, string.Join(",", fields));
            return JsonParsing("users.getSubscriptions", Parameters);
        }

        ////Возвращает список идентификаторов друзей пользователя или расширенную информацию о друзьях пользователя 
        public static List<Dictionary<string, object>> FriendsGet(string userID, string order, string count, string[] fields, string nameCase = "nom")
        {
            string Parameters = string.Format("user_id={0}&order={1}&fields={3}",
                userID, order, count, string.Join(",", fields), nameCase);
            return JsonParsing("friends.get", Parameters);
        }

        //Возвращает список идентификаторов общих друзей между парой пользователей
        public static List<Dictionary<string, object>> FriendsGetMutual(string sourceID, string[] targetIDs)
        {
            string Parameters = string.Format("source_uid={0}&target_uid={1}",
                sourceID, string.Join(",", targetIDs));
            return JsonParsing("friends.getMutual", Parameters);
        }

        #endregion

        public static List<Dictionary<string, object>> JsonParsing(string method, string parameters)
        {
            InitRequest usersGet = new InitRequest();
            usersGet.MethodName = method;
            usersGet.Parameters = parameters;
            string result = usersGet.ApiRequest();
            JObject obj;
            try
            {
                obj = JObject.Parse(result);
            }
            catch
            {
                return null;
            }
            var response = obj["response"];
            if (response == null)
            {
                return null;
            }
            List<Dictionary<string, object>> JsonListBox = new List<Dictionary<string, object>>();
            int count;
            try
            {
                count = (int)response["count"];
                response = response["items"];
            }
            catch
            {
                count = 1;
            }

            for (int i = 0; i < count; i++)
            {
                var item = response[i];
                var temp = item.ToObject<Dictionary<string, object>>();
                List<string> keys = new List<string>(temp.Keys);
                foreach (var key in keys)
                {
                    if (temp[key] is JToken x)
                    {
                        List<Dictionary<string, object>> innerList = new List<Dictionary<string, object>>();
                        foreach(var elem in x)
                        {
                            Dictionary<string, object> dict;
                            try
                            {
                                dict = elem.ToObject<Dictionary<string, object>>();
                            }
                            catch
                            {
                                dict = new Dictionary<string, object> { { "value", elem } };
                            }
                            innerList.Add(dict);
                        }
                        temp.Remove(key);
                        temp.Add(key, innerList);
                    }
                }
                JsonListBox.Add(temp);
            }
            return JsonListBox;
        }
    }
}