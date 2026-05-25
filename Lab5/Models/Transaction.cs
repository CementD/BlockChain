using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public byte[] SenderPublicKey { get; set; }
        public byte[]? Signature { get; set; }
        public string Memo { get; set; }
        public Transaction(string from, string to, decimal amount, string memo)
        {
            Id = Guid.NewGuid().ToString().ToLowerInvariant().GetHashCode();
            From = from;
            To = to;
            Amount = amount;
            TimeStamp = DateTime.Now;
            Memo = memo;
        }

        public byte[] GetDataToSign()
        {
            var data = $"{From}:{To}:{Amount}:{TimeStamp}:{Memo}";
            return Encoding.UTF8.GetBytes(data);
        }

        public string ToRowString()
        {
            string signatureHex = Signature != null ?
                Convert.ToHexString(Signature) : "";
            return $"{Id}\t{From}\t{To}\t{Amount}\t{TimeStamp}\t{signatureHex}\t{Memo}";
        }

        public override string ToString()
        {
            return $"Transaction ID: {Id}, From: {From}, To: {To}, Amount: {Amount}, TimeStamp: {TimeStamp}";
        }
    }
}
