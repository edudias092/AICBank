using AICBank.Core.DTOs;

namespace AICBank.Core.Interfaces;

public interface ISplitFactory
{
    Split CreateDefaultBoletoSplitPaymentMethod();
}