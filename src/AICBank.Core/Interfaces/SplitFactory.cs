using AICBank.Core.DTOs;
using Microsoft.Extensions.Configuration;

namespace AICBank.Core.Interfaces;

public class SplitFactory(IConfiguration configuration) : ISplitFactory
{
    public Split CreateDefaultBoletoSplitPaymentMethod()
    {
        var mainGalaxId = configuration.GetSection("CelCash").GetValue<int>("galaxId");
        return new Split
        {
            All = new SplitDetails()
            {
                Type = SplitType.Fixed,
                Companies = new[]
                {
                    new SplitCompany
                    {
                        GalaxPayId = mainGalaxId,
                        Value = 190
                    }
                }
            }
        };
    }
}