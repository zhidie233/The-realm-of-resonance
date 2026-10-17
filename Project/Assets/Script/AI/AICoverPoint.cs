using System.Collections.Generic;
using UnityEngine;

public class AICoverPoint : MonoBehaviour
{
    [Tooltip("Should the bot crouch when reach this cover point?")]
    [GameToggle] public bool Crouch = false;
    public float lastUseTime { get; set; } = 0;
    public List<AICoverPoint> NeighbordPoints = new List<AICoverPoint>();

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
        AICovertPointManager.Register(this);
    }

    public bool IsAvailable(float timeSpan)
    {
        return (Time.time - lastUseTime) > timeSpan;
    }

    public AICoverPoint TryGetAvailableNeighbord()
    {
        if (NeighbordPoints == null || NeighbordPoints.Count <= 0) return null;

        for (int i = 0; i < NeighbordPoints.Count; i++)
        {
            if (NeighbordPoints[i] == null) continue;

            if (NeighbordPoints[i].IsAvailable(AICovertPointManager.Instance.UsageTime))
                return NeighbordPoints[i];
        }
        return null;
    }

    public bool HasNeighbords() => NeighbordPoints != null && NeighbordPoints.Count > 0;

    private void OnDrawGizmos()
    {
        if(AICovertPointManager.Instance == null)
        {
            return;
        }
        if (!AICovertPointManager.Instance.ShowGizmos)
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