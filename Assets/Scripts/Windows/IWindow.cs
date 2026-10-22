using System;
using MintTest.Employees;

namespace MintTest.Windows
{
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

        void SetAvatars(IEmployeeAvatars source);

        bool Show(int id);
    }
}
