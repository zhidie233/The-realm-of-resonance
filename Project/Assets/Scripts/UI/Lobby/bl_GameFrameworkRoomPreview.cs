using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class bl_GFWKRoomPreview : MonoBehaviour
{

    [Header("References")]
    public Image MapPreview;
    public TextMeshProUGUI MapNameText;

    /// <summary>
    /// 
    /// </summary>
    public void Show(GFWKRoomInfo info)
    {
        var map = info.GetMapInfo();
        MapPreview.sprite = map.Preview;
        MapNameText.text = map.ShowName.ToUpper();
    }
}