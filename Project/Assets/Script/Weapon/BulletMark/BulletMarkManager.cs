using UnityEngine;

public class BulletMarkManager : MonoBehaviour
{
    public static void InstantiateDecal(RaycastHit raycastHit)
    {
        if (Instance == null) return;

        Instance.InstanceDecal(raycastHit);
    }

    private static BulletMarkManager _instance;
    public static BulletMarkManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<BulletMarkManager>(); }
            return _instance;
        }
    }

    public int maxDecalInstances = 100;
    public BulletMark decalPrefab;
    [ScriptableDrawer] public BulletMarkList decalList;

    private BulletMark[] decalPool;
    private int currentPool = -1;
    private Vector3 decalBaseScale = Vector3.one;
    private bool showDecals = true;

    private void Start()
    {
        Init();
    }
    /// 在射线检测命中点生成弹痕
    public void InstanceDecal(RaycastHit raycastHit)
    {
        if (raycastHit.transform == null || !showDecals) return;

        Init();

        if (!showDecals) return;

        var decalInstance = GetPool();

        // 弹痕挂在碰撞体上，碰撞体销毁时弹痕也会被销毁
        // 所以必须确认它不为空，为空就替换掉这个弹痕。
        if(decalInstance == null)
        {
            decalPool[currentPool] = Instantiate(decalPrefab.gameObject).GetComponent<BulletMark>();
            decalPool[currentPool].Init(transform);
            decalInstance = decalPool[currentPool];
        }

        var decalData = decalList.GetDecalForTag(raycastHit.transform);

        decalInstance
            .SetDecalMaterial(decalData.GetMaterial())
            .SetToHit(raycastHit, true)
            .SetScaleVariation(decalBaseScale, decalData.SizeRange);
    }

    public BulletMark GetPool()
    {
        currentPool = (currentPool + 1) % maxDecalInstances;
        return decalPool[currentPool];
    }
    /// 初始化弹痕池列表
    private void Init()
    {
        if (decalPool != null) return;

        showDecals = GameConfig.Instance.impactDecals;

        if (!showDecals) return;

        decalBaseScale = decalPrefab.transform.localScale;

        decalPool = new BulletMark[maxDecalInstances];
        for (int i = 0; i < maxDecalInstances; i++)
        {
            decalPool[i] = Instantiate(decalPrefab.gameObject).GetComponent<BulletMark>();
            decalPool[i].Init(transform);
        }
    }
}
