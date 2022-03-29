using meliheran;
using System;

namespace SampleApp
{
    class Program
    {
        private static BufferQ<DatabaseItem> _bufferQ;

        static void Main(string[] args)
        {
            _bufferQ = new BufferQ<DatabaseItem>();

            _bufferQ.OnError += BufferQ_OnError;
            _bufferQ.OnStopped += BufferQ_OnStopped;
            _bufferQ.OnStatusChanged += BufferQ_OnStatusChanged;
            _bufferQ.OnThreadCountChanged += BufferQ_OnThreadCountChanged;
            _bufferQ.OnWork += BufferQ_OnWork;

            _bufferQ.OnQueueEmpty += BufferQ_OnQueueEmpty; ;
            //If there is no item in queue wait for 10 seconds to call OnQueueEmpty event to fill queue again
            _bufferQ.NoWorkSleepTime = 1000 * 10;

            //To get only some properties of multi thread count
            //_bufferQ.ThreadCount
            //_bufferQ.QueueCurrentItemCount
            //_bufferQ.ActiveThreadCount

            int threadCount = 10;

            //Starts working of the threads to consume thread safe queue to process items with DoYourWorkHere method
            _bufferQ.Start(threadCount, DoYourWorkHere);

            //Stops working of threads. Threads stops to getting item from queue.
            //Wait for queue parameters defines waiting for processing all items should finish process or immediately stop without waiting to processing items in queue by theads
            _bufferQ.Stop(true);
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
        static void BufferQ_OnQueueEmpty(object sender, BufferQEventArgs<DatabaseItem> e)
        {
            //Fill thread safe queue with items using e.Queue.Enqueue()
        }

        static void BufferQ_OnWork(int queueItemCount)
        {
            //Notifies on every item process item count in current queue
        }

        static void BufferQ_OnThreadCountChanged(int activeThreadCount)
        {
            //Notifies on every thread count changed
        }

        static void BufferQ_OnStatusChanged(BufferQ<DatabaseItem>.WorkerStatus status)
        {
            //Notifies on multi thread queue status is Started or Stopped
        }

        static void BufferQ_OnStopped(System.Collections.Concurrent.ConcurrentQueue<DatabaseItem> queue)
        {
            //Returns queue with items on stopped. If stopped with wait for queue items selected queue comes with zero items if not comes with current items that not processed yet
        }

        static void BufferQ_OnError(Exception ex)
        {
            //Notifies if there is any error on item processing
        }
    }
}
