using Core;
using System;
using System.Collections.Generic;

namespace _demo
{
    class Program
    {
        static List<TEST> list = new List<TEST>();
        static void Main(string[] args)
        {
            // 128; 16384
            for (int k = 1; k < 20000; k++)
                //list.Add(new Test() { Id = k, Level = k * 10, Name = k.ToString() });
                list.Add(new TEST() { ID = k, LEVEL = k * 10, NAME = k.ToString() + "-" + Guid.NewGuid().ToString() });

             test_add();
            //test_SEARCH();
            // test_update();
            // test_remove();

            // test_002();
            // test_003();
            // test_004();

        }

        static void test_update()
        {
            var ab = DataFile.Open(typeof(TEST));

            var rs = ab.Update(
                new TEST() { ID = 1, LEVEL = 10, NAME = "1" },
                new TEST() { ID = 1, NAME = "update item 1" });
        }

        static void test_remove()
        {
            var ab = DataFile.Open(typeof(TEST));

            var r0 = ab.AddItems(new TEST[] {
                new TEST() { ID = 1, NAME = "item 1" },
                new TEST() { ID = 2, NAME = "item 2" },
                new TEST() { ID = 3, NAME = "item 3" },
            });
            var rs = ab.Remove(new TEST() { ID = 2, NAME = "item 2" });

        }

        static void test_add()
        {
            var ab = DataFile.Open(typeof(TEST));
            //var r0 = ab.AddItems(new Test[] {
            //    new Test() { Id = 1, Name = "item 1" },
            //    new Test() { Id = 2, Name = "item 2" },
            //    new Test() { Id = 3, Name = "item 3" },
            //});

            //var r1 = ab.AddItems(new Test[] { new Test() { Id = ab.Count, Name = ab.Count.ToString() } });
            //var r2 = ab.AddItems(new Test[] { new Test() { Id = 1, Name = "1" }, new Test(), new Test(), new Test() { Id = 3, Name = "3" } });
            //var r3 = ab.AddItems(new Test[] { new Test() { Id = 2, Name = "item 2" } });
            //var r4 = ab.AddItem(list[1]);
            var r5 = ab.AddItems(list.ToArray());
        }

        static void test_SEARCH()
        {
            var ab = DataFile.Open(typeof(TEST));
            //var r6 = ab.AddItems(list.ToArray());

            var m = new dbMsg(typeof(TEST))
            {
                Action = dbAction.DB_SELECT,
                Data = new SearchRequest(10, 1, "Name.Contains(@0)", "22"),
            };

            var r0 = m.Post();
            var r1 = m.Post();
            var r2 = m.GoPage(2).Post();

        }

        static void test_004()
        {
            //////////////////////////////////////////////////////
            //////// bytes len item (1 len = 4 bytes)
            //////byte[] _byteLenData = null;
            //////List<byte> li = new List<byte>(2000000);
            //////for (int k = 0; k < 2000000; k++)
            //////    //li.Add(0);
            //////    //li.Add(new Random().Next(k, 2000000 * 2 - k));
            //////    li.Add(255);

            //////using (var ms = new MemoryStream())
            //////{
            //////    ProtoBuf.Serializer.Serialize<List<byte>>(ms, li);
            //////    _byteLenData = ms.ToArray();
            //////}
            //////var _byteLen = BitConverter.GetBytes(_byteLenData.Length);
            //////byte[] b111 = System.Core.BinarySerializer.Serialize(li);


            //////DynamicProperty[] at = new DynamicProperty[]
            //////{
            //////    new DynamicProperty("Id", typeof(int)),
            //////    new DynamicProperty("Level", typeof(long)),
            //////    new DynamicProperty("Name", typeof(string)),
            //////};
            //////Type typeItem = DynamicExpression.CreateClass(at);
            //////Type typeList = typeof(List<>).MakeGenericType(typeItem);

            //////var item = list[0];
            //////object obj = Activator.CreateInstance(typeItem);
            //////foreach (PropertyInfo pi in item.GetType().GetProperties())
            //////    try
            //////    {
            //////        obj.GetType().GetProperty(pi.Name).SetValue(obj, pi.GetValue(item, null), null);
            //////    }
            //////    catch { }

            //////var ls1 = (IList)Activator.CreateInstance(typeList);
            //////ls1.Add(obj);

            //////var ls2 = (IList)typeof(List<>)
            //////    .MakeGenericType(typeItem)
            //////    .GetConstructor(Type.EmptyTypes)
            //////    .Invoke(null);
            //////ls2.Add(obj);

            //////byte[] b1 = System.Core.BinarySerializer.Serialize(obj);
            //////byte[] b2 = System.Core.BinarySerializer.Serialize(typeList, ls2);
            //////string j1 = string.Join(" ", b1.Select(x => x.ToString()).ToArray());
            //////string j2 = string.Join(" ", b2.Select(x => x.ToString()).ToArray());

            //////object v1 = System.Core.BinarySerializer.Deserialize(typeItem, b1);
            //////object v2 = System.Core.BinarySerializer.Deserialize(typeList, b2);

        }

        static void test_002()
        {
            //////var ls = list.AsQueryable().Where("Id > 0");//.Cast<Test>().ToArray();

            //////ItemList item = new ItemList() { Name = "Albert", Data = ls };

            //////DynamicProperty[] props = new DynamicProperty[]
            //////{
            //////    new DynamicProperty("Name", typeof(string)),
            //////    new DynamicProperty("Data", typeof(Test[]))
            //////};
            //////Type type = DynamicExpression.CreateClass(props);
            ////////object obj = Activator.CreateInstance(type);
            ////////type.GetProperty("Name").SetValue(obj, "Albert", null);
            ////////type.GetProperty("Data").SetValue(obj, l0, null);
            ////////string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            //////byte[] buf;
            //////object vall;
            ////////////////using (var ms = new MemoryStream())
            ////////////////{
            ////////////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            ////////////////    buf = ms.ToArray();
            ////////////////}
            ////////////////using (var ms = new MemoryStream(buf))
            ////////////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);

            //////using (var ms = new MemoryStream())
            //////{
            //////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            //////    buf = ms.ToArray();
            //////}
            //////buf = SevenZip.Compression.LZMA.SevenZipHelper.Compress(buf);
            //////buf = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(buf);
            //////using (var ms = new MemoryStream(buf))
            //////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);

            //////int imax = 1000000;
            //////int[] a1m = new int[imax];
            //////for (int k = 0; k < imax; k++)
            //////    a1m[k] = new Random().Next(k, imax * 2 - k);

            ////////byte[] bi1 = System.Core.BinarySerializer.Serialize(a1m);
            //////byte[] bi2 = null;
            //////using (var ms = new MemoryStream())
            //////{
            //////    ProtoBuf.Serializer.Serialize(ms, a1m);
            //////    bi2 = ms.ToArray();
            //////}
            ////////byte[] bi11 = SevenZip.Compression.LZMA.SevenZipHelper.Compress(bi1);
            ////////byte[] bi22 = SevenZip.Compression.LZMA.SevenZipHelper.Compress(bi2);



        }

        static void test_001()
        {
            ////////var ls = list.AsQueryable().Where("Id > 0");//.Cast<Test>().ToArray();


            ////////ItemList item = new ItemList() { Name = "Albert", Data = ls };

            ////////DynamicProperty[] props = new DynamicProperty[]
            ////////{
            ////////    new DynamicProperty("Name", typeof(string)),
            ////////    new DynamicProperty("Data", typeof(Test[]))
            ////////};
            ////////Type type = DynamicExpression.CreateClass(props);
            //////////object obj = Activator.CreateInstance(type);
            //////////type.GetProperty("Name").SetValue(obj, "Albert", null);
            //////////type.GetProperty("Data").SetValue(obj, l0, null);
            //////////string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            ////////byte[] buf;
            ////////object vall;
            //////////////////using (var ms = new MemoryStream())
            //////////////////{
            //////////////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            //////////////////    buf = ms.ToArray();
            //////////////////}
            //////////////////using (var ms = new MemoryStream(buf))
            //////////////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);

            ////////using (var ms = new MemoryStream())
            ////////{
            ////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            ////////    buf = ms.ToArray();
            ////////}
            ////////buf = SevenZip.Compression.LZMA.SevenZipHelper.Compress(buf);
            ////////buf = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(buf);
            ////////using (var ms = new MemoryStream(buf))
            ////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);


            //////////////////using (var ms = new MemoryStream())
            //////////////////{
            //////////////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            //////////////////    buf = ms.ToArray();
            //////////////////}
            //////////////////buf = Snappy.SnappyCodec.Compress(buf);
            //////////////////buf = Snappy.SnappyCodec.Uncompress(buf);
            //////////////////using (var ms = new MemoryStream(buf))
            //////////////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);


            ////////////////////using (var ms = new MemoryStream())
            ////////////////////{
            ////////////////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            ////////////////////    buf = ms.ToArray();
            ////////////////////}
            ////////////////////int compressedSize = new Snappy.Sharp.SnappyCompressor().MaxCompressedLength(buf.Length);
            ////////////////////byte[] bytes = new byte[compressedSize];
            ////////////////////int k = new Snappy.Sharp.SnappyCompressor().Compress(buf, 0, buf.Length, bytes);

            ////////////////////bytes = bytes.Take(k).ToArray();
            ////////////////////buf = new Snappy.Sharp.SnappyDecompressor().Decompress(bytes, 0, bytes.Length);
            ////////////////////using (var ms = new MemoryStream(buf))
            ////////////////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);

        }
    }

    [Serializable]
    public class TEST
    {
        public int ID { set; get; }
        public long LEVEL { set; get; }
        public string NAME { set; get; }
    }

    [Serializable]
    public class USER
    {
        public int ID { set; get; }
        //[dbField(IsKeyAuto = true)]
        //public long KEY { set; get; }

        //[dbField(IsDuplicate = false)]
        public string USERNAME { set; get; }

        //[dbField(IsEncrypt = true)]
        public string PASSWORD { set; get; }
    }

    public class ItemList
    {
        public string Name { set; get; }
        public object Data { set; get; }
    }

}
