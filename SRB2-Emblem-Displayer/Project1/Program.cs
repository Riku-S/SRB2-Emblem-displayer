using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CountEmblems
{
    class Program
    {
        // Previous amount of emblems
        static int previousTotal;
        // Previous error in the loop
        static string previousError;

        // 4 (version check) + 4 (playtime) + 1 (modified) + 1035 (maps visited)
        const int SKIPPED_BYTES = 1044;
        const int MAXEMBLEMS = 512;
        const int MAXEXTRAEMBLEMS = 16;
        const int EXIT_TIME = 10000;
        const int NO_PREVIOUS_TOTAL = -1;
        const string INI_NAME = "path.ini";
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
                // Function directly copied from SRB2 source code, where the gamedata handling happens
                int j;
                byte rtemp = ReadByte(ref bytes);
                for (j = 0; j < 8 && j + i < max_emblems; ++j)
                    result += ((rtemp >> j) & 1);
                i += j;
            }
            return result;
        }
        static void Analyze_file(string fileName, string outputName)
        {
            byte[] bytes;
            int total = previousTotal;
            try
            {
                bytes = File.ReadAllBytes(fileName);
                // We don't want to read empty/corrupted gamedata
                if (bytes.Length < SKIPPED_BYTES + MAXEMBLEMS + MAXEXTRAEMBLEMS && previousError != "short")
                {
                    Console.WriteLine("The gamedata is too short.");
                    previousError = "short";
                }
                else
                {
                    bytes = bytes.Skip(SKIPPED_BYTES).ToArray();
                    int emblems = CountEmblems(ref bytes, MAXEMBLEMS);
                    int extraEmblems = CountEmblems(ref bytes, MAXEXTRAEMBLEMS);
                    total = emblems + extraEmblems;
                }
            }
            catch (IOException e)
            {
                string errorName = e.GetType().Name;
                // We don't want error spam for every loop
                if (previousError != errorName)
                {
                    Console.WriteLine("{0}: {1}", errorName, e.Message);
                    previousError = errorName;
                }
            }
            if (total != previousTotal || total == NO_PREVIOUS_TOTAL)
            {
                try
                {
                    if (total == NO_PREVIOUS_TOTAL)
                    {
                        total = 0;
                    }
                    File.WriteAllText(outputName, total.ToString());
                    previousTotal = total;
                    Console.WriteLine("Emblems: " + total);

                }
                catch (IOException e)
                {
                    string errorName = e.GetType().Name;
                    // We don't want error spam for every loop
                    if (previousError != errorName)
                    {
                        Console.WriteLine("{0}: {1}", errorName, e.Message);
                        previousError = errorName;
                    }
                }
            }
        }
        static void Main()
        {
            string fileName = "";
            string outputName = "";
            try
            {
                string[] lines = File.ReadAllLines(INI_NAME);
                if (lines.Length < 2)
                {
                    Console.WriteLine("The file {0} has too few lines! ", INI_NAME);
                    Thread.Sleep(EXIT_TIME);
                    return;
                }
                fileName = lines[0];
                outputName = lines[1];
            }
            catch (IOException e)
            {
                Console.WriteLine("{0}: Unable to read the paths file {1}", e.GetType().Name, INI_NAME);
                Thread.Sleep(EXIT_TIME);
                return;
            }
            if (fileName == "")
            {
                Console.WriteLine("Input filename cannot be an empty string.");
                Thread.Sleep(EXIT_TIME);
                return;
            }
            if (outputName == "")
            {
                Console.WriteLine("Output filename cannot be an empty string.");
                Thread.Sleep(EXIT_TIME);
                return;
            }
            // We want to change the number in the output file on the first loop
            previousTotal = NO_PREVIOUS_TOTAL;
            Console.WriteLine("Game data file: " + fileName);
            Console.WriteLine("Output file: " + outputName);
            while (true)
            {
                Analyze_file(fileName, outputName);
                Thread.Sleep(100);
            }
        }
    }
}