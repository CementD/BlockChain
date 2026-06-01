using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Fee { get; set; }
        public byte[] SenderPublicKey { get; set; }
        public byte[]? Signature { get; set; }

        public int? ReplacesTxId { get; set; } = null;
        public int Size { get; private set; }
        public Transaction(string from, string to, decimal amount, decimal fee)
        {
            Id = Guid.NewGuid().ToString().ToLowerInvariant().GetHashCode();
            From = from;
            To = to;
            Amount = amount;
            TimeStamp = DateTime.Now;
            Fee = fee;
            Size = CalculateSize();
        }

        public byte[] GetDataToSign()
        {
            var data = $"{From}:{To}:{Amount}:{TimeStamp}{Fee}";
            return Encoding.UTF8.GetBytes(data);
        }

        public string ToRowString()
        {
            string signatureHex = Signature != null ?
                Convert.ToHexString(Signature) : "";
            return $"{Id}\t{From}\t{To}\t{Amount}\t{TimeStamp}\t{signatureHex}";
        }

        public override string ToString()
        {
            return $"Transaction ID: {Id}, From: {From}, To: {To}, Amount: {Amount}, TimeStamp: {TimeStamp}";
        }

        private int CalculateSize()
        {
            return Encoding.UTF8.GetByteCount($"{From}:{To}:{Amount}:{TimeStamp}{Fee}") + 
                   (SenderPublicKey?.Length ?? 0) + 
                   (Signature?.Length ?? 0);
        }
    }
}
