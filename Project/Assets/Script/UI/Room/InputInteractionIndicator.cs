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
    /// 仅当当前显示文本与之相同时，Set active 才会执行
    /// 与传入值一致。
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
    /// <param name="inputName">执行该操作的输入名称</param>
    /// <param name="description">操作说明。</param>
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