using System;

namespace Shmap.BusinessLogic.Transactions
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

        public decimal TransactionValue { get; set; }
    }
}
