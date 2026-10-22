using UnityEngine;
using UnityEngine.UI;

public class ProfileWindow : MonoBehaviour, IWindow
{
    [SerializeField] WindowManager manager;
    [SerializeField] Button backButton;

    public void Init()
    {
        if (manager == null || backButton == null)
        {
            Debug.LogError("ProfileWindow: заполни поля в инспекторе.", this);
            return;
        }

        backButton.onClick.AddListener(manager.Back);
    }

    void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(manager.Back);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
