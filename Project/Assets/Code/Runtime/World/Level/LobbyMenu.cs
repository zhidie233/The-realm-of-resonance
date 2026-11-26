using UnityEngine;
using System;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.World
{

    [DefaultExecutionOrder(-800)]
    public class LobbyMenu : NetworkActorBehaviour
    {
        public enum ControlFocus
        {
            None,
            Chat,
            InputBinding
        }

        [Range(0.0f, 5)]
        public float DelayLeave = 1.5f;
        /// 玩家进入但需等待当前回合结束时的事件。
        public Action<Team> onWaitUntilRoundFinish;
        /// 当前是否锁定光标？
        public bool isCursorLocked
        {
            get;
            private set;
        }
        /// 返回当前哪部分游戏正在使用输入/控制/键盘
        public ControlFocus CurrentControlFocus
        {
            get;
            set;
        } = ControlFocus.None;
        /// 游戏当前是否显示暂停菜单？
        public bool isPaused
        {
            get => PauseMenu.IsMenuOpen;
        }
        /// 游戏是否正在退出？
        public bool isApplicationQuitting
        {
            get;
            private set;
        } = false;

        protected virtual void Awake()
        {
            if (!isConnected)
                return;

            UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
            InputHandler.CheckGamePadRequired();
        }

        protected virtual void OnEnable()
        {
            EventBus.Subscribe<LocalPlayerSpawnedEvent>(OnEvt_onLocalPlayerSpawn_OnPlayerSpawn);
            EventBus.Subscribe<LocalPlayerDiedEvent>(OnEvt_onLocalPlayerDeath_OnPlayerLocalDeath);
            NetworkCallbacks.LeftRoom += OnLeftRoom;
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe<LocalPlayerSpawnedEvent>(OnEvt_onLocalPlayerSpawn_OnPlayerSpawn);
            EventBus.Unsubscribe<LocalPlayerDiedEvent>(OnEvt_onLocalPlayerDeath_OnPlayerLocalDeath);
            NetworkCallbacks.LeftRoom -= OnLeftRoom;
        }

        void OnPlayerSpawn()
        {
            UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = true;
            GameController.Instance.IsLocalPlaying = true;
        }

        void OnPlayerLocalDeath()
        {
            UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
            GameController.Instance.IsLocalPlaying = false;
        }

        protected virtual void Update()
        {
            isCursorLocked = CommonUtil.GetCursorState;
            PauseControl();
            ScoreboardInput();
        }
        /// 检测打开/隐藏暂停菜单的输入
        void PauseControl()
        {
            // 按下左 Alt + 鼠标左键时强制锁定光标
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    CommonUtil.LockCursor(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CommonUtil.LockCursor(false);
            }

            if (CurrentControlFocus == ControlFocus.InputBinding) return;

            if (GameInput.Pause()) TogglePause();
        }
        /// 切换当前暂停状态
        public void TogglePause()
        {
            if (!GameController.Instance.FirstSpawnDone || GameController.Instance.GameFinish) return;

            bool paused = isPaused;
            paused = !paused;
            UIReferences.Instance.ShowMenu(paused);
            CommonUtil.LockCursor(!paused);
            ReticlePainter.Instance?.Show(!paused);
            EventBus.Publish(new GamePausedEvent(paused));
        }

        void ScoreboardInput()
        {
            if (GameController.Instance.GameFinish || PauseMenu.IsMenuOpen) return;

            if (GameInput.Scoreboard())
            {
                PauseMenu.Instance.SetActiveLayouts(PauseMenu.LayoutPart.Body);
                PauseMenu.Instance.OpenWindow("scoreboard");
            }
            else if (GameInput.Scoreboard(GameInputType.Up))
            {
                PauseMenu.Instance.CloseWindow("scoreboard");
            }
        }
        /// 玩家将被自动分配到队伍时触发
        public void OnAutoTeam()
        {
            CommonUtil.LockCursor(true);
            GameController.Instance.IsLocalPlaying = true;
        }

        public void JoinTeam(int id)
        {
            Team team = (Team)id;
            string tn = team.GetTeamName();
            string joinText = isOneTeamMode ? GameTexts.JoinedInMatch : GameTexts.JoinIn;

            if (isOneTeamMode)
            {
                KillReportEntry.Instance.SendMessageEvent(string.Format("{0} {1}", NetworkWrapper.NickName, joinText));
            }
            else
            {
                string jt = string.Format("{0} {1}", joinText, tn);
                KillReportEntry.Instance.SendTeamHighlightMessage(NetworkWrapper.NickName, jt, team);
            }
            CommonUtil.LockCursor(true);
            //若玩家只在新回合开始时出生
            if (GetGameMode.GetGameModeInfo().spawnOnRoundBegin == GameModeSettings.OnRoundBegin.WaitUntilRoundFinish && GameController.Instance.GameMatchState == MatchState.Playing)
            {
                //订阅回合开始事件
                if (onWaitUntilRoundFinish != null) { onWaitUntilRoundFinish.Invoke(team); }
                GameController.Instance.SetLocalPlayerToTeam(team);
                return;
            }
            //将玩家设为所选队伍并出生
            GameController.Instance.SpawnPlayer(team);
        }
        /// 离开当前房间（若存在）并返回大厅
        public void LeaveRoom(bool save = true)
        {
            //适合在重置统计数据前保存信息
            if (NetworkWrapper.IsConnected && NetworkWrapper.InRoom)
            {
                NetworkWrapper.CleanBuffer();
                NetworkWrapper.LeaveRoom();
            }
            else
            {
    #if UNITY_EDITOR
                if (isApplicationQuitting) { return; }
    #endif
                CommonUtil.LoadLevel(ProjectConfigRoot.Instance.MainMenuScene);
            }
        }
        /// 服务器收到离开房间请求时调用。
        public void OnLeftRoom()
        {
            LobbyCamera.Instance?.SetActive(true);
            NetworkWrapper.IsMessageQueueRunning = false;
            MatchTimeManager.Instance.enabled = false;
            if (UIReferences.Instance != null)
                StartCoroutine(UIReferences.Instance.FinalFade(true));
        }

        void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

        private static LobbyMenu _instance;
        public static LobbyMenu Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<LobbyMenu>(); }
                return _instance;
            }
        }

        private void OnEvt_onLocalPlayerSpawn_OnPlayerSpawn(LocalPlayerSpawnedEvent e) => OnPlayerSpawn();
        private void OnEvt_onLocalPlayerDeath_OnPlayerLocalDeath(LocalPlayerDiedEvent e) => OnPlayerLocalDeath();
    }
}
