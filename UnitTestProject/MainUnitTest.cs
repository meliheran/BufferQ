using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class MainUnitTest
    {
        int _totalThreadCount = 100;
        int _maxItemFilledPerEmtpy = 100;

        System.Collections.Concurrent.ConcurrentQueue<Exception> _exceptionList;
        List<int> _queueItemCountList;

        int _processedItemCount;
        bool _processedItemsHasNull = false;
        bool _waitForQueueItemToFinish = true;
        
        Core.Library.MultiThreadQueue<Sms> worker;

        public void Initialize()
        {
            _exceptionList = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
            _queueItemCountList = new List<int>();
            
            _processedItemsHasNull = false;
            _waitForQueueItemToFinish = true;

            worker = new Core.Library.MultiThreadQueue<Sms>(); ;
            worker.OnQueueEmpty += worker_OnQueueEmpty;
            worker.OnWork += worker_OnWork;
            worker.OnError += worker_OnError;
            worker.Start(_totalThreadCount, Process);

            System.Threading.Thread.Sleep(1000 * 60);

            bool stopped = worker.Stop(_waitForQueueItemToFinish);
        }

        [TestMethod]
        public void CheckThreadSafe()
        {
            this.Initialize();

            Assert.IsFalse(_queueItemCountList.Max(i => i) > _maxItemFilledPerEmtpy, 
                "Queue filled same time by parallel threads, threadsafe exception");
        }
        [TestMethod]
        public void CheckConsistency()
        {
            this.Initialize();

            Assert.IsTrue(smsItemId == _processedItemCount, 
                String.Format("Not all filled items processed, it should be equal. Filled Item : {0}, Processed Item : {1}"
                , smsItemId, _processedItemCount));
        }
        [TestMethod]
        public void CheckNullItems()
        {
            this.Initialize();
            Assert.IsFalse(_processedItemsHasNull == null,
                "Queue returns null item, it should be one of the items in queue") ;
        }
        [TestMethod]
        public void CheckException()
        {
            this.Initialize();

            Assert.AreEqual(0, _exceptionList.Count, 
                String.Format("{0} Exception occured. Exception is : {1}",
                _exceptionList.Count,
                (_exceptionList.Count > 1) ? _exceptionList.FirstOrDefault().Message : string.Empty
                )
                );
        }
        [TestMethod]
        public void CheckItemCountOnStopAction()
        {
            this.Initialize();

            Assert.IsTrue((worker.QueueCurrentItemCount == 0 && _waitForQueueItemToFinish), 
                "There are two type of stop, 1.Force to stop without waiting queue items to finish, 2.Stop until queue items to finish");
        }

        void worker_OnError(Exception ex)
        {
            //lock (this)
            //{
                _exceptionList.Enqueue(ex);
            //}
        }

        private void Process(Sms queueItem)
        {
            System.Threading.Interlocked.Increment(ref _processedItemCount);
            if (queueItem == null) _processedItemsHasNull = true;

            //System.Threading.Thread.Sleep((new Random()).Next(1,1000));
        }

        void worker_OnWork(int queueItemCount)
        {
            //lock (this)
            //{
                _queueItemCountList.Add(queueItemCount);
            //}
        }

        int smsItemId = 0;
        void worker_OnQueueEmpty(object sender, Core.Library.MultiThreadQueueEventArgs<MainUnitTest.Sms> e)
        {
            for (int i = 0; i < _maxItemFilledPerEmtpy; i++)
            {
                e.Queue.Enqueue(new Sms() { SmsId = ++smsItemId });
                System.Threading.Thread.Sleep((new Random()).Next(1, 500));
            }
        }

        public class Sms
        {
            public int SmsId { get; set; }
        }
    }
}
