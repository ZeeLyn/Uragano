namespace Uragano.Caching.Memory
{
    public class MemoryCachingValue
    {
        public MemoryCachingValue(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}
