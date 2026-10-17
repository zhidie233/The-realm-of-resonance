using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;

namespace Game.Runtime.FriendList
{
    public class FriendListUI : FriendListUIBase
    {
        [Header("Settings")]
        public bool SortOnline = true;
        public float minifiedWidth = 30;
        public float expandWidth = 200;

        [Header("References")]
        public GameObject FriendUIPrefab = null;
        [SerializeField] private FriendInfoBase emptyRow = null;
        public Transform PanelList = null;
        public TextMeshProUGUI FriendsCountText = null;
        [SerializeField] private Image ArrowImage = null;
        [SerializeField] private Sprite UpArrowSprite = null;
        [SerializeField] private Sprite BottomArrowSprite = null;
        [SerializeField] private TextMeshProUGUI LogText = null;
        [SerializeField] private TextMeshProUGUI ConnectFriendsText = null;
        public GameObject Panel;
        [SerializeField] private RectTransform windowRect = null;
        public GameObject[] expandObjects;
        public AnimationCurve transitionCurve;

        private List<FriendInfoBase> cacheFriendsInfo = new List<FriendInfoBase>();
        private int OnlineCount = 0;
        private bool onlineFirst = false;
        private bool isExpand = false;

        void Awake()
        {
            onlineFirst = SortOnline;
            LogText.text = string.Empty;
            ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
            FriendUIPrefab.SetActive(false);
            UpdateFriendList(true);
            if (LogText != null)
            {
                LogText.canvasRenderer.SetAlpha(0);
            }
            if (!PhotonNetwork.IsConnected)
            {
                Panel.SetActive(false);
            }
            windowRect.sizeDelta = new Vector2(isExpand ? expandWidth : minifiedWidth, windowRect.sizeDelta.y);
            foreach (var item in expandObjects)
            {
                if (item == null) continue;
                item.SetActive(isExpand);
            }
            if (emptyRow != null) emptyRow.Expand(isExpand);
        }

        public override bool IsOpen()
        {
            return isExpand;
        }

        /// <summary>
        /// 好友面板里每个好友对应的 UI 实例
        /// </summary>
        /// <param name="friends"></param>
        public void InstanceFriendList(string[] friends)
        {
            for (int i = 0; i < friends.Length; i++)
            {
                GameObject f = Instantiate(FriendUIPrefab) as GameObject;
                f.SetActive(true);
                f.transform.SetParent(PanelList, false);
            }
        }

        /// <summary>
        /// 每次好友列表更新时调用
        /// 更新好友列表期间 Photon 会临时把
        // isOnline 和 isInRoom 置为 false
        // 如果按定时器更新，就会看到状态在
        // 离线和在线之间频繁切换
        // 因此把在线状态和房间先存进一个
        // 字典，等到真正收到更新时
        // 再记录更新后的值
        /// </summary>
        public void OnUpdatedFriendList()
        {
            if (!PhotonNetwork.IsConnected || PhotonNetwork.InRoom)
            {
                return;
            }

            if (cacheFriendsInfo.Count <= 0 || FriendListBase.Instance.GetStatus() == FriendListBase.Status.Fetching)
            {
                UpdateFriendList(true);
            }
            else
            {
                UpdateFriendList();
            }
        }

        public override void UpdateFriendList(bool instance = false)
        {
            if (FriendListBase.Instance.FriendsCount <= 0 || PhotonNetwork.InRoom)
            {
                return;
            }

            FriendInfo[] friends = FriendListBase.Instance.GetFriends();
            if (emptyRow != null) emptyRow.SetActive(friends.Length <= 0);
            if (friends.Length <= 0) return;

            if (instance)
            {
                CleanCacheList();
                List<FriendInfo> OnlineFriends = new List<FriendInfo>();
                List<FriendInfo> OfflineFriends = new List<FriendInfo>();

                for (int i = 0; i < friends.Length; i++)
                {
                    if (friends[i].UserId == "Null") continue;

                    if (onlineFirst)
                    {
                        if (friends[i].IsOnline)
                        {
                            OnlineFriends.Add(friends[i]);
                        }
                        else
                        {
                            OfflineFriends.Add(friends[i]);
                        }
                    }
                    else
                    {
                        GameObject f = Instantiate(FriendUIPrefab) as GameObject;
                        f.SetActive(true);
                        var info = f.GetComponent<FriendInfoBase>();
                        info.SetFriendData(friends[i], this);
                        f.name = friends[i].UserId;
                        cacheFriendsInfo.Add(info);

                        f.transform.SetParent(PanelList, false);
                    }
                }

                if (onlineFirst)
                {
                    if (OnlineFriends.Count > 0)
                    {
                        InstanceFriend(OnlineFriends.ToArray());
                    }
                    if (OfflineFriends.Count > 0)
                    {
                        InstanceFriend(OfflineFriends.ToArray());
                    }
                }
                ConnectFriendsText.text = OnlineFriends.Count.ToString();

            }
            else//Just update list
            {
                for (int i = 0; i < cacheFriendsInfo.Count; i++)
                {
                    if (cacheFriendsInfo[i] != null)
                    {
                        cacheFriendsInfo[i].UpdateFriendInfo(friends);
                    }
                    else
                    {
                        cacheFriendsInfo.RemoveAt(i);
                    }
                }
                if (emptyRow != null) emptyRow.SetActive(cacheFriendsInfo.Count <= 0);
            }
            UpdateCount(friends);
        }

        void InstanceFriend(FriendInfo[] friends)
        {
            for (int i = 0; i < friends.Length; i++)
            {
                GameObject f = Instantiate(FriendUIPrefab) as GameObject;
                f.SetActive(true);
                var info = f.GetComponent<FriendInfoBase>();
                info.SetFriendData(friends[i], this);
                cacheFriendsInfo.Add(info);
                f.name = friends[i].UserId;
                f.transform.SetParent(PanelList, false);
            }
        }

        void UpdateCount(FriendInfo[] friends)
        {
            OnlineCount = 0;
            foreach (FriendInfo info in friends)
            {
                if (info.IsOnline)
                {
                    OnlineCount++;
                }
            }
            if (FriendsCountText != null)
            {
                FriendsCountText.text = OnlineCount + "/" + friends.Length;
            }
        }

        void CleanCacheList()
        {
            if (cacheFriendsInfo.Count > 0)
            {
                for (int i = 0; i < cacheFriendsInfo.Count; i++)
                {
                    Destroy(cacheFriendsInfo[i].gameObject);
                }
            }
            cacheFriendsInfo.Clear();
        }

        public void SetOnlineFirst(bool first)
        {
            onlineFirst = first;
            ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
            UpdateFriendList(true);
        }

        public void ChangeOnlineFirst()
        {
            onlineFirst = !onlineFirst;
            ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
            UpdateFriendList(true);
        }

        public void ToggleView()
        {
            isExpand = !isExpand;
            ExpandView(isExpand);
        }

        public void ExpandView(bool expand)
        {
            isExpand = expand;
            StopCoroutine(nameof(DoExpand));
            StartCoroutine(nameof(DoExpand));
            foreach (var item in cacheFriendsInfo)
            {
                if (item == null) continue;
                item.Expand(isExpand);
            }
            foreach (var item in expandObjects)
            {
                if (item == null) continue;
                item.SetActive(expand);
            }
            if (emptyRow != null) emptyRow.Expand(isExpand);
        }

        IEnumerator DoExpand()
        {
            float d = 0;
            var origin = windowRect.sizeDelta;
            var width = isExpand ? expandWidth : minifiedWidth;
            var target = new Vector2(width, origin.y);
            float t = 0;

            while(d < 1)
            {
                d += Time.deltaTime / 0.3f;
                t = transitionCurve.Evaluate(d);
                windowRect.sizeDelta = Vector2.Lerp(origin, target, t);
                yield return null;
            }
        }

        public override void ShowMessage(string log)
        {
            CancelInvoke(nameof(HideLog));
            if (LogText != null)
            {
                LogText.text = log;
                LogText.CrossFadeAlpha(1, 0.7f, true);
            }
            Invoke(nameof(HideLog), 3);
        }

        void HideLog()
        {
            if (LogText != null)
            {
                LogText.CrossFadeAlpha(0, 1, true);
            }
        }

        public override void SetActiveList(bool active)
        {
            Panel.SetActive(active);
        }

        public override bool IsPlayerListed(string fname)
        {
            for (int i = 0; i < cacheFriendsInfo.Count; i++)
            {
                if (cacheFriendsInfo[i].name == fname)
                {
                    return true;
                }
            }
            return false;

        }
    }
}