using System.Collections.Generic;
using UnityEngine;
using System;

public class ViewRay : ViewRayBase
{
    #region Public members
    public int CheckFrameRate = 5;
    public CastMethod castMethod = CastMethod.Box;
    [Range(0.1f, 10)] public float DistanceCheck = 2;
    public LayerMask DetectLayers;
    public Vector3 boxDimesion = new Vector3(0.15f, 0.15f, 0.1f);
    #endregion

    #region Public properties

    private bool isChecking = false;
    public override bool IsCurrentlyDetecting
    { 
        get => isChecking;
        set
        {
            isChecking = value;
            if (!isChecking && m_currentDetectedItem != null)
            {
                m_currentDetectedItem.OnUnDetectedByPlayer();
            }
        } 
    }
    #endregion

    #region Private members
    private int currentFrame = 0;
    private RaycastHit RayHit;
    private IRayDetectable m_currentDetectedItem = null;
    private List<byte> activers = new List<byte>();
    private Dictionary<string, Action<bool>> triggers = new Dictionary<string, Action<bool>>();
    bool hasDectected = false;
    private byte increaseCounter = 0;
    #endregion

    public override void OnUpdate()
    {
        if (!IsCurrentlyDetecting) { currentFrame = 0; return; }

        if(currentFrame == 0)
        {
            Fire();
        }
        currentFrame = (currentFrame + 1) % CheckFrameRate;
    }

    void Fire()
    {
        bool detected = false;
        if (castMethod == CastMethod.Box || castMethod == CastMethod.Both)
        {
            detected = Physics.BoxCast(CachedTransform.position, boxDimesion, CachedTransform.forward,
                out RayHit, CachedTransform.rotation, RayDistance, DetectLayers, QueryTriggerInteraction.Ignore);
        }
        if((castMethod == CastMethod.Ray || castMethod == CastMethod.Both) && !detected)
        {
            Ray r = new Ray(CachedTransform.position, CachedTransform.forward);
            detected = Physics.Raycast(r, out RayHit, RayDistance, DetectLayers, QueryTriggerInteraction.Ignore);
        }

        if (detected)
        {
            hasDectected = true;
            OnHit();

            //逐个检查注册的触发器
            if (triggers.Count > 0)
            {
                foreach (var item in triggers.Keys)
                {
                    //如果前方物体的名字与注册触发器相同，就调用它的回调
                    if (RayHit.transform.name == item)
                    {
                        triggers[item].Invoke(true);
                    }
                }
            }
        }
        else
        {
            //如果玩家之前聚焦着某个物体，现在不再聚焦
            UnDetectCurrentItem();

            if (triggers.Count > 0 && hasDectected)
            {
                foreach (var item in triggers.Values)
                {
                    item.Invoke(false);
                }
            }
            hasDectected = false;
        }
    }

    /// <summary>
    /// 玩家相机射线击中地图上的物体时
    /// </summary>
    void OnHit()
    {
        var item = RayHit.transform.GetComponent<IRayDetectable>();
        if (item != null)
        {
            //如果之前聚焦的是另一个物体
            if (m_currentDetectedItem != null && m_currentDetectedItem != item)
            {
                //通知那个物体它不再被聚焦了。
                m_currentDetectedItem.OnUnDetectedByPlayer();
            }

            m_currentDetectedItem = item;
            m_currentDetectedItem.OnRayDetectedByPlayer();
        }
        else
        {
            UnDetectCurrentItem();
        }
    }

    public void UnDetectCurrentItem()
    {
        if (m_currentDetectedItem == null) return;

        m_currentDetectedItem.OnUnDetectedByPlayer();
        m_currentDetectedItem = null;
    }

    /// <summary>
    /// 想检测物体是否出现在本地玩家视野前方时
    /// 在这个函数里注册一个回调
    /// </summary>
    public override byte AddTrigger(DetecableInfo info)
    {
        if (triggers.ContainsKey(info.Name)) { return 0; }

        triggers.Add(info.Name, info.Callback);
        return SetActiver(true, info.ID);
    }

    /// <summary>
    /// 不再需要检测时记得移除触发器。
    /// </summary>
    public override void RemoveTrigger(DetecableInfo info)
    {
        if (!triggers.ContainsKey(info.Name)) return;
        triggers.Remove(info.Name);
        SetActiver(false, info.ID);
    }

    public override byte SetActiver(bool add, byte id)
    {
        if (add)
        {
            if (!activers.Contains(id))
            {
                activers.Add(id);
            }
            else
            {
                increaseCounter++;
                id = increaseCounter;
                activers.Add(id);
            }
            IsCurrentlyDetecting = true;
        }
        else
        {
            if (activers.Contains(id))
            {
                activers.Remove(id);
            }
            if (activers.Count <= 0)
            {
                IsCurrentlyDetecting = false;
            }
        }
        return id;
    }

    private float RayDistance => DistanceCheck + ExtraRayDistance;

    [Serializable]
    public enum CastMethod
    {
        Ray,
        Box,
        Both,
    }
}