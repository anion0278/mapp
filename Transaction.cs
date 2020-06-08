using System;
using System.Collections.Generic;
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

        public int Quantity { get; set; }

        public double ProductPrice { get; set; }

        public double PromotionRebate { get; set; }

        public double ShippingPrice { get; set; }

        public double TotalPrice => ProductPrice + ShippingPrice + PromotionRebate;
    }
}
