using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab4.Models;

namespace Lab4.Service
{
    public class TransactionService
    {
        public Transaction CreateTransaction(string from, string to, decimal amount)
        {
            var trans =  new Transaction(from, to, amount);
            if (ValidateTransaction(trans).isValid) {
                return trans;
            }
            else {
                throw new ArgumentException("Invalid transaction data.");
            }
        }

        public (bool isValid, string errorMessage) ValidateTransaction(Transaction transaction)
        {
            if (string.IsNullOrEmpty(transaction.From))
                return (false, "Sender address is required.");
            if (string.IsNullOrEmpty(transaction.To))
                return (false, "Recipient address is required.");
            if (transaction.Amount <= 0)
                return (false, "Amount must be greater than zero.");
            return (true, string.Empty);
        }
    }
}
