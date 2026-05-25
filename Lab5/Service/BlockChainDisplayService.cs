using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5.Service
{
    public class BlockChainDisplayService
    {
        public void PrintChain(List<Models.Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"Timestamp: {block.Timestamp}");

                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine($"Nonce: {block.Nonce}");
                Console.WriteLine($"Difficulty: {block.Difficulty}");
                Console.WriteLine($"Previous Hash: {block.PreviousHash}");
                Console.WriteLine(new string('-', 20));
                Console.WriteLine("Transactions");
                PrintTransactions(block.Transactions);
            }
        }
        public void PrintDifficultyHistory(List<Models.Block> chain)
        {
            Console.WriteLine("Difficulty History:");
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}, Difficulty: {block.Difficulty}");
            }
        }
        public void PrintChainValidity(bool isValid)
        {
            Console.WriteLine(isValid ? "Blockchain is valid." : "Blockchain is invalid.");
        }

        public void PrintTransactions(List<Models.Transaction> transactions)
        {
            foreach (var tx in transactions)
            {
                Console.WriteLine($"From: {tx.From}");
                Console.WriteLine($"To: {tx.To}");
                Console.WriteLine($"Amount: {tx.Amount}");
                Console.WriteLine($"Memo: {tx.Memo}");
                Console.WriteLine(new string('-', 20));
            }
        }
    }
}
