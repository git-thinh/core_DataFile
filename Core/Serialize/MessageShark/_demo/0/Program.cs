using MessageShark;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using SmartThread;
using System.IO;

namespace demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // ConcurrencyTest ===========================================================
            var tbl = new Table();
            tbl.Name = "table1";
            tbl.Fields.Add(new Field { Name = "myField", DataType = DbType.Byte });
            byte[] buf = MessageSharkSerializer.Serialize<Table>(tbl);
            var tbl2 = MessageSharkSerializer.Deserialize<Table>(buf);
            Console.WriteLine(tbl2.Name);

            Dictionary<string, string> whee = new Dictionary<string, string>();
            whee.Add("k", "v");
            var buffer = MessageSharkSerializer.Serialize(whee);
            Dictionary<string, string> dict = MessageSharkSerializer.Deserialize<Dictionary<string, string>>(buffer);
            Console.WriteLine(dict.Keys.ToArray()[0]);

            var message = new Message() { ID = 10, CreateDate = DateTime.Now, Data = "This is a test", test = new Test() { Int = 100, Str = "Testing", UUID = Guid.NewGuid() } };
            message.Tests = new List<Test> {
                new Test() { Int = 100, Str = "Testing", UUID = Guid.NewGuid() },
                new Test() { Int = 100, Str = "Testing2", UUID = Guid.NewGuid() },
                new Test() { Int = 100, Str = "Testing3", UUID = Guid.NewGuid() }
            };
            var buffer2 = MessageSharkSerializer.Serialize(message);
            var message2 = MessageSharkSerializer.Deserialize<Message>(buffer2);
            Console.WriteLine(message2.Data);

            ///// NonGenericSerializationTEst ===========================================================
            var message1 = new Message() { ID = 10, CreateDate = DateTime.Now, Data = "This is a test", test = new Test() { Int = 100, Str = "Testing", UUID = Guid.NewGuid() } };
            var buffer1 = MessageSharkSerializer.Serialize(typeof(Message), message1);
            var messageD1 = MessageSharkSerializer.Deserialize(typeof(Message), buffer1);

            object message22 = new Message() { ID = 10, CreateDate = DateTime.Now, Data = "This is a test", test = new Test() { Int = 100, Str = "Testing", UUID = Guid.NewGuid() } };
            var buffer22 = MessageSharkSerializer.Serialize(message22);
            var messageD22 = MessageSharkSerializer.Deserialize(typeof(Message), buffer22);


            //TestMultipleSerialization();

            ShouldDeserializeTOMessageFromMemoryStream();
            ShouldSerializeMessageToMemoryStream();
            ShouldSerializeMessageWithObjectToMemoryStream();


            Console.ReadLine();
        }

        public static void TestMultipleSerialization()
        {
            //MessageSharkSerializer.Build();
            var count = 10000;
            SmartThreadPool smartThreadPool = new SmartThreadPool();
            IWorkItemResult[] tasks = new IWorkItemResult[count];
            for (var i = 0; i < count; i++)
            {
                SmartThreadPool st = new SmartThreadPool();
                tasks[i] = st.QueueWorkItem(() =>
                {
                    var data = new Message() { ID = 10, CreateDate = DateTime.Now, Data = "This is a test" };
                    var buffer666 = MessageSharkSerializer.Serialize(data);
                    //threads.Add(Thread.CurrentThread.ManagedThreadId);
                });
            }
            bool success = SmartThreadPool.WaitAll(tasks);
            //if (success)  result1 = (int)wir1.Result;  
            smartThreadPool.Shutdown();

            //var tasks = new Task[count];
            ////var threads = new ConcurrentBag<int>();
            //for (var i = 0; i < count; i++)
            //{
            //    tasks[i] = Task.Factory.StartNew(() =>
            //    {
            //        var data = new Message() { ID = 10, CreateDate = DateTime.Now, Data = "This is a test" };
            //        var buffer = MessageSharkSerializer.Serialize(data);
            //        //threads.Add(Thread.CurrentThread.ManagedThreadId);
            //    }).ContinueWith(t => Handle(t), TaskContinuationOptions.OnlyOnFaulted)
            //    .ContinueWith(_ => { });
            //}
            //Task.WaitAll(tasks);
        }

        public static void ShouldDeserializeTOMessageFromMemoryStream()
        {
            byte[] buffer = null;
            using (var ms = new MemoryStream())
            {
                var message = new Message() { ID = 10, CreateDate = DateTime.Now, Data = "This is a test", test = new Test() { Int = 100, Str = "Testing", UUID = Guid.NewGuid() } };
                MessageSharkSerializer.Serialize(message, ms);
                buffer = ms.ToArray();
            }

            using (var ms2 = new MemoryStream(buffer))
            {
                ms2.Seek(0, SeekOrigin.Begin);
                var msg = MessageSharkSerializer.Deserialize(typeof(Message), ms2);
            }
        }


        public static void ShouldSerializeMessageToMemoryStream()
        {
            using (var ms = new MemoryStream())
            {
                var message = new Message() { ID = 10, CreateDate = DateTime.Now, Data = "This is a test", test = new Test() { Int = 100, Str = "Testing", UUID = Guid.NewGuid() } };
                MessageSharkSerializer.Serialize(message, ms);
                var buffer = ms.ToArray();
            }
        }



        public static void ShouldSerializeMessageWithObjectToMemoryStream()
        {
            using (var ms = new MemoryStream())
            {
                object message = new Message() { ID = 10, CreateDate = DateTime.Now, Data = "This is a test", test = new Test() { Int = 100, Str = "Testing", UUID = Guid.NewGuid() } };
                MessageSharkSerializer.Serialize(message, ms);
                var buffer = ms.ToArray();
            }
        }

    }
}
