using Lab5.Models;
using Lab5.Service;
using System.Diagnostics;
using System.Net.WebSockets;

namespace BlockChain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var blockChain = new BlockChainService();
            var displayService = new BlockChainDisplayService();
            var transactionService = new TransactionService();

            var AliceWallet = new WalletService().CreateWallet("Alice");
            var BobWallet = new WalletService().CreateWallet("Bob");
            var CharlieWallet = new WalletService().CreateWallet("Charlie");

            displayService.PrintWalletCard(AliceWallet);
            displayService.PrintWalletCard(BobWallet);

            //var tx1 = new TransactionService().CreateTransaction(AliceWallet, BobWallet.Address, 10, "For Pizza");
            //var tx2 = new TransactionService().CreateTransaction(BobWallet, AliceWallet.Address, 5, "For Donating in BS");
            //var tx3 = new TransactionService().CreateTransaction(CharlieWallet, AliceWallet.Address, 2, "For Tun tun tun Sahur");

            //blockChain.AddBlock(new List<Transaction> { tx1, tx2, tx3 });
            //displayService.PrintChain(blockChain.Chain);
            //displayService.PrintChainValidity(blockChain.IsChainValid());

            Transaction badTx = new Transaction(
                from: AliceWallet.Address,
                to: BobWallet.Address, 
                amount: 250.00m,
                memo: "Steal money"
            );
            badTx.SenderPublicKey = BobWallet.PublicKey;

            badTx.Signature = BobWallet.Sign(badTx.GetDataToSign());

            var (isValid, errorMessage) = transactionService.ValidateTransaction(badTx);

            if (isValid)
            {
                Console.WriteLine("Transaction successfull and money stolen :(");
            }
            else
            {
                Console.WriteLine("Hacker stopped!");   
            }

            Transaction validTx = new Transaction(
                from: AliceWallet.Address,
                to: BobWallet.Address,
                amount: 20.00m,
                memo: "Transaction for part 3"
            );
            validTx.SenderPublicKey = AliceWallet.PublicKey;
            validTx.Signature = AliceWallet.Sign(validTx.GetDataToSign());

            if (validTx.Signature != null && validTx.Signature.Length > 0)
            {
                validTx.Signature[0] = 99;
                Console.WriteLine("One byte was changed!");
            }

            var (isSigValid, sigErrorMessage) = transactionService.ValidateTransaction(validTx);

            if (!isSigValid)
            {
                Console.WriteLine($"Invalid sigh was successfully noticed!");
                Console.WriteLine($"{sigErrorMessage}");
            }
            else
            {
                Console.WriteLine($"Invalid sigh was not noticed!");
            }
        }
    }
}
