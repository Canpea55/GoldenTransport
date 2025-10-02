using UnityEngine;

public abstract class CanvasController : MonoBehaviour
{
    public virtual void OnCanvasLoaded()
    {
        //Debug.Log("Canvas Loadded!");
    }

    public virtual void OnCanvasUnloaded()
    {
        //Debug.Log("Canvas Unnnnnloadded!");
    }

    public virtual void OnReceiveData(object data) 
    { 
    }

    public virtual void Submit()
    {
    }
}
