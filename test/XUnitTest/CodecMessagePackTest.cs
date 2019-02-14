using System.Collections.Generic;
using Uragano.Codec.MessagePack;
using Xunit;

namespace XUnitTest
{
    public class CodecMessagePackTest
    {

        [Theory]
        [InlineData(null)]
        [MemberData(nameof(GetEntity))]
        public void SerializeTest(TestCodecEntity obj)
        {
            var bytes = SerializerHelper.Serialize(obj);
            Assert.NotNull(bytes);
            var codec = new MessagePackCodec();
            Assert.NotNull(codec.Serialize(obj));
        }

        [Fact]
        public void DeserializeTest()
        {
            var entity = new TestCodecEntity { Id = 1, Name = "Jack" };
            var bytes = SerializerHelper.Serialize(entity);
            Assert.Same(typeof(TestCodecEntity), SerializerHelper.Deserialize(bytes).GetType());
            Assert.Same(typeof(TestCodecEntity), SerializerHelper.Deserialize<TestCodecEntity>(bytes).GetType());

            Assert.Null(SerializerHelper.Deserialize(null));
            Assert.Null(SerializerHelper.Deserialize<TestCodecEntity>(null));

            var codec = new MessagePackCodec();
            Assert.Same(typeof(TestCodecEntity), codec.Deserialize(bytes, typeof(TestCodecEntity)).GetType());
            Assert.Same(typeof(TestCodecEntity), codec.Deserialize<TestCodecEntity>(bytes).GetType());

            Assert.Null(codec.Deserialize(null, typeof(TestCodecEntity)));
            Assert.Null(codec.Deserialize<TestCodecEntity>(null));
        }

        public static List<object[]> GetEntity()
        {
            return new List<object[]> { new object[] { new TestCodecEntity { Id = 1, Name = "Jack" } } };
        }
    }

    public class TestCodecEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}