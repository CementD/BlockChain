using BlockChain.Service;
using BlockChain.Models;

Console.WriteLine("Введіть порт для P2P мережі для власного вузла:");
int myport = int.Parse(Console.ReadLine());
Console.WriteLine("Введіть порт для сусіднього вузла:");
int nodeport = int.Parse(Console.ReadLine());

var blockChain = new BlockChainService(new FileService(myport));
var displayService = new BlockChainDisplayService();
var transactionService = new TransactionService();
var p2pNetworkService = new P2PNetworkService(myport, blockChain, new List<PeerInfo> { new PeerInfo("localhost", nodeport) });

var AliceWallet = new WalletService().GetOrCreateWallet("Alice");
var BobWallet = new WalletService().GetOrCreateWallet("Bob");

var tx2 = transactionService.CreateTransaction(BobWallet, AliceWallet.Address, 3, 0.05m);

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
    Console.WriteLine("7. Show Firewall Blacklist");
    Console.WriteLine("8. Exit");
    Console.WriteLine("9. Create tx1 (for 5001)");
    Console.WriteLine("10. Create tx2 (for 5002)");
    Console.WriteLine("11. Add tx2 to pending transaction");
    Console.Write("Choose an option: ");

    var choice = Console.ReadLine();
    switch (choice)
    {
        case "0":
            p2pNetworkService.Start();
            Console.WriteLine("P2P Network Service started.");

            var getChainMesage = new P2PMessage("REQUEST_CHAIN", "");
            await p2pNetworkService.BroadCastMessageAsync(getChainMesage);
            break;
        case "1":
            Console.WriteLine("Enter Fee:");
            var feeInput = Console.ReadLine();
            blockChain.AddTransaction(transactionService.CreateTransaction(BobWallet, AliceWallet.Address, 10, decimal.Parse(feeInput)));
            break;
        case "2":
            var block = blockChain.MinePendingTransactions(BobWallet.Address);
            await p2pNetworkService.BroadcastBlockAsync(block);
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
            var blacklist = p2pNetworkService.GetBlacklist();

            if (blacklist.IsEmpty)
            {
                Console.WriteLine("Peer strikes list is empty.");
            }
            else
            {
                Console.WriteLine($"{"IP-Address",-20} | {"Peer strikes",-15} | {"Status",-15}");
                Console.WriteLine(new string('-', 55));

                foreach (var strike in blacklist)
                {
                    string status = strike.Value >= 3 ? "Banned" : "";
                    Console.WriteLine($"{strike.Key,-20} | {strike.Value + " / 3",-15} | {status,-15}");
                }
            }
            break;
        case "8":
            return;
        case "9":
            var tx1 = transactionService.CreateTransaction(AliceWallet, BobWallet.Address, 5, 0.1m);
            blockChain.AddTransaction(tx1);
            Console.WriteLine($"Created Transaction: {tx1.Id} from Alice to Bob for 5 coins with fee 0.1");
            break;
        case "10":
            blockChain.AddTransaction(tx2);
            Console.WriteLine($"Created Transaction: {tx2.Id} from Bob to Alice for 3 coins with fee 0.05");
            break;
        case "11":
            try
            {
                blockChain.AddTransaction(tx2);
                Console.WriteLine($"Added Transaction: {tx2.Id} to pending transactions.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding transaction: {ex.Message}");
            }
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }
}