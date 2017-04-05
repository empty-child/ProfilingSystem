using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core
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

    public class Core
    {

    }

    public static class Utils
    {
        public static string Request(string target, string requestMethod = "GET", bool addHeader = false, string headerData = "")
        {
            string req = target;
            WebRequest request = WebRequest.Create(req);
            request.Method = requestMethod;
            if (requestMethod == "POST")
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(headerData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }
            else if (addHeader)
            {
                request.Headers.Add(headerData);
            }
            try
            {
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                return responseFromServer;
            }
            catch (WebException e)
            {
                //DialogResult error = MessageBox.Show(e.Message, "Ошибка", MessageBoxButtons.OK);
                return e.ToString();
            }
        }
    }
}
