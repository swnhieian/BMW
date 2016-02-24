using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace PhraseSet
{
    class Optimization
    {
        //private int SUB_NUM = 20;
        private int SOLUTION_SIZE = 20;
        private int ITER_NUM = 3000;
        private double T;
        private double DEC_RATE = 0.95;
        private double TEMP_ITER_NUM = 1000;
        private Solution current;
        private Solution gen;
        private Front front;
        private Random rand;
        private Corpus corpus;
        private int iter_start;
        private int taskno;
        private int roundno;


        public Optimization(Corpus corpus, int solutionsize)
        {
            this.SOLUTION_SIZE = solutionsize;
            this.corpus = corpus;
            Solution.corpus = corpus;
            Solution.corpusSize = corpus.CorpusSize;
            Solution.solutionSize = SOLUTION_SIZE;
            front = new Front();
            rand = new Random();
            //////////

        }
        private void buildFront(double alpha, double beta)
        {
            T = 300;
            double gamma = 1 - alpha - beta;
            Console.WriteLine("alpha {0}, beta{1}, gamma{2}", alpha, beta, gamma);
            if (current == null)
            {
                current = Solution.initSolution(SOLUTION_SIZE);
            }
            double curScore = getScore(current, alpha, beta, gamma);
            double genScore;
            int interval = ITER_NUM / 10;
            for (int i = iter_start; i < ITER_NUM; i++)
            {
                Tool.showTime();
                if ((i + 1) % interval == 0)
                {
                    Console.Write("\r{0}% complete!", (i + 1) * 10 / interval);
                    saveState(i, taskno, roundno);
                }
                for (int j = 0; j < TEMP_ITER_NUM; j++)
                {
                    gen = Solution.generateSolution(current);
                    genScore = getScore(gen, alpha, beta, gamma);
                    front.updateSolution(gen);
                    double delta = (genScore - curScore);
                    if (delta >= 0)
                    {
                        current = gen;
                    }
                    else
                    {
                        double p = Math.Exp(delta / T);
                        if (rand.NextDouble() < p)
                            current = gen;
                    }
                    curScore = getScore(current, alpha, beta, gamma);
                }
                //Console.WriteLine("{0},{1},{2}", i, T, curScore);
                //sw.WriteLine("{0},{1},{2}", i, T, curScore);
                T *= DEC_RATE;
            }
            Console.WriteLine();
           // current.printSolution();
        }

        private void extendFront()
        {
            front.extend();
        }
        public void run_oneRound(int taskno, double alpha, double beta, int roundno)
        {
            Console.WriteLine("-------- task {0}, round {1} --------", taskno, roundno);
            this.taskno = taskno;
            this.roundno = roundno;
            if (!loadState(taskno, roundno)) return;
            buildFront(alpha, beta);
            saveState(ITER_NUM, taskno, roundno);
            Console.WriteLine("Over!");
         //   extendFront();
         //   front.printFront();
         //   front.toFile(@"C:\Users\Weinan\Desktop\answer.txt");
        }
        public double getScore(Solution s, double a, double b, double c)
        {
            return (a * s.getClarity() + b * s.getMemoryable() + c * s.getFreq());
        }

        private bool loadState(int taskno, int roundno)
        {
            string name = "state_" + taskno + "_" + roundno;
            FileInfo file = new FileInfo(name + ".txt");
            if (!file.Exists)
            {
                file = new FileInfo("init.txt");
            }
            StreamReader srd;
            try
            {
                srd = file.OpenText();
            }
            catch
            {
                Console.WriteLine("Can't load state! waiting key...");
                Console.ReadKey();
                return false;
            }
            iter_start = Convert.ToInt32(srd.ReadLine());
            string line = srd.ReadLine();
            //current;
            if (iter_start != 0)
            {
                current = new Solution(line);
                string[] ps = srd.ReadLine().Split(',');
                Debug.Assert(ps.Length == 3);
                current.setClarity(Convert.ToDouble(ps[0]));
                current.setCER(Convert.ToDouble(ps[1]));
                current.setKL(Convert.ToDouble(ps[2]));
                line = srd.ReadLine();
            }
            //front;
            int frontSize = Convert.ToInt32(srd.ReadLine());
            for (int i = 0; i < frontSize; i++)
            {
                Solution s = new Solution(srd.ReadLine());
                string[] ps = srd.ReadLine().Split(',');
                Debug.Assert(ps.Length == 3);
                s.setClarity(Convert.ToDouble(ps[0]));
                s.setCER(Convert.ToDouble(ps[1]));
                s.setKL(Convert.ToDouble(ps[2]));
                front.addSolution(s);
                line = srd.ReadLine();
            }
            //time;
            Tool.loadState();
            srd.Close();
            return true;
        }


        public bool saveState(int iter, int taskno, int roundno)
        {
            string name = "state_" + taskno + "_" + roundno;
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(name + "_new.txt");
            }
            catch
            {
                Console.WriteLine("Can't save state! waiting key...");
                Console.ReadKey();
                return false;
            }
            sw.WriteLine(iter);
            if (iter > 0)
            {
                sw.WriteLine(current.toStateString());
                sw.WriteLine("{0},{1},{2}", current.getRawClarity(), current.getRawMemoryable(), current.getRawFreq());
                sw.WriteLine("{0},{1},{2}", current.getClarity(), current.getMemoryable(), current.getFreq());
            }
            front.saveState(sw);
            Tool.saveState();
            sw.Close();
            FileInfo file = new FileInfo(name + ".txt");
            file.Delete();
            file = new FileInfo(name + "_new.txt");
            file.MoveTo(name + ".txt");
            return true;
        }
    }
}
