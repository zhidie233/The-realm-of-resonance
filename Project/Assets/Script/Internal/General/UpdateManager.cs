﻿using UnityEngine;

namespace Game.Internal
{
    public class UpdateManager : MonoBehaviour
    {
        public float SlowUpdateTime = 0.5f;
        private int regularUpdateArrayCount = 0;
        private int fixedUpdateArrayCount = 0;
        private int lateUpdateArrayCount = 0;
        private int slowUpdateArrayCount = 0;
        private MonoBehaviourBase[] regularArray = new MonoBehaviourBase[0];
        private MonoBehaviourBase[] fixedArray = new MonoBehaviourBase[0];
        private MonoBehaviourBase[] lateArray = new MonoBehaviourBase[0];
        private MonoBehaviourBase[] slowArray = new MonoBehaviourBase[0];
        private bool Initializate = false;
        private float lastSlowCall = 0;

        /// <summary>
        /// 
        /// </summary>
        public static void AddItem(MonoBehaviourBase behaviour)
        {
            if (instance == null) return;
            instance.AddItemToArray(behaviour);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            Initializate = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="behaviour"></param>
        public static void RemoveSpecificItem(MonoBehaviourBase behaviour)
        {
            if (instance != null)
            {
                instance.RemoveSpecificItemFromArray(behaviour);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="behaviour"></param>
        public static void RemoveSpecificItemAndDestroyIt(MonoBehaviourBase behaviour)
        {
            instance.RemoveSpecificItemFromArray(behaviour);

            Destroy(behaviour.gameObject);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
            regularArray = new MonoBehaviourBase[0];
            fixedArray = new MonoBehaviourBase[0];
            lateArray = new MonoBehaviourBase[0];
            slowArray = new MonoBehaviourBase[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="behaviour"></param>
        private void AddItemToArray(MonoBehaviourBase behaviour)
        {
            if (behaviour.GetType().GetMethod("OnUpdate").DeclaringType != typeof(MonoBehaviourBase))
            {
                regularArray = ExtendAndAddItemToArray(regularArray, behaviour);
                regularUpdateArrayCount++;
            }

            if (behaviour.GetType().GetMethod("OnFixedUpdate").DeclaringType != typeof(MonoBehaviourBase))
            {
                fixedArray = ExtendAndAddItemToArray(fixedArray, behaviour);
                fixedUpdateArrayCount++;
            }

            if (behaviour.GetType().GetMethod("OnSlowUpdate").DeclaringType != typeof(MonoBehaviourBase))
            {
                slowArray = ExtendAndAddItemToArray(slowArray, behaviour);
                slowUpdateArrayCount++;
            }

            if (behaviour.GetType().GetMethod("OnLateUpdate").DeclaringType == typeof(MonoBehaviourBase))
                return;

            lateArray = ExtendAndAddItemToArray(lateArray, behaviour);
            lateUpdateArrayCount++;
        }

        private int size = 0;
        public MonoBehaviourBase[] ExtendAndAddItemToArray(MonoBehaviourBase[] original, MonoBehaviourBase itemToAdd)
        {
            size = original.Length;
            MonoBehaviourBase[] finalArray = new MonoBehaviourBase[size + 1];
            for (int i = 0; i < size; i++)
            {
                finalArray[i] = original[i];
            }
            finalArray[finalArray.Length - 1] = itemToAdd;
            return finalArray;
        }

        private void RemoveSpecificItemFromArray(MonoBehaviourBase behaviour)
        {
            if (CheckIfArrayContainsItem(regularArray, behaviour))
            {
                regularArray = ShrinkAndRemoveItemToArray(regularArray, behaviour);
                regularUpdateArrayCount--;
            }

            if (CheckIfArrayContainsItem(fixedArray, behaviour))
            {
                fixedArray = ShrinkAndRemoveItemToArray(fixedArray, behaviour);
                fixedUpdateArrayCount--;
            }

            if (CheckIfArrayContainsItem(slowArray, behaviour))
            {
                slowArray = ShrinkAndRemoveItemToArray(slowArray, behaviour);
                slowUpdateArrayCount--;
            }

            if (!CheckIfArrayContainsItem(lateArray, behaviour)) return;

            lateArray = ShrinkAndRemoveItemToArray(lateArray, behaviour);
            lateUpdateArrayCount--;
        }

        public bool CheckIfArrayContainsItem(MonoBehaviourBase[] arrayToCheck, MonoBehaviourBase objectToCheckFor)
        {
            int size = arrayToCheck.Length;

            for (int i = 0; i < size; i++)
            {
                if (objectToCheckFor == arrayToCheck[i]) return true;
            }

            return false;
        }

        public MonoBehaviourBase[] ShrinkAndRemoveItemToArray(MonoBehaviourBase[] original, MonoBehaviourBase itemToRemove)
        {
            int size = original.Length;
            int fix = 0;
            MonoBehaviourBase[] finalArray = new MonoBehaviourBase[size - 1];
            for (int i = 0; i < size; i++)
            {
                if (original[i] != itemToRemove)
                {
                    finalArray[i - fix] = original[i];
                }
                else
                {
                    fix++;
                }
            }
            return finalArray;
        }

        private void Update()
        {
            if (!Initializate)
                return;

            SlowUpdate();
            if (regularUpdateArrayCount == 0) return;

            for (int i = 0; i < regularUpdateArrayCount; i++)
            {
                if (regularArray[i] != null && regularArray[i].enabled)
                {
                    regularArray[i].OnUpdate();
                }
            }
        }

        void SlowUpdate()
        {
            if (slowUpdateArrayCount == 0) return;
            if ((Time.time - lastSlowCall) < SlowUpdateTime) return;

            lastSlowCall = Time.time;
            for (int i = 0; i < slowUpdateArrayCount; i++)
            {
                if (slowArray[i] != null && slowArray[i].enabled)
                {
                    slowArray[i].OnSlowUpdate();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!Initializate)
                return;

            if (fixedUpdateArrayCount == 0) return;

            for (int i = 0; i < fixedUpdateArrayCount; i++)
            {
                if (fixedArray[i] == null || !fixedArray[i].enabled) continue;

                fixedArray[i].OnFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            if (!Initializate)
                return;

            if (lateUpdateArrayCount == 0) return;

            for (int i = 0; i < lateUpdateArrayCount; i++)
            {
                if (lateArray[i] == null || !lateArray[i].enabled) continue;

                lateArray[i].OnLateUpdate();
            }
        }

        private static UpdateManager _instance;
        public static UpdateManager instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<UpdateManager>(); }
                return _instance;
            }
        }
    }
}