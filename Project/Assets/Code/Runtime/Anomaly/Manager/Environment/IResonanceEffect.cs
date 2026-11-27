namespace Resonance.Anomaly
{
    /// <summary>
    /// Contract for scene-level environmental effects driven by ResonanceManager.
    /// </summary>
    public interface IResonanceEffect
    {
        void Initialize(float initialResonance);
        void ApplyResonance(float resonance);
        void ResetEffect(float baselineResonance);
    }
}
