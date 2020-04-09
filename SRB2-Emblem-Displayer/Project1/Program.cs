using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CountEmblems
{
    class Program
    {
        static int previousTotal;
        const int MAXEMBLEMS = 512;
        const int MAXEXTRAEMBLEMS = 16;
        static byte ReadByte(ref byte[] bytes)
        {
            byte value = bytes[0];
            bytes = bytes.Skip(1).ToArray();
            return value;
        }
        static void Analyze_file(string fileName, string outputName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);

            bytes = bytes.Skip(1036).ToArray();

            int emblems = 0;
            for (int i = 0; i < MAXEMBLEMS;)
            {
                int j;
                byte rtemp = ReadByte(ref bytes);
                for (j = 0; j < 8 && j + i < MAXEMBLEMS; ++j)
                    emblems += ((rtemp >> j) & 1);
                i += j;
            }

            int extraEmblems = 0;
            for (int i = 0; i < MAXEXTRAEMBLEMS;)
            {
                int j;
                byte rtemp = ReadByte(ref bytes);
                for (j = 0; j < 8 && j + i < MAXEMBLEMS; ++j)
                    extraEmblems += ((rtemp >> j) & 1);
                i += j;
            }
            int total = (emblems + extraEmblems);

            if (total != previousTotal)
            {
                File.WriteAllText(outputName, total.ToString());
                previousTotal = total;
            }
        }
        static void Main()
        {
            previousTotal = -1;
            Console.Write("Game data file: ");
            string fileName = Console.ReadLine();
            Console.Write("Output file: ");
            string outputName = Console.ReadLine();
            while (true)
            {
                Analyze_file(fileName, outputName);
                Thread.Sleep(200);
            }
        }
    }
}