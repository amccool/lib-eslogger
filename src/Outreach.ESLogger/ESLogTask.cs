using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Outreach.ESLogger
{
    public class ESLogTask
    {
        private readonly Queue<Tuple<ESClientProvider, LogEntry>> queue = new Queue<Tuple<ESClientProvider, LogEntry>>();
        private readonly object locker = new object();
        private readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        public ESLogTask()
        {
            Thread t = new Thread(() => this.ProcessLogs()); // We want this to run on its own thread, so it doesn't use up a thread pool thread for its entire life
            t.Name = "ES Log Processor";
            t.IsBackground = true; // kill thread when foreground threads are gone
            t.Start();
        }

        void ProcessLogs()
        {
            List<Tuple<ESClientProvider, LogEntry>> logs = new List<Tuple<ESClientProvider, LogEntry>>();
            while(true)
            {
                waitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                
                lock(locker)
                {
                    while(queue.Count > 0)
                    {
                        logs.Add(queue.Dequeue());
                    }
                }

                if(logs.Count > 0)
                {
                    for(int i = 0; i < logs.Count; i++)
                    {
                        var client = logs[i].Item1;
                        var entry = logs[i].Item2;
                        client.Client.Index(entry);
                    }
                }

                logs.Clear();
            }
        }

        public void QueueLog(ESClientProvider client, LogEntry entry)
        {
            lock(locker)
            {
                queue.Enqueue(new Tuple<ESClientProvider, LogEntry>(client, entry));
            }

            // Signal log processor
            waitHandle.Set();
        }
    }
}