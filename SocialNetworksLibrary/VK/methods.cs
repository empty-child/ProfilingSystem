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
        public static List<Dictionary<string, string>> UsersGet(string[] userIDs, string[] fields, string nameCase = "nom")
        {
            string Parameters = string.Format("user_ids={0}&fields={1}&name_case={2}",
                string.Join(",", userIDs), string.Join(",", fields), nameCase);
            return JsonParsing("users.get", Parameters);
        }

        //Возвращает список идентификаторов пользователей и публичных страниц, которые входят в список подписок пользователя. 
        public static List<Dictionary<string, string>> UsersGetSubscriptions(string userID, string[] fields)
        {
            string Parameters = string.Format("user_id={0}&fields={1}",
                userID, string.Join(",", fields));
            return JsonParsing("users.getSubscriptions", Parameters);
        }

        ////Возвращает список идентификаторов друзей пользователя или расширенную информацию о друзьях пользователя 
        public static List<Dictionary<string, string>> FriendsGet(string userID, string order, string count, string[] fields, string nameCase = "nom")
        {
            string Parameters = string.Format("user_id={0}&order={1}&fields={3}",
                userID, order, count, string.Join(",", fields), nameCase);
            return JsonParsing("friends.get", Parameters);
        }

        //Возвращает список идентификаторов общих друзей между парой пользователей
        public static List<Dictionary<string, string>> FriendsGetMutual(string sourceID, string[] targetIDs)
        {
            string Parameters = string.Format("source_uid={0}&target_uid={1}",
                sourceID, string.Join(",", targetIDs));
            return JsonParsing("friends.getMutual", Parameters);
        }

        #endregion

        public static List<Dictionary<string, string>> JsonParsing(string method, string parameters)
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
            List<Dictionary<string, string>> JsonListBox = new List<Dictionary<string, string>>();
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
                var temp = item.ToObject<Dictionary<string, string>>();
                //Dictionary<string, string> temp = new Dictionary<string, string>();
                //foreach (var data in item)
                //{//TODO: необходима реализация для вложенных списков (типа schools, career и тд)
                //    string[] pair = data.ToString().Split(new char[] { ':', '"', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //    if (data.First.HasValues)
                //    {
                //        //foreach(var innerData in data)
                //        for (int k = 0; k < data.First().Count() - 1; k++)
                //        {
                //            int ind = 0;
                //            foreach (var innerData in data.ElementAt<JToken>(k))
                //            {
                //                var dict = innerData.ToObject<Dictionary<string, string>>();
                //                string[] innerPair = innerData.ToString().Split(new char[] { ':', '"', ' ', ',', '{', '}', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                //                try
                //                {
                //                    temp.Add(pair[0] + ind + "." + innerPair[0], innerPair[1]);
                //                }
                //                catch
                //                {
                //                    temp.Add(pair[0] + ind + "." + innerPair[0], null);
                //                }
                //                ind++;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        try
                //        {
                //            temp.Add(pair[0], pair[1]);
                //        }
                //        catch
                //        {
                //            temp.Add(pair[0], null);
                //        }
                //    }
                //}
                JsonListBox.Add(temp);
            }
            return JsonListBox;
        }
    }
}