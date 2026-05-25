using Lab5.Models;
using Lab5.Service;
using System.Diagnostics;
using System.Net.WebSockets;

namespace BlockChain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var blockChain = new BlockChainService();
            var displayService = new BlockChainDisplayService();
            var transactionService = new TransactionService();

            var AliceWallet = new WalletService().CreateWallet("Alice");
            var BobWallet = new WalletService().CreateWallet("Bob");
            var CharlieWallet = new WalletService().CreateWallet("Charlie");

            var tx1 = new TransactionService().CreateTransaction(AliceWallet, BobWallet.Address, 10, "For Pizza");
            var tx2 = new TransactionService().CreateTransaction(BobWallet, AliceWallet.Address, 5, "For Donating in BS");
            var tx3 = new TransactionService().CreateTransaction(CharlieWallet, AliceWallet.Address, 2, "For Tun tun tun Sahur");

            blockChain.AddBlock(new List<Transaction> { tx1, tx2, tx3 });
            displayService.PrintChain(blockChain.Chain);
            displayService.PrintChainValidity(blockChain.IsChainValid());

            var fakeBlock = new List<Transaction> { tx1 };
            blockChain.AddBlock(fakeBlock);
        }
    }
}
