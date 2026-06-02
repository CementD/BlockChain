using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Models
{
    public class PeerInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public PeerInfo(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}
