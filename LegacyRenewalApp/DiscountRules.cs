namespace LegacyRenewalApp
{
    public class SegmentDiscountRule : IDiscountRule
    {
        public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            var result = new DiscountResult();
            
            if (customer.Segment == "Silver") { result.DiscountAmount = baseAmount * 0.05m; result.Note = "silver discount; "; }
            else if (customer.Segment == "Gold") { result.DiscountAmount = baseAmount * 0.10m; result.Note = "gold discount; "; }
            else if (customer.Segment == "Platinum") { result.DiscountAmount = baseAmount * 0.15m; result.Note = "platinum discount; "; }
            else if (customer.Segment == "Education" && plan.IsEducationEligible) { result.DiscountAmount = baseAmount * 0.20m; result.Note = "education discount; "; }
            
            return result;
        }
    }

    public class LoyaltyTimeDiscountRule : IDiscountRule
    {
        public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            var result = new DiscountResult();
            
            if (customer.YearsWithCompany >= 5) { result.DiscountAmount = baseAmount * 0.07m; result.Note = "long-term loyalty discount; "; }
            else if (customer.YearsWithCompany >= 2) { result.DiscountAmount = baseAmount * 0.03m; result.Note = "basic loyalty discount; "; }
            
            return result;
        }
    }

    public class TeamSizeDiscountRule : IDiscountRule
    {
        public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            var result = new DiscountResult();
            
            if (seatCount >= 50) { result.DiscountAmount = baseAmount * 0.12m; result.Note = "large team discount; "; }
            else if (seatCount >= 20) { result.DiscountAmount = baseAmount * 0.08m; result.Note = "medium team discount; "; }
            else if (seatCount >= 10) { result.DiscountAmount = baseAmount * 0.04m; result.Note = "small team discount; "; }
            
            return result;
        }
    }

    public class LoyaltyPointsDiscountRule : IDiscountRule
    {
        public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            var result = new DiscountResult();
            
            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                result.DiscountAmount = pointsToUse;
                result.Note = $"loyalty points used: {pointsToUse}; ";
            }
            
            return result;
        }
    }
}