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

        public BlockChainService()
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            CreateGenesisBlock();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, "", "", "0");
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock);
            Chain.Add(genesisBlock);
        }
        public void AddBlock(string data, string author)
        {
            var previousBlock = Chain.Last();
            var newBlock = new Block(previousBlock.Index + 1, data, author, previousBlock.Hash);
            newBlock.Hash = _hashingService.ComputeHash(newBlock);
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
