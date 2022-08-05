using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Lab1
{
    class ResultMonitor
    {
        private int Size;
        private Player[] Results;
        private int Count;
        private readonly int WriterCount;
        private readonly object _lock = new object();

        public ResultMonitor(int size, int writers)
        {
            Results = new Player[size];
            WriterCount = writers;
            Size = size;
            Count = 0;
        }

        public void PrintResultsToFile(string filePath)
        {
            using (StreamWriter write = new StreamWriter(filePath, true))
            {
                write.WriteLine("Filtered results (beggining with numbers)");
                write.WriteLine(new string('-', 115));

                for (int i = 0; i < Count; i++)
                {
                    write.WriteLine("|{0,4}.|{1}|", i + 1, Results[i].ToString());
                }

                write.WriteLine(new string('-', 115));
            }
        }

        public void PrintResultsToConsole()
        {
            for (int i = 0; i < Count; i++)
            {
                Console.WriteLine(Results[i].ComputeHash());
                Console.WriteLine();
            }
        }

        public bool Add(Player player)
        {
            lock (_lock)
            {
                if(player == null)
                {
                    Monitor.PulseAll(_lock);
                    return false;
                }

                if (IsElementValid(player))
                {
                    Console.WriteLine($"ResultMonitor {player.ToString()}");
                    AddElementToSortedArray(player);
                    Count++;
                }

                Monitor.PulseAll(_lock);
            }

            return true;
        }

        

        private bool IsElementValid(Player player)
        {
            string hash = player.ComputeHash();
            char firstLetter = hash[0];
            if (char.IsDigit(firstLetter))
                return true;
            else
                return false;
        }

        private void AddElementToSortedArray(Player player)
        {
            int i;

            for( i = Count - 1; (i >= 0 && Results[i].CompareTo(player) > 0); i--)
            {
                Results[i + 1] = Results[i];
            }

            Results[i + 1] = player;
        }
    }
}
