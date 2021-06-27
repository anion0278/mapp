using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapp
{
    public enum TransactionTypes
    {
        Order = 2,
        Transfer = 1,
        Refund = 5,
        ServiceFee = 3,
    };

    public class Transaction
    {
        public DateTime Date { get; set; }

        public TransactionTypes Type { get; set; }

        public int MarketplaceId { get; set; }

        public string OrderId { get; set; }

        public double TransactionValue { get; set; }
    }
}
