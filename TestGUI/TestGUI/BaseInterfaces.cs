using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCVandUI
{
    public interface IAuthentication
    {
        string AppID { get; }
        string SecureKey { get; }
        string ServerUrl { get; }

        string RequestToken { get; set; }
        string AccessToken { get; set; }

        string GetAuthData();
        void WriteAccessToken(string token);
    }

    public interface IAPIRequest
    {
        string ServerUrl { get; }
        string Accesstoken { get; }
        string OutputData { get; }

        string Parameters { get; set; }
        string MethodName { get; set; }

        string ApiRequest();
    }
}
