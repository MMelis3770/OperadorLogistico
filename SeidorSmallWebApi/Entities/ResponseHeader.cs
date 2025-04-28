namespace SeidorSmallWebApi.Entities
{
    public class ResponseHeader
    {
        public int DocNum { get; set; }
        public string CardCode { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<ResponseLines> Lines { get; set; }
    }
}