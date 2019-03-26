namespace Sample.Common
{
    public class ResponseResult<T>
    {
        public bool Success;
        public T Result;
        public string Error;
    }
}
