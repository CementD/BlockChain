using Lab4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4.Service
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
                Console.WriteLine(new string('-', 20));
            }
        }

        public void PrintTransactionsHistory(List<Models.Block> chain, string address)
        {
            bool found = false;
            Console.WriteLine($"Transaction history for address: {address}");
            for (int i = 1; i < chain.Count; i++)
            {
                var block = chain[i];
                foreach (var tx in block.Transactions)
                {
                    if (tx.From == address || tx.To == address)
                    {
                        found = true;
                        Console.WriteLine($"Block Index: {block.Index}");
                        Console.WriteLine($"From: {tx.From}");
                        Console.WriteLine($"To: {tx.To}");
                        Console.WriteLine($"Amount: {tx.Amount}");
                        Console.WriteLine(new string('-', 20));
                    }
                }
            }
            if (!found)
            {
                Console.WriteLine("No transactions found for this address.");
            }
        }

        public void PrintWhaleTransaction(List<Block> chain)
        {
            var result = chain.Skip(1).SelectMany(b => b.Transactions.Select(tx => new { BlockIndex = b.Index, Transaction = tx }))
                .OrderByDescending(x => x.Transaction.Amount).FirstOrDefault();

            if (result != null)
            {
                Console.WriteLine($"Whale Transaction found in Block Index: {result.BlockIndex}");
                Console.WriteLine($"From: {result.Transaction.From}");
                Console.WriteLine($"To: {result.Transaction.To}");
                Console.WriteLine($"Amount: {result.Transaction.Amount}");
            }
            else
            {
                Console.WriteLine("No transactions found in the blockchain.");
            }
        }
    }
}
