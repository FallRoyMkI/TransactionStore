﻿namespace TransactionStore.Models.Dtos;
public class TransactionDtoTransferResponse
{
    public int WithdrawId { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public int DepositId { get; set; }
    public int TargetAccountId { get; set; }
    public decimal TargetAmount { get; set; }
    public string Type { get; set; }
    public DateTime time { get; set; }
}