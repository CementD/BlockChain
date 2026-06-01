using BlockChain.Models;
using System.Security.Cryptography;

namespace BlockChain.Service
{
    public class WalletService
    {
        private Wallet CreateWallet(string name)
        {
            using var ecdsa = ECDsa.Create();

            var privateKey = ecdsa.ExportECPrivateKey();
            var publicKey = ecdsa.ExportSubjectPublicKeyInfo();

            var address = string.Empty;
            int attempts = 0;

            while (true)
            {
                var newPublicKey = publicKey.Concat(BitConverter.GetBytes(attempts)).ToArray();
                address = Convert.ToHexString(SHA256.HashData(newPublicKey));

                if (address.StartsWith("000"))
                {
                    break;
                }
                attempts++;
            }
            Console.WriteLine($"Generated wallet with address {address} after {attempts} attempts.");
            return new Wallet(name, address, publicKey, privateKey);
        }

        public Wallet GetOrCreateWallet(string name)
        {
            string walletFilePath = $"{name}_wallet.json";
            if (File.Exists(walletFilePath))
            {
                var json = File.ReadAllText(walletFilePath);
                return System.Text.Json.JsonSerializer.Deserialize<Wallet>(json);
            }
            else
            {
                var wallet = CreateWallet(name);
                var json = System.Text.Json.JsonSerializer.Serialize(wallet);
                File.WriteAllText(walletFilePath, json);
                return wallet;
            }
        }

        public bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey)
        {
            using var ecdsa = System.Security.Cryptography.ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
            return ecdsa.VerifyData(data, signature, System.Security.Cryptography.HashAlgorithmName.SHA256);
        }
    }
}
