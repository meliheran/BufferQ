# BufferQ

![BufferQ](https://user-images.githubusercontent.com/11661323/160600103-9d9d35c0-5a20-40d6-8fb9-f862725338d1.jpg)

## What is BufferQ ?
BufferQ is a solution for managing batch records between source and target with buffering. BufferQ designed to store data in on a thread safe queue as a buffer and multiple thread workers to consume that queue.

# Benefits
It is ideal for doing multi thread work with thread safe structure.
Common usage for,
- Process multiple database items
- Preloading requests to get ready for busy times
- High density buffer usage

# Solves
- Backlogged items in a bottleneck
- Deadlocks when common accessed data
- Time waste of sequence process

# Sample Use Case
*Sms Provider*

You are a SMS Provider and need to send multiple SMS in a short time. But your sms items stored in database table, need to read these data and process on code base. Most used scenerios one by one data read from db and process to send sms. But using BufferQ provides you to read only one time and batch data. All these data stored in a thread safe queue in memory. Specified amount of threads starts to consume that queue and process sms sending. So millions of records will be melt in a short time.

*Coupon Generator*

You are creating coupons but takes time to create each one. Multiple requests can be cause to creation of each coupon and user to be wait. So buffering can be a simple way for that. Some coupon can be generated and added to queue to get ready on user demand.
