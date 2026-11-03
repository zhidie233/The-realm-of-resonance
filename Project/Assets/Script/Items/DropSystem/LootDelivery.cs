using System.Collections;
using UnityEngine;
/// 此脚本处理游戏默认的空投/包裹投放
public class LootDelivery : MonoBehaviour
{
    public struct LootData
    {
        public int PackID;
        public Vector3 DropPosition;
        public float DeliveryDuration;
        public GameObject DropPrefab;
    }

    public Transform PackageHolder;
    public Transform CircleTransform;
    public GameObject DeliveryEffect;
    public TextMesh TimeText;
    public AnimationCurve DropCurve;
    public AnimationCurve LineExpandCurve;
    public AudioClip DropSound;

    private float DeliveryTime = 4;
    private GameObject InstancePrefab;
    private LineRenderer lineRender;

    /// 处理空投/包裹实例化的函数
    public void Dispatch(LootData dropData)
    {
        DeliveryTime = dropData.DeliveryDuration;
        InstancePrefab = dropData.DropPrefab;
        lineRender = GetComponent<LineRenderer>();

        //设置所有物体的初始位置
        transform.position = dropData.DropPosition + new Vector3(0, -0.1f, 0);
        PackageHolder.position = transform.position;
        PackageHolder.position += new Vector3(0, 500, 0);
        CircleTransform.localScale = Vector3.zero;
        lineRender.SetPosition(1, Vector3.zero);
        if (TimeText != null) { TimeText.gameObject.SetActive(false); }
        StartCoroutine(Delivery());
    }

    IEnumerator Delivery()
    {
        float d = 0;
        //开始动画
        while (d < 1)
        {
            d += Time.deltaTime / 2;
            float t = LineExpandCurve.Evaluate(d);
            CircleTransform.localScale = ((Vector3.one * 3) * t);
            lineRender.SetPosition(1, new Vector3(0, 500 * t, 0));
            yield return null;
        }
        d = 0;
        CircleTransform.GetComponent<Animator>().enabled = true;
        Vector3 initPosition = PackageHolder.position;
        if (TimeText != null) { TimeText.gameObject.SetActive(true); }

        //开始投放包裹
        while (d < 1)
        {
            d += Time.deltaTime / DeliveryTime;
            float t = DropCurve.Evaluate(d);
            PackageHolder.position = Vector3.Lerp(initPosition, transform.position, t);
            if (TimeText != null)
            {
                float seconds = DeliveryTime - (DeliveryTime * d);
                string ti = StringHelpers.GetTimeFormat(0, seconds);
                TimeText.text = ti;
            }
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        //实例化包裹
        if (DeliveryEffect != null) { Instantiate(DeliveryEffect, transform.position, Quaternion.identity); }
        if (InstancePrefab != null)
        {
            var drop = Instantiate(InstancePrefab, transform.position, Quaternion.identity) as GameObject;
            if (drop.TryGetComponent(out NetworkedItem networkItem))
            {
                networkItem.OnLocalInstanced();
            }
        }
        else {  }
        if (DropSound != null)
        {
            AudioSource.PlayClipAtPoint(DropSound, transform.position, 1);
        }

        Destroy(gameObject);
    }
}
