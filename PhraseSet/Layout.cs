using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhraseSet
{
    class Layout
    {
        private double[] xpos;
        private double[] ypos;
        public Layout()
        {
            xpos = new double[26];
            ypos = new double[26];
            char[] line1 = new char[10] {'q','w','e','r','t','y','u','i','o','p'};
            char[] line2 = new char[9] { 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l' };
            char[] line3 = new char[7] { 'z', 'x', 'c', 'v', 'b', 'n', 'm' };
            for (int i = 0; i < 10; i++)
            {
                xpos[line1[i] - 'a'] = 0.5+i;
                ypos[line1[i] - 'a'] = 0.5;
            }
            for (int i = 0; i < 9; i++)
            {
                xpos[line2[i] - 'a'] = i + 1;
                ypos[line2[i] - 'a'] = 1.5;
            }
            for (int i = 0; i < 7; i++)
            {
                xpos[line3[i] - 'a'] = i + 2;
                ypos[line3[i] - 'a'] = 2.5;
            }
        }
        public double getDist(char c1, char c2)
        {
            int index1 = c1 - 'a';
            int index2 = c2 - 'a';
            double ret = Math.Pow((xpos[index1] - xpos[index2]), 2);
            ret += Math.Pow((ypos[index1] - ypos[index2]), 2);
            return Math.Sqrt(ret);
        }
    }
}
