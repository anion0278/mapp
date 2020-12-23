using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Martin_app
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

        public AmazonMarketplace Marketplace { get; set; }

        public string OrderId { get; set; }

        public string ProductDescription { get; set; }


        // TODO rename, its not price but TOTAL of transaction
        // in GPC only the price for item, shipping, sale(rebate) is relevant
        // in JP case also some additional Fees
        public double TransactionValue { get; set; }
    }
}
