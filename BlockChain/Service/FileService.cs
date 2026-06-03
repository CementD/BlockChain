using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace BlockChain.Service
{
    public class FileService
    {
        private readonly string chainFilePath = "blockchain_data.json";
        public FileService(int port)
        {
            chainFilePath = $"blockchain_data_{port}.json";
        }

        public void SaveChainToFile(List<Block> chain)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(chain);
            System.IO.File.WriteAllText(chainFilePath, json);
        }

        public List<Block> LoadChain() {
            if (File.Exists(chainFilePath))
            {
                var json = File.ReadAllText(chainFilePath);
                var chain = JsonSerializer.Deserialize<List<Block>>(json);
                if (chain != null)
                    return chain;
            }
            return new List<Block>();
        }
    }
}
