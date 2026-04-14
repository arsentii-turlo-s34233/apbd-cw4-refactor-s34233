using System;
using System.Collections.Generic;
using LegacyRenewalApp.Discounts;
using LegacyRenewalApp.Fees;
using LegacyRenewalApp.Infrastructure;
using LegacyRenewalApp.Interfaces;
using LegacyRenewalApp.Tax;
using LegacyRenewalApp.Validation;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IBillingGateway _billingGateway;
        private readonly IEnumerable<IDiscountStrategy> _discountStrategies;
        private readonly IPaymentFeeCalculator _paymentFeeCalculator;
        private readonly ITaxRateProvider _taxRateProvider;

        // Default constructor preserves the public contract used by LegacyRenewalAppConsumer
        public SubscriptionRenewalService()
            : this(
                new CustomerRepository(),
                new SubscriptionPlanRepository(),
                new LegacyBillingGatewayAdapter(),
                new IDiscountStrategy[]
                {
                    new SegmentDiscountStrategy(),
                    new LoyaltyYearsDiscountStrategy(),
                    new SeatCountDiscountStrategy(),
                    new LoyaltyPointsDiscountStrategy()
                },
                new PaymentFeeCalculator(),
                new CountryTaxRateProvider())
        { }

        // Full constructor for dependency injection / testing
        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            IBillingGateway billingGateway,
            IEnumerable<IDiscountStrategy> discountStrategies,
            IPaymentFeeCalculator paymentFeeCalculator,
            ITaxRateProvider taxRateProvider)
        {
            _customerRepository   = customerRepository;
            _planRepository       = planRepository;
            _billingGateway       = billingGateway;
            _discountStrategies   = discountStrategies;
            _paymentFeeCalculator = paymentFeeCalculator;
            _taxRateProvider      = taxRateProvider;
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            RenewalRequestValidator.Validate(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode      = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan     = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");

            decimal baseAmount     = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            var notes              = new List<string>();

            foreach (var strategy in _discountStrategies)
            {
                var (amount, note) = strategy.Calculate(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
                discountAmount += amount;
                if (!string.IsNullOrEmpty(note)) notes.Add(note);
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes.Add("minimum discounted subtotal applied");
            }

            decimal supportFee = PremiumSupportFeeCalculator.Calculate(includePremiumSupport, normalizedPlanCode);
            if (includePremiumSupport) notes.Add("premium support included");

            var (paymentFee, paymentNote) = _paymentFeeCalculator.Calculate(normalizedPaymentMethod, subtotalAfterDiscount + supportFee);
            notes.Add(paymentNote);

            decimal taxBase   = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxRate   = _taxRateProvider.GetRate(customer.Country);
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes.Add("minimum invoice amount applied");
            }

            var invoice = new RenewalInvoice
            {
                InvoiceNumber  = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName   = customer.FullName,
                PlanCode       = normalizedPlanCode,
                PaymentMethod  = normalizedPaymentMethod,
                SeatCount      = seatCount,
                BaseAmount     = Math.Round(baseAmount,     2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee     = Math.Round(supportFee,     2, MidpointRounding.AwayFromZero),
                PaymentFee     = Math.Round(paymentFee,     2, MidpointRounding.AwayFromZero),
                TaxAmount      = Math.Round(taxAmount,      2, MidpointRounding.AwayFromZero),
                FinalAmount    = Math.Round(finalAmount,    2, MidpointRounding.AwayFromZero),
                Notes          = string.Join("; ", notes),
                GeneratedAt    = DateTime.UtcNow
            };

            _billingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body    = $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                                 $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";
                _billingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}