using BlockChain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            string peerIp = client.Client.RemoteEndPoint.ToString();
            
            if (_peerStrikes.TryGetValue(peerIp, out int strikes) && strikes >= MaxStrikes)
            {
                Debug.WriteLine($"Peer {peerIp} is banned due to too many strikes.");
                client.Close();
                return;
            }

            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);

                var json = await reader.ReadLineAsync();

                if (json == null) return;

                var message = JsonSerializer.Deserialize<P2PMessage>(json);

                 await CommentExecutor(message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task CommentExecutor(P2PMessage message)
        {
            if (message.Type == "NEW_BLOCK")
            {
                var block = JsonSerializer.Deserialize<Block>(message.Data);
                if (block != null)
                {
                    _blockChainService.TryAddBlockFromPeer(block);
                    Debug.WriteLine($"Received new block from peer: {block.Index}");
                }
            }
        }

        public async Task BroadcastBlockAsync(Block block)
        {
            var message = new P2PMessage("NEW_BLOCK", JsonSerializer.Serialize(block));
            var json = JsonSerializer.Serialize(message);

            foreach (var peer in _peers)
            {
                SendMessageAsync(peer, json);
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
    }
}
