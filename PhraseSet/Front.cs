using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PhraseSet
{
    class Front
    {
        private int PASS_NUM = 100;
        private HashSet<Solution> set;
        public Front()
        {
            set = new HashSet<Solution>();
        }
        public void updateSolution(Solution s) {
            if (set.Count == 0)
            {
                set.Add(s);
            }
            bool dominate = false;
            bool dominated = false;
            Solution[] solutionArray = set.ToArray();
            for (int i = 0; i < solutionArray.Length; i++)
            {
                if (s > solutionArray[i])
                {
                    set.Remove(solutionArray[i]);
                    dominate = true;
                }
            }
            if (dominate)
            {
                set.Add(s);
            }
            else
            {
                for (int i = 0; i < solutionArray.Length; i++)
                {
                    if (solutionArray[i] > s)
                    {
                        dominated = true;
                        return;
                    }
                }
                if (!dominated) set.Add(s);                  
            }
        }
        
        public void extend()
        {
            for (int i = 0; i < set.Count; i++)
            {
                Solution s = set.ElementAt(i);
                for (int j = 0; j < PASS_NUM; j++)
                {
                    Solution newS = Solution.generateSolution(s);
                    this.updateSolution(newS);
                }
            }            
        }

        public void printFront()
        {
            Console.WriteLine("Front:{0}", set.Count);
            for (int i = 0; i < set.Count; i++)
            {
                Solution s = set.ElementAt(i);
                s.printSolution();
                Console.WriteLine("Score:{0} {1} {2}", s.getFreq(), s.getMemoryable(), s.getClarity());
            }
            Console.WriteLine("=========={0}=========", set.Count);
        }
        public void toFile(String fileName)
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(new FileStream(fileName, FileMode.OpenOrCreate));
            }
            catch
            {
                Console.WriteLine("File {0} open error!", fileName);
                return;
            }
            sw.WriteLine("Freq,Memoryable,Clarity,Clarity(no abs)");
            Solution[] solutionArray = set.ToArray();
            for (int i = 0; i < solutionArray.Length; i++)
            {
                Solution s = solutionArray[i];
                sw.WriteLine("{0},{1},{2},{3}", s.getFreq(), s.getMemoryable(), s.getClarity(), s.getClarityWithoutAbs());
            }
            sw.Close();
        }
        public void addSolution(Solution s)
        {
            set.Add(s);
        }
        public void saveState(StreamWriter sw)
        {
            sw.WriteLine(set.Count);
            Solution[] array = set.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                sw.WriteLine(array[i].toStateString());
                sw.WriteLine("{0},{1},{2}", array[i].getRawClarity(), array[i].getRawMemoryable(), array[i].getRawFreq());
                sw.WriteLine("{0},{1},{2}", array[i].getClarity(), array[i].getMemoryable(), array[i].getFreq());
            }
        }


    }
}
