using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField] MainWindow mainWindow;
    [SerializeField] FavoritesWindow favoritesWindow;
    [SerializeField] ProfileWindow profileWindow;

    IWindow current;
    IWindow overlay;

    void Awake()
    {
        if (mainWindow == null || favoritesWindow == null || profileWindow == null)
        {
            Debug.LogError("WindowManager: заполни окна в инспекторе.", this);
            return;
        }

        mainWindow.Init();
        favoritesWindow.Init();
        profileWindow.Init();
        favoritesWindow.Close();
        profileWindow.Close();
        Show(mainWindow);
    }

    void OnEnable()
    {
        OpenProfile.Raised += ShowProfile;
    }

    void OnDisable()
    {
        OpenProfile.Raised -= ShowProfile;
    }

    public void Show(IWindow window)
    {
        if (window == null)
            return;

        CloseOverlay();

        if (current == window)
        {
            window.Open();
            return;
        }

        current?.Close();
        window.Open();
        current = window;
    }

    public void ShowOver(IWindow window)
    {
        if (window == null || overlay == window)
            return;

        CloseOverlay();
        current?.Close();
        window.Open();
        overlay = window;
    }

    public void Back()
    {
        if (overlay == null)
            return;

        CloseOverlay();
        current?.Open();
    }

    void ShowProfile()
    {
        ShowOver(profileWindow);
    }

    void CloseOverlay()
    {
        if (overlay == null)
            return;

        overlay.Close();
        overlay = null;
    }
}
