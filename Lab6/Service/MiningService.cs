using Lab6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6.Service
{
    public class MiningService
    {
        private readonly HashingService _hashingService;

        public MiningService()
        {
            _hashingService = new HashingService();
        }

        //public void MineBlock(Block block, int difficulty)
        //{
        //    var target = new string('0', difficulty);
        //    int threadCount = Environment.ProcessorCount;

        //    long finalNonce = 0;
        //    string finalHash = string.Empty;
        //    bool found = false;
        //    object lockObj = new object();

        //    using (var cts = new CancellationTokenSource())
        //    {
        //        var tasks = new List<Task>();
        //        for (int i = 0; i < threadCount; i++)
        //        {
        //            int threadIndex = i;
        //            tasks.Add(Task.Run(() =>
        //            {
        //                var localBlock = new Block(block.Index, block.Data, block.Author, block.PreviousHash)
        //                {
        //                    Timestamp = block.Timestamp,
        //                    Difficulty = block.Difficulty,
        //                    Nonce = threadIndex
        //                };


        //                while (!found && !cts.Token.IsCancellationRequested)
        //                {
        //                    if (threadIndex == 0 && localBlock.Nonce % 1000000 == 0)
        //                        Console.Write(".");

        //                    var hash = _hashingService.ComputeHash(localBlock);

        //                    if (hash.StartsWith(target))
        //                    {
        //                        lock (lockObj)
        //                        {
        //                            if (!found)
        //                            {
        //                                found = true;
        //                                finalNonce = localBlock.Nonce;
        //                                finalHash = hash;
        //                                cts.Cancel();
        //                            }
        //                        }
        //                        break;
        //                    }

        //                    localBlock.Nonce += threadCount;
        //                }
        //            }));
        //        }
        //        Task.WaitAll(tasks.ToArray());
        //    }

        //    block.Nonce = finalNonce;
        //    block.Hash = finalHash;
        //}

        public void MineBlock(Block block, int difficulty)
        {
            var targetPrefix = new string('0', difficulty);
            var threadCount = Environment.ProcessorCount;
            var cancellationToken = new CancellationTokenSource();
            var lockObj = new object();
            Parallel.For(0, threadCount, (i, state) =>
            {
                long nonce = i;
                var localBlock = new Block(block.Index, block.Transactions, block.PreviousHash)
                {
                    Timestamp = block.Timestamp,
                    Difficulty = block.Difficulty,
                    Nonce = i
                };
                while (!cancellationToken.IsCancellationRequested)
                {
                    string hash = _hashingService.ComputeHash(localBlock);
                    if (hash.StartsWith(targetPrefix))
                    {
                        lock (lockObj)
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                block.Nonce = localBlock.Nonce;
                                block.Hash = hash;
                                cancellationToken.Cancel();
                            }
                        }
                    }
                    localBlock.Nonce += threadCount;
                }
            });
        }
    }
}
