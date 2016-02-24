using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace PhraseSet
{
    class Universe
    {
        public static int CHAR_NUM = 26;
        private Dictionary<String, int> map;
        private Dictionary<String, double> clarityMap;
        private Dictionary<double, double> cdf;
        private Dictionary<double, int> cdfIndex;
        private KeyValuePair<double, double>[] cdfArray;
        private int totalWordNum;
        private double[] bigramFreq;
        //private Layout layout; 
        public Universe(String fileName, string clarityFilename)
        {
            StreamReader srd;
            try
            {
                srd = File.OpenText(fileName);
            }
            catch
            {
                Console.WriteLine("File {0} open failed!", fileName);
                return;
            }
            map = new Dictionary<String, int>();
            bigramFreq = new double[CHAR_NUM * CHAR_NUM];
            for (int i = 0; i < CHAR_NUM * CHAR_NUM; i++)
            {
                bigramFreq[i] = 0;
            }
            totalWordNum = 0;
            while (srd.Peek() != -1)
            {
                String s = srd.ReadLine().ToLower();                
                String[] words = s.Split(',');
                Debug.Assert(words.Length == 2);
                Debug.Assert(!map.ContainsKey(words[0]));                
                int num = Convert.ToInt32(words[1]);
                addBigramInfo(words[0], num);
                map.Add(words[0], num);
                totalWordNum += num;
            }
            srd.Close();
            double tot = 0;
            for (int i = 0; i < CHAR_NUM * CHAR_NUM; i++)
            {
                tot += bigramFreq[i];
            }
            for (int i = 0; i < CHAR_NUM * CHAR_NUM; i++)
            {
                bigramFreq[i] /= tot;
            }
            Console.WriteLine("get {0} words successfully!", totalWordNum);
            //layout = new Layout();
            Console.WriteLine("begin to get clarity!");
            calculateClarity(clarityFilename);
            Console.WriteLine("get clarity end!");
        }
        private void addBigramInfo(string sentence, int num)
        {
            for (int i = 0; i < sentence.Length - 1; i++)
            {
                int no = getBigramIndex(sentence.Substring(i, 2));
                bigramFreq[no]+= num;
            }
        }

        public bool isOOV(String word)
        {
            return !(map.ContainsKey(word));
        }
        public double getFreq(String word)
        {
            if (isOOV(word)) return 0;
            return ((double)(map[word]) / totalWordNum);
        }
        public static int getBigramIndex(String s)
        {
            Debug.Assert(s.Length == 2);
            int i1 = Char.ToLower(s[0]) - 'a';
            int i2 = Char.ToLower(s[1]) - 'a';
            if (s[0].Equals(' ')) i1 = 26;
            if (s[1].Equals(' ')) i2 = 26;
            return (i1 * CHAR_NUM + i2);
        }
        public static String getBigramStr(int index)
        {
            char c1 = (char)('a' + index / CHAR_NUM);
            char c2 = (char)('a' + index % CHAR_NUM);
            if (index / CHAR_NUM == 26) c1 = ' ';
            if (index % CHAR_NUM == 26) c1 = ' ';
            return (Char.ToString(c1)+Char.ToString(c2));
        }
        public double calculateKLDivergence(int[] b) {
            double ret = 0.0;
            int btot = 0;
            for (int i = 0; i < CHAR_NUM * CHAR_NUM; i++)
            {
                btot += b[i];
            }
            for (int i = 0; i < CHAR_NUM * CHAR_NUM; i++)
            {
                if (b[i] == 0) continue;
                Debug.Assert(bigramFreq[i] != 0);
                double f = (double)b[i] / btot;
                ret += (f * Math.Log((f / bigramFreq[i]), 2));
            }
            return ret;
        }
        private void calculateClarity(string clarityPath)
        {
            clarityMap = new Dictionary<string,double>();
            StreamReader srd;
            try
            {
                srd = File.OpenText(clarityPath);
            }
            catch
            {
                Console.WriteLine("file {0} open failed!", clarityPath);
                return;
            }
            while (srd.Peek() != -1)
            {
                string line = srd.ReadLine();
                string[] s = line.Split(',');
                Debug.Assert(s.Length == 2);
                Debug.Assert(!clarityMap.ContainsKey(s[0]));
                clarityMap.Add(s[0], Convert.ToDouble(s[1]));
            }
            srd.Close();
            Console.WriteLine("begin to calculate cdf for universe!");
            //get cdf
            cdf = getCDF(map, totalWordNum, clarityMap);
            cdfArray = cdf.ToArray();
            //build index for ks-test
            cdfIndex = new Dictionary<double, int>();
            for (int i = 0; i < cdfArray.Length; i++)
            {
                Debug.Assert(!cdfIndex.ContainsKey(cdfArray[i].Key));
                cdfIndex.Add(cdfArray[i].Key, i);
            }            
        }
        public Dictionary<double, double> getCDF(Dictionary<string, int>freq, int totNum, Dictionary<string, double> clarity)
        {
            Dictionary<double, double> temp = new Dictionary<double, double>();
            var query = from t in clarity orderby t.Value select t;
            KeyValuePair<string , double>[] a = query.ToArray();
            double last = 0;
            double num = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (Math.Abs(last - a[i].Value) > Double.Epsilon)
                {
                    temp.Add(last, num / totNum);
                    last = a[i].Value;
                }
                num += freq[a[i].Key];
            }
            temp.Add(last, num / totNum);
            return temp;
        }
        public double getWordClarity(String w)
        {
            if (!clarityMap.ContainsKey(w))
            {
                Console.WriteLine("Something wrong in wordclarity");
                Console.ReadKey();
                return Double.MaxValue;
            }
            return clarityMap[w];
        }
        /*private double dist(string w1, string w2)
        {
            double ret = 0.0;
            Debug.Assert(w1.Length == w2.Length);            
            for (int i = 0; i < w1.Length; i++)
            {
                ret += layout.getDist(w1[i], w2[i]);                
            }
            ret /= w1.Length;
            return ret;
        }*/
        public double calculateKSTest(Dictionary<double, double> solutionCdf)
        {
            Debug.Assert(solutionCdf.Count <= cdf.Count);
            int pointer = 0;
            double ret = 0.0;
            double lastvalue = 0.0;
            solutionCdf.Add(cdfArray[cdfArray.Length-1].Key, 1);
            KeyValuePair<double, double>[] solutionArray = solutionCdf.ToArray();
            for (int i=0; i<solutionCdf.Count; i++) {
              /*  while (solutionArray[i].Key > cdfArray[pointer].Key) {
                    double d = lastvalue - cdfArray[pointer].Value;
                    if (Math.Abs(d) > Math.Abs(ret)) ret = d;
                    pointer ++;
                }*/
                if (solutionArray[i].Key > cdfArray[pointer].Key)
                {
                    pointer = cdfIndex[solutionArray[i].Key] - 1;
                    double d = lastvalue - cdfArray[pointer].Value;
                    if (Math.Abs(d) > Math.Abs(ret)) ret = d;
                    pointer++;
                }
                double dist = solutionArray[i].Value - cdfArray[pointer].Value;
                if (Math.Abs(dist) > Math.Abs(ret))
                    ret = dist;
                lastvalue = solutionArray[i].Value;
                pointer++;
            }
            return ret;
        }
    }
}
