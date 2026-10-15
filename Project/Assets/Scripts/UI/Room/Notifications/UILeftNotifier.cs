﻿using UnityEngine;
using System.Collections;
using TMPro;

namespace Game.Runtime.UI.Bindings
{
    public class UILeftNotifier : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI m_Text = null;


        public void SetInfo(string t, float time)
        {
            m_Text.text = t;
            StartCoroutine(Hide(time));
        }

        IEnumerator Hide(float t)
        {
            yield return new WaitForSeconds(t);
            gameObject.SetActive(false);
        }
    }
}