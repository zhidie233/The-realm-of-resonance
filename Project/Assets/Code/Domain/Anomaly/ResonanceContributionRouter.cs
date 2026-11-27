namespace Resonance.Domain.Anomaly
{
    public enum ResonanceObjectiveKind
    {
        FlagPickedUp = 0,
        FlagCaptured = 1,
        FlagRecovered = 2
    }

    public readonly struct ResonanceContribution
    {
        public bool IsValid { get; }
        public string Key { get; }
        public float Multiplier { get; }
        public bool AuthorityOnly { get; }

        public ResonanceContribution(bool isValid, string key, float multiplier, bool authorityOnly)
        {
            IsValid = isValid;
            Key = key;
            Multiplier = multiplier;
            AuthorityOnly = authorityOnly;
        }
    }

    /// <summary>
    /// Centralizes mapping from combat, objective, and facility events into resonance rule keys.
    /// </summary>
    public static class ResonanceContributionRouter
    {
        public const string EnemyHitKey = "Enemy";
        public const string TeammateHitKey = "Teammate";
        public const string FlagPickUpKey = "FlagPickUp";
        public const string FlagCaptureKey = "FlagCapture";
        public const string FlagRecoverKey = "FlagRecover";

        public static ResonanceContribution RouteCombatDamage(
            int attackerTeamId,
            int victimTeamId,
            float damage,
            bool isFreeForAll)
        {
            if (damage <= 0f) return default;

            bool isHostile = isFreeForAll || attackerTeamId != victimTeamId;
            return new ResonanceContribution(
                true,
                isHostile ? EnemyHitKey : TeammateHitKey,
                damage,
                false);
        }

        public static ResonanceContribution RouteObjective(ResonanceObjectiveKind kind)
        {
            switch (kind)
            {
                case ResonanceObjectiveKind.FlagPickedUp:
                    return new ResonanceContribution(true, FlagPickUpKey, 1f, true);
                case ResonanceObjectiveKind.FlagCaptured:
                    return new ResonanceContribution(true, FlagCaptureKey, 1f, true);
                case ResonanceObjectiveKind.FlagRecovered:
                    return new ResonanceContribution(true, FlagRecoverKey, 1f, true);
                default:
                    return default;
            }
        }

        public static ResonanceContribution RouteDirectImpulse(string key, float multiplier)
        {
            if (string.IsNullOrEmpty(key)) return default;
            return new ResonanceContribution(true, key, multiplier, false);
        }
    }
}
