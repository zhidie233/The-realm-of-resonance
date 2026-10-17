using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameRoomPreview : MonoBehaviour
{

    [Header("References")]
    public Image MapPreview;
    public TextMeshProUGUI MapNameText;

    public void Show(GameRoomInfo info)
    {
        var map = info.GetMapInfo();
        MapPreview.sprite = map.Preview;
        MapNameText.text = map.ShowName.ToUpper();
    }
}