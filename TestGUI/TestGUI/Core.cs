using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using SocialNetworksLibrary;

namespace TestGUI
{
    public class PGPI
    {
        string _superUserName;
        string _superUserID;
        Dictionary<string, object> _superUserPrimaryInfo;
        Graph<string> _socialGraph;

        public PGPI(string superUserName)
        {
            _superUserName = superUserName;
        }

        public void Init()
        {
            RequestPrimaryInfo(_superUserName, "universities");
            RequestFriendConnections(2, "universities");
            InitPGPIN();
            //InitPGPIG();
        }

        void RequestPrimaryInfo(string superUserName, string parameters = null)
        {
            var result = Methods.UsersGet(new string[] { superUserName }, new string[] { parameters });
            var answer = (Dictionary<string, object>)result[0];
            _superUserID = answer["id"].ToString(); //TODO: унифицировать форму параметров
            _superUserPrimaryInfo = answer;
            _superUserPrimaryInfo.Remove("id");
        }

        void RequestFriendConnections(int depth = 1, string parameters = null)
        {
            _socialGraph = new Graph<string>();
            _socialGraph.AddNode(_superUserID, _superUserPrimaryInfo);

            int socialGraphMark = 0;

            for (int i = 0; i < depth; i++)
            {
                List<string> targetIDs = new List<string>();
                List<Dictionary<string, object>> targetData = new List<Dictionary<string, object>>();

                var executeParameters = new List<Dictionary<string, string>>();
                Dictionary<string, string> queryParameters = new Dictionary<string, string>();
                List<string> sourceIDs = new List<string>();

                int ArrayFringe = _socialGraph.Count;

                for (int j = socialGraphMark; j < ArrayFringe; j++)
                {
                    executeParameters.Add(Utils.ExecuteRequestGeneration("friends.get", _socialGraph, j, sourceIDs, true, parameters));
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
                            GraphUtils.AddNodesAndConnections(_socialGraph, targetIDs, sourceIDs[c], targetData);
                            //TODO: восстановить соответствующий getMutual
                            c++;
                        }
                        executeParameters = new List<Dictionary<string, string>>();
                        socialGraphMark = j == 0 ? 1 : j; //для корректного чтения нод при глубине больше 1
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
            double FValue = 0;

            Dictionary<Node<string>, int> commonCost = _socialGraph.Distance(_superUserID);
            var ni = _socialGraph.Nodes[0];
            Dictionary<string, dynamic> niParameters = ni.ParametersData;
            if (niParameters.Count == 0) throw new System.Exception();

            IdQuene.Enqueue(_socialGraph.Nodes[0]);
            Seen.Add(ni);

            while (IdQuene.Count > 0) //TODO: добавить maxFacts (необ)
            {
                var nj = IdQuene.Dequeue();
                njParameters = nj.ParametersData;
                int count = 0;
                foreach (var elem in niParameters.Keys)
                {
                    try
                    {
                        if (niParameters[elem] == njParameters[elem]) count++; //TODO: обработка вложенных словарей
                    }
                    catch { }
                }
                if (commonCost[nj] != 0) FValue = (count / niParameters.Count) / commonCost[nj];
                else FValue = 0;
                foreach (var key in nj.ParametersData.Keys)
                {
                    if (nj.ParametersData[key] is List<Dictionary<string, object>>)
                    {
                        var w = nj.ParametersData[key];
                        //foreach (var item in nj.ParametersData[key])
                        //{
                        //    string temp = "";
                        //    if (Values.ContainsKey(temp))
                        //    {
                        //    }
                        //}
                    }
                    if (Values.ContainsKey(key))
                    {

                        if (Values[key].ContainsKey(nj.ParametersData[key].ToString())) //TODO: обработка вложенных словарей
                        {

                            Values[key][nj.ParametersData[key].ToString()] += FValue;
                        }
                        else
                        {
                            Values[key].Add(nj.ParametersData[key].ToString(), FValue);
                        }
                    }
                    else
                    {
                        Values.Add(key, new Dictionary<string, double> { { nj.ParametersData[key].ToString(), FValue } });
                    }
                }
                //Values: структура - {"sex": {"female": цифра, "male": цифра}, "city": ...}

                foreach (var node in nj.Neighbors)
                {
                    if (!Seen.Contains(node))
                    {
                        IdQuene.Enqueue(node);
                        Seen.Add(node);
                    }
                }
            }
        }

        void InitPGPIG()
        {
            int socialGraphMark = 0;
            List<string> targetIDs = new List<string>();
            List<Dictionary<string, object>> targetData = new List<Dictionary<string, object>>();
            var executeParameters = new List<Dictionary<string, string>>();
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            List<string> sourceIDs = new List<string>();

            int ArrayFringe = _socialGraph.Count;

            for (int j = socialGraphMark; j < ArrayFringe; j++)
            {
                executeParameters.Add(Utils.ExecuteRequestGeneration("groups.get", _socialGraph, j, sourceIDs));
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
                            if (_socialGraph.GroupsMembership.ContainsKey(response["id"].ToString()))
                            {
                                _socialGraph.GroupsMembership[response["id"].ToString()].Add(sourceIDs[c]);
                            }
                            else
                            {
                                _socialGraph.GroupsMembership.Add(response["id"].ToString(), new List<string> { sourceIDs[c] });

                            }
                        }
                        //if (targetData.Count == 0) targetData = null;
                        //GraphUtils.AddNodesAndConnections(_socialGraph, targetIDs, sourceIDs[c], targetData);
                        c++;
                    }
                    executeParameters = new List<Dictionary<string, string>>();
                }
            }

            foreach (var group in _socialGraph.GroupsMembership.Keys)
            {
                foreach (var user in _socialGraph.GroupsMembership[group])
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Thread.Sleep(350);
                        var data = Methods.Execute(WebUtility.UrlEncode("var response={};\nvar posts = API.wall.get({\"owner_id\":\"" + "-" + group + "\",\"count\":24});\nvar b = 0;\nvar s = posts.items.length;\nwhile (s != 0){\nvar data = API.likes.isLiked({ \"user_id\":\"" + user + "\",\"type\":\"post\",\"owner_id\":\"" + "-" + group + "\",\"item_id\":posts.items[b].id});\nresponse=response+[{\"item_id\":posts.items[b].id, \"liked\":data.liked}];\nb =b+1;\ns=s-1;\n}\nreturn {\"items\":response};\n"));

                        GraphUtils.AddLikesAndPublications(_socialGraph, group, user, data);
                    }
                }
            }
        }

        void PGPIG()
        {

        }
    }

    public static class GraphUtils
    {
        public static void AddNodesAndConnections(Graph<string> _socialGraph, List<string> targetIDs, string sourceID, List<Dictionary<string, object>> targetData = null)
        {
            for (int i = 0; i < targetIDs.Count; i++)
            {
                if (targetData != null) { _socialGraph.AddNode(targetIDs[i], targetData[i]); }
                else { _socialGraph.AddNode(targetIDs[i]); }

                _socialGraph.AddUndirectedEdge(sourceID, targetIDs[i], 1);
            }
        }

        public static void AddNodesInterConnections(Graph<string> _socialGraph, List<Dictionary<string, object>> result)
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

        public static void AddGroupsAndMembership(Graph<string> _socialGraph, List<string> groups)
        {
            ///
        }

        public static void AddLikesAndPublications(Graph<string> _socialGraph, string group, string user, List<object> likesAndPublications)
        {
            foreach (Dictionary<string, dynamic> items in likesAndPublications)
            {
                string itemID = items["item_id"].ToString();
                bool liked = Convert.ToBoolean(items["liked"]);
                if (_socialGraph.Publications.ContainsKey(group))
                {
                    if (!_socialGraph.Publications[group].Contains(itemID)) _socialGraph.Publications[group].Add(itemID);
                }
                else
                {
                    _socialGraph.Publications.Add(group, new List<string> { itemID });
                }
                if (liked)
                {
                    if (_socialGraph.LikesAndViewes.ContainsKey(itemID))
                    {
                        if (!_socialGraph.LikesAndViewes[itemID].Contains(user)) _socialGraph.LikesAndViewes[itemID].Add(user);
                    }
                    else
                    {
                        _socialGraph.LikesAndViewes.Add(itemID, new List<string> { user });
                    }
                }
            }
        }
    }

    public static class Utils
    {
        public static Dictionary<string, string> ExecuteRequestGeneration(string method, Graph<string> _socialGraph, int count, List<string> sourceIDs, bool withParameters = false, string parameters = null)
        {
            //var executeParameters = new List<Dictionary<string, string>>();
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();

            var nodeID = _socialGraph.Nodes[count];
            sourceIDs.Add(nodeID.Value);
            queryParameters = parameters == null ?
                new Dictionary<string, string>() {
                    { "method", method },
                    { "parameters", "\"user_id\":" + nodeID.Value }
                } :
                new Dictionary<string, string>() {
                    { "method", method },
                    { "parameters", "\"user_id\":" + nodeID.Value + ",\"fields\":\"" + parameters + "\""}
                }; //TODO: изменить логику parameters
            //executeParameters.Add(queryParameters);
            return queryParameters;
        }

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

        public static string PostMultipart(string target, string token = "", string filepath = null)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            string req = target;
            WebRequest request = WebRequest.Create(req);
            request.Method = "POST";

            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Headers.Add("Authorization: " + token);

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(boundarybytes, 0, boundarybytes.Length);

            //string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

            //string formitem = string.Format(formdataTemplate, "user_id", "1414112");
            //byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
            //dataStream.Write(formitembytes, 0, formitembytes.Length);

            //dataStream.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, "image", filepath, "image/jpeg");
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            dataStream.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                dataStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            dataStream.Write(trailer, 0, trailer.Length);
            dataStream.Close();

            try
            {
                WebResponse response = request.GetResponse();
                Stream dataStream2 = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream2);
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
