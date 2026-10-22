using UnityEngine;
using UnityEngine.UI;

public class EmployeeCard : MonoBehaviour
{
    [SerializeField] Button openProfileButton;

    void Awake()
    {
        if (openProfileButton == null)
        {
            Debug.LogError("EmployeeCard: не задана OpenProfileButton.", this);
            return;
        }

        openProfileButton.onClick.AddListener(RequestOpenProfile);
    }

    void OnDestroy()
    {
        if (openProfileButton != null)
            openProfileButton.onClick.RemoveListener(RequestOpenProfile);
    }

    void RequestOpenProfile()
    {
        OpenProfile.Raise();
    }
}
