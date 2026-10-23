using UnityEngine;
using UnityEngine.UI;
using TMPro;

   public class CaptureOfFlagUI : MonoBehaviour
   {
       public GameObject Content;
       public TextMeshProUGUI Team1ScoreText, Team2ScoreText;
       public Image FlagImg1, FlagImg2;

       public void SetScores(int team1, int team2)
       {
           Team1ScoreText.text = team1.ToString();
           Team2ScoreText.text = team2.ToString();
       }

       public void ShowUp()
       {
           if (UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.TopScoreBoard))
               Content.SetActive(true);

           Team1ScoreText.color = Team.Team1.GetTeamColor();
           Team2ScoreText.color = Team.Team2.GetTeamColor();
           FlagImg1.color = Team.Team1.GetTeamColor();
           FlagImg2.color = Team.Team2.GetTeamColor();
       }

       public void Hide()
       {
           Content.SetActive(false);
       }

       private static CaptureOfFlagUI _instance;
       public static CaptureOfFlagUI Instance
       {
           get
           {
               if (_instance == null)
               {
                   _instance = FindObjectOfType<CaptureOfFlagUI>();
               }
               return _instance;
           }
       }
   }