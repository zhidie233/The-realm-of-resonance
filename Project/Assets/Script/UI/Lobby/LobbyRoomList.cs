using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyRoomList : LobbyRoomListBase
{
    public ListLayoutHelper roomListHandler;
    [SerializeField] private TextMeshProUGUI NoRoomText = null;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private void Awake()
    {
        if (!roomListHandler.IsInitialize) roomListHandler.Initialize();
    }

    /// <summary>
    /// 显示所有可用房间
    /// </summary>
    public override void SetRoomList(List<RoomInfo> rooms)
    {
        //移除旧列表
        roomListHandler.Clear();
        UpdateCachedRoomList(rooms);
        InstanceRoomList();     
    }

    void InstanceRoomList()
    {
        if (cachedRoomList.Count > 0)
        {
            NoRoomText.text = string.Empty;
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                if (info.Name == MatchLobby.Instance.justCreatedRoomName) continue;

                var entry = roomListHandler.InstatiateAndGet<RoomListItemUIBase>();
                entry.SetInfo(info);
            }
        }
        else
        {
            NoRoomText.text = GameTexts.NoRoomsCreated.Localized("norooms");
        }
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // 房间被关闭、不可见或标记为删除时，从缓存的房间列表移除
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
                continue;
            }

            // 更新缓存的房间信息
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // 把新房间信息加入缓存
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    public override void ClearList()
    {
        cachedRoomList.Clear();
        roomListHandler.Clear();
    }
}