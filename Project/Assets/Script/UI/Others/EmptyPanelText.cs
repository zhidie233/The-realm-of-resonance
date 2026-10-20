using System.Collections;
using System.Collections.Generic;
using UnityEngine;

   public class EmptyPanelText : MonoBehaviour
   {
       private void Start()
       {
           Check();
       }

       private void OnTransformParentChanged()
       {
           Debug.Log("change");
       }

       public void Check()
       {
           var childs = transform.parent.childCount;
           if(childs <= 1)
           {
               gameObject.SetActive(true);
           }
           else
           {
               gameObject.SetActive(false);
           }
       }
   }