using Lab6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6.Service
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }
        public int Difficulty { get; set; } = 3;

        private readonly int _adjustmentInterval = 2;
        private readonly double _targetBlockTime = 5;
        public  List<Transaction> PendingTransactions { get; set; }
        public decimal MiningReward { get; set; } = 50;

        public int maxTransactionsBlock = 5;
        public int maxPendingTransactionsPerUser = 2;


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
            PendingTransactions = new List<Transaction>();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, new List<Transaction>(), "0");
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock);
            Chain.Add(genesisBlock);
        }

        public void MinePendingTransactions(string minerAddress)
        {
            var rewardTransaction = new Transaction("COINBASE", minerAddress, MiningReward);

            var transactionsToMine = PendingTransactions.Take(maxTransactionsBlock - 1).ToList();

            var previousBlock = Chain.Last();

            var totalTransactions = new List<Transaction>(transactionsToMine) { rewardTransaction };
            var newBlock = new Block(previousBlock.Index + 1, totalTransactions, previousBlock.Hash);

            newBlock.Difficulty = Difficulty;
            _miningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);

            PendingTransactions = PendingTransactions.Skip(transactionsToMine.Count).ToList();

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

        public void AddTransaction(Transaction transaction)
        {
            if (!_transactionService.ValidateTransaction(transaction).isValid)
            {
                throw new ArgumentException($"Invalid transaction from {transaction.From} to {transaction.To} for amount {transaction.Amount}");
            }
            if (PendingTransactions.Count(tx => tx.From == transaction.From) >= maxPendingTransactionsPerUser)
            {
                throw new InvalidOperationException($"User {transaction.From} has too many pending transactions.");
            }
            if (Chain.Any(block => block.Transactions.Any(tx => tx.Id == transaction.Id)))
            {
                throw new InvalidOperationException($"Transaction with ID {transaction.Id} already exists in the blockchain.");
            }
            PendingTransactions.Add(transaction);
        }
    }
}
