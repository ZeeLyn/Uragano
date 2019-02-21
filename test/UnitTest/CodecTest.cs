using NUnit.Framework;
using Uragano.Codec.MessagePack;

namespace UnitTest
{
    public class CodecTest
    {
        [Test]
        public void SerializeTest()
        {
            var bytes = SerializerHelper.Serialize(new TestCodecEntity { Id = 1, Name = "Jack" });
            Assert.IsNotNull(bytes);
        }

        [Test]
        public void DeserializeTest()
        {
            var entity = new TestCodecEntity { Id = 1, Name = "Jack" };
            var bytes = SerializerHelper.Serialize(entity);
            Assert.IsInstanceOf(typeof(TestCodecEntity), SerializerHelper.Deserialize(bytes));
            Assert.IsInstanceOf(typeof(TestCodecEntity), SerializerHelper.Deserialize<TestCodecEntity>(bytes));
        }
    }

    public class TestCodecEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}