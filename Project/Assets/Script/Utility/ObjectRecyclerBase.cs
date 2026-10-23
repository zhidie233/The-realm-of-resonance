using UnityEngine;

public abstract class ObjectRecyclerBase : MonoBehaviour
{
    /// 获取一个池化预制体实例。
    public abstract GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation);

    private static ObjectRecyclerBase _instance;
    public static ObjectRecyclerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<ObjectRecyclerBase>(); }
            return _instance;
        }
    }
}
