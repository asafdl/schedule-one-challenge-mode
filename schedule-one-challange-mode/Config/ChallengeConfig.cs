namespace challange_mode
{
    public static class ChallengeConfig
    {
        public const float DRUG_AFFINITY_MAX_IMPACT = 0.3f;
        
        public const float EFFECT_MATCH_NONE = -0.35f;
        public const float EFFECT_MATCH_ONE = 0.0f;
        public const float EFFECT_MATCH_TWO = 0.15f;
        public const float EFFECT_MATCH_THREE = 0.25f;
        
        public const float ENJOYMENT_CRITICAL_LOW = 0.25f;
        public const float ENJOYMENT_LOW = 0.45f;
        public const float ENJOYMENT_MEDIUM = 0.65f;
        
        public const float MULTIPLIER_CRITICAL_LOW = 0.1f;
        public const float MULTIPLIER_LOW = 0.3f;
        public const float MULTIPLIER_MEDIUM = 0.6f;
        public const float MULTIPLIER_HIGH = 1.0f;
        
        public const float COUNTEROFFER_PRICE_LIMIT_MULTIPLIER = 2.0f;
        public const float COUNTEROFFER_MIN_ENJOYMENT = 0.4f;
        public const float COUNTEROFFER_MEDIOCRE_THRESHOLD = 0.6f;
        public const float COUNTEROFFER_MAX_REJECT_CHANCE = 0.5f;
    }

    public static class CustomerBehaviorHelpers
    {
        public static float GetSuccessMultiplier(float enjoyment)
        {
            if (enjoyment < ChallengeConfig.ENJOYMENT_CRITICAL_LOW)
                return ChallengeConfig.MULTIPLIER_CRITICAL_LOW;
            
            if (enjoyment < ChallengeConfig.ENJOYMENT_LOW)
                return ChallengeConfig.MULTIPLIER_LOW;
            
            if (enjoyment < ChallengeConfig.ENJOYMENT_MEDIUM)
                return ChallengeConfig.MULTIPLIER_MEDIUM;
            
            return ChallengeConfig.MULTIPLIER_HIGH;
        }
    }
}

