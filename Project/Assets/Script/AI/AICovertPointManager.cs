using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AICovertPointManager : MonoBehaviour
{
    public float MaxDistance = 50;
    public float UsageTime = 10;
    public float maxNeighbordDistance = 25;
    [GameToggle] public bool onlyAllowOneBotPerCover = true;
    [GameToggle] public bool ShowGizmos = true;

    public static List<AICoverPoint> AllCovers = new List<AICoverPoint>();

    private void OnDestroy()
    {
        //这是静态属性，卸载场景后要记得清理。
        AllCovers.Clear();
    }

    public static void Register(AICoverPoint co)
    {
        AllCovers.Add(co);
    }

    public AICoverPoint GetCloseCover(Transform target)
    {
        if (AllCovers == null || AllCovers.Count <= 0)
        {
            Debug.LogWarning("There is no Cover Points for bots in this scene, bots behave will be limited.");
            return null;
        }

        AICoverPoint cover = null;
        float d = MaxDistance;
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = UtilityHelper.Distance(target.localPosition, AllCovers[i].Position);
            if (dis < MaxDistance && dis < d)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }
        cover = CheckCoverUsage(cover);
        return cover;
    }

    public AICoverPoint GetCoverOnRadius(Transform target, float radius)
    {
        if (AllCovers == null || AllCovers.Count <= 0)
        {
            Debug.LogWarning("There is no Cover Points for bots in this scene, bots behave will be limited.");
            return null;
        }

        List<AICoverPoint> list = new List<AICoverPoint>();
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = UtilityHelper.Distance(target.localPosition, AllCovers[i].Position);
            if (dis <= radius)
            {
                list.Add(AllCovers[i]);
            }
        }
        AICoverPoint cp = null;
        if (list.Count > 0)
        {
            cp = list[Random.Range(0, list.Count)];
        }
        if (cp == null) { cp = AllCovers[Random.Range(0, AllCovers.Count)]; }

        return cp;
    }

    public AICoverPoint GetCloseCoverForced(Transform target)
    {
        if (AllCovers == null || AllCovers.Count <= 0)
        {
            Debug.LogWarning("There is no Cover Points for bots in this scene, bots behave will be limited.");
            return null;
        }

        AICoverPoint cover = null;
        float d = 100000;
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = UtilityHelper.Distance(target.localPosition, AllCovers[i].Position);
            if (dis < d)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }
        cover = CheckCoverUsage(cover);
        return cover;
    }

    public AICoverPoint GetCloseCover(Transform target, AICoverPoint overrdidePoint)
    {
        AICoverPoint cover = null;
        float d = MaxDistance;
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = UtilityHelper.Distance(target.localPosition, AllCovers[i].Position);
            if (dis < MaxDistance && dis < d && AllCovers[i] != overrdidePoint)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }
        cover = CheckCoverUsage(cover);
        return cover;
    }

    public AICoverPoint CheckCoverUsage(AICoverPoint coverSource)
    {
        if (coverSource == null) return null;

        //如果这个掩体最近被用过，就换一个
        if ((Time.time - coverSource.lastUseTime) <= UsageTime)
        {
            if (coverSource.HasNeighbords())
            {
                var neighbord = coverSource.TryGetAvailableNeighbord();
                if (neighbord == null)
                    neighbord = coverSource.NeighbordPoints[Random.Range(0, coverSource.NeighbordPoints.Count)];

                coverSource = neighbord;
            }
            else
            {
                if (onlyAllowOneBotPerCover) coverSource = null;
            }
        }

        if (coverSource == null) return null;

        coverSource.lastUseTime = Time.time;
        return coverSource;
    }

    private static AICovertPointManager _instance;
    public static AICovertPointManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<AICovertPointManager>(); }
            return _instance;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Fix Points")]
    public void FixedFloorPos()
    {
        AICoverPoint[] sp = FindObjectsOfType<AICoverPoint>();
        RaycastHit r;
        for (int i = 0; i < sp.Length; i++)
        {
            Transform t = sp[i].transform;
            Ray ray = new Ray(t.position + t.up, Vector3.down);
            if (Physics.Raycast(ray, out r, 100))
            {
                t.position = r.point;
            }
        }
    }

    [ContextMenu("Calculate Neighbors")]
    public void CalcuNeighbords()
    {
        AICoverPoint[] allCovers = FindObjectsOfType<AICoverPoint>();
        for (int i = 0; i < allCovers.Length; i++)
        {
            Transform t = allCovers[i].transform;
            allCovers[i].NeighbordPoints.Clear();
            for (int e = 0; e < allCovers.Length; e++)
            {
                if (allCovers[i] == allCovers[e]) continue;
                if (Vector3.Distance(allCovers[i].transform.position, allCovers[e].transform.position) <= maxNeighbordDistance)
                {
                    allCovers[i].NeighbordPoints.Add(allCovers[e]);
                }
            }
            UnityEditor.EditorUtility.SetDirty(allCovers[i]);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AICovertPointManager))]
public class AICovertPointManagerEditor : Editor
{
    AICovertPointManager script;

    private void OnEnable()
    {
        script = (AICovertPointManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);

        if (GUILayout.Button("Bake Neighbors points"))
        {
            script.CalcuNeighbords();
        }
        if (GUILayout.Button("Align points to floors"))
        {
            script.FixedFloorPos();
        }
    }

}
#endif