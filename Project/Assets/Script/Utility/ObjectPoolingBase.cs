using UnityEngine;

public abstract class ObjectPoolingBase : MonoBehaviour
{

    /// <summary>
    /// 获取一个池化预制体实例。
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public abstract GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation);

    private static ObjectPoolingBase _instance;
    public static ObjectPoolingBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<ObjectPoolingBase>(); }
            return _instance;
        }
    }
}