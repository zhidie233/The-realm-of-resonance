using UnityEngine;

/// <summary>
/// Inherited classes should take care of display to the player the information of the weapon
/// that they are about to pick up.
/// </summary>
public abstract class PickUpUIBase : MonoBehaviour
{
    /// <summary>
    /// Called when the player enter in the trigger or focus a gun pick up in the map
    /// </summary>
    public abstract void OnOverWeapon(GunPickUpBase toPickUpWeapon);

    public abstract void Show(DisplayInformation information);

    public abstract void Hide();

    public struct DisplayInformation
    {
        public string Text;
        public Sprite Icon;
        public string InputName;
    }

    private static PickUpUIBase _instance;
    public static PickUpUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<PickUpUIBase>(); }
            return _instance;
        }
    }
}