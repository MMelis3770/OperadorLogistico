namespace SeidorSmallWebApi.Exceptions
{
    public class EntityAlreadyExistsException : Exception
    {
        public int Code { get; set; }
        public string MessageSL { get; set; }

        public EntityAlreadyExistsException(int code, string messageSL, Exception innerException) : base(messageSL, innerException)
        {
            Code = code;
            MessageSL = messageSL;
        }
    }
}