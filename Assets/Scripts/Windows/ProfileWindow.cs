using System;
using MintTest.Employees;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MintTest.Windows
{
    public class ProfileWindow : MonoBehaviour, IPopupWindow
    {
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text genderText;
        [SerializeField] TMP_Text emailText;
        [SerializeField] TMP_Text ipText;
        [SerializeField] Image avatarImage;

        IEmployeeLookup lookup;
        IEmployeeAvatars avatars;
        int shownId = -1;

        public event Action BackRequested;

        public void SetLookup(IEmployeeLookup source)
        {
            lookup = source;
        }

        public void SetAvatars(IEmployeeAvatars source)
        {
            if (avatars != null)
                avatars.Changed -= OnAvatarsChanged;

            avatars = source;

            if (avatars != null)
                avatars.Changed += OnAvatarsChanged;
        }

        public bool Show(int id)
        {
            if (lookup == null)
            {
                Debug.LogError("ProfileWindow: не задан справочник сотрудников.", this);
                return false;
            }

            if (!lookup.TryGet(id, out EmployeeRecord record) || record == null)
            {
                Debug.LogError("ProfileWindow: нет записи " + id + ".", this);
                return false;
            }

            if (nameText == null || genderText == null || emailText == null || ipText == null)
            {
                Debug.LogError("ProfileWindow: не все тексты профиля найдены.", this);
                return false;
            }

            nameText.text = record.first_name + " " + record.last_name;
            genderText.text = "Gender: <color=#1A1F32><b>" + record.gender + "</b></color>";
            emailText.text = record.email;
            ipText.text = record.ip_address;
            shownId = id;
            ApplyPicture(id);
            return true;
        }

        public void Init()
        {
            if (backButton == null)
            {
                Debug.LogError("ProfileWindow: заполни поля в инспекторе.", this);
                return;
            }

            backButton.onClick.AddListener(RequestBack);
        }

        void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(RequestBack);
            if (avatars != null)
                avatars.Changed -= OnAvatarsChanged;
        }

        void OnAvatarsChanged()
        {
            if (!isActiveAndEnabled || shownId < 0)
                return;

            ApplyPicture(shownId);
        }

        void ApplyPicture(int id)
        {
            if (avatarImage == null)
            {
                Debug.LogError("ProfileWindow: не задан Avatar.", this);
                return;
            }

            Sprite sprite = avatars == null ? null : avatars.SpriteFor(id);
            Sprite next = sprite != null ? sprite : EmployeeAvatars.Placeholder;
            if (next != null)
                avatarImage.sprite = next;
        }

        void RequestBack()
        {
            BackRequested?.Invoke();
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
}
