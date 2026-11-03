using UnityEngine;

public class KillReportUI : MonoBehaviour
{
    private static KillReportUI _instance;
    public static KillReportUI Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<KillReportUI>(); }
            return _instance;
        }
    }

    public int numberOfPooledPrefabs = 6;
    public Transform KillfeedPanel;
    public GameObject KillfeedPrefab;

    private KillReportBinding[] pool;
    private int currentPooled = 0;

    private void Start()
    {
        PreparePool();
    }

    void PreparePool()
    {
        if (pool != null) return;

        pool = new KillReportBinding[numberOfPooledPrefabs];
        for (int i = 0; i < numberOfPooledPrefabs; i++)
        {
            var obj = Instantiate(KillfeedPrefab) as GameObject;
            obj.transform.SetParent(KillfeedPanel, false);
            pool[i] = obj.GetComponent<KillReportBinding>();
            obj.SetActive(false);
        }
        KillfeedPrefab.SetActive(false);
    }
    /// 击杀他人时在右上角弹出全局提示
    public void SetKillFeed(KillReport feed)
    {
        if (!UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.KillReport)) return;

        if (pool == null) PreparePool();

        var newkillfeed = pool[currentPooled];
        newkillfeed.Init(feed);
        newkillfeed.transform.SetParent(KillfeedPanel, false);
        newkillfeed.transform.SetAsFirstSibling();

        currentPooled = (currentPooled + 1) % numberOfPooledPrefabs;
    }
}
