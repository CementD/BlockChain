using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab8.Models
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public long Nonce { get; set; }
        public int Difficulty { get; set; }

        public string MerkleRoot { get; set; }

        public Block(int index, List<Transaction> transactions, string previousHash)
        {
            Index = index;
            Timestamp = DateTime.Now;
            Transactions = transactions;
            PreviousHash = previousHash;
            Hash = "";
        }
    }
}
