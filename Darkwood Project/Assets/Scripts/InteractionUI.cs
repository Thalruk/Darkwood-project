using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(IInteractable))]
public class InteractionUI : MonoBehaviour
{
    [Header("Referencje")]
    private PlayerController player;
    [SerializeField] private GameObject uiContainer;

    [Header("Elementy UI")]
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private Slider dragSlider;

    void Start()
    {
        player = PlayerController.Instance;

        if (dragSlider != null)
        {
            dragSlider.minValue = 0f;
            dragSlider.maxValue = 1f;
        }
    }

    void Update()
    {
        string targetName = player.GetLookingAtObjectName();

        if (!player.IsDraggingObject() && string.IsNullOrEmpty(targetName))
        {
            uiContainer.SetActive(false);
            return;
        }

        uiContainer.SetActive(true);

        if (player.IsDraggingObject())
        {
            mainText.text = "[E] stop";
            dragSlider.gameObject.SetActive(false);
            return;
        }

        float progress = player.GetHoldProgress();

        if (progress > 0.15f)
        {
            mainText.text = "[E] ";
            dragSlider.gameObject.SetActive(true);

            dragSlider.value = (progress - 0.15f) / 0.85f;
        }
        else
        {
            mainText.text = "[E] light";
            dragSlider.gameObject.SetActive(false);
        }
    }
}