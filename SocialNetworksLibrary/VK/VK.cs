using System;
using TestGUI;

using VkProperties = VK.Properties.Settings;

namespace VK
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
            string Response = Utils.Request(string.Format("{0}{1}?{2}&access_token={3}", _serverUrl, _methodName, _parameters, _accessToken), "POST");
            _outputData = Response;
            return Response;
        }
    }


}
