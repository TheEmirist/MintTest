using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmployeeCard : MonoBehaviour
{
    [SerializeField] Button openProfileButton;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text descriptionText;

    int employeeId = -1;

    public event Action<int> ProfileRequested;

    void Awake()
    {
        ResolveTexts();

        if (openProfileButton == null)
        {
            Debug.LogError("EmployeeCard: не задана OpenProfileButton.", this);
            return;
        }

        openProfileButton.onClick.AddListener(RequestOpenProfile);
    }

    public void Bind(EmployeeRecord record)
    {
        if (record == null)
            return;

        employeeId = record.id;
        ResolveTexts();

        if (nameText == null || descriptionText == null)
        {
            Debug.LogError("EmployeeCard: нет NameText или DescriptionText.", this);
            return;
        }

        nameText.text = record.first_name + " " + record.last_name;
        descriptionText.text = record.email + " | " + record.ip_address;
    }

    void ResolveTexts()
    {
        if (nameText != null && descriptionText != null)
            return;

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (nameText == null && texts[i].name == "NameText")
                nameText = texts[i];
            else if (descriptionText == null && texts[i].name == "DescriptionText")
                descriptionText = texts[i];
        }
    }

    void OnDestroy()
    {
        if (openProfileButton != null)
            openProfileButton.onClick.RemoveListener(RequestOpenProfile);
    }

    void RequestOpenProfile()
    {
        if (employeeId < 0)
            return;

        ProfileRequested?.Invoke(employeeId);
    }
}
