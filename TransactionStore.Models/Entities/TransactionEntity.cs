﻿using TransactionStore.Models.Enums;

namespace TransactionStore.Models.Entities;

public class TransactionEntity
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Time { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is TransactionEntity entity &&
               Id == entity.Id &&
               AccountId == entity.AccountId &&
               Amount == entity.Amount &&
               Type == entity.Type &&
               Time == entity.Time;
    }
}