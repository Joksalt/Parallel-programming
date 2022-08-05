using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace Lab1
{
    public class Player 
    {
        public string Name { get; set; }
        public int KillCount { get; set; }
        public double Money { get; set; }
        public override string ToString()
        {
            return string.Format(" {0,-16} | {1,8} | {2,9} | {3, -64}", Name, KillCount, Money, ComputeHash());
        }

        public int CompareTo(Player player)
        {
            return String.Compare(this.ComputeHash(), player.ComputeHash());
        }

        public string ComputeHash()
        {
            using (SHA256 sha = SHA256.Create())
            {
                string data = Name + KillCount.ToString() + Money.ToString();
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));

                StringBuilder build = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    build.Append(bytes[i].ToString("x2"));
                }
                return build.ToString();
            }
        }
    }

    class Program
    {
        private const string ResultFilePath = @"../../AppData/IFF-8-13_Keturakis_J_L1_rez.txt";
        const string FilePath = @"../../AppData/IFF_8_13_KeturakisJ_L1_dat_1.json";

        const int readerCount = 4;

        static void Main(string[] args)
        {
            Program p = new Program();

            Player[] players = p.LoadJson(FilePath);

            if(File.Exists(ResultFilePath))
            {
                File.Delete(ResultFilePath);
            }

            p.PrintData(ResultFilePath, players);

            int playerSize = players.Count();

            //Creating data and result monitors:

            DataMonitor dataMonitor = new DataMonitor();
            ResultMonitor resultMonitor = new ResultMonitor(playerSize - readerCount, 4);


            //Create threads:

            var threads = new List<Thread>();

            for(int i = 0; i < readerCount; i++)
            {
                threads.Add(new Thread(() =>
                {
                    Player player = dataMonitor.Remove();
                    while (player != null)
                    {
                        Console.WriteLine("dataMonitor {0}", player.ToString());
                        resultMonitor.Add(player);
                        player = dataMonitor.Remove();
                    }

                }));
            }



            //Starting threads:

            foreach(var thread in threads)
            {
                thread.Start();
            }

            p.AddAllItemsToDataMonitor(players, dataMonitor);
            
            foreach (var thread in threads)
            {
                thread.Join();
            }

            resultMonitor.PrintResultsToFile(ResultFilePath);


        }


        public Player[] LoadJson(string filePath)
        {
            Player[] temp;
            using (StreamReader read = new StreamReader(filePath))
            {
                string data = read.ReadToEnd();
                temp = JsonConvert.DeserializeObject<Player[]>(data);
            }

            return temp;
        }

        public void PrintData(string filePath, Player[] players)
        {
            using(StreamWriter write = new StreamWriter(filePath, true))
            {
                int i = 1;
                write.WriteLine(new string('-', 115));
                write.WriteLine("|{0,5}| {1,-17}|{2,10}|{3,10} |{4,-64} |", "Nr","Name","Kill Count","Money","Sha256 Hash");
                write.WriteLine(new string('-', 115));
                foreach(var player in players)
                {
                    if(player.Name != "Stop")
                    {
                        write.WriteLine("|{0,4}.|{1}|", i, player.ToString());
                        i++;
                    }
                }

                write.WriteLine(new string('-', 115));
            }
        }

        private void AddAllItemsToDataMonitor(Player[] players, DataMonitor dataMonitor)
        {
            foreach (var player in players)
            {
                dataMonitor.Add(player);
            }
        }
    }
}
