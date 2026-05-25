using Lab5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5.Service
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }
        public int Difficulty { get; set; } = 3;

        private readonly int _adjustmentInterval = 2;
        private readonly double _targetBlockTime = 5;


        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly TransactionService _transactionService;
        public BlockChainService()
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            CreateGenesisBlock();
            _miningService = new MiningService();
            _transactionService = new TransactionService();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, new List<Transaction>(), "0");
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock);
            Chain.Add(genesisBlock);
        }
        public void AddBlock(List<Transaction> data)
        {
            foreach (var tx in data)
            {
                if (!_transactionService.ValidateTransaction(tx).isValid)
                {
                    throw new ArgumentException("Invalid transaction in block data.");
                }
                if (TransactionExists(tx.Id))
                {
                    Console.WriteLine($"Warning: Transaction with ID {tx.Id} already exists in the blockchain. Skipping duplicate transaction.");
                    return;
                }
            }


            var previousBlock = Chain.Last();
            var newBlock = new Block(previousBlock.Index + 1, data, previousBlock.Hash);
            newBlock.Difficulty = Difficulty;
            _miningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);
            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }

        private void AdjustDifficulty()
        {
            var recentBlock = Chain.Where(x => x.Index > 0).TakeLast(_adjustmentInterval).ToList(); 

            if (recentBlock.Count == 0)
            {
                return;
            }

            double averageTime = recentBlock.Average(x => (x.Timestamp - Chain[x.Index - 1].Timestamp).TotalSeconds);

            if (averageTime < _targetBlockTime) 
            {
                Difficulty++;
            }
            else if (averageTime > _targetBlockTime) 
            {
                Difficulty = Math.Max(1, Difficulty - 1);
            }
        }
        public bool IsChainValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return false;
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return false;
                if (!currentBlock.Hash.StartsWith(new string('0', currentBlock.Difficulty)))
                    return false;
            }
            return true;
        }

        public int GetCorruptedBlockIndex()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock) || currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return i;
                }
            }
            return -1;
        }
        public void HackTheChain(int startIndex)
        {
            for (int i = startIndex; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];
                currentBlock.PreviousHash = previousBlock.Hash;
                currentBlock.Hash = _hashingService.ComputeHash(currentBlock);
            }
        }

        public bool TransactionExists(int id)
        {
            foreach (var block in Chain)
            {
                if (block.Transactions.Any(tx => tx.Id == id))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
