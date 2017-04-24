using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestGUI
{
    class Core
    {

    }

    //public class PGPIN
    //{
    //    string _ni;
    //    int _maxFacts;
    //    int _maxDepth;

    //    public PGPIN()
    //    {

    //    }

    //    public void Init()
    //    {
    //        Queue<string> IdQuene = new Queue<string>();
    //        Dictionary<string, double> Values = new Dictionary<string, double>();
    //        List<string> Seen = new List<string>();

    //        IdQuene.Enqueue(_ni);
    //        while (IdQuene.Count > 0)
    //        {
    //            string nj = IdQuene.Dequeue();
    //            double FValue; //=
    //            Values.Add(nj, FValue);

    //        }
    //    }
    //}

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
