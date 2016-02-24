using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace PhraseSet
{
    class Solution
    {
        public static int corpusSize;
        public static int solutionSize;
        public static Corpus corpus;
        private HashSet<int> list;
        private int[] bigramInfo = null;
        private double KLDivergence;
        private double mem;
        private double clarityBias;
        public Solution()
        {
            list = new HashSet<int>();
        }
        public Solution(string line)
        {
            list = new HashSet<int>();
            string[] nos = line.Split(',');
            for (int i = 0; i < nos.Length; i++)
            {
                list.Add(Convert.ToInt32(nos[i]));
            }
            this.calculateParameters();
        }
        public Solution(Solution s)
        {
            list = new HashSet<int>();
            for (int i = 0; i < solutionSize; i++)
            {
                this.list.Add(s.list.ElementAt(i));
            }
        }
        public double getFreq()
        {
            //return ((1.66634438 - KLDivergence)/1.614588);
            Debug.Assert(list.Count == Solution.solutionSize);
            if (Solution.solutionSize == 20)
            {
                return ((2.4796158615 - KLDivergence) / (2.4796158615 - 0.1837411982));
            }
            else if (Solution.solutionSize == 40)
            {
                return ((1.9145457238 - KLDivergence) / (1.9145457238 - 0.0960464599));
            }
            else if (Solution.solutionSize == 80)
            {
                return ((1.4593687563 - KLDivergence) / (1.4593687563 - 0.0547371472));
            }
            else
            {
                Debug.Assert(Solution.solutionSize == 160);
                return ((1.0419980644 - KLDivergence) / (1.0419980644 - 0.0419602665));
            }
        }
        public double getRawFreq()
        {
            return KLDivergence;
        }
        public double getMemoryable()
        {
            //return ((4.5127931 - mem)/5.3659241);
            Debug.Assert(list.Count == Solution.solutionSize);
            if (Solution.solutionSize == 20)
            {
                return ((5.4617154 - mem) / (5.4617154 + 1.984062));
            }
            else if (Solution.solutionSize == 40)
            {
                return ((5.1660739 - mem) / (5.1660739 + 1.746762));
            }
            else if (Solution.solutionSize == 80)
            {
                return ((4.7553278 - mem) / (4.7553278 + 1.40921));
            }
            else
            {
                Debug.Assert(Solution.solutionSize == 160);
                return ((4.2636156 - mem) / (4.2636156 + 0.949749));
            }
        }
        public double getRawMemoryable()
        {
            return mem;
        }
        public double getClarity()
        {
            //return ((0.46557304 - Math.Abs(clarityBias))/0.46155099);
            Debug.Assert(list.Count == Solution.solutionSize);
            if (Solution.solutionSize == 20)
            {
                return ((0.49892329 - Math.Abs(clarityBias)) / (0.49892329 - 0.0089033221));
            }
            else if (Solution.solutionSize == 40)
            {
                return ((0.4518142182 - Math.Abs(clarityBias)) / (0.4518142182 - 0.0051018835));
            }
            else if (Solution.solutionSize == 80)
            {
                return ((0.3819997196 - Math.Abs(clarityBias)) / (0.3819997196 - 0.004117239));
            }
            else
            {
                Debug.Assert(Solution.solutionSize == 160);
                return ((0.2104286782 - Math.Abs(clarityBias)) / (0.2104286782 - 0.0042700744));
            }
        }
        public double getRawClarity()
        {
            return clarityBias;
        }
        public double getClarityWithoutAbs()
        {
            return clarityBias;
        }
        public static Solution initSolution(int solutionSize)
        {
            Solution s = new Solution();
            Random rand = new Random();
            for (int i = 0; i < solutionSize; i++)
            {
                int no;
                do
                {
                    no = rand.Next(corpusSize);
                } while (s.list.Contains(no));
                s.list.Add(no);
            }
            s.calculateParameters();
            return s;
        }
        public void calculateParameters()
        {
            //calculate memorable:
            mem = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                mem += corpus.getCER(list.ElementAt(i));
            }
            mem /= list.Count;            
            Debug.Assert(!(Double.IsInfinity(mem) || Double.IsNaN(mem)));
           //calculate K-L divergence:
            if (bigramInfo == null)
                bigramInfo = new int[Universe.CHAR_NUM * Universe.CHAR_NUM];
            for (int i = 0; i < Universe.CHAR_NUM * Universe.CHAR_NUM; i++)
            {
                bigramInfo[i] = 0;
            }
            for (int i = 0; i < list.Count; i++)
            {
                String s = corpus.getSentence(list.ElementAt(i));
                string[] wordlist = s.Split(' ');
                foreach (string word in wordlist)
                {
                    for (int j = 0; j < word.Length - 1; j++)
                    {
                        int index = Universe.getBigramIndex(word.Substring(j, 2));
                        bigramInfo[index]++;
                    }
                }
            }
            KLDivergence = corpus.getKLDivergence(bigramInfo);
            clarityBias = corpus.getClarityBias(list.ToList());        
        }
        public static Solution generateSolution(Solution old)
        {
            Solution newS = new Solution(old);
            Random rand = new Random();
            int inno, outno;
            do
            {
                inno = rand.Next(corpusSize);
            } while (old.list.Contains(inno));
            outno = rand.Next(solutionSize);
            newS.list.Remove(old.list.ElementAt(outno));
            newS.list.Add(inno);
            Debug.Assert(newS.list.Count == solutionSize);
            newS.calculateParameters();
            return newS;            
        }
        public static bool operator > (Solution a, Solution b)
        {
            double freqa = a.getFreq();
            double freqb = b.getFreq();
            double mema = a.getMemoryable();
            double memb = b.getMemoryable();
            double claa = a.getClarity();
            double clab = b.getClarity();
            bool f = (freqa > freqb) && (mema >= memb) && (claa >= clab);
            bool m = (freqa >= freqb) && (mema > memb) && (claa >= clab);
            bool c = (freqa >= freqb) && (mema >= memb) && (claa > clab);
            return (f || m || c);                
        }
        public static bool operator < (Solution a, Solution b)
        {
            return (b > a);
        }
        public void printSolution()
        {
            Console.WriteLine("Solution:{0}", solutionSize);
            for (int i = 0; i < solutionSize; i++)
            {
                Console.Write("{0} ", list.ElementAt(i));
            }
            Console.WriteLine();
            Console.WriteLine("=============================================");
        }
        public string toStateString()
        {
            int[] a = list.ToArray();
            string ret = "";
            for (int i = 0; i < a.Length - 1; i++)
            {
                ret += (a[i] + ",");
            }
            ret += a[a.Length - 1];
            return ret;
        }
        public void setClarity(double s)
        {
            this.clarityBias = s;
        }
        public void setCER(double s)
        {
            this.mem = s;
        }
        public void setKL(double s)
        {
            this.KLDivergence = s;
        }

    }
}
