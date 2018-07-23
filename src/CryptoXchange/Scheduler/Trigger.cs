using System;

namespace CryptoXchange.Scheduler
{
    public class Trigger
    {
        public Action TaskAction { get; set; }
        public DateTime StartTime { get; set; }
    }
}
