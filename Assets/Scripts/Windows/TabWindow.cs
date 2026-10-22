using System;
using UnityEngine;
using UnityEngine.UI;

namespace MintTest.Windows
{
    public class TabWindow : MonoBehaviour, ISelectableWindow
    {
        [SerializeField] Button tabButton;
        [SerializeField] Button otherTabButton;

        public event Action<IWindow> ShowRequested;

        public void Init()
        {
            if (tabButton == null || otherTabButton == null)
            {
                Debug.LogError("TabWindow: заполни поля в инспекторе.", this);
                return;
            }

            tabButton.onClick.AddListener(RequestShow);
        }

        void OnDestroy()
        {
            if (tabButton != null)
                tabButton.onClick.RemoveListener(RequestShow);
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

        void RequestShow()
        {
            ShowRequested?.Invoke(this);
        }
    }
}
