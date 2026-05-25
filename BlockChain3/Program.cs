using BlockChain.Service;
using System.Net.WebSockets;
using System.Diagnostics;
using BlockChain.Models;

namespace BlockChain3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int difficulty = 2;
            string signature = "de";
            Func<string, bool> consensusRule = hash =>
            {
                string target = new string('0', difficulty) + signature;
                return hash.StartsWith(target);
            };
            var blockChain = new BlockChainService(consensusRule);
            var displayService = new BlockChainDisplayService();

            Console.WriteLine($"Word {signature} and difficulty {difficulty}");
            var stopwatch = Stopwatch.StartNew();
            blockChain.AddBlock("First block data", "Alice");
            stopwatch.Stop();
            Console.WriteLine($"Time taken to mine first block: {stopwatch.Elapsed.TotalSeconds} seconds");
            stopwatch.Restart();
            blockChain.AddBlock("Second block data", "Bob");
            stopwatch.Stop();
            Console.WriteLine($"Time taken to mine second block: {stopwatch.Elapsed.TotalSeconds} seconds");

            displayService.PrintChain(blockChain.Chain);
            Console.WriteLine("Is blockchain valid? " + blockChain.IsChainValid());
            
            foreach (var block in blockChain.Chain)
            {
                Console.WriteLine($"Block {block.Index} Hash: {block.Hash}");
            }

            Console.WriteLine("Atack:");
            List<Block> attackerChain = new List<Block>();
            attackerChain.Add(blockChain.Chain[0]);
            attackerChain.Add(blockChain.Chain[1]);

            var attackerBlockchain = new BlockChainService(consensusRule);
            attackerBlockchain.Chain = attackerChain;

            attackerBlockchain.AddBlock("ATTACKER STOLE 1000 BTC", "HACKER");
            attackerBlockchain.AddBlock("Fake transaction #3", "HACKER");
            attackerBlockchain.AddBlock("Fake transaction #4", "HACKER");
            attackerBlockchain.AddBlock("Fake transaction #5", "HACKER");

            Console.WriteLine("Attacker's blockchain:");
            displayService.PrintChain(attackerBlockchain.Chain);
            Console.WriteLine("Is attacker's blockchain valid? " + attackerBlockchain.IsChainValid());

            bool replaceChain = blockChain.ResolveConsensus(attackerBlockchain.Chain);
            Console.WriteLine("Did the honest chain get replaced? " + replaceChain);
            displayService.PrintChain(blockChain.Chain);
        }
    }
}
