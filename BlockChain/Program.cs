using BlockChain.Service;
using BlockChain.Models;

Console.WriteLine("Введіть порт для P2P мережі для власного вузла:");
int myport = int.Parse(Console.ReadLine());
Console.WriteLine("Введіть порт для сусіднього вузла:");
int nodeport = int.Parse(Console.ReadLine());

var blockChain = new BlockChainService();
var displayService = new BlockChainDisplayService();
var transactionService = new TransactionService();
var p2pNetworkService = new P2PNetworkService(myport, blockChain, new List<PeerInfo> { new PeerInfo("localhost", nodeport) });

var AliceWallet = new WalletService().GetOrCreateWallet("Alice");
var BobWallet = new WalletService().GetOrCreateWallet("Bob");

while (true)
{
    Console.WriteLine("BlockChain Menu:");
    Console.WriteLine("0. Connect");
    Console.WriteLine("1. Add Transaction");
    Console.WriteLine("2. Mine Pending Transactions");
    Console.WriteLine("3. Display BlockChain");
    Console.WriteLine("4. Show Alice's Balance");
    Console.WriteLine("5. Show Bob's Balance");
    Console.WriteLine("6. Show Transaction Confirmations");
    Console.WriteLine("7. Exit");
    Console.Write("Choose an option: ");

    var choice = Console.ReadLine();
    switch (choice)
    {
        case "0":
            p2pNetworkService.Start();
            Console.WriteLine("P2P Network Service started.");
            break;
        case "1":
            Console.WriteLine("Enter Fee:");
            var feeInput = Console.ReadLine();
            blockChain.AddTransaction(transactionService.CreateTransaction(BobWallet, AliceWallet.Address, 10, decimal.Parse(feeInput)));
            break;
        case "2":
            var block = blockChain.MinePendingTransactions(BobWallet.Address);
            p2pNetworkService.BroadcastBlockAsync(block);
            break;
        case "3":
            displayService.PrintChain(blockChain.Chain);
            displayService.PrintChainValidity(blockChain.IsChainValid());
            break;
        case "4":
            Console.WriteLine($"Alice's Balance: {blockChain.GetPendingBalance(AliceWallet.Address)}");
            break;
        case "5":
            Console.WriteLine($"Bob's Balance: {blockChain.GetPendingBalance(BobWallet.Address)}");
            break;
        case "6":
            Console.WriteLine("Enter Transaction ID:");
            var transactionId = int.Parse(Console.ReadLine());
            Console.WriteLine($"Transaction Confirmations: {blockChain.GetTransactionConfirmations(transactionId)}");
            break;
        case "7":
            return;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }
}