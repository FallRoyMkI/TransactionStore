﻿using Microsoft.EntityFrameworkCore;
using TransactionStore.Models.Entities;

namespace TransactionStore.DAL;

public class Context : DbContext
{
    public DbSet<TransactionEntity> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseSqlServer(@"Data Source= DESKTOP-MH87Q5L\SQLEXPRESS;Initial Catalog = TransactionStoreSSSS;Trusted_Connection=True; TrustServerCertificate=True;", builder => builder.EnableRetryOnFailure());
    }
}