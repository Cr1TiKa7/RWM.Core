using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWM.Core.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadWriteMemory rwm = new ReadWriteMemory("Lightshot");
            Console.WriteLine("Before: " + rwm.ReadString(0x00DC7FF5, 8));
            rwm.Write(0x00DC7FF5, "This is a test");
            Console.WriteLine("After: " + rwm.ReadString(0x00DC7FF5, 16));
            Console.ReadLine();
        }

    }
}
