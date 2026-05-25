using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Service
{
    public class MiningService
    {
        private readonly HashingService _hashingService;

        public MiningService()
        {
            _hashingService = new HashingService();
        }

        public void MineBlock(Block block, int difficulty)
        {
            var target = new string('0', difficulty);
            while (true)
            {
                if (block.Nonce % 1000000 == 0)
                    Console.Write(".");

                block.Hash = _hashingService.ComputeHash(block);
                if (block.Hash.StartsWith(target))
                    break;
                block.Nonce++;
            }
        }
    }
}
