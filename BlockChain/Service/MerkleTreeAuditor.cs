using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Service
{
    public class MerkleTreeAuditor
    {
        private HashingService _hashingService;
        public MerkleTreeAuditor()
        {
            _hashingService = new HashingService();
        }

        public List<List<string>> BuildFullTree(List<Transaction> transactions)
        {
            List<List<string>> tree = new List<List<string>>();
            if (transactions == null || transactions.Count == 0)
            {
                tree.Add(new List<string> { string.Empty });
                return tree;
            }

            List<string> currentLevel = transactions.Select(t => _hashingService.ComputeSha256(t.ToRowString())).ToList();
            tree.Add(currentLevel);
            while (currentLevel.Count > 1)
            {
                tree.Add(currentLevel);
                if (currentLevel.Count % 2 != 0)
                {
                    currentLevel.Add(currentLevel.Last());
                }

                List<string> nextLevel = new List<string>();
                for (int i = 0; i < currentLevel.Count; i += 2)
                {
                    string combinedHash = _hashingService.ComputeSha256(currentLevel[i] + currentLevel[i + 1]);
                    nextLevel.Add(combinedHash);
                }
                tree.Add(nextLevel);
                currentLevel = nextLevel;
            }

            return tree;
        }

        public void PrintTreeStructure(List<List<string>> fullTree)
        {
            Console.WriteLine("--- MERKLE TREE STRUCTURE ---");

            for (int i = fullTree.Count - 1; i >= 0; i--)
            {
                var level = fullTree[i];
                string levelName = (i == fullTree.Count - 1) ? $" {i} (Root)" : (i == 0) ? $"{i} (Leaves)" : $"{i} (Branches)";
                string hashWord = level.Count == 1 ? "hash" : "hashes";

                Console.WriteLine($"Level {levelName}: {level.Count} {hashWord}");

                var formattedHashes = level.Select(h =>
                    h.Length > 10 ? $"[{h.Substring(0, 8)}...{h.Substring(h.Length - 2)}]" : $"[{h}]"
                );

                Console.WriteLine("  -> " + string.Join(", ", formattedHashes));
            }
            Console.WriteLine();
        }

        public void DetectTampering(Block originalBlock, Transaction tamperedTransaction)
        {
            Console.WriteLine("--- TAMPER DETECTION REPORT ---");

            if (originalBlock == null || originalBlock.Transactions == null || originalBlock.Transactions.Count == 0)
            {
                return;
            }

            List<List<string>> originalTree = BuildFullTree(originalBlock.Transactions);
            List<string> originalLeaves = originalTree[0];

            List<Transaction> tamperedTransactions = new List<Transaction>();
            int targetIndex = -1;

            for (int i = 0; i < originalBlock.Transactions.Count; i++)
            {
                Transaction currentTx = originalBlock.Transactions[i];

                if (currentTx == tamperedTransaction)
                {
                    targetIndex = i;

                    Transaction hackedTx = new Transaction(
                        currentTx.From,
                        currentTx.To + "_HACKED",
                        currentTx.Amount + 500.50m,
                        currentTx.Size,
                        currentTx.TokenSymbol
                    );
                    hackedTx.Fee = currentTx.Fee;

                    tamperedTransactions.Add(hackedTx);
                }
                else
                {
                    tamperedTransactions.Add(currentTx);
                }
            }

            if (targetIndex == -1)
            {
                return;
            }

            List<List<string>> tamperedTree = BuildFullTree(tamperedTransactions);
            List<string> tamperedLeaves = tamperedTree[0];

            for (int i = 0; i < originalLeaves.Count; i++)
            {
                if (originalLeaves[i] != tamperedLeaves[i])
                {
                    string txId = originalBlock.Transactions[i].Id.ToString() ?? "N/A";

                    Console.WriteLine($"Увага! Транзакція з індексом [{i}] (ID: [{txId}]) була підроблена!");
                    Console.WriteLine($"  -> Expected hash: {originalLeaves[i].Substring(0, 8)}...");
                    Console.WriteLine($"  -> Received hash:  {tamperedLeaves[i].Substring(0, 8)}...");
                }
            }
        }

        public List<string> MerkleProof(List<List<string>> fullTree, string targetHash)
        {
            List<string> proof = new List<string>();

            if (fullTree == null || fullTree.Count == 0)
                return proof;

            int index = fullTree[0].FindIndex(h => h == targetHash);
            if (index == -1)
                return proof;

            proof.Add(index.ToString());

            for (int i = 0; i < fullTree.Count - 1; i++)
            {
                List<string> currentLevel = fullTree[i];
                int siblingIndex = (index % 2 == 0) ? index + 1 : index - 1;

                if (siblingIndex < currentLevel.Count)
                {
                    proof.Add(currentLevel[siblingIndex]);
                }
                else
                {
                    proof.Add(currentLevel[index]);
                }

                index /= 2;
            }
            return proof;
        }

        public bool VerifyMerkleProof(string targetHash, List<string> proof, string expectedRoot)
        {
            if (proof == null || proof.Count == 0)
                return false;

            int index = int.Parse(proof[0]);
            string computedHash = targetHash;
            for (int i = 1; i < proof.Count; i++)
            {
                string siblingHash = proof[i];
                if (index % 2 == 0)
                {
                    computedHash = _hashingService.ComputeSha256(computedHash + siblingHash);
                }
                else
                {
                    computedHash = _hashingService.ComputeSha256(siblingHash + computedHash);
                }
                index /= 2;
            }
            return computedHash == expectedRoot;
        }
    }
}
