using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Library
{
    /// <summary>
    /// Provides methods for take and process items from a threadsafe queue. Queue item should be filled on OnEmpty event let the threads process these items on each seperate thread
    /// </summary>
    /// <typeparam name="T">Type of the object that thread safe queue will remain</typeparam>
    public class MultiThreadQueue<T>
    {
        #region Properties
        private bool _isRunning;
        private bool _isRunningWaitForQueue;
        private int _totalActiveThreadCount;
        /// <summary>
        /// Provides to calling OnEventEmpty event from one thread in a time
        /// </summary>
        //private bool _callOnce;//TODO:Bu değer static olmalı?
        /// <summary>
        /// Default value of NoWorkSleepTime in milliseconds
        /// </summary>
        private const int DEFAULTWAITMILLISECONDS = 10;
        /// <summary>
        /// Sleep time in milliseconds if there is no item in queue
        /// </summary>
        public int NoWorkSleepTime { get; set; }
        /// <summary>
        /// Current item count in queue
        /// </summary>
        public int QueueCurrentItemCount 
        {
            get 
            {
                if (_queue == null) return 0;
                return _queue.Count; 
            } 
            //set; 
        }
        private int _threadcount;
        /// <summary>
        /// Total thread count will consume the queue
        /// </summary>
        public int ThreadCount 
        {
            get { return _threadcount; } 
            //set; 
        }
        /// <summary>
        /// Total current active thread count
        /// </summary>
        public int ActiveThreadCount 
        { 
            get
            {
                return this._totalActiveThreadCount;
            }
            //set; 
        }

        /// <summary>
        /// Working threads
        /// </summary>
        private System.Threading.Tasks.Task[] _threads;
        /// <summary>
        /// Threadsafe queue item that holds all queue items
        /// </summary>
        private System.Collections.Concurrent.ConcurrentQueue<T> _queue;
        #endregion

        #region EventsDelegates
        /// <summary>
        /// Represents the method that will handle the OnQueueEmpty event of MultiThreadQueue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">MultiThreadQueueWorker arguments that contains the event data</param>
        public delegate void OnQueueEmptyHandler(object sender,MultiThreadQueueEventArgs<T> e);
        /// <summary>
        /// Occurs when item count is zero in queue for each thread
        /// </summary>
        public event OnQueueEmptyHandler OnQueueEmpty;
        /// <summary>
        /// Occurs only once when item count is zero in queue
        /// </summary>
        //private event OnQueueEmptyHandler OnQueueEmptyInThread;
        /// <summary>
        /// Represents worker status information
        /// </summary>
        public enum WorkerStatus { Started ,Stopped };
        /// <summary>
        /// Represents the method that will handle the OnStatusChanged event of MultiThreadQueue
        /// </summary>
        /// <param name="status">Retreives workers current working status</param>
        public delegate void OnStatusChangeHandler(WorkerStatus status);
        /// <summary>
        /// Represents the method that will handle the OnStoppedHandler event of MultiThreadQueue
        /// </summary>
        /// <param name="status">Retreives queue object</param>
        public delegate void OnStoppedHandler(System.Collections.Concurrent.ConcurrentQueue<T> queue);//v1.0.0.1
        /// <summary>
        /// Occurs when worker status change
        /// </summary>
        public event OnStatusChangeHandler OnStatusChanged;
        /// <summary>
        /// Occurs when worker stopped
        /// </summary>
        public event OnStoppedHandler OnStopped;//v1.0.0.1
        /// <summary>
        /// Represents the method that will handle the OnThreadCountChanged event of MultiThreadQueue
        /// </summary>
        /// <param name="activeThreadCount">Retreives worker active thread count</param>
        public delegate void OnThreadCountChangeHandler(int activeThreadCount);
        /// <summary>
        /// Occures when worker current working thread count change
        /// </summary>
        public event OnThreadCountChangeHandler OnThreadCountChanged;
        /// <summary>
        /// Represents the method that will handle the OnError event of MultiThreadQueue
        /// </summary>
        /// <param name="ex">Retreives the exception that occures in workers threads</param>
        public delegate void OnErrorHandler(Exception ex);
        /// <summary>
        /// Occurs when exception thrown in workers working threads
        /// </summary>
        public event OnErrorHandler OnError;
        /// <summary>
        /// Represents the method that will handle the OnWork event of MultiThreadQueue
        /// </summary>
        /// <param name="queueItemCount">Retreives current queue item count in the queue</param>
        public delegate void OnWorkingHandler(int queueItemCount);
        /// <summary>
        /// Occures when worker call work method to process queue item
        /// </summary>
        public event OnWorkingHandler OnWork;
        /// <summary>
        /// Represents the method that will handle the AsyncWorkCallBack method
        /// </summary>
        /// <param name="queueItem"></param>
        public delegate void AsyncWorkCallBack(T queueItem);
        /// <summary>
        /// Represents the method that will threads call for work
        /// </summary>
        private AsyncWorkCallBack _asyncWorkCallBack;
        #endregion

        #region Methods
        public MultiThreadQueue()
        {
            this.NoWorkSleepTime = DEFAULTWAITMILLISECONDS;//default value
            _queue = new System.Collections.Concurrent.ConcurrentQueue<T>();
            //OnQueueEmptyInThread += MultiThreadQueue_OnQueueEmptyInThread;
        }
        
        /// <summary>
        /// Starts threads to consume queue items from queue
        /// </summary>
        /// <param name="threadCount">Total thread count</param>
        /// <param name="callbackMethod">Method that call back asynchronously after queue item taken from queue</param>
        /// <param name="testStart">Starts processing with 1 synchronous thread</param>
        public void Start(int threadCount, AsyncWorkCallBack callbackMethod,bool testStart = false)
        {
            try
            {
                _asyncWorkCallBack = callbackMethod;
                _totalActiveThreadCount = 0;
                _threadcount = threadCount;
                _isRunning = true;
                _isRunningWaitForQueue = false;
                //_callOnce = true;//no need to use



                //create threads
                _threads = new System.Threading.Tasks.Task[_threadcount];
                
                //notify that worker started
                CallStatusChangedEvent(WorkerStatus.Started);

                if (!testStart)
                {
                    //start all threads using thread pool structure
                    for (int i = 0; i < _threadcount; i++)
                    {
                        int temp = i;
                        _threads[temp] =
                            System.Threading.Tasks.Task.Factory.StartNew
                            (
                                () => Work() //start work method in all threads
                                , System.Threading.Tasks.TaskCreationOptions.LongRunning
                            );
                    }
                }
                else
                {
                    Work();
                }
            }
            catch (Exception ex)
            {
                CallErrorEvent(ex);
            }
        }
        /// <summary>
        /// Stops all threads that consuming the queue
        /// </summary>
        public bool Stop(bool waitForQueue)
        {
            //if service is in already stop status
            if (!_isRunning) return true;//v1.0.0.4


            if (waitForQueue)
                _isRunningWaitForQueue = waitForQueue;
            else
                _isRunning = false;

            System.Threading.Tasks.Task.WaitAll(_threads);
            
            CallStatusChangedEvent(WorkerStatus.Stopped);
            CallStoppedEvent(this._queue);

            return true;
        }

        /// <summary>
        /// Consumes queue items from queue and callback related method and retreives that queue item
        /// </summary>
        private void Work()
        {
            //increase active thread count +1 and notify with event
            this.CallIncreaseThreadCountChangedEvent();

            //start getting items from queue
            while (_isRunning)
            {
                try
                {
                    T myVal;

                    //get item from queue
                    if (_queue.TryDequeue(out myVal))
                    {
                        //v1.0.0.5
                        try
                        {
                            //call method and pass item that taken from the queue
                            //this method can throw exception client side, so it is in a try/catch block. Client error should not effect here
                            _asyncWorkCallBack(myVal);
                        }
                        catch (Exception)
                        { 
                        
                        }

                        //notify queue item count
                        CallWorkEvent();
                    }
                    else
                    {
                        //if there is no item in queue wait for a while
                        System.Threading.Thread.Sleep(NoWorkSleepTime);
                    }



                    //control the queue count 
                    //and decide to fire empty event
                    lock (this)
                    {
                        if (_isRunningWaitForQueue)
                        {
                            //ends working on user call when queue is empty
                            _isRunning = !(_queue.Count == 0);
                        }
                        else
                        {
                            //if (_callOnce && _queue.Count == 0)
                            if (_queue.Count == 0)//v1.0.0.4
                            {
                                //_callOnce = false;
                                this.CallEmptyThreadEventOnce();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO:Hata verdiğinde geriye queue objesini dönmeli mi? Böylece kuyruktaki itemlara ulaşmak istenebilir.
                    this.CallErrorEvent(ex);
                }
            }

            //decrease current active thread count -1 and notify with event
            this.CallDecreaseThreadCountChangedEvent();
        }

        private void CallWorkEvent()
        {
            try
            {
                if (OnWork != null) OnWork(_queue.Count);
            }
            catch (Exception)
            {

            }
        }
        //this is a private method can be called from each parallel threads when queue is empty
        //but this method can be called only once
        //private void MultiThreadQueue_OnQueueEmptyInThread(object sender, MultiThreadQueueEventArgs<T> e)
        //{
        //    //Unsubscribe the OnQueueEmptyInThread private event to prevent call from other parallel thread
        //    if (OnQueueEmptyInThread != null) OnQueueEmptyInThread -= MultiThreadQueue_OnQueueEmptyInThread;

        //    //OnQueueEmpty public event notifies queue is empty
        //    if (OnQueueEmpty != null) OnQueueEmpty(sender, e);

        //    //Subscribew the OnQueueEmptyInThread private event can be called from all threads
        //    if (OnQueueEmptyInThread == null) OnQueueEmptyInThread += MultiThreadQueue_OnQueueEmptyInThread;
        //}
        private void CallIncreaseThreadCountChangedEvent()
        {
            System.Threading.Interlocked.Increment(ref _totalActiveThreadCount);
            try
            {
                if (OnThreadCountChanged != null) OnThreadCountChanged(_totalActiveThreadCount);
            }
            catch (Exception)
            {

            }
            
        }
        private void CallDecreaseThreadCountChangedEvent()
        {
            System.Threading.Interlocked.Decrement(ref _totalActiveThreadCount);
            try
            {
                if (OnThreadCountChanged != null) OnThreadCountChanged(_totalActiveThreadCount);
            }
            catch (Exception)
            {

            }
        }
        private void CallEmptyThreadEventOnce()//v1.0.0.1
        {
            try
            {
                //OnQueueEmpty public event notifies queue is empty
                if (OnQueueEmpty != null) OnQueueEmpty(this, new MultiThreadQueueEventArgs<T>() { Queue = _queue });

                //end of event, reset _callOnce=true make it available to call this method again
                //_callOnce = true;
            }
            catch (Exception)
            {
                //v1.0.0.2
                //in any exception mark _callOnce=true
                //_callOnce = true;
            }

        }
        private void CallStatusChangedEvent(WorkerStatus status)
        {
            try
            {
                if (OnStatusChanged != null) OnStatusChanged(status);
            }
            catch (Exception)
            {

            }
        }
        private void CallStoppedEvent(System.Collections.Concurrent.ConcurrentQueue<T> concurrentQueue)
        {
            try
            {
                if (OnStopped != null) OnStopped(this._queue);
            }
            catch (Exception)
            {
                
            }
        }//v1.0.0.4
        private void CallErrorEvent(Exception ex)
        {
            try
            {
                if (OnError != null) OnError(ex);
            }
            catch (Exception)
            {

            }
        }
        #endregion
    }
}
