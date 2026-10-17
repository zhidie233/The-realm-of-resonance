using UnityEngine;
using System.Collections;

public class ItemFloater : MonoBehaviour
{

    public float amount = 1.0f;
    public float speed = 4.0f;

    private Vector3 originPosition, targetPosition;

    private void Awake()
    {
        originPosition = transform.localPosition;
        targetPosition = originPosition + new Vector3(0, amount, 0);
    }

    void OnEnable()
    {
        StartCoroutine(StartIE());
    }

    IEnumerator StartIE()
    {
        while (true)
        {
            yield return StartCoroutine(MoveObject(transform, originPosition, targetPosition, speed));
            yield return StartCoroutine(MoveObject(transform, targetPosition, originPosition, speed));
        }
    }

    IEnumerator MoveObject(Transform thisTransform, Vector3 startPos, Vector3 endPos, float time)
    {
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            thisTransform.localPosition = Vector3.Lerp(startPos, endPos, i);
            yield return null;
        }
    }
}