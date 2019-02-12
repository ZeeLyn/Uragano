using System.Collections.Generic;
using Uragano.Codec.MessagePack;
using Xunit;

namespace XUnitTest
{
    public class CodecTest
    {

        [Theory]
        [InlineData(null)]
        [MemberData(nameof(GetEntity))]
        public void SerializeTest(TestCodecEntity obj)
        {
            var bytes = SerializerHelper.Serialize(obj);
            Assert.NotNull(bytes);
        }

        [Fact]
        public void DeserializeTest()
        {
            var entity = new TestCodecEntity { Id = 1, Name = "Jack" };
            var bytes = SerializerHelper.Serialize(entity);
            Assert.Same(typeof(TestCodecEntity), SerializerHelper.Deserialize(bytes).GetType());
            Assert.Same(typeof(TestCodecEntity), SerializerHelper.Deserialize<TestCodecEntity>(bytes).GetType());
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