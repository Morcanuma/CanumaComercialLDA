using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lojaCanuma.Models
{
    public class LoyaltySettings
    {
        public bool IsEnabled { get; set; }
        public bool IsPromoEnabled { get; set; }
        public int PointsExpireDays { get; set; }
        public int InactivityMaxDays { get; set; }
        public decimal KzPerPointEarn { get; set; }
        public int MinPointsToRedeem { get; set; }
        public int RedeemUnitPoints { get; set; }
        public decimal RedeemDiscountKz { get; set; }
        public decimal MaxDiscountPct { get; set; }
        public decimal MinGrossMarginPct { get; set; }
        public int AutoDisableIfBelowDays { get; set; }
    }

    public class BusinessGoals
    {
        public DateTime EffectiveFrom { get; set; }
        public decimal MonthlyRevenueTargetKz { get; set; }
        public decimal NetMarginTargetPct { get; set; }
        public decimal MinGrossMarginPctPerSale { get; set; }
    }

    public class CartLine
    {
        public string PCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal Cost { get; set; } // custo unitário

        public decimal LineTotal => Price * Qty;
        public decimal LineCost => Cost * Qty;
    }
}
