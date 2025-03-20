namespace Console.Communication.Api.Dto
{
    public class ConfirmationDto
    {
        public string OrderNumber { get; set; }
        public DateTime ConfirmationDate { get; set; }
        public string Status { get; set; }
        public List<ConfirmationLineDto> Lines { get; set; } = new List<ConfirmationLineDto>();
    }

    public class ConfirmationLineDto
    {
        public int LineNumber { get; set; }
        public string ProductCode { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal AssignedQuantity { get; set; }
        public string Status { get; set; }
        public List<BatchAssignmentDto> Batches { get; set; } = new List<BatchAssignmentDto>();
    }

    public class BatchAssignmentDto
    {
        public string BatchCode { get; set; }
        public decimal Quantity { get; set; }
    }
}
