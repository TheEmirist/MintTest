using System;
using System.Collections.Generic;
using MintTest.Employees;
using UnityEngine;

namespace MintTest.Windows
{
    public class WindowManager : MonoBehaviour
    {
        const string EmployeeResource = "test_task_mock_data";

        [SerializeField] List<WindowSlot> windows;
        [SerializeField] EmployeeListView employeeList;
        [SerializeField] EmployeeListView favoritesList;

        readonly List<IWindow> windowsList = new List<IWindow>();
        IWindow current;
        IWindow overlay;
        IPopupWindow popup;

        void Awake()
        {
            if (!Collect())
                return;

            BindEmployees();
            for (int i = 0; i < windowsList.Count; i++)
                windowsList[i].Close();

            Show(windowsList[0]);
        }

        bool Collect()
        {
            if (windows == null || windows.Count == 0 || employeeList == null || favoritesList == null)
            {
                Debug.LogError("WindowManager: заполни поля в инспекторе.", this);
                return false;
            }

            windowsList.Clear();

            for (int i = 0; i < windows.Count; i++)
            {
                WindowSlot slot = windows[i];
                IWindow window = slot == null ? null : slot.ToWindow();
                if (window == null)
                {
                    Debug.LogError("WindowManager: элемент списка не реализует IWindow.", this);
                    windowsList.Clear();
                    return false;
                }

                windowsList.Add(window);
            }

            for (int i = 0; i < windowsList.Count; i++)
            {
                windowsList[i].Init();

                if (windowsList[i] is IPopupWindow found)
                    popup = found;
            }

            if (popup == null)
            {
                Debug.LogError("WindowManager: в списке нет окна профиля.", this);
                windowsList.Clear();
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
            FavoriteStore favorites = FavoriteStore.Load();
            employeeList.Bind(repository, favorites, false);
            favoritesList.Bind(repository, favorites, true);
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
            for (int i = 0; i < windowsList.Count; i++)
            {
                IWindow window = windowsList[i];
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

            if (employeeList != null)
            {
                if (enabled)
                    employeeList.PopupRequested += ShoPopup;
                else
                    employeeList.PopupRequested -= ShoPopup;
            }

            if (favoritesList != null)
            {
                if (enabled)
                    favoritesList.PopupRequested += ShoPopup;
                else
                    favoritesList.PopupRequested -= ShoPopup;
            }
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

    [Serializable]
    public sealed class WindowSlot
    {
        [SerializeField] MonoBehaviour source;

        public IWindow ToWindow()
        {
            return source as IWindow;
        }
    }
}
