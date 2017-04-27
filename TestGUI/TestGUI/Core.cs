using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK;

namespace TestGUI
{
    class Core
    {

    }

    public class PGPIN
    {
        //string _ni;
        //int _maxFacts;
        //int _maxDepth;

        string _superUserName;
        string _superUserID;
        List<Dictionary<string, object>> _superUserPrimaryInfo;
        Graph<string> _socialGraph;

        public PGPIN(string superUserName)
        {
            _superUserName = superUserName;
        }

        public void Init()
        {
            //Queue<string> IdQuene = new Queue<string>();
            //Dictionary<string, double> Values = new Dictionary<string, double>();
            //List<string> Seen = new List<string>();

            //IdQuene.Enqueue(_ni);
            //while (IdQuene.Count > 0)
            //{
            //    string nj = IdQuene.Dequeue();
            //    double FValue; //=
            //    Values.Add(nj, FValue);

            //}
            ResponsePrimaryInfo(_superUserName);
            CreateSocialGraph();
        }

        void ResponsePrimaryInfo(string superUserName, string parameters = null)
        {
            var result = Methods.UsersGet(new string[] { superUserName }, new string[] { parameters });
            _superUserID = result[0]["id"].ToString();
            _superUserPrimaryInfo = result;
        }

        void CreateSocialGraph(int depth = 1, string parameters = null)
        {
            _socialGraph = new Graph<string>();
            _socialGraph.AddNode(_superUserID);

            for (int i = 0; i < depth; i++)
            {

            }
            var result = Methods.FriendsGet(_superUserID, "hints", "", new string[] { parameters }); //TODO: адаптировать под 100 и более друзей
            List<string> targetIDs = new List<string>();
            foreach (Dictionary<string, object> items in result)
            {
                targetIDs.Add(items["id"].ToString());
            }

            foreach (string elem in targetIDs)
            {
                _socialGraph.AddNode(elem);
                _socialGraph.AddUndirectedEdge(_superUserID, elem, 1);
            }

            result = Methods.FriendsGetMutual(_superUserID, targetIDs.ToArray());
            foreach (Dictionary<string, dynamic> elem in result)
            {
                string mainID = elem["id"].ToString();
                foreach (Dictionary<string, dynamic> innerElem in elem["common_friends"])
                {
                    foreach (string value in innerElem.Values)
                    {
                        if (!_socialGraph.Contains(value)) _socialGraph.AddNode(value);
                        _socialGraph.AddUndirectedEdge(mainID, value, 1);
                    }
                }
            }

            var executeParameters = new List<Dictionary<string, string>>();
            Dictionary<string, string> queryParameters = new Dictionary<string,string>();
            for (int i = 0; i < _socialGraph.Count; i++)
            {
                var nodeID = _socialGraph.Nodes[i];
                queryParameters = new Dictionary<string, string>() {
                    { "method", "friends.get" },
                    { "parameters", "\"user_id\":" + nodeID.Value }
                };
                executeParameters.Add(queryParameters);
                if (_socialGraph.Count % 25 == 0 || _socialGraph.Count - 1 == i)
                {
                    Thread.Sleep(350);
                    var data = Methods.Execute(executeParameters);
                }
            }
            
        }
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
