using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocialNetworksLibrary;

namespace TestGUI
{
    public class VisualСortex
    {
        private string _filepath;
        string _token = TestGUI.Properties.Settings.Default.FindFaceToken;
        string _id = TestGUI.Properties.Settings.Default.FindFaceID;

        public VisualСortex(string filepath)
        {
            _filepath = filepath;
        }

        public string FindPerson()
        {
            //string response = JSONParser.PostMultipart("https://webapi.findface.ru/v2/search/detect?user_id="+_id, token: "Bearer " + _token, filepath: _filepath);
            Dictionary<string, object> clearJson = JSONParser.JsonParsing(System.IO.File.ReadAllText(@"text.txt")); //System.IO.File.ReadAllText(@"text.txt")

            List<dynamic> bboxesList = (List<dynamic>)clearJson["bboxes"];
            Dictionary<string, object> bboxesData = (Dictionary<string, object>)bboxesList[0];

            for (int i = 0; i < 5; i++)
            {
                string response = Utils.Request(string.Format("https://webapi.findface.ru/v2/search/list3?hash={0}&image_hash={1}&n=100&user_id={2}&x1={3}&x2={4}&y1={5}&y2={6}", bboxesData["hash"].ToString(), clearJson["image_hash"].ToString(), _id, bboxesData["x1"].ToString(), bboxesData["x2"].ToString(), bboxesData["y1"].ToString(), bboxesData["y2"].ToString()), addHeader: true, headerData: "Authorization: Bearer " + _token);
                Thread.Sleep(5000);
                if (response != null)
                {
                    break;
                }
                if (i == 4) return null;
            }

            List<dynamic> clearJson2 = (List<dynamic>)JSONParser.JsonParsing(System.IO.File.ReadAllText(@"2.txt"));
            Dictionary<string, object> userData = (Dictionary<string, object>)clearJson2[0];

            return userData["user_id"].ToString();
        }
    }
}

