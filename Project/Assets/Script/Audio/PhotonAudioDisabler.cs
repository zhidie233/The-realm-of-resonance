using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonAudioDisabler : MonoBehaviour
{
    public bool isGlobal = false;

    private void Awake()
    {
        PhotonAudioDisabler p = FindObjectOfType<PhotonAudioDisabler>();
        if(p != null && p != this)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        if(!GameConfig.isDataCached)
        {
            while (!GameConfig.isDataCached) { yield return null; }
        }

        if (!GameConfig.Instance.UseVoiceChat)
        {
            Destroy(gameObject);
        }
        else if (isGlobal)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}