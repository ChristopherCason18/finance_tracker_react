namespace finance_tracker.Models;

public class transactions
{
    public int id {get; set;}
    public string type {get; set;}
    public string details {get; set;}
    public string particulars {get; set;}
    public string code {get; set;}
    public string reference {get; set;}
    public decimal amount {get; set;}
    public string date {get; set;}
}