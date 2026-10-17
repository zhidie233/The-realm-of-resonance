using System.Collections;
using UnityEngine;

/// <summary>
/// Camera Identity 用于识别当前正在渲染的是哪个相机
/// 而不是用 Unity 的 Camera.current，它慢且不能在循环函数里使用
/// 想使用的相机都要挂上这个脚本。
/// 要获取当前正在渲染的相机，用：GameController.Instance.CameraRendered。
/// </summary>
public class CameraIdentity : MonoBehaviour
{
    private Camera m_Camera;

    private void OnEnable()
    {
        StartCoroutine(Set());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator Set()
    {
        yield return new WaitForSeconds(0.5f);
        if (m_Camera == null)
        {
            m_Camera = GetComponent<Camera>();
        }
        if (m_Camera != null)
            GameController.Instance.CameraRendered = m_Camera;
    }

    public static Camera CurrentCamera => GameController.Instance.CameraRendered;
}