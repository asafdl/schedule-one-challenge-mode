namespace challenge_mode
{
    public static class ChallengeConfig
    {
        public const float DRUG_AFFINITY_MAX_IMPACT = 0.3f;
        
        public const float EFFECT_MATCH_NONE = 0.0f;
        public const float EFFECT_MATCH_ONE = 0.10f;
        public const float EFFECT_MATCH_TWO = 0.25f;
        public const float EFFECT_MATCH_THREE = 0.45f;
        
        // Difficulty scaling - continuous function instead of buckets
        public const float DIFFICULTY_SCALE = 1.25f;        // Scale factor (1.2 means 0.83+ enjoyment = no penalty)
        public const float DIFFICULTY_MIN = 0.1f;          // Minimum multiplier (never impossible)
        
        public const float COUNTEROFFER_MIN_ENJOYMENT = 0.4f;
        public const float COUNTEROFFER_MEDIOCRE_THRESHOLD = 0.6f;
        public const float COUNTEROFFER_MAX_REJECT_CHANCE = 0.5f;
        
        public const float MIN_APPEAL_FOR_REQUEST = 0.6f;

        public const float MIN_APPEAL_FOR_SUCCESS = 0.3f;
        public const int MESSAGE_COOLDOWN_DAYS = 3;
        public const int MIN_FAILED_REQUESTS_BEFORE_MESSAGE = 2;
        public const bool ENABLE_PRODUCT_REQUEST_MESSAGES = true;

        public static float GetSuccessMultiplier(float enjoyment)
        {
            // Continuous scaling: scale enjoyment by constant, clamp to [min, 1.0]
            float multiplier = enjoyment * DIFFICULTY_SCALE;
            return multiplier;
        }

        public static float GetStandardsPenalty(int standards)
        {
            return standards switch
            {
                0 => 1.0f,   // VeryLow - no penalty
                1 => 0.95f,  // Low - minimal penalty
                2 => 0.9f,   // Moderate - minor penalty
                3 => 0.8f,   // High - moderate penalty
                4 => 0.7f,   // VeryHigh - significant penalty
                _ => 1.0f
            };
        }
    }
}

