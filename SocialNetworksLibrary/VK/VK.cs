using System;
using System.Collections.Generic;
using TestGUI;

using VkProperties = VK.Properties.Settings;

namespace SocialNetworksLibrary
{
    public class Authenticate : IAuthentication
    {
        private string _appId;
        private string _secureKey;
        private string _serverUrl;
        private string _requestToken;
        private string _accessToken;

        private string _scope;

        public Authenticate()
        {
            _appId = VkProperties.Default.AppID;
            _secureKey = VkProperties.Default.SecureKey;
            AccessToken = VkProperties.Default.AccessToken;
            _serverUrl = "https://oauth.vk.com/authorize?client_id=";
        }

        public string AppID
        {
            get { return _appId; }
        }

        public string SecureKey
        {
            get { return _secureKey; }
        }

        public string ServerUrl
        {
            get { return _serverUrl; }
        }

        public string RequestToken
        {
            get { return _requestToken; }
            set { _requestToken = value; }
        }

        public string AccessToken
        {
            get { return _accessToken; }
            set { _accessToken = value; }
        }

        public string Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        public void WriteAccessToken(string token)
        {
            if (token != "" || token != null)
            {
                AccessToken = token;
                VkProperties.Default.AccessToken = token;
                VkProperties.Default.Save();
            }
        }

        public string GetAuthData()
        {
            if (_accessToken == "" || _accessToken == null)
            {
                return string.Format("{0}{1}&display=page&redirect_uri=https://oauth.vk.com/blank.html&scope={2}&response_type=token&v=5.63", _serverUrl, _appId, _scope);
            }
            return null;
        }
    }

    public class InitRequest : IAPIRequest
    {
        private string _accessToken;
        private string _parameters;
        private string _serverUrl;
        private string _outputData;
        private string _methodName;

        public InitRequest()
        {
            _serverUrl = "https://api.vk.com/method/";
            _accessToken = VkProperties.Default.AccessToken;
        }

        public string Accesstoken
        {
            get { return _accessToken; }
        }

        public string Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string ServerUrl
        {
            get { return _serverUrl; }
        }

        public string MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }

        public string OutputData
        {
            get { return _outputData; }
        }

        public string ApiRequest()
        {
            string Response = Utils.Request(string.Format("{0}{1}?{2}&v=5.63&access_token={3}", _serverUrl, _methodName, _parameters, _accessToken), "POST");
            _outputData = Response;
            return Response;
        }
    }

    public static class Methods
    {
        #region Methods for PGPI-N

        //Возвращает расширенную информацию о пользователях. 
        public static List<object> UsersGet(string[] userIDs, string[] fields, string nameCase = "nom")
        {
            string Parameters = string.Format("user_ids={0}&fields={1}&name_case={2}",
                string.Join(",", userIDs), string.Join(",", fields), nameCase);
            return JsonParsing("users.get", Parameters);
        }

        //Возвращает список идентификаторов пользователей и публичных страниц, которые входят в список подписок пользователя. 
        public static List<object> UsersGetSubscriptions(string userID, string[] fields)
        {
            string Parameters = string.Format("user_id={0}&fields={1}",
                userID, string.Join(",", fields));
            return JsonParsing("users.getSubscriptions", Parameters);
        }

        ////Возвращает список идентификаторов друзей пользователя или расширенную информацию о друзьях пользователя 
        public static List<object> FriendsGet(string userID, string order, string count, string[] fields, string nameCase = "nom")
        {
            string Parameters = string.Format("user_id={0}&order={1}&fields={3}",
                userID, order, count, string.Join(",", fields), nameCase);
            return JsonParsing("friends.get", Parameters);
        }

        //Возвращает список идентификаторов общих друзей между парой пользователей
        public static List<object> FriendsGetMutual(string sourceID, string[] targetIDs)
        {
            string Parameters = string.Format("source_uid={0}&target_uids={1}",
                sourceID, string.Join(",", targetIDs));
            return JsonParsing("friends.getMutual", Parameters);
        }

        #endregion

        public static List<object> Execute(List<Dictionary<string, string>> data)
        {
            string code = null;
            foreach (var query in data)
            {
                if (query["parameters"] != null)
                {
                    code += "API." + query["method"] + "({" + query["parameters"] + "}),";
                }
            }
            string Parameters = string.Format("code=return [{0}];",
                code);
            return JsonParsing("execute", Parameters);
        }

        public static List<object> Execute(string code)
        {
            return JsonParsing("execute", "code=" + code);
        }

        public static List<object> JsonParsing(string method, string parameters)
        {
            InitRequest usersGet = new InitRequest();
            usersGet.MethodName = method;
            usersGet.Parameters = parameters;
            string result = usersGet.ApiRequest();
            //return JSONParser.JsonParsing(result);
            var response = (Dictionary<string, object>)JSONParser.JsonParsing(result);
            var responseList = (List<object>)response["response"];
            return responseList;
        }
    }

}
