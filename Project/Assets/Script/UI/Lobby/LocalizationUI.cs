using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class LocalizationUI : MonoBehaviour
{
    public string StringID = "";
    public string[] StringIDs;
    public string ExtraString = string.Empty;
    public UIType m_UIType = UIType.Text;
    [Header("Options")]
    public int StringCase = 0;
    public bool Plural = false;
    public bool Extra = false;
    public bool FormatedText = false;

    [HideInInspector] public bool ManuallyAssignId = false;
    [HideInInspector] public int _arrayID = 0;


    [Serializable]
    public enum UIType
    {
        Text = 0,
        DropDown = 1,
    }
}