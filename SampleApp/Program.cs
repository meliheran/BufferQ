using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        private static Core.Library.MultiThreadQueue<DatabaseItem> _multiThreadQueue;

        static void Main(string[] args)
        {
            _multiThreadQueue = new Core.Library.MultiThreadQueue<DatabaseItem>();

            _multiThreadQueue.OnError += _multiThreadQueue_OnError;
            _multiThreadQueue.OnStopped += _multiThreadQueue_OnStopped;
            _multiThreadQueue.OnStatusChanged += _multiThreadQueue_OnStatusChanged;
            _multiThreadQueue.OnThreadCountChanged += _multiThreadQueue_OnThreadCountChanged;
            _multiThreadQueue.OnWork += _multiThreadQueue_OnWork;

            _multiThreadQueue.OnQueueEmpty += _multiThreadQueue_OnQueueEmpty;
            //If there is no item in queue wait for 10 seconds to call OnQueueEmpty event to fill queue again
            _multiThreadQueue.NoWorkSleepTime = 1000 * 10;

            //To get only some properties of multi thread count
            //_multiThreadQueue.ThreadCount
            //_multiThreadQueue.QueueCurrentItemCount
            //_multiThreadQueue.ActiveThreadCount

            int threadCount = 10;

            //Starts working of the threads to consume thread safe queue to process items with DoYourWorkHere method
            _multiThreadQueue.Start(threadCount, DoYourWorkHere);

            //Stops working of threads. Threads stops to getting item from queue.
            //Wait for queue parameters defines waiting for processing all items should finish process or immediately stop without waiting to processing items in queue by theads
            _multiThreadQueue.Stop(true);
        }

        /// <summary>
        /// This method called with different threads each time. Thread count is can be defined in Start Method
        /// </summary>
        /// <param name="queueItem"></param>
        private static void DoYourWorkHere(DatabaseItem queueItem)
        {
            //Do your work here with the item in the queue that given on initializing
        }

        /// <summary>
        /// Multi threaded queue should fill when queue is empty. Fill queue with items to be processed with multi threaded.
        /// This method is thread same and fired only by one thread which realized that queue is empty 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Queue event arguments include queue item</param>
        static void _multiThreadQueue_OnQueueEmpty(object sender, Core.Library.MultiThreadQueueEventArgs<DatabaseItem> e)
        {
            //Fill thread safe queue with items using e.Queue.Enqueue()
        }

        static void _multiThreadQueue_OnWork(int queueItemCount)
        {
            //Notifies on every item process item count in current queue
        }

        static void _multiThreadQueue_OnThreadCountChanged(int activeThreadCount)
        {
            //Notifies on every thread count changed
        }

        static void _multiThreadQueue_OnStatusChanged(Core.Library.MultiThreadQueue<DatabaseItem>.WorkerStatus status)
        {
            //Notifies on multi thread queue status is Started or Stopped
        }

        static void _multiThreadQueue_OnStopped(System.Collections.Concurrent.ConcurrentQueue<DatabaseItem> queue)
        {
            //Returns queue with items on stopped. If stopped with wait for queue items selected queue comes with zero items if not comes with current items that not processed yet
        }

        static void _multiThreadQueue_OnError(Exception ex)
        {
            //Notifies if there is any error on item processing
        }
    }
}
