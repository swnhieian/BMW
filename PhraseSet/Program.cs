using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PhraseSet
{
    class Program
    {
        static void Main(string[] args)
        {
            string corpusPath = @"enron_result.txt";
            string universePath = @"newcorpus.txt";
            string clarityPath = @"clarity.txt";
            Console.WriteLine("This is the program with new normalization values!");
            Corpus corpus = new Corpus(corpusPath, universePath, clarityPath);            
            StreamReader srd;
            try
            {
                srd = File.OpenText("config.txt");
            }
            catch
            {
                Console.WriteLine("config file open failed! Waiting key ...");
                Console.ReadKey();
                return;
            }
            string line = srd.ReadLine();
            int runNum = Convert.ToInt32(line);
            DirectoryInfo dir = new DirectoryInfo(".");
            Regex find = new Regex(@"state_(\d*)_(\d*).txt");
            int runstart = 0;
            int roundstart = 0;
            Dictionary<int, int> getmax = new Dictionary<int, int>();
            for (int i=0; i<runNum; i++)
                getmax.Add(i, 0);
            foreach (FileInfo file in dir.GetFiles())
            {
                string name = file.Name;
                if (find.IsMatch(name))
                {
                    Match m = find.Match(name);
                    int num1 = Convert.ToInt32(m.Groups[1].Value);
                    int num2 = Convert.ToInt32(m.Groups[2].Value);
                    if (num1 > runstart)
                        runstart = num1;
                    if (num2 > getmax[num1])
                        getmax[num1] = num2;
                }
            }
            Tool.start();
            for (int i = 0; i < runstart; i++)
            {
                line = srd.ReadLine();
            }
            for (int i = runstart; i < runNum; i++)
            {
                if (i == runstart)
                {
                    roundstart = getmax[runstart];
                }
                else
                {
                    roundstart = 0;
                }
                line = srd.ReadLine();
                string[] paras = line.Split(',');
                Debug.Assert(paras.Length == 4);
                int size = Convert.ToInt32(paras[0]);
                double alpha = Convert.ToDouble(paras[1]);
                double beta = Convert.ToDouble(paras[2]);
                int round = Convert.ToInt32(paras[3]);                
                for (int j = roundstart; j < round; j++)
                {
                    Optimization opt = new Optimization(corpus, size);
                    opt.run_oneRound(i, alpha, beta, j);
                }
                Console.WriteLine("\nTotal time : {0} s\n", Tool.getTime());
            }
            srd.Close();
            Tool.saveState();
            Tool.saveState();
            Console.WriteLine("Calculate over! Total time:{0} s !Waiting key...", Tool.getTime());
            Console.ReadKey(); 
            /*
             * 要设置的参数
             * Optimization.ITER_NUM
             * Optimization.TEMP_ITER_NUM
             * Optimization.T 的初值
             * Optimization.DEC_RATE
             * Optimization.SUB_NUM
             * Front.PASS_NUM
             * */
        }
    }
}
