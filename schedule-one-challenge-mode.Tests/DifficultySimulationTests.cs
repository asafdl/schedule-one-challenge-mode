using System;
using Xunit;
using Xunit.Abstractions;
using challenge_mode;

namespace challenge_mode.Tests;

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
        _output.WriteLine("=== CHALLENGE MODE DIFFICULTY SIMULATION ===\n");

        // Test each difficulty multiplier bracket
        // Enjoyment thresholds: <0.20 (0.3×), 0.20-0.35 (0.5×), 0.35-0.55 (0.75×), >=0.55 (1.0×)
        
        _output.WriteLine("=== ENJOYMENT: HIGH (>0.55) - No difficulty penalty ===");
        _output.WriteLine("Neutral affinity (0.0), 1 effect match → enjoyment 0.60\n");
        SimulateScenario("VeryLow (0)", 0.0f, 1, 1.0f);
        SimulateScenario("Moderate (2)", 0.0f, 1, 0.9f);
        SimulateScenario("VeryHigh (4)", 0.0f, 1, 0.7f);

        _output.WriteLine("\n=== ENJOYMENT: MEDIUM (0.35-0.55) - 0.75× difficulty penalty ===");
        _output.WriteLine("Neutral affinity (0.0), 0 effect matches → enjoyment 0.50\n");
        SimulateScenario("VeryLow (0)", 0.0f, 0, 1.0f);
        SimulateScenario("Moderate (2)", 0.0f, 0, 0.9f);
        SimulateScenario("VeryHigh (4)", 0.0f, 0, 0.7f);

        _output.WriteLine("\n=== ENJOYMENT: LOW (0.20-0.35) - 0.5× difficulty penalty ===");
        _output.WriteLine("Strong dislike (-0.8 affinity), 0 effect matches → enjoyment 0.26\n");
        SimulateScenario("VeryLow (0)", -0.8f, 0, 1.0f);
        SimulateScenario("Moderate (2)", -0.8f, 0, 0.9f);
        SimulateScenario("VeryHigh (4)", -0.8f, 0, 0.7f);

        _output.WriteLine("\n=== ENJOYMENT: CRITICAL LOW (<0.20) - 0.3× difficulty penalty ===");
        _output.WriteLine("Hates drug (-1.0 affinity), 0 effect matches → enjoyment 0.20\n");
        SimulateScenario("VeryLow (0)", -1.0f, 0, 1.0f);
        SimulateScenario("Moderate (2)", -1.0f, 0, 0.9f);
        SimulateScenario("VeryHigh (4)", -1.0f, 0, 0.7f);

        // Real customer data from DefaultSave
        _output.WriteLine("\n=== DEFAULT SAVE CUSTOMERS ===\n");

        // Austin Steiner
        // ProductAffinities: [0.78 (Marijuana), -0.66 (Meth), 0.15 (Cocaine)]
        // Standards: Moderate (2)
        _output.WriteLine("--- Austin Steiner (Marijuana Lover, Meth Hater) ---\n");
        SimulateScenario("Selling Marijuana + 3 effect matches", 0.78f, 3, 0.9f);
        SimulateScenario("Selling Marijuana + 2 effect matches", 0.78f, 2, 0.9f);
        SimulateScenario("Selling Marijuana + 1 effect match", 0.78f, 1, 0.9f);
        SimulateScenario("Selling Marijuana + 0 effect matches", 0.78f, 0, 0.9f);
        SimulateScenario("Selling Cocaine (neutral) + 3 effects", 0.15f, 3, 0.9f);
        SimulateScenario("Selling Meth (hates it) + 3 effects", -0.66f, 3, 0.9f);
        SimulateScenario("Selling Meth + 0 effects (worst case)", -0.66f, 0, 0.9f);

        // Jessi Waters
        // ProductAffinities: [0.0 (Marijuana), 1.0 (Meth), -0.27 (Cocaine)]
        // Standards: Moderate (2)
        _output.WriteLine("\n--- Jessi Waters (Meth Enthusiast, Cocaine Dislike) ---\n");
        SimulateScenario("Selling Meth + 3 effect matches", 1.0f, 3, 0.9f);
        SimulateScenario("Selling Meth + 2 effect matches", 1.0f, 2, 0.9f);
        SimulateScenario("Selling Meth + 1 effect match", 1.0f, 1, 0.9f);
        SimulateScenario("Selling Marijuana (neutral) + 2 effects", 0.0f, 2, 0.9f);
        SimulateScenario("Selling Cocaine (dislikes) + 3 effects", -0.27f, 3, 0.9f);

        // Kathy Henderson
        // ProductAffinities: [0.55 (Marijuana), 0.27 (Meth), -0.61 (Cocaine)]
        // Standards: Moderate (2)
        _output.WriteLine("\n--- Kathy Henderson (Balanced Preference, Cocaine Hater) ---\n");
        SimulateScenario("Selling Marijuana (liked) + 3 effects", 0.55f, 3, 0.9f);
        SimulateScenario("Selling Marijuana + 1 effect", 0.55f, 1, 0.9f);
        SimulateScenario("Selling Meth (slight like) + 2 effects", 0.27f, 2, 0.9f);
        SimulateScenario("Selling Cocaine (hates) + 3 effects", -0.61f, 3, 0.9f);
        SimulateScenario("Selling Cocaine + 0 effects (worst)", -0.61f, 0, 0.9f);
    }

    private void SimulateScenario(string scenario, float drugAffinity, int effectMatches, float standardsPenalty = 1.0f)
    {
        float baseEnjoyment = 0.5f;

        float drugBonus = drugAffinity * ChallengeConfig.DRUG_AFFINITY_MAX_IMPACT;
        float effectBonus = GetEffectBonus(effectMatches);
        
        float finalEnjoyment = baseEnjoyment + drugBonus + effectBonus;
        finalEnjoyment = Math.Clamp(finalEnjoyment, 0f, 1f);

        float difficultyMultiplier = ChallengeConfig.GetSuccessMultiplier(finalEnjoyment);
        float totalMultiplier = difficultyMultiplier * standardsPenalty;

        float vanillaSuccess = 0.70f;
        float modifiedSuccess = vanillaSuccess * totalMultiplier;

        _output.WriteLine($"{scenario}:");
        _output.WriteLine($"  BASE: {vanillaSuccess:P0}  →  MOD: {modifiedSuccess:P0}");
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


}

