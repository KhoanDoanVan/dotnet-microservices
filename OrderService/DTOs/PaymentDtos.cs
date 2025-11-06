namespace OrderService.DTOs;



public class PaymentDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}





public class CreatePaymentRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "cash";
}