using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Runtime.Level
{
    public class DoorCoordinator : MonoBehaviourBase
    {
        public List<DoorCore> allDoors = new List<DoorCore>();
        private List<DoorCore> toUpdateDoors = new List<DoorCore>();

        protected override void OnEnable()
        {
            base.OnEnable();
            NetworkCallbacks.PlayerEnteredRoom += OnNewPlayerEnter;
            NetworkWrapper.AddNetworkCallback(NetworkKeys.DoorSystem, OnNetworkMessage);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            NetworkCallbacks.PlayerEnteredRoom -= OnNewPlayerEnter;
            NetworkWrapper.RemoveNetworkCallback(OnNetworkMessage);
        }

        public override void OnUpdate()
        {
            UpdateDoors();
        }

        void OnNetworkMessage(ExitGames.Client.Photon.Hashtable data)
        {
            int command = (int)data["cmd"];
            switch (command)
            {
                case 0:
                    PrepareAllDoors((string)data["data"]);
                    break;
                case 1:
                    SentDoorStateLocally(data);
                    break;
            }
        }

        public static void SentDoorStateToAll(DoorCore door, DoorCore.State newState)
        {
            if(door.DoorID == -1)
            {
                door.DoorID = Instance.allDoors.FindIndex(x => x == door);
            }

            if (door.DoorID == -1)
            {
                Debug.LogWarning("This door is not listed in DoorCoordinator -> All Doors.", door);
                return;
            }

            var data = CommonUtil.CreatePhotonHashTable();
            data.Add("cmd", 1);
            data.Add("id", door.DoorID);
            data.Add("state", newState);

            NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.DoorSystem, data);
        }

        public void SentDoorStateLocally(ExitGames.Client.Photon.Hashtable data)
        {
            int doorId = (int)data["id"];
            var state = (DoorCore.State)data["state"];

            allDoors[doorId].SetDoorState(state);
        }

        /// <summary>
        /// 仅在需要时更新门
        /// 门会动态加入更新列表，以便只在需要时更新
        /// 从而让地图容纳大量门而不带来性能开销。
        /// </summary>
        void UpdateDoors()
        {
            if (toUpdateDoors == null || toUpdateDoors.Count <= 0) return;

            int count = toUpdateDoors.Count;
            for (int i = 0; i < count; i++)
            {
                toUpdateDoors[i].OnUpdateDoor();
            }
        }

        public static void AddUpdateDoor(DoorCore door)
        {
            if (Instance == null) return;

            if (Instance.toUpdateDoors.Exists(x => x == door)) return;

            Instance.toUpdateDoors.Add(door);
        }

        public static void RemoveUpdateDoor(DoorCore door)
        {
            if (Instance == null) return;

            int index = Instance.toUpdateDoors.FindIndex(x => x == door);
            if (index == -1) return;

            Instance.toUpdateDoors.RemoveAt(index);
        }

        void PrepareAllDoors(string compiledData)
        {
            string[] doorStates = compiledData.Split('|');
            for (int i = 0; i < doorStates.Length; i++)
            {
                if (string.IsNullOrEmpty(doorStates[i])) continue;
                if (allDoors[i] == null) continue;

                var state = (DoorCore.State)(byte)int.Parse(doorStates[i]);
                allDoors[i].SetDoorStateInstantly(state);
            }
        }

        void OnNewPlayerEnter(Player newPlayer)
        {
            if (!NetworkWrapper.IsMasterClient) return;

            string line = "";
            for (int i = 0; i < allDoors.Count; i++)
            {
                if (allDoors[i] == null) continue;

                line += $"{(byte)allDoors[i].DoorState}|";
            }

            var data = CommonUtil.CreatePhotonHashTable();
            data.Add("cmd", 0);
            data.Add("data", line);

            NetworkWrapper.Instance.SendDataOverNetworkToPlayer(NetworkKeys.DoorSystem, data, newPlayer);
        }

        private static DoorCoordinator _instance;
        public static DoorCoordinator Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<DoorCoordinator>();
                return _instance;
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(DoorCoordinator))]
    public class DoorCoordinatorEditor : Editor
    {
        DoorCoordinator script;
        private void OnEnable()
        {
            script = (DoorCoordinator)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            if (GUILayout.Button("Collect all active doors in scene"))
            {
                var all = FindObjectsOfType<DoorCore>();
                script.allDoors.Clear();
                script.allDoors.AddRange(all);

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}