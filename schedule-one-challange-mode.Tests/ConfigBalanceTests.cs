using Xunit;
using Xunit.Abstractions;
using challange_mode;

namespace challange_mode.Tests;

/// <summary>
/// Tests to ensure difficulty configuration is balanced
/// </summary>
public class ConfigBalanceTests
{
    private readonly ITestOutputHelper _output;

    public ConfigBalanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SuccessMultipliers_AreProgressive()
    {
        Assert.True(ChallengeConfig.MULTIPLIER_CRITICAL_LOW < ChallengeConfig.MULTIPLIER_LOW);
        Assert.True(ChallengeConfig.MULTIPLIER_LOW < ChallengeConfig.MULTIPLIER_MEDIUM);
        Assert.True(ChallengeConfig.MULTIPLIER_MEDIUM < ChallengeConfig.MULTIPLIER_HIGH);

        _output.WriteLine("Success Chance Multipliers:");
        _output.WriteLine($"  Critical Low (<{ChallengeConfig.ENJOYMENT_CRITICAL_LOW:P0}): {ChallengeConfig.MULTIPLIER_CRITICAL_LOW:P0}");
        _output.WriteLine($"  Low (<{ChallengeConfig.ENJOYMENT_LOW:P0}):          {ChallengeConfig.MULTIPLIER_LOW:P0}");
        _output.WriteLine($"  Medium (<{ChallengeConfig.ENJOYMENT_MEDIUM:P0}):       {ChallengeConfig.MULTIPLIER_MEDIUM:P0}");
        _output.WriteLine($"  High (≥{ChallengeConfig.ENJOYMENT_MEDIUM:P0}):        {ChallengeConfig.MULTIPLIER_HIGH:P0}");
    }

    [Fact]
    public void DrugAffinity_IsSecondaryFactor()
    {
        float maxImpact = ChallengeConfig.DRUG_AFFINITY_MAX_IMPACT;
        
        Assert.InRange(maxImpact, 0.2f, 0.35f);

        _output.WriteLine($"Drug Affinity Max Impact: ±{maxImpact:P0}");
    }
}

