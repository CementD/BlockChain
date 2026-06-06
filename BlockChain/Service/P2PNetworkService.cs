using BlockChain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChain.Service
{
    public class P2PNetworkService
    {
        private readonly int _port;
        private List<PeerInfo> _peers;
        private readonly BlockChainService _blockChainService;

        private readonly ConcurrentDictionary<string, int> _peerStrikes = new ConcurrentDictionary<string, int>();
        private const int MaxStrikes = 3;

        public P2PNetworkService(int port, BlockChainService blockChainService, List<PeerInfo> peerInfos)
        {
            _port = port;
            _blockChainService = blockChainService;
            _peers = peerInfos;
            _peerStrikes = new ConcurrentDictionary<string, int>();
        }

        public void Start()
        {
            Task.Run(StartServerAsync);
        }

        private async Task StartServerAsync()
        {
            var listener = new TcpListener(System.Net.IPAddress.Any, _port);
            listener.Start();
            Debug.WriteLine($"P2P Network Service started on port {_port}");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Debug.WriteLine("New peer connected");
                _ = Task.Run(() => HandlePeerAsync(client));
            }
        }

        private async Task HandlePeerAsync(TcpClient client)
        {
            string peerIp = "Unknown";
            if (client.Client.RemoteEndPoint is System.Net.IPEndPoint remoteEndPoint)
            {
                peerIp = remoteEndPoint.Address.ToString();
            }

            if (_peerStrikes.TryGetValue(peerIp, out int strikes) && strikes >= MaxStrikes)
            {
                Console.WriteLine($"[Firewall] Connection blocked from malicious peer: {peerIp}");
                client.Close();
                return;
            }

            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);

                var json = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(json))
                {
                    int currentStrikes = _peerStrikes.AddOrUpdate(peerIp, 1, (key, oldValue) => oldValue + 1);
                    Console.WriteLine($"[Firewall WARNING] Received empty packet from {peerIp}. Strike: {currentStrikes}/{MaxStrikes}");
                    return;
                }

                P2PMessage message = null;
                try
                {
                    message = JsonSerializer.Deserialize<P2PMessage>(json);
                    if (message == null)
                        throw new JsonException();
                }
                catch (JsonException)
                {
                    int currentStrikes = _peerStrikes.AddOrUpdate(peerIp, 1, (key, oldValue) => oldValue + 1);
                    Console.WriteLine($"[Firewall WARNING] Invalid JSON format from {peerIp}. Strike: {currentStrikes}/{MaxStrikes}");
                    return;
                }

                await CommentExecutor(message, peerIp);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling peer {peerIp}: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private async Task CommentExecutor(P2PMessage message, string peerIp)
        {
            if (message.Type == "NEW_BLOCK")
            {
                try
                {
                    var block = JsonSerializer.Deserialize<Block>(message.Data);
                    if (block != null)
                    {
                        bool isBlockValid = _blockChainService.TryAddBlockFromPeer(block);
                        if (!isBlockValid)
                        {
                            int currentStrikes = _peerStrikes.AddOrUpdate(peerIp, 2, (key, oldValue) => oldValue + 2);
                            Console.WriteLine($"[Firewall WARNING] Cryptographic attack detected! Invalid block from {peerIp}. Strike: {currentStrikes}/{MaxStrikes}");
                        }
                        else
                        {
                            Console.WriteLine($"[P2P] Successfully accepted and added new block #{block.Index} from {peerIp}");
                        }
                    }
                }
                catch (JsonException)
                {
                    int currentStrikes = _peerStrikes.AddOrUpdate(peerIp, 1, (key, oldValue) => oldValue + 1);
                    Console.WriteLine($"[Firewall WARNING] Corrupted block data from {peerIp}. Strike: {currentStrikes}/{MaxStrikes}");
                }
            }
            if (message.Type == "REQUEST_CHAIN")
            {
                var chain = _blockChainService.Chain;
                var responseMessage = new P2PMessage("CHAIN_RESPONSE", JsonSerializer.Serialize(chain));
                var json = JsonSerializer.Serialize(responseMessage);
                foreach (var peer in _peers)
                {
                    await SendMessageAsync(peer, json);
                }
            }

            if (message.Type == "CHAIN_RESPONSE")
            {
                var chain = JsonSerializer.Deserialize<List<Block>>(message.Data);
                if (chain != null)
                {
                    var res = _blockChainService.ResolveConflicts(chain);
                    if (res)
                    {
                        Debug.WriteLine("Chain replaced with received chain");
                    }
                }
            }
            if (message.Type != "NEW_BLOCK" && message.Type != "REQUEST_CHAIN" && message.Type != "CHAIN_RESPONSE")
            {
                int currentStrikes = _peerStrikes.AddOrUpdate(peerIp, 1, (key, oldValue) => oldValue + 1);
                Console.WriteLine($"[Firewall WARNING] Unknown protocol message '{message.Type}' from {peerIp}. Strike: {currentStrikes}/{MaxStrikes}");
            }
        }

        public async Task BroadcastBlockAsync(Block block)
        {
            var message = new P2PMessage("NEW_BLOCK", JsonSerializer.Serialize(block));
            await BroadCastMessageAsync(message);
        }

        public async Task BroadCastMessageAsync(P2PMessage p2PMessage)
        {
            var json = JsonSerializer.Serialize(p2PMessage);
            foreach (var peer in _peers)
            {
                await SendMessageAsync(peer, json);
            }
        }

        private async Task SendMessageAsync(PeerInfo peer, string message)
        {
            try
            {
                using var client = new TcpClient(peer.Host, peer.Port);
                using var stream = client.GetStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                await writer.WriteLineAsync(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send message to {peer.Host}:{peer.Port} - {ex.Message}");
            }
        }

        public ConcurrentDictionary<string, int> GetBlacklist()
        {
            return _peerStrikes;
        }
    }
}