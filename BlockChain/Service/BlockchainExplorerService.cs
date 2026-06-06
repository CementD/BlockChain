using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Service
{
    internal class BlockchainExplorerService
    {
        public readonly BlockChainService _blockChainService;
        public BlockchainExplorerService(BlockChainService blockChainService)
        {
            _blockChainService = blockChainService;
        }

        public Transaction FindTransactionById(string txId) => _blockChainService.Chain.SelectMany(b => b.Transactions).FirstOrDefault(t => t.Id.ToString() == txId);

        public Block FindBlockByTransactionId(string txId) => _blockChainService.Chain.FirstOrDefault(b => b.Transactions.Any(t => t.Id.ToString() == txId));

        public List<Transaction> GetTransactionHistory(string address) => _blockChainService.Chain.SelectMany(b => b.Transactions)
            .Where(t => t.From == address || t.To == address)
            .OrderByDescending(t => t.TimeStamp)
            .ToList();

        public decimal GetTotalFeesEarned(string minerAddress) => _blockChainService.Chain.SelectMany(b => b.Transactions)
            .Where(t => t.From == "COINBASE" && t.To == minerAddress)
            .Sum(t => t.Amount - _blockChainService.MiningReward);

        public List<string> GetOwnedNFTs(string address) => _blockChainService.Chain.SelectMany(block => block.Transactions)
            .Where(tx => !string.IsNullOrEmpty(tx.NftDataUrl))
            .GroupBy(tx => tx.NftDataUrl)
            .Where(group => group.Count(tx => tx.To == address) > group.Count(tx => tx.From == address))
            .Select(group => group.Key)
            .ToList();
    }
}
