using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CountEmblems
{
    class Program
    {
        static int previousValue_;
        const int NUMMAPS = 1035;
        const int MAXEMBLEMS = 512;
        const int MAXEXTRAEMBLEMS = 16;
        static UInt32 power_of_256(int power)
        {
            UInt32 result = 1;
            for (int i = 0; i < power; i++)
            {
                result *= 256;
            }
            return result;
        }
        static Byte ReadByte(ref byte[] bytes)
        {
            byte value = bytes[0];
            bytes = bytes.Skip(1).ToArray();
            return value;
        }
        static UInt32 ReadUInt32(ref byte[] bytes)
        {
            UInt32 sum = 0;
            for (int i = 0; i < 4; i++)
            {
                sum += bytes[0]*power_of_256(i);
                bytes = bytes.Skip(1).ToArray();
            }
            return sum;
        }
        static void Analyze_file(string fileName, string outputName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);

            int length = bytes.Length;

            // Version check
            ReadUInt32(ref bytes);
            // Total playtime
            ReadUInt32(ref bytes);
            // Is the game modded
            ReadByte(ref bytes);
            // Check map visitations
            for (int i = 0; i < NUMMAPS; i++)
            {
                ReadByte(ref bytes);
            }

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
            /*
            Console.WriteLine("Emblems: " + emblems);
            Console.WriteLine("Extra emblems: " + extraEmblems);
            Console.WriteLine("Total emblems: " + total);
            */

            if (total != previousValue_)
            {
                File.WriteAllText(outputName, total.ToString());
                previousValue_ = total;
            }
        }
        static void Main(string[] args)
        {
            previousValue_ = -1;
            Console.Write("Game data file: ");
            string fileName = Console.ReadLine();
            Console.Write("Output file: ");
            string outputName = Console.ReadLine();
            while (true)
            {
                Analyze_file(fileName, outputName);
                System.Threading.Thread.Sleep(200);
            }
        }
    }
}
