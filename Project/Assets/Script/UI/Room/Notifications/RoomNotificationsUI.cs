using UnityEngine;

public class RoomNotificationsUI : MonoBehaviour
{
    public ListLayoutHelper listHandler;
    public float showTime = 5;

    private void OnEnable()
    {
        listHandler.Prefab.SetActive(false);
        GameEvents.onLocalNotification += OnLocalNotification;
    }

    private void OnDisable()
    {
        GameEvents.onLocalNotification -= OnLocalNotification;
    }

    void OnLocalNotification(GameLocalNotification notification)
    {
        listHandler.Initialize();
        listHandler.InstantiateAndGet<UILeftNotifier>().SetInfo(notification.Message, showTime);
    }
}