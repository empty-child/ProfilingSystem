﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ServiceModel.Syndication;
using EP;
using EP.Semantix;

namespace TextAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            //RssReader reader = new RssReader();
            //var rssItem = reader.Init();
            TextProcessor tProcessor = new TextProcessor(null);
            var textProcessingResult = tProcessor.Init();
            Matrix matrixCalculator = new Matrix();
            var factorizeResult = matrixCalculator.Factorize(textProcessingResult.Item2);
            ShowingFeature sf = new ShowingFeature(factorizeResult.Item1, factorizeResult.Item2, textProcessingResult.Item3, textProcessingResult.Item1);
            sf.Init();

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(textProcessingResult.Item1[i]);
            }

            Console.WriteLine("\nEnd");
            Console.ReadKey();
        }
    }

    //public class RssReader
    //{
    //    string[] rssUrl = new string[] { "http://rss.newsru.com/all_news/", "https://news.yandex.ru/index.rss",
    //        "https://news.yandex.ru/world.rss", "https://news.yandex.ru/finances.rss", "https://news.yandex.ru/incident.rss",
    //        "https://news.yandex.ru/politics.rss", "https://news.yandex.ru/society.rss" };

    //    public List<SyndicationItem> Init()
    //    {
    //        return RssOpen();
    //    }

    //    List<SyndicationItem> RssOpen()
    //    {
    //        List<SyndicationItem> rssItem = new List<SyndicationItem>();
    //        foreach (string rss in rssUrl)
    //        {
    //            XmlReader reader = XmlReader.Create(rss);
    //            SyndicationFeed feed = SyndicationFeed.Load(reader);
    //            reader.Close();
    //            foreach (SyndicationItem item in feed.Items)
    //            {
    //                rssItem.Add(item);
    //            }
    //        }
    //        return rssItem;
    //    }
    //}

    public class TextProcessor
    {
        Dictionary<string, int> allWords = new Dictionary<string, int>();
        List<Dictionary<string, int>> articleWords = new List<Dictionary<string, int>>();
        List<string> wordVector = new List<string>();
        List<string> articleTitle = new List<string>();
        double[,] wordMatrix;
        int c = 0;

        Processor processor = new Processor();

        Dictionary<string, Dictionary<string, string>> textItems;

        public TextProcessor(Dictionary<string, Dictionary<string, string>> textItems)
        {
            this.textItems = textItems;
        }

        public Tuple<List<string>, double[,], List<string>> Init()
        {
            TextParisng(textItems);
            CreateTextMatrix();
            return new Tuple<List<string>, double[,], List<string>>(wordVector, wordMatrix, articleTitle);
        }

        void TextParisng(Dictionary<string, Dictionary<string, string>> items)
        {
            foreach (var key in items.Keys)
            {
                foreach (var innerkey in items[key].Keys)
                {
                    string subject = key;
                    string summary = items[key][innerkey];

                    TextProcessing(subject + "." + innerkey, summary);
                }
            }
        }

        void TextProcessing(string id, string body)
        {
            string[] words = body.Split(new[] { ' ', ',', ':', '?', '!', '.', '"', '-', '—' }, StringSplitOptions.RemoveEmptyEntries);
            articleWords.Add(new Dictionary<string, int>());
            articleTitle.Add(id);

            foreach (string word in words)
            {
                AnalysisResult ar = processor.Process(new SourceOfAnalysis(word));
                Token t = ar.FirstToken;
                if (!(t is TextToken)) continue;
                if (t.Morph.Class.IsConjunction || t.Morph.Class.IsMisc ||
                    t.Morph.Class.IsUndefined || t.Morph.Class.IsPersonalPronoun ||
                    t.Morph.Class.IsPreposition || t.Morph.Class.IsPronoun) continue;
                string norm = t.GetNormalCaseText(null, true);
                //string norm = word.ToLower();
                if (allWords.ContainsKey(norm))
                {
                    allWords[norm] += 1;
                }
                else
                {
                    allWords.Add(norm, 1);
                }

                if (articleWords[c].ContainsKey(norm))
                {
                    articleWords[c][norm] += 1;
                }
                else
                {
                    articleWords[c].Add(norm, 1);
                }
            }
            c++;
        }

        void CreateTextMatrix()
        {
            foreach (string word in allWords.Keys)
            {
                if (allWords[word] > 3 && allWords[word] < articleWords.Count * 0.6)
                {
                    wordVector.Add(word);
                }
            }

            wordMatrix = new double[articleWords.Count, wordVector.Count];

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
        }
    }

    public class Matrix
    {
        public double MatrixCost(double[,] a, double[,] b)
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

        public double[,] Transpose(double[,] source)
        {
            double[,] output = new double[source.GetLength(1), source.GetLength(0)];
            for (int i = 0; i < source.GetLength(0); i++)
            {
                for (int j = 0; j < source.GetLength(1); j++)
                {
                    output[j, i] = source[i, j];
                }
            }
            return output;
        }

        public int[,] Multiply(int[,] a, int[,] b)
        {
            int[,] output = new int[a.GetLength(0), b.GetLength(1)];
            if (a.GetLength(1) == b.GetLength(0))
            {
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    for (int j = 0; j < b.GetLength(1); j++)
                    {
                        output[i, j] = 0;
                        for (int k = 0; k < a.GetLength(1); k++) // OR k<b.GetLength(0)
                            output[i, j] = output[i, j] + a[i, k] * b[k, j];
                    }
                }
            }
            return output;
        }

        public double[,] Multiply(double[,] a, double[,] b)
        {
            double[,] output = new double[a.GetLength(0), b.GetLength(1)];
            if (a.GetLength(1) == b.GetLength(0))
            {
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    for (int j = 0; j < b.GetLength(1); j++)
                    {
                        output[i, j] = 0;
                        for (int k = 0; k < a.GetLength(1); k++) // OR k<b.GetLength(0)
                            output[i, j] = output[i, j] + a[i, k] * b[k, j];
                    }
                }
            }
            return output;
        }

        public double[,] SeqDivision(int[,] a, int[,] b)
        {
            double[,] output = new double[a.GetLength(0), a.GetLength(1)];
            if (a.GetLength(1) == b.GetLength(1) && a.GetLength(0) == b.GetLength(0))
            {
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    for (int j = 0; j < b.GetLength(1); j++)
                    {
                        output[i, j] = (double)a[i, j] / (double)b[i, j];
                    }
                }
            }
            return output;
        }

        public double[,] SeqDivision(double[,] a, double[,] b)
        {
            double[,] output = new double[a.GetLength(0), a.GetLength(1)];
            if (a.GetLength(1) == b.GetLength(1) && a.GetLength(0) == b.GetLength(0))
            {
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    for (int j = 0; j < b.GetLength(1); j++)
                    {
                        output[i, j] = a[i, j] / b[i, j];
                    }
                }
            }
            return output;
        }

        public double[,] SeqMultiply(double[,] a, double[,] b)
        {
            double[,] output = new double[a.GetLength(0), a.GetLength(1)];
            if (a.GetLength(1) == b.GetLength(1) && a.GetLength(0) == b.GetLength(0))
            {
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    for (int j = 0; j < b.GetLength(1); j++)
                    {
                        output[i, j] = a[i, j] * b[i, j];
                    }
                }
            }
            return output;
        }

        public Tuple<double[,], double[,]> Factorize(double[,] v, int pc = 10, int iter = 20)
        {
            int ic = v.GetLength(0);
            int fc = v.GetLength(1);
            Random random = new Random();

            double[,] w = new double[ic, pc];
            double[,] h = new double[pc, fc];

            double[,] wh;

            for (int i = 0; i < ic; i++)
            {
                for (int j = 0; j < pc; j++)
                {
                    w[i, j] = random.NextDouble();
                }
            }

            for (int i = 0; i < pc; i++)
            {
                for (int j = 0; j < fc; j++)
                {
                    h[i, j] = random.NextDouble();
                }
            }



            for (int i = 0; i < iter; i++)
            {
                wh = Multiply(w, h);

                var cost = MatrixCost(v, wh);
                if (cost == 0 || double.IsNaN(cost)) break;

                double[,] hn = Multiply(Transpose(w), v);
                double[,] hd = Multiply(Multiply(Transpose(w), w), h);

                h = SeqDivision(SeqMultiply(h, hn), hd);

                double[,] wn = Multiply(v, Transpose(h));
                double[,] wd = Multiply(Multiply(w, h), Transpose(h));

                w = SeqDivision(SeqMultiply(w, wn), wd);
            }

            //int[,] outputH = new int[h.GetLength(0), h.GetLength(1)];
            //int[,] outputW = new int[w.GetLength(0), w.GetLength(1)];

            //for (int i = 0; i < outputH.GetLength(0); i++)
            //{
            //    for (int j = 0; j < outputH.GetLength(1); j++)
            //    {
            //        outputH[i, j] = (int)Math.Round(h[i, j]);
            //    }
            //}

            //for (int i = 0; i < outputW.GetLength(0); i++)
            //{
            //    for (int j = 0; j < outputW.GetLength(1); j++)
            //    {
            //        outputW[i, j] = (int)Math.Round(w[i, j]);
            //    }
            //}

            return new Tuple<double[,], double[,]>(w, h);
        }
    }

    public class TopPatterns
    {
        public double w { get; set; }
        public int i { get; set; }
        public string title { get; set; }
    }

    public class ShowingFeature
    {
        double[,] w, h;
        List<string> articleTitle, wordVector, patternNames = new List<string>();
        List<List<TopPatterns>> topPatterns = new List<List<TopPatterns>>();
        Dictionary<string, double> slist, top;

        public ShowingFeature(double[,] w, double[,] h, List<string> articleTitle, List<string> wordVector)
        {
            this.w = w;
            this.h = h;
            this.articleTitle = articleTitle;
            this.wordVector = wordVector;
        }

        public List<string> Init()
        {
            StreamWriter sw = new StreamWriter("features.txt");
            List<string> groupsID = new List<string>();

            for (int i = 0, pc = h.GetLength(0); i < pc; i++)
            {
                slist = new Dictionary<string, double>();
                patternNames = new List<string>();
                topPatterns.Add(new List<TopPatterns>());

                for (int j = 0, wc = h.GetLength(1); j < wc; j++)
                {
                    slist.Add(wordVector[j], h[i, j]);
                }

                top = slist.OrderByDescending(pair => pair.Value).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (string word in top.Keys)
                {
                    sw.WriteLine(word);
                    patternNames.Add(word);
                }

                slist = new Dictionary<string, double>();
                for (int j = 0; j < articleTitle.Count(); j++)
                {
                    string temp = articleTitle[j];
                    while (slist.ContainsKey(temp))
                    {
                        temp += "(1)";
                    }
                    slist.Add(temp, w[j, i]);
                    topPatterns[i].Add(new TopPatterns { w = w[j, i], i = i, title = articleTitle[j] });
                }

                top = slist.OrderByDescending(pair => pair.Value).Take(6).ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (string word in top.Keys)
                {
                    sw.WriteLine(word);
                    groupsID.Add(word);
                }

                sw.Write("\n");
            }
            sw.Close();
            return groupsID;
        }
    }

}
