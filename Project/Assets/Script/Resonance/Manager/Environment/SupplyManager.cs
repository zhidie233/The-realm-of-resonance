using UnityEngine;

public class SupplyManager : EnvironmentEffectManager
{
    [Header("掉落区域")]
    private BoxCollider dropArea;

    private int lastTriggeredMultiple = -1;
    private int lastIndex = 0;

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;

        GameObject dropAreaObject = GameObject.FindGameObjectWithTag("DropArea");
        if (dropAreaObject == null || dropAreaObject.GetComponent<BoxCollider>() == null)
        {
            enabled = false;
            return;
        }

        dropArea = dropAreaObject.GetComponent<BoxCollider>();
        lastTriggeredMultiple = Mathf.FloorToInt(currentResonance / GameConfig.Instance.supplyConfig.supplyInterval);
    }

    public override void UpdateEffect()
    {
        if (!NetworkWrapper.IsMasterClient) return;

        int currentMultiple = Mathf.FloorToInt(currentResonance / GameConfig.Instance.supplyConfig.supplyInterval);

        if (currentMultiple <= 0 || currentMultiple <= lastTriggeredMultiple) return;

        lastTriggeredMultiple = currentMultiple;
        DropItem();
    }

    private void DropItem()
    {
        if (dropArea == null) return;

        int[] packIDs = GameConfig.Instance.supplyConfig.supplyPackIDs;
        if (packIDs == null || packIDs.Length == 0) return;

        GameEvents.DispatchDropEvent(GetRandomPositionInDropArea(), packIDs[lastIndex]);

        lastIndex++;
        if (lastIndex >= packIDs.Length)
            lastIndex = 0;
    }

    private Vector3 GetRandomPositionInDropArea()
    {
        if (dropArea == null) return Vector3.zero;

        Vector3 localRandomPos = new Vector3(
            Random.Range(-dropArea.size.x * 0.5f, dropArea.size.x * 0.5f),
            Random.Range(-dropArea.size.y * 0.5f, dropArea.size.y * 0.5f),
            Random.Range(-dropArea.size.z * 0.5f, dropArea.size.z * 0.5f)
        );

        Vector3 worldPos = dropArea.transform.TransformPoint(localRandomPos);
        Vector3 offset = new Vector3(0, 1f, 0);
        return worldPos - offset;
    }
}
