using Lab6.Models;
using Lab6.Service;
using System.Diagnostics;
using System.Net.WebSockets;

namespace Lab6
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

            for (int i = 0; i < 5; i++)
            {
                blockChain.MinePendingTransactions(AliceWallet.Address);
            }

            var tx1Old = transactionService.CreateTransaction(AliceWallet, BobWallet.Address, 100, 1);
            var tx2 = transactionService.CreateTransaction(AliceWallet, CharlieWallet.Address, 10, 3);

            try {
                Console.WriteLine("Adding transaction 1");
                blockChain.AddTransaction(tx1Old);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction error: {ex.Message}");
            }
            try {
                Console.WriteLine("Adding transaction 2");
                blockChain.AddTransaction(tx2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction error: {ex.Message}");
            }
            Console.WriteLine("\nPending transactions after adding tx1 and tx2:");
            displayService.PrintTransactions(blockChain.PendingTransactions);

            var tx1New = transactionService.CreateTransaction(AliceWallet, CharlieWallet.Address, 100, 2);
            tx1New.ReplacesTxId = tx1Old.Id;
            try {
                Console.WriteLine("\nAdding replacement transaction");
                blockChain.AddTransaction(tx1New);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction error: {ex.Message}");
            }

            Console.WriteLine("\nPending transactions after adding replacement transaction:");
            displayService.PrintTransactions(blockChain.PendingTransactions);
        }
    }
}
