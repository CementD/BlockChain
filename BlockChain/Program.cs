using BlockChain.Service;
using System.Net.WebSockets;
using System.Diagnostics;
using BlockChain.Models;

namespace BlockChain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var blockChain = new BlockChainService();
            var displayService = new BlockChainDisplayService();
            var transactionService = new TransactionService();

            //var AliceWallet = new WalletService().CreateWallet("Alice");
            //var BobWallet = new WalletService().CreateWallet("Bob");
            //var CharlieWallet = new WalletService().CreateWallet("Charlie");

            //while (true)
            //{

            //    Console.WriteLine("Blockchain Menu:");
            //    Console.WriteLine("1. Add Transaction");
            //    Console.WriteLine("2. Mine Pending Block");
            //    Console.WriteLine("3. Display Blockchain");
            //    Console.WriteLine("4. Show Alice Balance");
            //    Console.WriteLine("5. Show Bob Balance");
            //    Console.WriteLine("6. Exit");
            //    Console.WriteLine("Enter your choice:");

            //    var choice = Console.ReadLine();
            //    switch (choice)
            //    {
            //        case "1":
            //            try
            //            {
            //                blockChain.AddTransaction(transactionService.CreateTransaction(AliceWallet, BobWallet.Address, 10, 8));
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine($"Error adding transaction: {ex.Message}");
            //            }
            //            break;
            //        case "2":
            //            blockChain.MinePendingTransactions(AliceWallet.Address);
            //            break;
            //        case "3":
            //            displayService.PrintChain(blockChain.Chain);
            //            displayService.PrintChainValidity(blockChain.IsChainValid());
            //            break;
            //        case "4":
            //            var aliceBalance = blockChain.GetPendingBalance(AliceWallet.Address);
            //            Console.WriteLine($"Alice's Balance: {aliceBalance}");
            //            break;
            //        case "5":
            //            var bobBalance = blockChain.GetPendingBalance(BobWallet.Address);
            //            Console.WriteLine($"Bob's Balance: {bobBalance}");
            //            break;
            //        case "6":
            //            return;
            //        default:
            //            Console.WriteLine("Invalid choice. Please try again.");
            //            break;
            //    }
            //}
            var tx1 = new Transaction("Alice", "Bob", 10, 2);
            var tx2 = new Transaction("Bob", "Charlie", 5, 1);
            var tx3 = new Transaction("Charlie", "David", 20, 1);
            var tx4 = new Transaction("David", "Elena", 15, 1);

            var txList = new List<Transaction> { tx1, tx2, tx3, tx4 };

            var block = new Block(1, txList, "000abc123");

            var auditor = new MerkleTreeAuditor();

            var fullTree = auditor.BuildFullTree(block.Transactions);
            auditor.PrintTreeStructure(fullTree);

            auditor.DetectTampering(block, tx3);
        }

        
    }
}
