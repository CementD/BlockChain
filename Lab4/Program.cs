using Lab4.Models;
using Lab4.Service;

namespace BlockChain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var blockChain = new BlockChainService();
            var displayService = new BlockChainDisplayService();
            var transactionService = new TransactionService();

            blockChain.AddBlock(new List<Transaction>
            {
                transactionService.CreateTransaction("Alice","Bob",10),
                transactionService.CreateTransaction("Bob","Charlie",5),
                transactionService.CreateTransaction("Charlie","Dave",2),
            });

            blockChain.AddBlock(new List<Transaction>
            {
                transactionService.CreateTransaction("Bob","Alice",7),
                transactionService.CreateTransaction("Charlie","Bob",12),
                transactionService.CreateTransaction("Alice","Dave",4),
            });

            blockChain.AddBlock(new List<Transaction>
            {
                transactionService.CreateTransaction("Charlie","Eve",15),
                transactionService.CreateTransaction("Bob","Dave",9),
                transactionService.CreateTransaction("Alice","Charlie",11),
            });

            blockChain.AddBlock(new List<Transaction>
            {
                transactionService.CreateTransaction("Eve","Dave",20),
                transactionService.CreateTransaction("Alice","Eve",6),
                transactionService.CreateTransaction("Bob","Alice",3),
            });

            blockChain.AddBlock(new List<Transaction>
            {
                transactionService.CreateTransaction("Bob","Eve",14),
                transactionService.CreateTransaction("Eve","Alice",9),
                transactionService.CreateTransaction("Alice","Charlie",13),
            });

            displayService.PrintChain(blockChain.Chain);
            displayService.PrintChainValidity(blockChain.IsChainValid());

            displayService.PrintTransactionsHistory(blockChain.Chain, "Alice");
            displayService.PrintTransactionsHistory(blockChain.Chain, "Spiderman");

            displayService.PrintWhaleTransaction(blockChain.Chain);
        }
    }
}