using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace PhraseSet
{
    class Tool
    {
        public static Stopwatch watch;
        public static double basetime = 0;
        public static int getTime()
        {
            return (int)((basetime+watch.Elapsed.TotalMilliseconds)/1000);
        }
        public static void start()
        {
            watch = new Stopwatch();
            FileInfo file = new FileInfo("time.txt");
            if (file.Exists)
            {
                try
                {
                    StreamReader srd = file.OpenText();
                    basetime = Convert.ToDouble(srd.ReadLine());
                    srd.Close();
                }
                catch
                {
                    Console.WriteLine("error while load time state! waiting key ...");
                    Console.ReadKey();
                }

            }
            else
            {
                basetime = 0;
                saveState();
            }
            watch.Start();
        }
        public static void stop()
        {
            watch.Stop();
            saveState();
        }
        public static void loadState()
        {
           /* FileInfo file = new FileInfo("time.txt");
            try
            {
                StreamReader srd = file.OpenText();
                basetime = Convert.ToInt32(srd.ReadLine());
                srd.Close();
            }
            catch
            {
                Console.WriteLine("error while load time state! waiting key ...");
                Console.ReadKey();
            }*/
        }
        public static void saveState()
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter("time.txt");
                double savetime = basetime + watch.Elapsed.TotalMilliseconds;
                sw.WriteLine(savetime);
                sw.Close();
            }
            catch
            {
                Console.WriteLine("error while save time state! waiting key ...");
                Console.ReadKey();
            }
        }
        public static void showTime()
        {
            int left = Console.CursorLeft;
            int top = Console.CursorTop;
            Console.SetCursorPosition(52, top);
            Console.Write("== Time: {0} s ==", getTime());
            Console.SetCursorPosition(left, top);
        }
    }
}
