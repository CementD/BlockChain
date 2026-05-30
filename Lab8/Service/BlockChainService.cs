using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Service
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }
        public int Difficulty { get; set; } = 3;

        private readonly int _adjustmentInterval = 2;
        private readonly double _targetBlockTime = 5;
        public List<Transaction> PendingTransactions { get; set; }
        public decimal MiningReward { get; set; } = 50;

        public decimal MinFeePerByte { get; set; } = 0.05m;

        public int MaxBlockSizeBytes { get; set; } = 500;

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
            var previousBlock = Chain.Last();

            var totalFees = PendingTransactions.Sum(tx => tx.Fee);

            var rewardTransaction = new Transaction(
                "COINBASE",
                minerAddress,
                MiningReward + totalFees,
                0
            );

            int remainingSize = MaxBlockSizeBytes - rewardTransaction.Size;

            var transactionsToInclude = new List<Transaction>
            {
                rewardTransaction
            };

            var sortedTransactions = PendingTransactions
                .OrderByDescending(tx => tx.Fee / (decimal)tx.Size)
                .ToList();

            foreach (var tx in sortedTransactions)
            {
                if (tx.Size <= remainingSize)
                {
                    transactionsToInclude.Add(tx);
                    remainingSize -= tx.Size;
                }
            }

            var newBlock = new Block(
                previousBlock.Index + 1,
                transactionsToInclude,
                previousBlock.Hash
            );

            newBlock.Difficulty = Difficulty;

            _miningService.MineBlock(newBlock, Difficulty);

            Chain.Add(newBlock);

            PendingTransactions.Clear();

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


            if (transaction.From != "COINBASE")
            {
                decimal senderBalance = GetPendingBalance(transaction.From);
                if (senderBalance < transaction.Amount + transaction.Fee)
                {
                    throw new InvalidOperationException($"Insufficient balance for address {transaction.From}. Available: {senderBalance}, Required: {transaction.Amount}");
                }
                if (transaction.Fee < MinFeePerByte * transaction.Size)
                {
                    throw new InvalidOperationException($"Transaction fee is too low. Minimum required: {MinFeePerByte * transaction.Size}");
                }
            }
            PendingTransactions.Add(transaction);
        }

        private decimal GetBalance(string address)
        {
            decimal balance = 0;
            foreach (var block in Chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.From == address)
                    {
                        balance -= transaction.Amount + transaction.Fee;
                    }
                    if (transaction.To == address)
                    {
                        balance += transaction.Amount;
                    }
                }
            }
            return balance;
        }

        public decimal GetPendingBalance(string address)
        {
            decimal balance = GetBalance(address);
            foreach (var transaction in PendingTransactions)
            {
                if (transaction.From == address)
                {
                    balance -= transaction.Amount + transaction.Fee;
                }
                if (transaction.To == address)
                {
                    balance += transaction.Amount;
                }
            }
            return balance;
        }
    }
}
