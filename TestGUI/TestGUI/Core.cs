using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using VK;

namespace TestGUI
{
    public class PGPIN
    {
        string _superUserName;
        string _superUserID;
        Dictionary<string, object> _superUserPrimaryInfo;
        Graph<string> _socialGraph;

        public PGPIN(string superUserName)
        {
            _superUserName = superUserName;
        }

        public void Init()
        {
            RequestPrimaryInfo(_superUserName);
            RequestFriendConnections(2);
            InitPGPIN();
        }

        void RequestPrimaryInfo(string superUserName, string parameters = null)
        {
            var result = Methods.UsersGet(new string[] { superUserName }, new string[] { parameters });
            _superUserID = result[0]["id"].ToString();
            _superUserPrimaryInfo = result[0];
            _superUserPrimaryInfo.Remove("id");
        }

        void RequestFriendConnections(int depth = 1, string parameters = "universities")
        {
            _socialGraph = new Graph<string>();
            _socialGraph.AddNode(_superUserID, _superUserPrimaryInfo);

            int socialGraphMark = 0;

            for (int i = 0; i < depth; i++)
            {
                List<string> targetIDs = new List<string>();
                List<Dictionary<string, object>> targetData = new List<Dictionary<string, object>>();

                if (i == 0)
                {
                    socialGraphMark = 1;
                    var result = Methods.FriendsGet(_superUserID, "hints", "", new string[] { parameters }); //TODO: адаптировать под 100 и более друзей

                    foreach (Dictionary<string, object> items in result)
                    {
                        targetIDs.Add(items["id"].ToString());
                        items.Remove("id");
                        targetData.Add(items);
                    }

                    result = Methods.FriendsGetMutual(_superUserID, targetIDs.ToArray());
                    AddNodesAndConnections(targetIDs, _superUserID, targetData);
                    AddNodesInterConnections(result);
                }
                else
                {
                    var executeParameters = new List<Dictionary<string, string>>();
                    Dictionary<string, string> queryParameters = new Dictionary<string, string>();
                    List<string> sourceIDs = new List<string>();

                    int ArrayFringe = _socialGraph.Count;

                    for (int j = socialGraphMark; j < ArrayFringe; j++) //TODO: лишняя итерация
                    {
                        var nodeID = _socialGraph.Nodes[j];
                        sourceIDs.Add(nodeID.Value);
                        queryParameters = parameters == null ?
                        new Dictionary<string, string>() {
                            { "method", "friends.get" },
                            { "parameters", "\"user_id\":" + nodeID.Value }
                        } :
                        new Dictionary<string, string>() {
                            { "method", "friends.get" },
                            { "parameters", "\"user_id\":" + nodeID.Value + ",\"fields\":\"" + parameters + "\""} 
                             //TODO: изменить логику parameters
                        };
                        executeParameters.Add(queryParameters);
                        if (j % 25 == 0 || _socialGraph.Count - 1 == j)
                        {
                            Thread.Sleep(350);
                            var data = Methods.Execute(executeParameters);

                            int c = 0;
                            foreach (Dictionary<string, dynamic> items in data)
                            {
                                targetIDs = new List<string>();
                                targetData = new List<Dictionary<string, object>>();
                                foreach (var response in items["items"])
                                {
                                    targetIDs.Add(response["id"].ToString());
                                    if (response.Count > 1)
                                    {
                                        response.Remove("id");
                                        targetData.Add(response);
                                    }
                                }
                                if (targetData.Count == 0) targetData = null;
                                AddNodesAndConnections(targetIDs, sourceIDs[c], targetData);
                                c++;
                            }
                            executeParameters = new List<Dictionary<string, string>>();
                            socialGraphMark = j; //для корректного чтения нод при глубине больше 1
                        }
                        //TODO: добавить соответствующий getMutual
                    }
                }

            }
        }

        void AddNodesAndConnections(List<string> targetIDs, string sourceID, List<Dictionary<string, object>> targetData = null)
        {
            for (int i = 0; i < targetIDs.Count; i++)
            {
                if (targetData != null) { _socialGraph.AddNode(targetIDs[i], targetData[i]); }
                else { _socialGraph.AddNode(targetIDs[i]); }

                _socialGraph.AddUndirectedEdge(sourceID, targetIDs[i], 1);
            }
        }

        void AddNodesInterConnections(List<Dictionary<string, object>> result)
        {
            //TODO: проверить второй getMutual
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
        }

        void InitPGPIN(int accessedFacts = 10)
        {
            Queue<Node<string>> IdQuene = new Queue<Node<string>>();
            Dictionary<string, Dictionary<string, double>> Values = new Dictionary<string, Dictionary<string, double>>();
            List<Node<string>> Seen = new List<Node<string>>();
            Dictionary<string, dynamic> njParameters;

            Dictionary<Node<string>, int> commonCost = _socialGraph.Distance(_superUserID);
            var ni = _socialGraph.Nodes[0]; //TODO: научить выдавать веса и считать расстояние
            Dictionary<string, dynamic> niParameters = ni.ParametersData;
            if (niParameters.Count == 0) throw new System.Exception();

            IdQuene.Enqueue(_socialGraph.Nodes[0]);
            Seen.Add(ni);
            //commonCost.Add(ni, 0);

            while (IdQuene.Count > 0) //TODO: добавить maxFacts
            {
                var nj = IdQuene.Dequeue();
                njParameters = nj.ParametersData;
                int count = 0;
                foreach (var elem in niParameters.Keys)
                {
                    try
                    {
                        if (niParameters[elem] == njParameters[elem]) count++;
                    }
                    catch { }
                }
                //if(distance == 0) Values.Add(v, 0)
                double FValue = count / niParameters.Count; // делить на расстояние
                                                            //Values.Add(nj, FValue); //структура - {"sex": {"female": цифра, "male": цифра}, "city": ...}

                foreach (var node in nj.Neighbors)
                {
                    if (!Seen.Contains(node))
                    {
                        IdQuene.Enqueue(node);
                        Seen.Add(node);
                        //commonCost.Add(node, 0);
                    }
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
