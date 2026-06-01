using BlockChain.Service;
using System.Net.WebSockets;
using System.Diagnostics;
using BlockChain.Models;

namespace BlockChain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var blockChain = new BlockChainService();
            var transactionService = new TransactionService();
            var displayService = new BlockChainDisplayService();

            var AliceWallet = new WalletService().GetOrCreateWallet("Alice");
            var BobWallet = new WalletService().GetOrCreateWallet("Bob");

            displayService.PrintTransactions(blockChain.PendingTransactions);

            Console.WriteLine($"Початковий звичайний баланс Боба: {blockChain.GetBalance(BobWallet.Address)}");
            Console.WriteLine($"Початковий безпечний баланс Alice: {blockChain.GetBalance(AliceWallet.Address)}");

            Console.WriteLine("\n--> Alice sends 10 coins to Bob (Fee: 2)");
            var tx = transactionService.CreateTransaction(AliceWallet, BobWallet.Address, 10, 2);
            blockChain.AddTransaction(tx);

            // 1-й майнінг (Транзакція сідає в Блок №1)
            blockChain.MinePendingTransactions(AliceWallet.Address);
            Console.WriteLine("\n[1st mining / confirmation]");
            PrintStatus(blockChain, AliceWallet.Address, BobWallet.Address);

            // 2-й майнінг (Створюється Блок №2)
            blockChain.MinePendingTransactions(AliceWallet.Address);
            Console.WriteLine("\n[2nd mining / confirmation]");
            PrintStatus(blockChain, AliceWallet.Address, BobWallet.Address);

            // 3-й майнінг (Створюється Блок №3)
            blockChain.MinePendingTransactions(AliceWallet.Address);
            Console.WriteLine("\n[3rd mining / confirmation]");
            PrintStatus(blockChain, AliceWallet.Address, BobWallet.Address);
        }

        static void PrintStatus(BlockChainService bc, string alice, string bob)
        {
            // Явно передаємо 3 підтвердження у GetSafeBalance
            Console.WriteLine($"Alice Balance: {bc.GetBalance(alice)}, Safe balance: {bc.GetSafeBalance(alice, 3)}");
            Console.WriteLine($"Bob Balance: {bc.GetBalance(bob)}, Safe balance: {bc.GetSafeBalance(bob, 3)}");
        }

    }
}
