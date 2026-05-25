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
        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly Func<string, bool> _consensusRule;
        public BlockChainService(Func<string, bool> consensusRule)
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            _consensusRule = consensusRule;
            _miningService = new MiningService(_consensusRule);
            CreateGenesisBlock();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, "", "", "0");

            _miningService.MineBlock(genesisBlock);

            Chain.Add(genesisBlock);
        }
        public void AddBlock(string data, string author)
        {
            var previousBlock = Chain.Last();
            var newBlock = new Block(previousBlock.Index + 1, data, author, previousBlock.Hash);
            _miningService.MineBlock(newBlock);
            Chain.Add(newBlock);
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
                if (!_consensusRule(currentBlock.Hash))
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

        public bool IsChainValid(List<Block> chain)
        {
            for (int i = 1; i < chain.Count; i++)
            {
                var currentBlock = chain[i];
                var previousBlock = chain[i - 1];
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return false;
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return false;
                if (!_consensusRule(currentBlock.Hash))
                    return false;
            }
            return true;
        }

        public bool ResolveConsensus(List<Block> competingChain)
        {
            if (competingChain.Count > Chain.Count && IsChainValid(competingChain))
            {
                Chain = competingChain;
                return true;
            }
            return false;
        }
    }
}
