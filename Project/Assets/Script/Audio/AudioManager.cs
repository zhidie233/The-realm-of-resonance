using System.Collections;
using UnityEngine;

   public class AudioManager : MonoBehaviour
   {
       [Header("Audio Handler")]
       public VirtualAudioController audioController;

       [Header("Scene Settings")]
       public float maxWeaponDistance = 75;
       public float maxExplosionDistance = 100;
       public float maxFootstepDistance = 30;
       public AudioRolloffMode audioRolloffMode = AudioRolloffMode.Logarithmic;

       [Header("Background")]
       [SerializeField] private AudioClip BackgroundClip;
       public float MaxBackgroundVolume = 0.3f;
       public AudioSource backgroundSource;

       private void Start()
       {
           audioController.Initialized(this);
       }

       private void OnEnable()
       {
           MaxBackgroundVolume = GameCore.MusicVolume;
       }

       public void PlayClip(string clipName)
       {
           audioController.PlayClip(clipName);
       }

       public void PlayBackground()
       {
           if (BackgroundClip == null) return;
           if (backgroundSource == null) { backgroundSource = gameObject.AddComponent<AudioSource>(); }

           backgroundSource.clip = BackgroundClip;
           backgroundSource.volume = 0;
           backgroundSource.playOnAwake = false;
           backgroundSource.loop = true;
           StartCoroutine(FadeAudio(backgroundSource, true, MaxBackgroundVolume));
       }

       public void StopBackground()
       {
           if (backgroundSource == null) return;

           FadeAudio(backgroundSource, false);
       }

       public void ForceStopAllFades()
       {
           StopAllCoroutines();
       }

       IEnumerator FadeAudio(AudioSource source, bool up, float volume = 1)
       {
           if (up)
           {
               source.Play();
               while (source.volume < volume)
               {
                   source.volume += Time.deltaTime * 0.01f;
                   yield return null;
               }
           }
           else
           {
               while (source.volume > 0)
               {
                   source.volume -= Time.deltaTime * 0.5f;
                   yield return null;
               }
           }
       }

       public float BackgroundVolume
       {
           set
           {
               if (backgroundSource != null) { backgroundSource.volume = value; }
           }
       }


       private static AudioManager _instance;
       public static AudioManager Instance
       {
           get
           {
               if (_instance == null)
               {
                   _instance = FindObjectOfType<AudioManager>();
               }
               return _instance;
           }
       }
   }