using Lab4.Models;
using Lab4.Service;
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
            var hashingService = new HashingService();

            //for (int i = 0; i < 6; i++)
            //{
            //    var transactions = GenerateRandomTransactions();
            //    blockChain.AddBlock(transactions);
            //    displayService.PrintChain(blockChain.Chain.TakeLast(5).ToList());
            //    displayService.PrintChainValidity(blockChain.IsChainValid());
            //    Thread.Sleep(2000);
            //    Console.Clear();
            //}
            //displayService.PrintDifficultyHistory(blockChain.Chain);
            try
            {
                blockChain.AddBlock(new List<Transaction>
                {
                    transactionService.CreateTransaction("Alice", "Bob", 10),
                    transactionService.CreateTransaction("Bob", "Charlie", 5),
                    transactionService.CreateTransaction("Charlie", "Dave", 2),
                    transactionService.CreateTransaction("Dave", "Eve", 1)

                });
                displayService.PrintChain(blockChain.Chain);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("ERROR: " + ex.Message);

                Console.ResetColor();
            }

            blockChain.AddBlock(new List<Transaction>
            {
                new Transaction("Alice", "Bob", 50)
            });

            blockChain.AddBlock(new List<Transaction>
            {
                new Transaction("Bob", "Charlie", 20)
            });

            blockChain.AddBlock(new List<Transaction>
            {
                new Transaction("Charlie", "Dave", 10)
            });

            displayService.PrintChain(blockChain.Chain);
            displayService.PrintChainValidity(blockChain.IsChainValid());

            var hackedBlock = blockChain.Chain[1];
            hackedBlock.Transactions[0].Amount = 1000000;
            hackedBlock.Hash = hashingService.ComputeHash(hackedBlock);

            displayService.PrintChain(blockChain.Chain);
            displayService.PrintChainValidity(blockChain.IsChainValid());
        }

        public static List<Transaction> GenerateRandomTransactions()
        {
            var tx1 = new Transaction("Alice", "Bob", 10);
            var tx2 = new Transaction("Bob", "Charlie", 5);
            var tx3 = new Transaction("Charlie", "Dave", 2);
            var tx4 = new Transaction("Dave", "Eve", 1);
            var tx5 = new Transaction("Eve", "Alice", 0.5m);
            var tx6 = new Transaction("Alice", "Charlie", 3);
            var tx7 = new Transaction("Bob", "Dave", 4);

            var random = new Random();
            var numOfTransactions = random.Next(1, 8);
            var transactions = new List<Transaction>();

            foreach (var tx in new[] { tx1, tx2, tx3, tx4, tx5, tx6, tx7 }.OrderBy(x => random.Next()).Take(numOfTransactions))
            {
                transactions.Add(tx);
            }
            return transactions;
        }
    }
}
