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
        public static List<List<string>> UsersGet(string[] userIDs, string[] fields, string nameCase = "nom")
        {
            string Parameters = string.Format("user_ids={0}&fields={1}&name_case={2}", 
                string.Join(",", userIDs), string.Join(",", fields), nameCase);
            return JsonParsing("users.get", Parameters);
        }

        //Возвращает список идентификаторов пользователей и публичных страниц, которые входят в список подписок пользователя. 
        public static List<List<string>> UsersGetSubscriptions(string userID, string[] fields)
        {
            string Parameters = string.Format("user_id={0}&fields={1}",
                userID, string.Join(",", fields));
            return JsonParsing("users.getSubscriptions", Parameters);
        }

        ////Возвращает список идентификаторов друзей пользователя или расширенную информацию о друзьях пользователя 
        public static List<List<string>> FriendsGet(string userID, string order, int count, int offset, string[] fields, string nameCase = "nom")
        {
            string Parameters = string.Format("user_id={0}&order={1}&count={2}&offset={3}&fields={4}&name_case={5}",
                userID, order, count, offset, string.Join(",", fields), nameCase);
            return JsonParsing("friends.get", Parameters);
        }

        //Возвращает список идентификаторов общих друзей между парой пользователей
        public static List<List<string>> FriendsGetMutual(string sourceID, string[] targetIDs)
        {
            string Parameters = string.Format("user_id={0}&fields={1}",
                sourceID, string.Join(",", targetIDs));
            return JsonParsing("friends.getMutual", Parameters);
        }

        #endregion

        public static List<List<string>> JsonParsing(string method, string parameters)
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
            response = obj["response"];
            List<List<string>> JsonListBox = new List<List<string>>();
            if (response == null)
            {
                return null;
            }

            int count = obj["items"] == null ? 1 : (int)obj["items"];

            for (int i = 0; i < count; i++)
            {
                var item = response[i];
                List<string> temp = new List<string>();
                foreach (string data in item)
                {
                    temp.Add(data);
                }
                JsonListBox.Add(temp);
            }
            return JsonListBox;
        }
    }
}