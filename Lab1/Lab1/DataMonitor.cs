using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Lab1
{
    class DataMonitor
    {
        private int MaxSize = 10;
        private Player[] Buffer;
        private int From;
        private int To;
        private int CurrentSize;
        private readonly object _lock = new object();

        public DataMonitor()
        {
            this.Buffer = new Player[MaxSize];
            this.From = 0;
            this.To = 0;
            this.CurrentSize = 0;
        }

        public void Add(Player player)
        {
            lock(_lock)
            {
                while(CurrentSize == MaxSize)
                {
                    Monitor.Wait(_lock);
                }

                if(player.Name == "Stop")
                {
                    CurrentSize++;
                    Monitor.PulseAll(_lock);
                    return;
                }

                Buffer[To] = player;
                To = (To + 1) % MaxSize;
                CurrentSize++;

                Monitor.PulseAll(_lock);
            }
        }

        public Player Remove()
        {
            Player player;
            lock (_lock)
            {
                while (CurrentSize == 0)
                {
                    Monitor.Wait(_lock);
                }

                player = Buffer[From];
                Buffer[From] = null;
                From = (From + 1) % MaxSize;
                CurrentSize--;

                Monitor.PulseAll(_lock);
            }

            return player;
        }
    }
}
