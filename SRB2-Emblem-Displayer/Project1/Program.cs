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
        static int CountEmblems(ref byte[] bytes, int max_emblems)
        {
            int result = 0;
            for (int i = 0; i < max_emblems;)
            {
                int j;
                byte rtemp = ReadByte(ref bytes);
                for (j = 0; j < 8 && j + i < MAXEMBLEMS; ++j)
                    result += ((rtemp >> j) & 1);
                i += j;
            }
            return result;
        }
        static void Analyze_file(string fileName, string outputName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);

            // 4 (version check) + 4 (playtime) + 1 (modified) + 1035 (maps visited)
            bytes = bytes.Skip(1044).ToArray();

            int emblems = CountEmblems(ref bytes, MAXEMBLEMS);
            int extraEmblems = CountEmblems(ref bytes, MAXEXTRAEMBLEMS);
            int total = emblems + extraEmblems;

            if (total != previousTotal)
            {
                File.WriteAllText(outputName, total.ToString());
                previousTotal = total;
                Console.WriteLine("Emblems: " + total);
            }
        }
        static void Main()
        {
            string fileName = "";
            string outputName = "";

            try
            {
                string[] lines = File.ReadAllLines("path.ini");
                fileName = lines[0];
                outputName = lines[1];
            }
            catch
            {
                Console.WriteLine("Unable to read the paths file");
            }
            previousTotal = -1;
            Console.Write("Game data file: " + fileName + "\n");
            Console.Write("Output file: " + outputName);
            while (true)
            {
                Analyze_file(fileName, outputName);
                Thread.Sleep(100);
            }
        }
    }
}