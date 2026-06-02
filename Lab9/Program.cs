using BlockChain.Service;

namespace Lab9
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var blockChainService = new BlockChainService();
            var walletService = new WalletService();
            var displayService = new BlockChainDisplayService();
            var transactionService = new TransactionService();

            var AliceWallet = walletService.CreateWallet("Alice");
            var BobWallet = walletService.CreateWallet("Bob");
            var CarolWallet = walletService.CreateWallet("Carol");

            Console.WriteLine("Mining block (1)");
            blockChainService.MinePendingTransactions(AliceWallet.Address);
            Console.WriteLine($"Alice balance: {blockChainService.GetPendingBalance(AliceWallet.Address)}");

            Console.WriteLine("Alice sends 10 coins to Bob");
            try
            {
                blockChainService.AddTransaction(transactionService.CreateTransaction(AliceWallet, BobWallet.Address, 10, 1));
                Console.WriteLine("Transaction added to pending transactions.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction failed: {ex.Message}");
            }

            Console.WriteLine("Mining block (2)");
            blockChainService.MinePendingTransactions(AliceWallet.Address);
            Console.WriteLine($"Alice balance: {blockChainService.GetPendingBalance(AliceWallet.Address)}");

            Console.WriteLine("Mining block (3)");
            blockChainService.MinePendingTransactions(AliceWallet.Address);
            Console.WriteLine($"Alice balance: {blockChainService.GetPendingBalance(AliceWallet.Address)}");

            Console.WriteLine("Alice sends 10 coins to Bob");
            try
            {
                blockChainService.AddTransaction(transactionService.CreateTransaction(AliceWallet, BobWallet.Address, 10, 1));
                Console.WriteLine("Transaction added to pending transactions.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction failed: {ex.Message}");
            }
        }
    }
}
