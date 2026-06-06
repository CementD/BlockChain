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
        public int MaxTransactionsPerBlock { get; set; } = 5;

        public decimal MinFeePerByte { get; set; } = 0.00005m;

        public int MaxBlockSizeBytes { get; set; } = 1000;

        public int MaxReorgDepth { get; set; } = 5;

        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly TransactionService _transactionService;
        private readonly FileService _fileService;
        public BlockChainService(FileService fileService)
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            CreateGenesisBlock();
            _miningService = new MiningService();
            _transactionService = new TransactionService();
            _fileService = fileService;

            var loadChain = _fileService.LoadChain();
            if (loadChain.Any())
            {
                Chain = loadChain;
            }

            string memPoolFilePath = "mempool.json";
            if (File.Exists(memPoolFilePath))
            {
                var json = File.ReadAllText(memPoolFilePath);
                var pendingTransactions = System.Text.Json.JsonSerializer.Deserialize<List<Transaction>>(json);
                if (pendingTransactions != null)
                    PendingTransactions = pendingTransactions;
            }
            if (PendingTransactions == null)
            {
                PendingTransactions = new List<Transaction>();
            }
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, new List<Transaction>(), "0");
            genesisBlock.Timestamp = new DateTime(2024, 1, 1);
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock);
            Chain.Add(genesisBlock);
        }

        //public void MinePendingTransactions(string minerAddress)
        //{
        //    var previousBlock = Chain.Last();

        //    var totalFees = PendingTransactions.OrderByDescending(tx => tx.Fee).Take(MaxTransactionsPerBlock);


        //    var rewardTransaction = new Transaction(
        //        "COINBASE",
        //        minerAddress,
        //        MiningReward + totalFees.Sum(tx => tx.Fee),
        //        0
        //    );

        //    int remainingSize = MaxBlockSizeBytes - rewardTransaction.Size;

        //    var transactionsToInclude = new List<Transaction>
        //    {
        //        rewardTransaction
        //    };

        //    var sortedTransactions = PendingTransactions
        //        .OrderByDescending(tx => tx.Fee / (decimal)tx.Size)
        //        .ToList();

        //    foreach (var tx in sortedTransactions)
        //    {
        //        if (tx.Size <= remainingSize)
        //        {
        //            transactionsToInclude.Add(tx);
        //            remainingSize -= tx.Size;
        //        }
        //    }

        //    var newBlock = new Block(
        //        previousBlock.Index + 1,
        //        transactionsToInclude,
        //        previousBlock.Hash
        //    );

        //    newBlock.Difficulty = Difficulty;

        //    _miningService.MineBlock(newBlock, Difficulty);

        //    Chain.Add(newBlock);

        //    PendingTransactions.RemoveAll(tx => transactionsToInclude.Contains(tx));

        //    if (newBlock.Index % _adjustmentInterval == 0)
        //    {
        //        AdjustDifficulty();
        //    }
        //}

        public Block MinePendingTransactions(string minerAddress)
        {
            var previousBlock = Chain.Last();
            var transactions = PendingTransactions.OrderByDescending(x => x.Fee).Take(MaxTransactionsPerBlock).ToList();
            var rewardTransaction = new Transaction("COINBASE", minerAddress, MiningReward + transactions.Sum(t => t.Fee), 0);
            transactions.Add(rewardTransaction);

            var newBlock = new Block(previousBlock.Index + 1, transactions, previousBlock.Hash);
            newBlock.Difficulty = Difficulty;
            _miningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);

            _fileService.SaveChainToFile(Chain);

            foreach (var tx in transactions)
            {
                PendingTransactions.Remove(tx);
            }

            SaveMemPool();

            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
            return newBlock;
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
                {
                    return false;
                }

                if (currentBlock.Timestamp > DateTime.Now.AddHours(2))
                {
                    return false;
                }

                var transactionsSeenIds = new List<int>();
                foreach (var transaction in currentBlock.Transactions)
                {
                    if (transaction.From != "COINBASE")
                    {
                        if (transactionsSeenIds.Contains(transaction.Id))
                        {
                            return false; 
                        }
                        transactionsSeenIds.Add(transaction.Id);
                    }
                }
            }
            return true;
        }

        public bool IsChainValid(List<Block> chain)
        {
            for (int i = 1; i < chain.Count; i++)
            {
                var currentBlock = chain[i];
                var previousBlock = chain[i - 1];
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return false;
                if (previousBlock != null && currentBlock.PreviousHash != previousBlock.Hash)
                    return false;
                if (!currentBlock.Hash.StartsWith(new string('0', currentBlock.Difficulty)))
                    return false;
                if (currentBlock.Timestamp <= previousBlock.Timestamp)
                {
                    return false;
                }

                if (currentBlock.Timestamp > DateTime.Now.AddHours(2))
                {
                    return false;
                }

                var transactionsSeenIds = new List<int>();
                foreach (var transaction in currentBlock.Transactions)
                {
                    if (transaction.From != "COINBASE")
                    {
                        if (transactionsSeenIds.Contains(transaction.Id))
                        {
                            return false;
                        }
                        transactionsSeenIds.Add(transaction.Id);
                    }
                }
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
                if (PendingTransactions.Any(tx => tx.Id == transaction.Id))
                {
                    throw new InvalidOperationException($"Transaction with ID {transaction.Id} is already in the mempool.");
                }
                if (Chain.Any(b => b.Transactions.Any(t => t.Id == transaction.Id)))
                {
                    throw new InvalidOperationException($"Transaction with ID {transaction.Id} is already confirmed in the blockchain.");
                }
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
            SaveMemPool();
        }

        public decimal GetBalance(string address)
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

        private void SaveMemPool()
        {
            string memPoolFilePath = "mempool.json";
            var json = System.Text.Json.JsonSerializer.Serialize(PendingTransactions);
            System.IO.File.WriteAllText(memPoolFilePath, json);
        }

        public int GetTransactionConfirmations(int transactionId)
        {
            int confirmations = 0;
            foreach (var block in Chain)
            {
                if (block.Transactions.Any(t => t.Id == transactionId))
                {
                    confirmations = Chain.Count - block.Index;
                    break;
                }
            }
            return confirmations;
        }

        public decimal GetSafeBalance(string address, int requiredConfirmations = 3)
        {
            decimal balance = 0;

            foreach (var block in Chain)
            {
                if ((Chain.Count - block.Index) >= requiredConfirmations)
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
            }

            return balance;
        }

        public bool TryAddBlockFromPeer(Block block)
        {
            var lastBlock = Chain.Last();

            if (block.PreviousHash != lastBlock.Hash)
            {
                return false;
            }

            if (block.Hash != _hashingService.ComputeHash(block))
            {
                return false;
            }

            if (!block.Hash.StartsWith(new string('0', block.Difficulty)))
            {
                return false;
            }

            if (block.Timestamp <= lastBlock.Timestamp)
            {
                return false;
            }

            if (block.Timestamp > DateTime.Now.AddHours(2))
            {
                return false;
            }

            foreach (var transaction in block.Transactions)
            {
                if (!_transactionService.ValidateTransaction(transaction).isValid)
                {
                    return false;
                }
            }

            Chain.Add(block);

            foreach (var transaction in block.Transactions)
            {
                PendingTransactions.Remove(transaction);
            }

            _fileService.SaveChainToFile(Chain);

            return true;
        }

        public bool ResolveConflicts(List<Block> peerChain)
        {
            if (peerChain.Count > Chain.Count && IsChainValid(peerChain) && peerChain.Sum(b => b.Difficulty) > Chain.Sum(b => b.Difficulty))
            {
                int forkPoint = 0;
                for (int i = 0; i < Chain.Count; i++)
                {
                    if (Chain[i].Hash != peerChain[i].Hash)
                    {
                        forkPoint = i - 1;
                        break;
                    }
                }

                int depth = Chain.Count - forkPoint;
                if (depth > MaxReorgDepth)
                {
                    Console.WriteLine($"[Firewall WARNING] Reorganization depth {depth} exceeds maximum allowed {MaxReorgDepth}. Rejecting new chain.");
                    return false;
                }

                var chainTransactions = new List<Transaction>();
                for (int i = forkPoint + 1; i < Chain.Count; i++)
                {
                    chainTransactions.AddRange(Chain[i].Transactions);
                }

                var peerTransactionIds = peerChain.Skip(forkPoint + 1).SelectMany(b => b.Transactions.Select(t => t.Id)).ToList();
                var savedTransactions = chainTransactions.Where(tx => tx.From != "COINBASE" && !peerTransactionIds.Contains(tx.Id)).ToList();

                foreach (var tx in savedTransactions)
                {
                    PendingTransactions.Add(tx);
                }

                Chain = peerChain;
                _fileService.SaveChainToFile(Chain);
                SaveMemPool();
                return true;
            }
            return false;
        }
    }
}

