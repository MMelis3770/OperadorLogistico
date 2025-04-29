namespace OperadorLogistico.Console.Models
{
    public class Batch
    {
        public string ItemCode { get; set; }
        public string BatchNum { get; set; }
        public DateTime PrdDate { get; set; }
        public DateTime ExpDate { get; set; }
        public int Quantity { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockReason { get; set; }

        public override string ToString()
        {
            return $"Batch [ItemCode={ItemCode}, BatchNum={BatchNum}, PrdDate={PrdDate:yyyy-MM-dd}, ExpDate={ExpDate:yyyy-MM-dd}, Quantity={Quantity}, IsBlocked={IsBlocked}, BlockReason={BlockReason}]";
        }
    }
}
