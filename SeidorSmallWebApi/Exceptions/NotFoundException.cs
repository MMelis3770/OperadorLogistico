namespace SeidorSmallWebApi.Exceptions
{
    public class NotFoundException : Exception
    {
        public int Code
        {
            get; set;
        }
        public string MessageSL
        {
            get; set;
        }
        public NotFoundException(int code, string messageSL, Exception innerException) : base(messageSL, innerException) { Code = code; MessageSL = messageSL; }
    }
}
