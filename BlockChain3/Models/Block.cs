using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Models
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string Data { get; set; } = string.Empty;
        public string Author { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public long Nonce { get; set; }

        public Block(int index, string data, string author, string previousHash)
        {
            Index = index;
            Timestamp = DateTime.Now;
            Data = data;
            Author = author;
            PreviousHash = previousHash;
            Hash = "";
        }
    }
}
