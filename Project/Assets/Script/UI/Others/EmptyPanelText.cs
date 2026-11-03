using UnityEngine;

public class EmptyPanelText : MonoBehaviour
{
    private void Start()
    {
        Check();
    }

    private void OnTransformParentChanged()
    {
    }

    public void Check()
    {
        var children = transform.parent.childCount;
        if(children <= 1)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
