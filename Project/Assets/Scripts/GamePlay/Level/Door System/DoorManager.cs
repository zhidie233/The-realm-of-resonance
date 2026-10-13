using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GFWK.Runtime.Level
{
    public class DoorManager : MonoBehaviourBase
    {
        public List<DoorBase> allDoors = new List<DoorBase>();
        private List<DoorBase> toUpdateDoors = new List<DoorBase>();

        /// <summary>
        /// 
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            PhotonCallbacks.PlayerEnteredRoom += OnNewPlayerEnter;
            PhotonNetworkWrapper.AddNetworkCallback(PropertiesKeys.DoorSystem, OnNetworkMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            PhotonCallbacks.PlayerEnteredRoom -= OnNewPlayerEnter;
            PhotonNetworkWrapper.RemoveNetworkCallback(OnNetworkMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnUpdate()
        {
            UpdateDoors();
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public static void SentDoorStateToAll(DoorBase door, DoorBase.State newState)
        {
            if(door.DoorID == -1)
            {
                door.DoorID = Instance.allDoors.FindIndex(x => x == door);
            }

            if (door.DoorID == -1)
            {
                Debug.LogWarning("This door is not listed in DoorManager -> All Doors.", door);
                return;
            }

            var data = UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 1);
            data.Add("id", door.DoorID);
            data.Add("state", newState);

            PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.DoorSystem, data);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SentDoorStateLocally(ExitGames.Client.Photon.Hashtable data)
        {
            int doorId = (int)data["id"];
            var state = (DoorBase.State)data["state"];

            allDoors[doorId].SetDoorState(state);
        }

        /// <summary>
        /// Update doors only when needed
        /// Doors are dynamically added to the updated list to update them only when is needed
        /// in order to allow many doors per map without performance cost.
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

        /// <summary>
        /// 
        /// </summary>
        public static void AddUpdateDoor(DoorBase door)
        {
            if (Instance == null) return;

            if (Instance.toUpdateDoors.Exists(x => x == door)) return;

            Instance.toUpdateDoors.Add(door);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="door"></param>
        public static void RemoveUpdateDoor(DoorBase door)
        {
            if (Instance == null) return;

            int index = Instance.toUpdateDoors.FindIndex(x => x == door);
            if (index == -1) return;

            Instance.toUpdateDoors.RemoveAt(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compiledData"></param>
        void PrepareAllDoors(string compiledData)
        {
            string[] doorStates = compiledData.Split('|');
            for (int i = 0; i < doorStates.Length; i++)
            {
                if (string.IsNullOrEmpty(doorStates[i])) continue;
                if (allDoors[i] == null) continue;

                var state = (DoorBase.State)(byte)int.Parse(doorStates[i]);
                allDoors[i].SetDoorStateInstantly(state);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnNewPlayerEnter(Player newPlayer)
        {
            if (!PhotonNetworkWrapper.IsMasterClient) return;

            string line = "";
            for (int i = 0; i < allDoors.Count; i++)
            {
                if (allDoors[i] == null) continue;

                line += $"{(byte)allDoors[i].DoorState}|";
            }

            var data = UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 0);
            data.Add("data", line);

            PhotonNetworkWrapper.Instance.SendDataOverNetworkToPlayer(PropertiesKeys.DoorSystem, data, newPlayer);
        }

        private static DoorManager _instance;
        public static DoorManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<DoorManager>();
                return _instance;
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(DoorManager))]
    public class DoorManagerEditor : Editor
    {
        DoorManager script;
        private void OnEnable()
        {
            script = (DoorManager)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            if (GUILayout.Button("Collect all active doors in scene"))
            {
                var all = FindObjectsOfType<DoorBase>();
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