using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Service
{
    public class WalletKeystoreService
    {
        public WalletKeystoreService() { }

        public void SaveWallet(Wallet wallet, string password)
        {
            string filePath = $"{wallet.Name}_wallet.json";

            byte[] key = new byte[32];
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            Array.Copy(passwordBytes, key, Math.Min(passwordBytes.Length, key.Length));

            byte[] iv = new byte[] { 1, 3, 5, 7, 9, 11, 13, 15, 2, 4, 6, 8, 10, 12, 14, 16 };

            byte[] encryptedPrivateKey;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                using var encryptor = aes.CreateEncryptor();
                encryptedPrivateKey = encryptor.TransformFinalBlock(wallet.PrivateKey, 0, wallet.PrivateKey.Length);
            }

            var jsonModel = new
            {
                Address = wallet.Address,
                PublicKey = Convert.ToBase64String(wallet.PublicKey),
                EncryptedPrivateKey = Convert.ToBase64String(encryptedPrivateKey)
            };

            string jsonString = System.Text.Json.JsonSerializer.Serialize(jsonModel, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);

            Console.WriteLine($"Wallet saved to {filePath}");
        }

        public Wallet LoadWallet(string name, string password)
        {
            string filePath = $"{name}_wallet.json";
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Wallet file {filePath} not found.");
            }

            string jsonString = File.ReadAllText(filePath);
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(jsonString);
            System.Text.Json.JsonElement root = doc.RootElement;

            string address = root.GetProperty("Address").GetString();
            byte[] publicKey = Convert.FromBase64String(root.GetProperty("PublicKey").GetString());
            byte[] encryptedPrivateKey = Convert.FromBase64String(root.GetProperty("EncryptedPrivateKey").GetString());

            byte[] key = new byte[32];
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            Array.Copy(passwordBytes, key, Math.Min(passwordBytes.Length, key.Length));

            byte[] iv = new byte[] { 1, 3, 5, 7, 9, 11, 13, 15, 2, 4, 6, 8, 10, 12, 14, 16 };
            byte[] decryptedPrivateKey;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    using var decryptor = aes.CreateDecryptor();
                    decryptedPrivateKey = decryptor.TransformFinalBlock(encryptedPrivateKey, 0, encryptedPrivateKey.Length);
                }
            }
            catch (CryptographicException)
            {
                throw new Exception("Incorrect password or corrupted wallet file.");
            }

            // Возвращаем кошелек через твой полный конструктор
            return new Wallet(name, address, publicKey, decryptedPrivateKey);
        }
    }
}

class WalletStorageModel
{
    public string PublicKey { get; set; }
    public string EncryptedPrivateKey { get; set; }
}
