namespace challange_mode
{
    public static class ChallengeConfig
    {
        public const float DRUG_AFFINITY_MAX_IMPACT = 0.3f;
        
        public const float EFFECT_MATCH_NONE = 0.0f;      // Neutral - early game playable
        public const float EFFECT_MATCH_ONE = 0.10f;      // +10% small bonus
        public const float EFFECT_MATCH_TWO = 0.25f;      // +25% good bonus  
        public const float EFFECT_MATCH_THREE = 0.45f;    // +45% HUGE bonus - effects dominate late game
        
        // Enjoyment thresholds - lower = more forgiving
        public const float ENJOYMENT_CRITICAL_LOW = 0.20f;  // Below 20% = critical
        public const float ENJOYMENT_LOW = 0.35f;           // 20-35% = low
        public const float ENJOYMENT_MEDIUM = 0.55f;        // 35-55% = medium
        
        // Success multipliers - higher = more forgiving
        public const float MULTIPLIER_CRITICAL_LOW = 0.3f;  // 30% penalty at critical
        public const float MULTIPLIER_LOW = 0.5f;           // 50% penalty at low
        public const float MULTIPLIER_MEDIUM = 0.75f;       // 75% penalty at medium
        public const float MULTIPLIER_HIGH = 1.0f;          // No penalty at high
        
        public const float COUNTEROFFER_PRICE_LIMIT_MULTIPLIER = 2.0f;
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
            if (enjoyment < ENJOYMENT_CRITICAL_LOW)
                return MULTIPLIER_CRITICAL_LOW;
            
            if (enjoyment < ENJOYMENT_LOW)
                return MULTIPLIER_LOW;
            
            if (enjoyment < ENJOYMENT_MEDIUM)
                return MULTIPLIER_MEDIUM;
            
            return MULTIPLIER_HIGH;
        }

        /// <summary>
        /// Get difficulty multiplier based on customer quality standards
        /// Early game customers (low standards) are more forgiving
        /// Late game customers (high standards) are stricter
        /// </summary>
        public static float GetStandardsMultiplier(int standardsValue)
        {
            // Standards: 0=VeryLow, 1=Low, 2=Moderate, 3=High, 4=VeryHigh
            return standardsValue switch
            {
                0 => 1.3f,     // VeryLow: +30% success (very forgiving - early game)
                1 => 1.15f,    // Low: +15% success (forgiving)
                2 => 1.0f,     // Moderate: Normal baseline
                3 => 0.85f,    // High: -15% success (stricter - late game)
                4 => 0.7f,     // VeryHigh: -30% success (very demanding - endgame)
                _ => 1.0f
            };
        }
    }
}

