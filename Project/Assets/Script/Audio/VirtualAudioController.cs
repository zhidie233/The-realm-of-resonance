using System.Collections.Generic;
using UnityEngine;

   [System.Serializable]
   public class VirtualAudioController
   {

       [ScriptableDrawer] public AudioBankData audioBank;
       public int numberOfAudioInstances = 10;

       private List<AudioInstance> audioInstances = new List<AudioInstance>();

       private bool isInit = false;
       private int currentInstance = 0;

       public void Initialized(MonoBehaviour parent)
       {
           if (isInit || audioBank == null) return;

           GameObject g = new GameObject("Audio List");
           Transform root = g.transform;
           root.parent = parent.transform;
           root.localPosition = Vector3.zero;
           root.localEulerAngles = Vector3.zero;

           g = new GameObject("Audio Instance");
           g.transform.parent = root;
           AudioInstance ai = g.AddComponent<AudioInstance>();
           ai.Init();
           audioInstances.Add(ai);
           g.SetActive(false);
           ai.ID = 0;

           var template = g;

           for (int i = 1; i < numberOfAudioInstances; i++)
           {
               g = GameObject.Instantiate(template) as GameObject;
               g.transform.parent = root;
               ai = g.AddComponent<AudioInstance>();
               ai.ID = i;
               ai.Init();
               g.SetActive(false);
               audioInstances.Add(ai);
           }
           isInit = true;
       }

       public AudioInstance PlayClip(string audioName)
       {
           if (audioBank == null || audioBank.AudioBank == null) return null;

           int index = audioBank.AudioBank.FindIndex(x => x.Name.ToLower() == audioName.ToLower());
           if (index < 0) { Debug.LogWarning("音频片段 " + audioName + " 未找到"); return null; }
           return PlayClip(index);
       }

       public AudioInstance PlayClip(int id)
       {
           if (audioBank == null || audioBank.AudioBank == null) return null;

           AudioBankData.AudioInfo info = audioBank.AudioBank[id];
           AudioInstance ai = audioInstances[currentInstance];
           if (ai.isPlaying)
           {
               ai = null;
               for (int i = 0; i < audioInstances.Count; i++)
               {
                   if (!audioInstances[i].isPlaying) { ai = audioInstances[i]; currentInstance = i; break; }
               }
           }
           if (ai == null) { ai = CreateNewInstance(); }
           ai.gameObject.SetActive(true);
           ai.SetInstance(info);
           return ai;
       }

       public void StopClip(string audioName)
       {
           for (int i = 0; i < audioInstances.Count; i++)
           {
               if (audioInstances[i].isPlayingClip(audioName))
               {
                   audioInstances[i].Stop();
                   break;
               }
           }
       }

       public AudioBankData.AudioInfo GetAudioInfo(string name)
       {
           if (audioBank == null) return null;
           return audioBank.AudioBank.Find(x => x.Name == name);
       }

       public AudioBankData.AudioInfo GetAudioInfo(int indexID)
       {
           if (audioBank == null) return null;
           return audioBank.AudioBank[indexID];
       }

       public AudioInstance CreateNewInstance()
       {
           if (!isInit) { return null; }

           GameObject g = GameObject.Instantiate(audioInstances[0].gameObject) as GameObject;
           g.transform.parent = audioInstances[0].transform.parent;
           AudioInstance ai = g.GetComponent<AudioInstance>();
           audioInstances.Add(ai);
           ai.Init();
           ai.ID = audioInstances.Count;
           return ai;
       }
   }
