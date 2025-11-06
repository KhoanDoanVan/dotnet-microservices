namespace OrderService.Models;


public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}


public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer,
    EWallet
}