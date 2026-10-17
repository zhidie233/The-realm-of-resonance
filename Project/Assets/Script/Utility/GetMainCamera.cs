using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMainCamera : MonoBehaviour
{
    // Start 在第一帧更新前调用
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }
}
