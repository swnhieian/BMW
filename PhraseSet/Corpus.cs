using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace PhraseSet
{
    class Corpus
    {
        private Universe universe;
        private List<String> sentences;
        private List<Double> memorable;

        public Corpus(String fileName, String universePath, string clarityPath)
        {
            universe = new Universe(universePath, clarityPath);
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
            sentences = new List<string>();
            memorable = new List<double>();
            while (srd.Peek() != -1)
            {
                String s = srd.ReadLine().ToLower();
                sentences.Add(s);
            }
            srd.Close();
            Console.WriteLine("get {0} sentences in corpus successfully!", CorpusSize);
            for (int i = 0; i < CorpusSize; i++)
            {
                calculateMemorable(i);
            }
            //double max, min;
            
            //max = query.First();
            //min = query.Last();
            Console.WriteLine("calculate CER in corpus successfully!");
        }
        private void calculateMemorable(int index)
        {
            //calculate CER of each sentence
            //CER = -11.65+0.83*Nw+0.48*SDchr+6.94*OOV-1.00*LProb
            String sen = sentences.ElementAt(index);
            String[] words = sen.Split(' ');
            int Nw = words.Length;
            double LProb = 0.0;
            double avg = 0.0;
            for (int i = 0; i < Nw; i++)
            {
                avg += words[i].Length;
                LProb += Math.Log(universe.getFreq(words[i]));
            }
            LProb = LProb / Nw;
            avg /= Nw;
            double SDchr = 0.0;
            for (int i = 0; i < Nw; i++)
            {
                SDchr += Math.Pow(words[i].Length - avg, 2);
            }
            if (Nw == 1)
            {
                SDchr = 0;
            }
            else
            {
                SDchr = Math.Sqrt(SDchr / (Nw - 1));
            }            
            double mem = -11.65 + 0.83 * Nw + 0.48 * SDchr  - LProb;
            memorable.Add(mem);
            Debug.Assert(memorable.Count -1 == index);
        }
        public int CorpusSize
        {
            get
            {
                return sentences.Count;
            }
        }
        public String getSentence(int i)
        {

            return sentences[i];
        }
        public double getCER(int id)
        {
            return memorable.ElementAt(id);
        }
        public double getKLDivergence(int[] b) {
            return universe.calculateKLDivergence(b);
        }
        public double getClarityBias(List<int> solutionList)
        {
            Dictionary<string, double> solutionClars = new Dictionary<string, double>();
            Dictionary<string, int> solutionFreq = new Dictionary<string, int>();
            int totword = 0;
            for (int i = 0; i < solutionList.Count; i++)
            {
                String w = sentences.ElementAt(solutionList[i]);
                string[] wordlist = w.Split(' ');
                for (int j = 0; j < wordlist.Length; j++)
                {
                    if (!solutionClars.ContainsKey(wordlist[j]))
                    {
                        solutionClars.Add(wordlist[j], universe.getWordClarity(wordlist[j]));
                    }
                    if (!solutionFreq.ContainsKey(wordlist[j]))
                    {
                        solutionFreq.Add(wordlist[j], 1);
                    }
                    else
                    {
                        solutionFreq[wordlist[j]]++;
                    }
                }
                totword += wordlist.Length;
            }
            //get cdf
            Dictionary<double, double> cdf = universe.getCDF(solutionFreq, totword, solutionClars);
            return universe.calculateKSTest(cdf);
        }
    }
}
