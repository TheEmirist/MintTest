using UnityEngine;
using UnityEngine.UI;

public class FavoritesWindow : MonoBehaviour, IWindow
{
    [SerializeField] WindowManager manager;
    [SerializeField] Button tabButton;
    [SerializeField] Button otherTabButton;

    public void Init()
    {
        if (manager == null || tabButton == null || otherTabButton == null)
        {
            Debug.LogError("FavoritesWindow: заполни поля в инспекторе.", this);
            return;
        }

        tabButton.onClick.AddListener(Show);
    }

    void OnDestroy()
    {
        if (tabButton != null)
            tabButton.onClick.RemoveListener(Show);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        tabButton.interactable = false;
        otherTabButton.interactable = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void Show()
    {
        manager.Show(this);
    }
}
