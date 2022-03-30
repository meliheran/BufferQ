using System;
using System.Collections.Concurrent;

namespace meliheran
{
    public class BufferQEventArgs<T> : EventArgs
    {
        public ConcurrentQueue<T> Queue { get; set; }
    }
}
