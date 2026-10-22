using System.Collections.Generic;
using UnityEngine;

public class AICoverSpot : MonoBehaviour
{
    [Tooltip("机器人到达该掩体点时是否蹲下")]
    [GameToggle] public bool Crouch = false;
    public float lastUseTime { get; set; } = 0;
    public List<AICoverSpot> NeighbordPoints = new List<AICoverSpot>();

    private bool m_positionCached = false;
    private Vector3 _position;
    public Vector3 Position
    {
        get
        {
            if (!m_positionCached)
            {
                _position = ThisTransform.position;
                m_positionCached = true;
            }
            return _position;
        }
    }

    private Transform _transform;
    public Transform ThisTransform
    {
        get
        {
            if (_transform == null) _transform = transform;
            return _transform;
        }
    }

    private void Awake()
    {
        AICoverSpotManager.Register(this);
    }

    public bool IsAvailable(float timeSpan)
    {
        return (Time.time - lastUseTime) > timeSpan;
    }

    public AICoverSpot TryGetAvailableNeighbord()
    {
        if (NeighbordPoints == null || NeighbordPoints.Count <= 0) return null;

        for (int i = 0; i < NeighbordPoints.Count; i++)
        {
            if (NeighbordPoints[i] == null) continue;

            if (NeighbordPoints[i].IsAvailable(AICoverSpotManager.Instance.UsageTime))
                return NeighbordPoints[i];
        }
        return null;
    }

    public bool HasNeighbords() => NeighbordPoints != null && NeighbordPoints.Count > 0;

    private void OnDrawGizmos()
    {
        if(AICoverSpotManager.Instance == null)
        {
            return;
        }
        if (!AICoverSpotManager.Instance.ShowGizmos)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1);
        Gizmos.DrawCube(transform.position, new Vector3(1, 0.1f, 1));

        Gizmos.color = Color.red;
        if (NeighbordPoints.Count > 0)
        {
            for (int i = 0; i < NeighbordPoints.Count; i++)
            {
                if (NeighbordPoints[i] == null) continue;
                Gizmos.DrawLine(transform.position, NeighbordPoints[i].transform.position);
            }
        }
    }
}
