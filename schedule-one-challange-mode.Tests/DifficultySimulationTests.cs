using System;
using Xunit;
using Xunit.Abstractions;
using challange_mode;

namespace challange_mode.Tests;

/// <summary>
/// Simulates customer deal scenarios to visualize difficulty impact
/// Uses realistic customer data from base game
/// </summary>
public class DifficultySimulationTests
{
    private readonly ITestOutputHelper _output;

    public DifficultySimulationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Simulate_RealCustomerScenarios()
    {
        _output.WriteLine("=== CHALLENGE MODE DIFFICULTY SIMULATION ===");
        _output.WriteLine("Using actual customer data from Schedule I base game\n");

        // Austin Steiner - Real data from DefaultSave (MODERATE standards - typical early game)
        // ProductAffinities: [0.78 (Marijuana), -0.66 (Meth), 0.15 (Cocaine)]
        _output.WriteLine("=== AUSTIN STEINER (Marijuana Lover, Meth Hater) - MODERATE Standards ===\n");
        
        SimulateScenario("Selling Marijuana + 3 effect matches", 0.78f, 3, 1.0f);
        SimulateScenario("Selling Marijuana + 2 effect matches", 0.78f, 2, 1.0f);
        SimulateScenario("Selling Marijuana + 1 effect match", 0.78f, 1, 1.0f);
        SimulateScenario("Selling Marijuana + 0 effect matches", 0.78f, 0, 1.0f);
        SimulateScenario("Selling Cocaine (neutral) + 3 effects", 0.15f, 3, 1.0f);
        SimulateScenario("Selling Meth (hates it) + 3 effects", -0.66f, 3, 1.0f);
        SimulateScenario("Selling Meth + 0 effects (worst case)", -0.66f, 0, 1.0f);

        // Show early game (LOW standards) - more forgiving
        _output.WriteLine("\n=== EARLY GAME CUSTOMER (LOW Standards) - Same scenario ===\n");
        SimulateScenario("Marijuana + 0 effects (early game)", 0.78f, 0, 1.25f);
        
        // Show late game (PREMIUM standards) - much stricter
        _output.WriteLine("\n=== LATE GAME CUSTOMER (PREMIUM Standards) - Same scenario ===\n");
        SimulateScenario("Marijuana + 0 effects (late game)", 0.78f, 0, 0.7f);

        // Jessi Waters - Real data from DefaultSave
        // ProductAffinities: [0.0 (Marijuana), 1.0 (Meth), -0.27 (Cocaine)]
        _output.WriteLine("\n=== JESSI WATERS (Meth Enthusiast, Cocaine Dislike) - MODERATE Standards ===\n");
        
        SimulateScenario("Selling Meth + 3 effect matches", 1.0f, 3, 1.0f);
        SimulateScenario("Selling Meth + 2 effect matches", 1.0f, 2, 1.0f);
        SimulateScenario("Selling Meth + 1 effect match", 1.0f, 1, 1.0f);
        SimulateScenario("Selling Marijuana (neutral) + 2 effects", 0.0f, 2, 1.0f);
        SimulateScenario("Selling Cocaine (dislikes) + 3 effects", -0.27f, 3, 1.0f);

        // Kathy Henderson - Real data from DefaultSave
        // ProductAffinities: [0.55 (Marijuana), 0.27 (Meth), -0.61 (Cocaine)]
        _output.WriteLine("\n=== KATHY HENDERSON (Balanced Preference, Cocaine Hater) - MODERATE Standards ===\n");
        
        SimulateScenario("Selling Marijuana (liked) + 3 effects", 0.55f, 3, 1.0f);
        SimulateScenario("Selling Marijuana + 1 effect", 0.55f, 1, 1.0f);
        SimulateScenario("Selling Meth (slight like) + 2 effects", 0.27f, 2, 1.0f);
        SimulateScenario("Selling Cocaine (hates) + 3 effects", -0.61f, 3, 1.0f);
        SimulateScenario("Selling Cocaine + 0 effects (worst)", -0.61f, 0, 1.0f);
    }

    private void SimulateScenario(string scenario, float drugAffinity, int effectMatches, float standardsMultiplier = 1.0f)
    {
        float baseEnjoyment = 0.5f;

        float drugBonus = drugAffinity * ChallengeConfig.DRUG_AFFINITY_MAX_IMPACT;
        float effectBonus = GetEffectBonus(effectMatches);
        
        float finalEnjoyment = baseEnjoyment + drugBonus + effectBonus;
        finalEnjoyment = Math.Clamp(finalEnjoyment, 0f, 1f);

        float enjoymentMultiplier = ChallengeConfig.GetSuccessMultiplier(finalEnjoyment);
        float totalMultiplier = enjoymentMultiplier * standardsMultiplier;
        
        float vanillaSuccess = 0.70f;
        float modifiedSuccess = vanillaSuccess * totalMultiplier;

        string standardsNote = standardsMultiplier != 1.0f 
            ? $" × {standardsMultiplier:P0} standards" 
            : "";

        _output.WriteLine($"{scenario}:");
        _output.WriteLine($"  Enjoyment: {baseEnjoyment:P0} + {drugBonus:+0.0%;-0.0%;0} drug + {effectBonus:+0.0%;-0.0%;0} effects = {finalEnjoyment:P0}");
        _output.WriteLine($"  Success:   {vanillaSuccess:P0} → {modifiedSuccess:P0} ({enjoymentMultiplier:P0}{standardsNote} = {totalMultiplier:P0} total) - {GetDifficultyRating(modifiedSuccess)}");
        _output.WriteLine("");
    }

    private float GetEffectBonus(int matches)
    {
        return matches switch
        {
            0 => ChallengeConfig.EFFECT_MATCH_NONE,
            1 => ChallengeConfig.EFFECT_MATCH_ONE,
            2 => ChallengeConfig.EFFECT_MATCH_TWO,
            _ => ChallengeConfig.EFFECT_MATCH_THREE
        };
    }

    private string GetDifficultyRating(float successRate)
    {
        return successRate switch
        {
            < 0.10f => "NEARLY IMPOSSIBLE",
            < 0.30f => "EXTREMELY HARD",
            < 0.50f => "HARD",
            < 0.70f => "MEDIUM",
            < 0.85f => "EASY",
            _ => "VERY EASY"
        };
    }

}

