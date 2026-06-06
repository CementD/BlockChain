using BlockChain.Service;
using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    private static BlockChainService blockChain;
    private static BlockchainExplorerService explorerService;
    private static WalletKeystoreService keystore = new WalletKeystoreService();
    private static TransactionService transactionService = new TransactionService();
    private static BlockChainDisplayService displayService = new BlockChainDisplayService();
    private static P2PNetworkService p2pNetworkService;
    private static WalletService walletService = new WalletService();

    private static Wallet currentWallet = null;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Enter the P2P network port for your own node:");
        int myport = int.Parse(Console.ReadLine());
        Console.WriteLine("Enter the P2P network port for the neighbor node:");
        int nodeport = int.Parse(Console.ReadLine());

        blockChain = new BlockChainService(new FileService(myport));
        explorerService = new BlockchainExplorerService(blockChain);
        p2pNetworkService = new P2PNetworkService(myport, blockChain, new List<PeerInfo> { new PeerInfo("localhost", nodeport) });

        StartWalletMenu();

        while (true)
        {
            Console.WriteLine($"\nBlockChain Menu (Current Wallet: {currentWallet.Name})");
            Console.WriteLine("0. Connect to P2P Network");
            Console.WriteLine("1. Add Transaction");
            Console.WriteLine("2. Mine Pending Transactions");
            Console.WriteLine("3. Display BlockChain");
            Console.WriteLine("4. Show all balances");
            Console.WriteLine("5. Wallet History");
            Console.WriteLine("6. Find block by transaction id");
            Console.WriteLine("7. Make your own token (Mint)");
            Console.WriteLine("8. Mint New NFT (MINT_NFT)");
            Console.WriteLine("9. View My NFTs");
            Console.WriteLine("e. Exit");
            Console.Write("Choose an option: ");

            var choice = Console.ReadLine().ToLower();
            switch (choice)
            {
                case "0":
                    p2pNetworkService.Start();
                    Console.WriteLine("P2P Network Service started.");
                    var getChainMessage = new P2PMessage("REQUEST_CHAIN", "");
                    await p2pNetworkService.BroadCastMessageAsync(getChainMessage);
                    break;

                case "1":
                    Console.Write("Enter Recipient Address: ");
                    string toAddress = Console.ReadLine();
                    Console.Write("Enter Token Symbol: ");
                    string tokenSymbol = Console.ReadLine().ToUpper();
                    Console.Write("Enter Amount: ");
                    decimal amount = decimal.Parse(Console.ReadLine());
                    Console.Write("Enter Fee (in MAIN coins): ");
                    decimal fee = decimal.Parse(Console.ReadLine());

                    try
                    {
                        var tx = transactionService.CreateTransaction(currentWallet, toAddress, amount, fee, tokenSymbol);
                        blockChain.AddTransaction(tx);
                        Console.WriteLine("Transaction added to memory pool successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding transaction: {ex.Message}");
                    }
                    break;

                case "2":
                    var block = blockChain.MinePendingTransactions(currentWallet.Address);
                    await p2pNetworkService.BroadcastBlockAsync(block);
                    Console.WriteLine("Block mined and broadcasted successfully.");
                    break;

                case "3":
                    displayService.PrintChain(blockChain.Chain);
                    displayService.PrintChainValidity(blockChain.IsChainValid());
                    break;

                case "4":
                    ShowAllBalances();
                    break;

                case "5":
                    ShowWalletHistory();
                    break;

                case "6":
                    FindBlockByTxId();
                    break;

                case "7":
                    MintNewToken();
                    break;

                case "8":
                    MintNewNft();
                    break;
                case "9":
                    ViewMyNfts();
                    break;
                case "e":
                    Console.WriteLine("Exiting program...");
                    return;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static void StartWalletMenu()
    {
        while (currentWallet == null)
        {
            Console.WriteLine("WALLET MANAGEMENT");
            Console.WriteLine("1. Create a new wallet");
            Console.WriteLine("2. Load an existing wallet");
            Console.Write("Your choice: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Enter a name for the new wallet: ");
                string name = Console.ReadLine();
                Console.Write("Create a password: ");
                string password = Console.ReadLine();

                currentWallet = walletService.CreateWallet(name);

                keystore.SaveWallet(currentWallet, password);

                Console.WriteLine($"Wallet '{name}' with address [{currentWallet.Address}] successfully created!");
            }
            else if (choice == "2")
            {
                Console.Write("Enter the wallet name to load: ");
                string name = Console.ReadLine();
                Console.Write("Enter the password: ");
                string password = Console.ReadLine();

                try
                {
                    currentWallet = keystore.LoadWallet(name, password);
                    Console.WriteLine($"Wallet '{name}' successfully loaded! Address: {currentWallet.Address}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Load error: {ex.Message}");
                }
            }
        }
    }

    private static void ShowAllBalances()
    {
        string myAddress = currentWallet.Address;
        Console.WriteLine($"Balances for Address: {myAddress}");

        var allTokens = new HashSet<string> { "MAIN" };
        foreach (var b in blockChain.Chain)
        {
            foreach (var t in b.Transactions)
            {
                if (!string.IsNullOrEmpty(t.TokenSymbol)) allTokens.Add(t.TokenSymbol);
            }
        }
        foreach (var t in blockChain.PendingTransactions)
        {
            if (!string.IsNullOrEmpty(t.TokenSymbol)) allTokens.Add(t.TokenSymbol);
        }

        foreach (var token in allTokens)
        {
            decimal confirmed = blockChain.GetBalance(myAddress, token);
            decimal pending = blockChain.GetPendingBalance(myAddress, token);
            Console.WriteLine($"{token}: {confirmed} (Pending in Mempool: {pending})");
        }

        decimal feesEarned = explorerService.GetTotalFeesEarned(myAddress);
        Console.WriteLine($"Total Miner Fees Earned: {feesEarned} MAIN");
    }

    private static void ShowWalletHistory()
    {
        string myAddress = currentWallet.Address;
        var history = explorerService.GetTransactionHistory(myAddress);

        Console.WriteLine($"\nTransaction History for Wallet: {currentWallet.Name}");
        if (!history.Any())
        {
            Console.WriteLine("No transactions found.");
            return;
        }

        foreach (var tx in history)
        {
            string direction = tx.From == myAddress ? "SENT" : "RECEIVED";
            if (tx.From == "MINT") direction = "EMISSION (MINT)";
            if (tx.From == "COINBASE") direction = "MINING REWARD (COINBASE)";

            Console.WriteLine($"[{direction}] From: {tx.From} | To: {tx.To} | Amount: {tx.Amount} {tx.TokenSymbol} | Fee: {tx.Fee} MAIN");
        }
    }

    private static void FindBlockByTxId()
    {
        Console.Write("Enter Transaction ID: ");
        string txId = Console.ReadLine();

        var block = explorerService.FindBlockByTransactionId(txId);
        if (block != null)
        {
            Console.WriteLine($"Transaction found in Block #{block.Index}");
            Console.WriteLine($"Block Hash: {block.Hash}");
            Console.WriteLine($"Previous Hash: {block.PreviousHash}");
        }
        else
        {
            Console.WriteLine("Block containing this transaction was not found in the blockchain.");
        }
    }

    private static void MintNewToken()
    {
        Console.Write("Enter the name of your new token (e.g., MY_COIN): ");
        string tokenSymbol = Console.ReadLine().ToUpper();
        Console.Write("Enter the token supply amount to mint: ");
        decimal amount = decimal.Parse(Console.ReadLine());

        string myAddress = currentWallet.Address;

        var mintTx = new Transaction("MINT", myAddress, amount, 0, tokenSymbol);

        mintTx.SenderPublicKey = currentWallet.PublicKey;

        using (var ecdsa = System.Security.Cryptography.ECDsa.Create())
        {
            ecdsa.ImportECPrivateKey(currentWallet.PrivateKey, out _);
            mintTx.Signature = ecdsa.SignData(mintTx.GetDataToSign(), System.Security.Cryptography.HashAlgorithmName.SHA256);
        }

        try
        {
            blockChain.AddTransaction(mintTx);
            Console.WriteLine($"\nMint request for {amount} {tokenSymbol} successfully added to the mempool!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nMint error: {ex.Message}");
        }
    }

    private static void MintNewNft()
    {
        Console.Write("Enter NFT Data URL: ");
        string nftUrl = Console.ReadLine();

        decimal amount = 1;
        string myAddress = currentWallet.Address;

        var nftTx = new Transaction("MINT_NFT", myAddress, amount, 0, "NFT", nftUrl);

        nftTx.SenderPublicKey = currentWallet.PublicKey;

        using (var ecdsa = System.Security.Cryptography.ECDsa.Create())
        {
            ecdsa.ImportECPrivateKey(currentWallet.PrivateKey, out _);
            nftTx.Signature = ecdsa.SignData(nftTx.GetDataToSign(), System.Security.Cryptography.HashAlgorithmName.SHA256);
        }

        try
        {
            blockChain.AddTransaction(nftTx);
            Console.WriteLine($"\nNFT Mint request successfully added to mempool!");
            Console.WriteLine($"Asset Link: {nftUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nNFT Mint error: {ex.Message}");
        }
    }

    private static void ViewMyNfts()
    {
        string myAddress = currentWallet.Address;
        Console.WriteLine($"\nNFT Collection for Address: {myAddress}");

        var myNfts = explorerService.GetOwnedNFTs(myAddress);

        if (!myNfts.Any())
        {
            Console.WriteLine("You do not own any NFTs yet.");
            return;
        }

        int counter = 1;
        foreach (var nftUrl in myNfts)
        {
            Console.WriteLine($"{counter}. [NFT Token] -> {nftUrl}");
            counter++;
        }
    }
}