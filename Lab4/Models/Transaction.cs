using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }

        public Transaction(string from, string to, decimal amount)
        {
            Id = Guid.NewGuid().ToString().ToLowerInvariant().GetHashCode();
            From = from;
            To = to;
            Amount = amount;
            TimeStamp = DateTime.Now;
        }

        public string ToRowString()
        {
            return $"{Id}\t{From}\t{To}\t{Amount}\t{TimeStamp}";
        }

        public override string ToString()
        {
            return $"Transaction ID: {Id}, From: {From}, To: {To}, Amount: {Amount}, TimeStamp: {TimeStamp}";
        }
    }
}
