namespace SeidorSmallWebApi.Entities
{
    public class ResponseLines
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string, int> Batches { get; set; }
    }
}