using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class GravityFacility : MonoBehaviourPunCallbacks, IPunObservable
{
    public Vector3 StartLimitPosition = new Vector3(0, 500f, 0); // 初始坐标
    public Vector3 FinalLimitPosition = new Vector3(0, 60f, 0);  // 最终坐标
    public Quaternion StartLimitRotation = Quaternion.identity;
    public Quaternion FinalLimitRotation = Quaternion.identity;
    
    // 添加同步标识，确保所有客户端位置和旋转一致
    private Vector3 _syncedPosition;
    private Quaternion _syncedRotation;
    private bool _transformNeedsUpdate = true;

    private void Start()
    {
        // 初始位置和旋转设置为起始值
        transform.position = StartLimitPosition;
        transform.rotation = StartLimitRotation;
        _syncedPosition = StartLimitPosition;
        _syncedRotation = StartLimitRotation;
    }

    public void UpdateTransformBasedOnResonance(float resonanceValue, float maxResonance, AnimationCurve curve)
    {
        // 将共鸣值映射到0-1的范围
        float t = Mathf.Clamp01(resonanceValue / maxResonance);
        
        // 应用曲线控制
        t = curve.Evaluate(t);
        
        // 计算新位置和旋转
        Vector3 newPosition = Vector3.Lerp(StartLimitPosition, FinalLimitPosition, t);
        Quaternion newRotation = Quaternion.Lerp(StartLimitRotation, FinalLimitRotation, t);
        
        // 只在位置或旋转有显著变化时更新
        bool positionChanged = Vector3.Distance(transform.position, newPosition) > 0.1f;
        bool rotationChanged = Quaternion.Angle(transform.rotation, newRotation) > 0.1f;
        
        if (positionChanged || rotationChanged)
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
            _syncedPosition = newPosition;
            _syncedRotation = newRotation;
            _transformNeedsUpdate = true;
        }
    }

    // Photon网络同步
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 主客户端发送数据
            stream.SendNext(_syncedPosition);
            stream.SendNext(_syncedRotation);
            stream.SendNext(StartLimitPosition);
            stream.SendNext(FinalLimitPosition);
            stream.SendNext(StartLimitRotation);
            stream.SendNext(FinalLimitRotation);
            
            if (_transformNeedsUpdate)
            {
                _transformNeedsUpdate = false;
            }
        }
        else
        {
            // 其他客户端接收数据
            _syncedPosition = (Vector3)stream.ReceiveNext();
            _syncedRotation = (Quaternion)stream.ReceiveNext();
            StartLimitPosition = (Vector3)stream.ReceiveNext();
            FinalLimitPosition = (Vector3)stream.ReceiveNext();
            StartLimitRotation = (Quaternion)stream.ReceiveNext();
            FinalLimitRotation = (Quaternion)stream.ReceiveNext();
            
            // 立即应用同步的位置和旋转
            transform.position = _syncedPosition;
            transform.rotation = _syncedRotation;
        }
    }

    // 编辑器方法：移动到StartLimitPosition和StartLimitRotation
    public void MoveToStartTransform()
    {
        transform.position = StartLimitPosition;
        transform.rotation = StartLimitRotation;
        _syncedPosition = StartLimitPosition;
        _syncedRotation = StartLimitRotation;
        _transformNeedsUpdate = true;
    }

    // 编辑器方法：移动到FinalLimitPosition和FinalLimitRotation
    public void MoveToFinalTransform()
    {
        transform.position = FinalLimitPosition;
        transform.rotation = FinalLimitRotation;
        _syncedPosition = FinalLimitPosition;
        _syncedRotation = FinalLimitRotation;
        _transformNeedsUpdate = true;
    }
}

#if UNITY_EDITOR
// 自定义编辑器
[UnityEditor.CustomEditor(typeof(GravityFacility))]
public class GravityFacilityEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GravityFacility script = (GravityFacility)target;
        
        GUILayout.Space(10);
        
        // 第一行：设置位置和旋转的按钮
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Current as Start Transform"))
        {
            script.StartLimitPosition = script.transform.position;
            script.StartLimitRotation = script.transform.rotation;
            UnityEditor.EditorUtility.SetDirty(script);
            Debug.Log($"StartLimitPosition设置为: {script.StartLimitPosition}");
            Debug.Log($"StartLimitRotation设置为: {script.StartLimitRotation}");
        }
        
        if (GUILayout.Button("Set Current as Final Transform"))
        {
            script.FinalLimitPosition = script.transform.position;
            script.FinalLimitRotation = script.transform.rotation;
            UnityEditor.EditorUtility.SetDirty(script);
            Debug.Log($"FinalLimitPosition设置为: {script.FinalLimitPosition}");
            Debug.Log($"FinalLimitRotation设置为: {script.FinalLimitRotation}");
        }
        GUILayout.EndHorizontal();
        
        // 第二行：回到位置和旋转的按钮
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Move to Start Transform"))
        {
            script.MoveToStartTransform();
            UnityEditor.EditorUtility.SetDirty(script);
            Debug.Log($"已移动到Start Transform");
        }
        
        if (GUILayout.Button("Move to Final Transform"))
        {
            script.MoveToFinalTransform();
            UnityEditor.EditorUtility.SetDirty(script);
            Debug.Log($"已移动到Final Transform");
        }
        GUILayout.EndHorizontal();
    }
}
#endif