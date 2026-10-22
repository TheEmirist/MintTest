using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MintTest.Employees
{
    public class EmployeeCard : MonoBehaviour
    {
        [SerializeField] Button openProfileButton;
        [SerializeField] Button favoriteButton;
        [SerializeField] Image favoriteIcon;
        [SerializeField] Sprite favoriteOff;
        [SerializeField] Sprite favoriteOn;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Image picture;

        int employeeId = -1;

        public event Action<int> ProfileRequested;
        public event Action<int> FavoriteToggled;

        void Awake()
        {
            ResolveControls();

            if (openProfileButton == null)
                Debug.LogError("EmployeeCard: не задана OpenProfileButton.", this);
            else
                openProfileButton.onClick.AddListener(RequestOpenProfile);

            if (favoriteButton == null)
                Debug.LogError("EmployeeCard: не задана AddToFavoriteButton.", this);
            else
                favoriteButton.onClick.AddListener(RequestToggleFavorite);
        }

        public void Bind(EmployeeRecord record, bool favorite, Sprite avatar)
        {
            if (record == null)
                return;

            employeeId = record.id;
            ResolveControls();

            if (nameText == null || descriptionText == null)
            {
                Debug.LogError("EmployeeCard: нет NameText или DescriptionText.", this);
                return;
            }

            nameText.text = record.first_name + " " + record.last_name;
            descriptionText.text = record.email + " | " + record.ip_address;
            ApplyPicture(avatar);
            SetFavorite(favorite);
        }

        public void SetFavorite(bool favorite)
        {
            if (favoriteIcon == null)
                return;

            if (favorite && favoriteOn != null)
                favoriteIcon.sprite = favoriteOn;
            else if (!favorite && favoriteOff != null)
                favoriteIcon.sprite = favoriteOff;
        }

        void ResolveControls()
        {
            if (nameText == null || descriptionText == null)
            {
                TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (nameText == null && texts[i].name == "NameText")
                        nameText = texts[i];
                    else if (descriptionText == null && texts[i].name == "DescriptionText")
                        descriptionText = texts[i];
                }
            }

            if (favoriteButton == null)
            {
                Transform button = transform.Find("AddToFavoriteButton");
                if (button != null)
                    favoriteButton = button.GetComponent<Button>();
            }

            if (favoriteIcon == null && favoriteButton != null)
            {
                Transform icon = favoriteButton.transform.Find("FavoriteIcon");
                if (icon != null)
                    favoriteIcon = icon.GetComponent<Image>();
            }

            if (picture == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i].name == "ProfilePicture")
                    {
                        picture = images[i];
                        break;
                    }
                }
            }
        }

        void ApplyPicture(Sprite avatar)
        {
            if (picture == null)
            {
                Debug.LogError("EmployeeCard: нет ProfilePicture.", this);
                return;
            }

            Sprite next = avatar != null ? avatar : EmployeeAvatars.Placeholder;
            if (next != null)
                picture.sprite = next;
        }

        void OnDestroy()
        {
            if (openProfileButton != null)
                openProfileButton.onClick.RemoveListener(RequestOpenProfile);
            if (favoriteButton != null)
                favoriteButton.onClick.RemoveListener(RequestToggleFavorite);
        }

        void RequestOpenProfile()
        {
            if (employeeId < 0)
                return;

            ProfileRequested?.Invoke(employeeId);
        }

        void RequestToggleFavorite()
        {
            if (employeeId < 0)
                return;

            FavoriteToggled?.Invoke(employeeId);
        }
    }
}
