using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    const string EmployeeResource = "test_task_mock_data";

    [SerializeField] List<Window> windows;
    [SerializeField] EmployeeListView employeeList;

    IWindow current;
    IWindow overlay;
    IPopupWindow popup;

    void Awake()
    {
        if (!Collect())
            return;

        BindEmployees();
        for (int i = 0; i < windows.Count; i++)
            windows[i].Close();

        Show(windows[0]);
    }

    bool Collect()
    {
        if (windows == null || windows.Count == 0 || employeeList == null)
        {
            Debug.LogError("WindowManager: заполни поля в инспекторе.", this);
            return false;
        }

        for (int i = 0; i < windows.Count; i++)
        {
            if (windows[i] == null)
            {
                Debug.LogError("WindowManager: пустой элемент списка окон.", this);
                return false;
            }
        }

        for (int i = 0; i < windows.Count; i++)
        {
            Window window = windows[i];
            window.Init();

            if (window is IPopupWindow found)
                popup = found;
        }

        if (popup == null)
        {
            Debug.LogError("WindowManager: в списке нет окна профиля.", this);
            return false;
        }

        return true;
    }

    void BindEmployees()
    {
        if (!JsonEmployeeRepository.TryLoad(EmployeeResource, out JsonEmployeeRepository repository))
        {
            Debug.LogError("WindowManager: не прочитал test_task_mock_data.", this);
            repository = JsonEmployeeRepository.Empty;
        }

        popup.SetLookup(repository);
        employeeList.SetCatalog(repository);
    }

    void OnEnable()
    {
        SetListening(true);
    }

    void OnDisable()
    {
        SetListening(false);
    }

    void SetListening(bool enabled)
    {
        if (windows != null)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                Window window = windows[i];
                if (window is ISelectableWindow selectable)
                {
                    if (enabled)
                        selectable.ShowRequested += Show;
                    else
                        selectable.ShowRequested -= Show;
                }

                if (window is IPopupWindow popupWindow)
                {
                    if (enabled)
                        popupWindow.BackRequested += Back;
                    else
                        popupWindow.BackRequested -= Back;
                }
            }
        }

        if (employeeList == null)
            return;

        if (enabled)
            employeeList.PopupRequested += ShoPopup;
        else
            employeeList.PopupRequested -= ShoPopup;
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

    public void ShowPopup(IWindow window)
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

    void ShoPopup(int id)
    {
        if (popup == null || !popup.Show(id))
            return;

        ShowPopup(popup);
    }

    void CloseOverlay()
    {
        if (overlay == null)
            return;

        overlay.Close();
        overlay = null;
    }
}
