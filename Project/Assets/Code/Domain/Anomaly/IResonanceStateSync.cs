namespace Resonance.Domain.Anomaly
{
    /// <summary>
    /// Transport-agnostic synchronization boundary for room-wide resonance state.
    /// </summary>
    public interface IResonanceStateSync
    {
        bool IsAuthority { get; }
        bool TryReadCurrentValue(out float value);
        void PublishValue(float value);
        void RequestAuthorityChange(string changeKey, float multiplier);
    }
}
