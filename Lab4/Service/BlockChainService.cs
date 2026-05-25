using Lab4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4.Service
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }
        public int Difficulty { get; set; } = 3;

        private readonly int _adjustmentInterval = 2;
        private readonly double _targetBlockTime = 5;
        public const int MaxTransactionsPerBlock = 3;


        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        public BlockChainService()
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            CreateGenesisBlock();
            _miningService = new MiningService();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, new List<Transaction>(), "0");
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock);
            Chain.Add(genesisBlock);
        }
        public void AddBlock(List<Transaction> data)
        {
            if (data.Count > MaxTransactionsPerBlock)
            {
                throw new ArgumentException($"A block can contain a maximum of {MaxTransactionsPerBlock} transactions.");
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
                if (currentBlock.Timestamp <= previousBlock.Timestamp)
                    return false;
                if (currentBlock.Transactions.Count > MaxTransactionsPerBlock)
                    return false;
                if (currentBlock.Timestamp > DateTime.Now.AddMinutes(1))
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
    }
}
