using System;

public interface IWindow
{
    void Init();
    void Open();
    void Close();
}

public interface ISelectableWindow : IWindow
{
    event Action<IWindow> ShowRequested;
}

public interface IPopupWindow : IWindow
{
    event Action BackRequested;

    void SetLookup(IEmployeeLookup source);

    bool Show(int id);
}
