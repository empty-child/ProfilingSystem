using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ServiceModel.Syndication;

namespace TextAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            RssReader reader = new RssReader();
            reader.RssOpen();

            Console.WriteLine("End");
            Console.ReadKey();
        }
    }

    class RssReader
    {
        string rssUrl = @"http://rss.newsru.com/all_news/";
        Dictionary<string, int> allWords = new Dictionary<string, int>();
        List<Dictionary<string, int>> articleWords = new List<Dictionary<string, int>>();
        int c = 0;

        public void RssOpen()
        {
            XmlReader reader = XmlReader.Create(rssUrl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            foreach (SyndicationItem item in feed.Items)
            {
                string subject = item.Title.Text;
                string summary = item.Summary.Text;

                TextProcessing(subject, summary);
            }
            
        }

        void TextProcessing(string title, string body)
        {
            string[] words = body.Split(new[] { ' ', ',', ':', '?', '!', '.', '"' }, StringSplitOptions.RemoveEmptyEntries);
            articleWords.Add(new Dictionary<string, int>());
            foreach (string word in words)
            {
                if (allWords.ContainsKey(word))
                {
                    allWords[word] += 1;
                }
                else
                {
                    allWords.Add(word, 1);
                }

                if (articleWords[c].ContainsKey(word))
                {
                    articleWords[c][word] += 1;
                }
                else
                {
                    articleWords[c].Add(word, 1);
                }
            }
            c++;
        }
    }

    class Utils
    {
        public static string ReadAllSettings()
        {
            try
            {
                string text = File.ReadAllText(@"settings.txt");

                return text;
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка прочтения файла настроек: ");
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
