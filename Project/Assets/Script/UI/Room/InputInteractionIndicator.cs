using System;
using UnityEngine;
using TMPro;

public class InputInteractionIndicator : MonoBehaviour
{
    [GameToggle] public bool forceUpperCase = true;
    [SerializeField] private GameObject content = null;
    [SerializeField] private TextMeshProUGUI inputNameText = null;
    [SerializeField] private TextMeshProUGUI descriptionText = null;

    private Action onClickCallback;

    private void OnEnable()
    {
        GameEvents.onLocalPlayerDeath += OnLocalPlayerDie;
    }

    private void OnDisable()
    {
        GameEvents.onLocalPlayerDeath -= OnLocalPlayerDie;
    }

    public static void SetActive(bool active)
    {
        if (Instance == null) return;

        Instance.content.SetActive(active);
    }

    /// <summary>
    /// Set active will only execute if the current text showing is the same
    /// as the provided.
    /// </summary>
    public static void SetActiveIfSame(bool active, string description)
    {
        if (Instance == null) return;
        if (description.ToLower() != Instance.descriptionText.text.ToLower()) return;

        Instance.content.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputName">Input Name to execute the action</param>
    /// <param name="description">Description of the action.</param>
    public static void ShowIndication(string inputName, string description, Action clickCallback = null)
    {
        if (Instance == null) return;

        Instance.inputNameText.text = inputName.ToUpper();
        Instance.descriptionText.text = Instance.forceUpperCase ? description.ToUpper() : description;
        Instance.onClickCallback = clickCallback;
        SetActive(true);
    }

    public void OnClick()
    {
        onClickCallback?.Invoke();
    }

    void OnLocalPlayerDie()
    {
        SetActive(false);
    }

    private static InputInteractionIndicator _instance;
    public static InputInteractionIndicator Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<InputInteractionIndicator>();
            return _instance;
        }
    }
}