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
            var input = $"{block.Index}{block.Timestamp}{block.Data}{block.Author}{block.PreviousHash}";
            return ComputeSha256(input);
        }

        private string ComputeSha256(string input)
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(input);
                    var hashBytes = sha256.ComputeHash(bytes);
                    return Convert.ToHexString(hashBytes).ToLower();
                }
        }
    }
}
