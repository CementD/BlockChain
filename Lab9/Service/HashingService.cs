using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Service
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            block.MerkleRoot = CalculateMerkleRoot(block.Transactions);
            var input = $"{block.Index}{block.Timestamp}{block.MerkleRoot}{block.PreviousHash}{block.Nonce}";
            return ComputeSha256(input);
        }

        public string ComputeSha256(string input)
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(input);
                    var hashBytes = sha256.ComputeHash(bytes);
                    return Convert.ToHexString(hashBytes).ToLower();
                }
        }

        public string CalculateMerkleRoot(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
                return string.Empty;

            List<string> hashes = transactions.Select(t => ComputeSha256(t.ToRowString())).ToList();
            while (hashes.Count > 1)
            {
                if (hashes.Count % 2 != 0)
                    hashes.Add(hashes.Last()); 

                List<string> newHashes = new List<string>();
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    string combinedHash = ComputeSha256(hashes[i] + hashes[i + 1]);
                    newHashes.Add(combinedHash);
                }

                hashes = newHashes;
            }

            return hashes[0];
        }
    }
}
