/// <summary>
/// Base class for the player weapon recoil movement
/// Inherited from this your custom recoil script.
/// </summary>
public abstract class RecoilBase : MonoBehaviourBase
{
    public struct RecoilData
    {
        public float Amount;
        public float Speed;

        public RecoilData(float amount)
        {
            Amount = amount;
            Speed = 2;
        }
    };

    public abstract void SetRecoil(RecoilData data);

}