using BlockChain.Service;
using System.Net.WebSockets;
using System.Diagnostics;

namespace BlockChain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var blockChain = new BlockChainService();
            var displayService = new BlockChainDisplayService();
            
            blockChain.AddBlock("First block data", "Alice");
            blockChain.AddBlock("Second block data", "Bob");

            displayService.PrintChain(blockChain.Chain);
            Console.WriteLine("Is blockchain valid? " + blockChain.IsChainValid());

            foreach (var difficulty in new[] { 1, 2, 3, 4, 5 })
            {
                Console.WriteLine($"Testing difficulty {difficulty}");
                var sw = Stopwatch.StartNew();
                var testBlock = new BlockChainService();
                testBlock.Difficulty = difficulty;

                testBlock.AddBlock("Test block data", "Tester");

                sw.Stop();
                Console.WriteLine($"Time taken to mine block with difficulty {difficulty}: {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}
