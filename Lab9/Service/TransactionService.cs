using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Service
{
    public class TransactionService
    {
        private readonly WalletService _walletService;
        public TransactionService()
        {
            _walletService = new WalletService();
        }
        public Transaction CreateTransaction(Wallet sender, string to, decimal amount, decimal fee)
        {
            var tx =  new Transaction(sender.Address, to, amount, fee);
            tx.SenderPublicKey = sender.PublicKey;
            tx.Signature = sender.Sign(tx.GetDataToSign());
            if (ValidateTransaction(tx).isValid) {
                return tx;
            }
            else {
                throw new ArgumentException("Invalid transaction data.");
            }
        }

        public (bool isValid, string errorMessage) ValidateTransaction(Transaction transaction)
        {
            if (transaction.From == "COINBASE") 
                return (true, string.Empty); // Coinbase transactions are always valid
            if (string.IsNullOrEmpty(transaction.From))
                return (false, "Sender address is required.");
            if (string.IsNullOrEmpty(transaction.To))
                return (false, "Recipient address is required.");
            if (transaction.Amount <= 0)
                return (false, "Amount must be greater than zero.");
            if (transaction.SenderPublicKey == null || transaction.Signature == null)
                return (false, "Transaction must be signed.");
            
            bool signatureValid = _walletService.VerifySignature(
                transaction.GetDataToSign(),
                transaction.Signature,
                transaction.SenderPublicKey
            );
            if (!signatureValid)
                return (false, "Invalid transaction signature.");

            return (true, string.Empty);
        }
    }
}
