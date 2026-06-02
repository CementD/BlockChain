using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Models
{
    public class P2PMessage
    {
        public string Type { get; set; }
        public string Data { get; set; }
        public P2PMessage(string type, string data)
        {
            Type = type;
            Data = data;
        }
    }
}
