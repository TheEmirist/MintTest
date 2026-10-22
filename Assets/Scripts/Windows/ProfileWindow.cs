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

        IEmployeeLookup lookup;

        public event Action BackRequested;

        public void SetLookup(IEmployeeLookup source)
        {
            lookup = source;
        }

        public bool Show(int id)
        {
            ResolveTexts();

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

        void ResolveTexts()
        {
            if (nameText != null && genderText != null && emailText != null && ipText != null)
                return;

            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                switch (texts[i].name)
                {
                    case "NameText":
                        if (nameText == null)
                            nameText = texts[i];
                        break;
                    case "GenderText":
                        if (genderText == null)
                            genderText = texts[i];
                        break;
                    case "EmailValue":
                        if (emailText == null)
                            emailText = texts[i];
                        break;
                    case "PhoneValue":
                        if (ipText == null)
                            ipText = texts[i];
                        break;
                }
            }
        }
    }
}
