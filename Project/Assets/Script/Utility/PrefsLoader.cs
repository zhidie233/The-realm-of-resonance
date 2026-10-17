using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefsLoader : MonoBehaviour
{

    public string PrefsKey;
    public ApplyType m_Type = ApplyType.Toogle;

    [Header("Defaults")]
    public bool DefaultBool = true;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(PrefsKey))
        {
            LoadPrefs();
        }
    }

    void LoadPrefs()
    {
        if(m_Type == ApplyType.Toogle)
        {
            GetComponent<Toggle>().isOn = GetUnityKey.GetBoolPrefs(DefaultBool);
        }
    }

    public void Set(bool v)
    {
        GetUnityKey.SetBoolPrefs(v);
    }

    private string GetUnityKey
    {
        get { return string.Format("{0}.{1}.{2}", Application.companyName, Application.productName, PrefsKey); }
    }

    [System.Serializable]
    public enum ApplyType
    {
        Toogle,
        Text,
    }
}