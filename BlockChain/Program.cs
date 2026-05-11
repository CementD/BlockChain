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
            Console.WriteLine("Adding blocks to the blockchain...");
            blockChain.AddBlock("Alice -> Bob: 5 BTC", "Alice");
            blockChain.AddBlock("Bob -> Charlie: 2 BTC", "Bob");
            blockChain.AddBlock("Charlie -> Dave: 1 BTC", "Charlie");

            displayService.PrintChain(blockChain.Chain);
            Console.WriteLine("Is the blockchain valid? " + blockChain.IsChainValid());

            Console.WriteLine("Changing data in block[2]: ");
            blockChain.Chain[2].Data = "Bob -> Charlie: 100 BTC";
            displayService.PrintChain(blockChain.Chain);
            Console.WriteLine("Is the blockchain valid? " + blockChain.IsChainValid());

            var index = blockChain.GetCorruptedBlockIndex();
            Console.WriteLine(index != -1 ? $"Corrupted block found at index: {index}" : "No corrupted blocks found.");

            var StopWatch = Stopwatch.StartNew();
            Console.WriteLine("Recomputing hashes for all blocks...");
            if (index != -1)
            {
                blockChain.HackTheChain(index);
            }
            StopWatch.Stop();
            Console.WriteLine($"Time taken to hack the chain: {StopWatch.ElapsedMilliseconds} ms");
            displayService.PrintChain(blockChain.Chain);
            Console.WriteLine("Is the blockchain valid? " + blockChain.IsChainValid());

            //HW
            Console.WriteLine("----------------HOMEWORK----------------- ");
            Console.WriteLine("Changing data in block[2]: ");
            blockChain.Chain[2].Author = "Dave";
            displayService.PrintChain(blockChain.Chain);
            Console.WriteLine("Is the blockchain valid? " + blockChain.IsChainValid());

            index = blockChain.GetCorruptedBlockIndex();
            Console.WriteLine(index != -1 ? $"Corrupted block found at index: {index}" : "No corrupted blocks found.");

            StopWatch = Stopwatch.StartNew();
            Console.WriteLine("Recomputing hashes for all blocks...");
            if (index != -1)
            {
                blockChain.HackTheChain(index);
            }
            StopWatch.Stop();
            Console.WriteLine($"Time taken to hack the chain: {StopWatch.ElapsedMilliseconds} ms");
            displayService.PrintChain(blockChain.Chain);
            Console.WriteLine("Is the blockchain valid? " + blockChain.IsChainValid());
        }
    }
}
