using System;
using UnityEngine;
using UnityEngine.UI;

public class TabWindow : Window, ISelectableWindow
{
    [SerializeField] Button tabButton;
    [SerializeField] Button otherTabButton;

    public event Action<IWindow> ShowRequested;

    public override void Init()
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

    public override void Open()
    {
        base.Open();
        tabButton.interactable = false;
        otherTabButton.interactable = true;
    }

    void RequestShow()
    {
        ShowRequested?.Invoke(this);
    }
}
