﻿using AutoMapper;
using TransactionStore.Contracts;
using TransactionStore.Models.Entities;
using TransactionStore.Models.Enums;
using TransactionStore.Models.Exceptions;
using TransactionStore.Models.Models;
using ILogger = NLog.ILogger;

namespace TransactionStore.BLL;

public class TransactionManager : ITransactionManager
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly CurrencyRate _currencyRate;
    private readonly ILogger _logger;

    public TransactionManager(ITransactionRepository transactionRepository, IMapper mapper, ILogger logger)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _currencyRate = new CurrencyRate();
        _logger = logger;
    }

    public async Task<int> CreateTransactionAsync(Transaction transaction)
    {
        transaction.Type = transaction.Amount < 0 ? TransactionType.Withdraw : TransactionType.Deposit;

        if (transaction.Type == TransactionType.Withdraw)
        {
            if (!await IsEnoughMoneyForTransaction(transaction))
            {
                _logger.Warn("Not enough money for transaction");
                throw new MoneyIsNotEnoughException("Not enough money for transaction");
            }
        }

        TransactionEntity transactionEntity = _mapper.Map<TransactionEntity>(transaction);
        int transactionId = await _transactionRepository.CreateTransactionAsync(transactionEntity);

        return transactionId;
    }

    public async Task<List<int>> CreateTransferTransactionAsync(TransferTransaction transaction)
    {
        Transaction transferWithdraw = new Transaction()
        {
            AccountId = transaction.AccountId,
            Type = TransactionType.TransferWithdraw,
            Amount = -transaction.Amount
        };

        if (!await IsEnoughMoneyForTransaction(transferWithdraw))
        {
            MoneyIsNotEnoughException ex = new("Not enough money for transaction");
            _logger.Warn(ex.Message);
            throw ex;
        }

        Transaction transferDeposit = new Transaction()
        {
            AccountId = transaction.TargetAccountId,
            Type = TransactionType.TransferDeposit,
            Amount = transaction.Amount * _currencyRate.GetRate(transaction.MoneyType, transaction.TargetMoneyType)
        };

        TransactionEntity transferWithdrawEntity = _mapper.Map<TransactionEntity>(transferWithdraw);
        TransactionEntity transferDepositEntity = _mapper.Map<TransactionEntity>(transferDeposit);
        List<int> resultIds = await _transactionRepository.CreateTransferTransactionAsync(transferWithdrawEntity, transferDepositEntity);

        return resultIds;
    }

    public async Task<decimal> GetAccountBalanceAsync(int accountId)
    {
        return await _transactionRepository.GetAccountBalanceAsync(accountId);
    }

    public async Task<Transaction> GetTransactionByIdAsync(int transactionId)
    {
        TransactionEntity callback = await _transactionRepository.GetTransactionByIdAsync(transactionId);
        Transaction transaction = _mapper.Map<Transaction>(callback);

        return transaction;
    }

    public async Task<List<Object>> GetAllTransactionsByAccountIdAsync(int accountId)
    {
        List<TransactionEntity> callback = await _transactionRepository.GetAllTransactionsByAccountIdAsync(accountId);
        List<Transaction> transactions = _mapper.Map<List<Transaction>>(callback);
        List<Object> result = CreateTransactionsResponse(transactions);

        return result;
    }

    private async Task<bool> IsEnoughMoneyForTransaction(Transaction transaction)
    {
        decimal accountBalance = await _transactionRepository.GetAccountBalanceAsync(transaction.AccountId);

        return accountBalance >= Math.Abs(transaction.Amount);
    }

    private List<Object> CreateTransactionsResponse(List<Transaction> transactions)
    {
        List<Object> result = new List<Object>();

        foreach (var transaction in transactions)
        {
            if (transaction.Type == TransactionType.TransferWithdraw)
            {
                Transaction deposit = transactions.Find(x => x.Type == TransactionType.TransferDeposit && x.Time == transaction.Time)!;
                TransferTransactionResponse transfer = new(transaction, deposit);

                result.Add(transfer);
            }
            else if (transaction.Type != TransactionType.TransferDeposit)
            {
                result.Add(transaction);
            }
        }

        return result;
    }
}