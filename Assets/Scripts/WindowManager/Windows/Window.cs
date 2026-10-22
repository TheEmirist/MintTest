using UnityEngine;

public abstract class Window : MonoBehaviour, IWindow
{
    public abstract void Init();

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}
