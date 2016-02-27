using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Library
{
    public class MultiThreadQueueEventArgs<T> : EventArgs
    {
        public System.Collections.Concurrent.ConcurrentQueue<T> Queue { get; set; }
    }
}
