using BlockChain.Models;

namespace BlockChain.Service
{
    public class MiningService
    {
        private readonly HashingService _hashingService;

        private readonly Func<string, bool> _consensusRule;

        public MiningService(Func<string, bool> consensusRule)
        {
            _hashingService = new HashingService();
            _consensusRule = consensusRule;
        }

        public void MineBlock(Block block)
        {
            while (true)
            {
                if (block.Nonce % 1000000 == 0)
                    Console.Write(".");

                block.Hash = _hashingService.ComputeHash(block);

                if (_consensusRule(block.Hash))
                    break;

                block.Nonce++;
            }
        }
    }
}