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
            var rssItem = reader.Init();
            TextProcessor tProcessor = new TextProcessor(rssItem);

            Console.WriteLine("\nEnd");
            Console.ReadKey();
        }
    }

    class RssReader
    {
        string[] rssUrl = new string[] { "http://rss.newsru.com/all_news/", "https://news.yandex.ru/index.rss",
            "https://news.yandex.ru/world.rss", "https://news.yandex.ru/finances.rss", "https://news.yandex.ru/incident.rss",
            "https://news.yandex.ru/politics.rss", "https://news.yandex.ru/society.rss" };

        public List<SyndicationItem> Init()
        {
            return RssOpen();
        }

        //public RssReader()
        //{
        //    RssOpen();

        //    for (int i = 0; i < 10; i++)
        //    {
        //        Console.WriteLine(wordVector[i]);
        //    }
        //}

        List<SyndicationItem> RssOpen()
        {
            List<SyndicationItem> rssItem = new List<SyndicationItem>();
            foreach (string rss in rssUrl)
            {
                XmlReader reader = XmlReader.Create(rss);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();
                foreach (SyndicationItem item in feed.Items)
                {
                    rssItem.Add(item);
                }
            }
            return rssItem;

            //CreateMatrix();
        }
    }

    class TextProcessor
    {
        Dictionary<string, int> allWords = new Dictionary<string, int>();
        List<Dictionary<string, int>> articleWords = new List<Dictionary<string, int>>();
        List<string> wordVector = new List<string>();
        int[,] wordMatrix;
        int c = 0;

        public TextProcessor(List<SyndicationItem> rssItem)
        {
            var _rssItem = rssItem;
            RssProcessing(_rssItem);
        }

        void RssProcessing(List<SyndicationItem> _rssItem)
        {
            foreach (SyndicationItem item in _rssItem)
            {
                string subject = item.Title.Text;
                string summary = item.Summary.Text;

                TextProcessing(subject, summary);
            }
        }

        void TextProcessing(string title, string body)
        {
            string[] words = body.Split(new[] { ' ', ',', ':', '?', '!', '.', '"', '-', '—' }, StringSplitOptions.RemoveEmptyEntries);
            articleWords.Add(new Dictionary<string, int>());
            foreach (string word in words)
            {
                string lowerWord = word.ToLower();
                if (allWords.ContainsKey(lowerWord))
                {
                    allWords[lowerWord] += 1;
                }
                else
                {
                    allWords.Add(lowerWord, 1);
                }

                if (articleWords[c].ContainsKey(lowerWord))
                {
                    articleWords[c][lowerWord] += 1;
                }
                else
                {
                    articleWords[c].Add(lowerWord, 1);
                }
            }
            c++;
        }
    }

    class Matrix
    {

        void CreateMatrix()
        {
            foreach (string word in allWords.Keys)
            {
                if (allWords[word] > 3 && allWords[word] < articleWords.Count * 0.6)
                {
                    wordVector.Add(word);
                }
            }

            wordMatrix = new int[articleWords.Count, wordVector.Count];

            for (int x = 0; x < articleWords.Count; x++)
            {
                for (int y = 0; y < wordVector.Count; y++)
                {

                    if (articleWords[x].ContainsKey(wordVector[y]) && articleWords[x][wordVector[y]] > 0)
                    {
                        wordMatrix[x, y] = articleWords[x][wordVector[y]];
                    }
                    else wordMatrix[x, y] = 0;
                }
            }
            var a = wordMatrix.GetLength(1);
        }

        double MatrixCost(int[,] a, int[,] b)
        {
            double cost = 0;
            for (int x = 0; x < a.GetLength(0); x++)
            {
                for (int y = 0; y < a.GetLength(1); y++)
                {
                    cost += Math.Pow(a[x, y] - b[x, y], 2);
                }
            }
            return cost;
        }

        void Factorize(int[,] a, int pc = 10, int iter = 50)
        {
            int ic = a.GetLength(0);
            int fc = a.GetLength(1);
            Random random = new Random();

            int[,] w = new int[ic, pc];
            int[,] h = new int[pc, fc];

            int[,] wh;

            for (int i = 0; i < ic; i++)
            {
                for (int j = 0; j < pc; j++)
                {
                    w[i, j] = random.Next();
                }
            }

            for (int i = 0; i < pc; i++)
            {
                for (int j = 0; j < fc; j++)
                {
                    h[i, j] = random.Next();
                }
            }

            wh = new int[w.GetLength(0), h.GetLength(1)];
            if (w.GetLength(1) == h.GetLength(0))
            {
                for (int i = 0; i < w.GetLength(0); i++)
                {
                    for (int j = 0; j < h.GetLength(1); j++)
                    {
                        wh[i, j] = 0;
                        for (int k = 0; k < w.GetLength(1); k++) // OR k<b.GetLength(0)
                            wh[i, j] = wh[i, j] + w[i, k] * h[k, j];
                    }
                }
            }

            for (int i = 0; i < iter; i++)
            {
                var cost = MatrixCost(a, wh);
                if (cost == 0) break;


            }
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
