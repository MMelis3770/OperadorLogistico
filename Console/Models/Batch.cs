namespace OperadorLogistico.Console.Models
{
    public class Batch
    {
        public string ItemCode { get; set; }
        public string BatchCode { get; set; }
        public DateTime PrdDate { get; set; }
        public DateTime ExpDate { get; set; }
        public int Quantity { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockReason { get; set; }
    }
}
