using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

   public class RoomListItemUI : RoomListItemUIBase
   {
       public TextMeshProUGUI RoomNameText = null;
       public TextMeshProUGUI MapNameText = null;
       public TextMeshProUGUI PlayersText = null;
       public TextMeshProUGUI GameModeText = null;
       public TextMeshProUGUI PingText = null;
       [SerializeField] private TextMeshProUGUI MaxKillText = null;
       public GameObject JoinButton = null;
       public GameObject FullText = null;
       [SerializeField] private GameObject PrivateUI = null;
       private RoomInfo cacheInfo = null;
       /// 此方法赋值 RoomInfo 并读取其属性
       /// 用于在 UI 中显示
       public override void SetInfo(RoomInfo info)
       {
           cacheInfo = info;
           if(RoomNameText != null)  RoomNameText.text = info.Name;
           if (MapNameText != null) MapNameText.text = (string)info.CustomProperties[NetworkKeys.CustomSceneName];
           if (GameModeText != null) GameModeText.text = (string)info.CustomProperties[NetworkKeys.GameModeKey];
           if (PlayersText != null) PlayersText.text = info.PlayerCount + "/" + info.MaxPlayers;
           if (MaxKillText != null) MaxKillText.text = string.Format("{0} {1}", info.CustomProperties[NetworkKeys.RoomGoal], info.GetGameMode().GetModeInfo().GoalName);
           if (PingText != null) PingText.text = ((int)info.CustomProperties[NetworkKeys.MaxPing]).ToString() + " ms";

           bool _active = (info.PlayerCount < info.MaxPlayers) ? true : false;
           PrivateUI.SetActive((string.IsNullOrEmpty((string)cacheInfo.CustomProperties[NetworkKeys.RoomPassword]) == false));
           JoinButton.SetActive(_active);
           FullText.SetActive(!_active);
       }
       /// 加入此行 UI 所代表的房间
       public void JoinRoom()
       {
           //本地玩家延迟高于房间允许的上限时
           if (PhotonNetwork.GetPing() >= (int)cacheInfo.CustomProperties[NetworkKeys.MaxPing])
           {
               //显示提示信息，不加入该房间
               LobbyUI.Instance.ShowPopUpWindow("max room ping");
               return;
           }

           //房间不需要密码时
           if (string.IsNullOrEmpty((string)cacheInfo.CustomProperties[NetworkKeys.RoomPassword]))
           {
               LobbyUI.Instance.blackScreenFader.FadeIn(1);
               if (cacheInfo.PlayerCount < cacheInfo.MaxPlayers)
               {
                   PhotonNetwork.JoinRoom(cacheInfo.Name);
               }
           }
           else
           {
               LobbyUI.Instance.CheckRoomPassword(cacheInfo);
           }
       }
   }
