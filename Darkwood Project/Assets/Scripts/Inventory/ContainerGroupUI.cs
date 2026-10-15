using TMPro;
using UnityEngine;

public class ContainerGroupUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI headerText;
    public Transform pocketsContainer;

    public void Initialize(string groupName)
    {
        if (headerText != null)
        {
            headerText.text = groupName;
        }
    }
}