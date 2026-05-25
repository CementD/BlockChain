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


            while (true)
            {

                Console.WriteLine("Blockchain Menu:");
                Console.WriteLine("1. Add Transaction");
                Console.WriteLine("2. Mine Pending Block");
                Console.WriteLine("3. Display Blockchain");
                Console.WriteLine("4. Exit");
                Console.WriteLine("5. Task 3");
                Console.WriteLine("Enter your choice:");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        try
                        {
                            blockChain.AddTransaction(transactionService.CreateTransaction(AliceWallet, BobWallet.Address, 10));
                            Console.WriteLine("Transaction successfully added to Mempool!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] {ex.Message}");
                        }
                        break;
                    case "2":
                        blockChain.MinePendingTransactions(BobWallet.Address);
                        break;
                    case "3":
                        displayService.PrintChain(blockChain.Chain);
                        displayService.PrintChainValidity(blockChain.IsChainValid());
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }


    }
}
