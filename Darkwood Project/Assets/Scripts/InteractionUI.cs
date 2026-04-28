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

    private IInteractable myInteractable;

    void Start()
    {
        player = PlayerController.Instance;
        myInteractable = GetComponent<IInteractable>();

        if (dragSlider != null)
        {
            dragSlider.minValue = 0f;
            dragSlider.maxValue = 1f;
        }
    }

    void Update()
    {
        bool isHovered = (player.GetHoveredObject() == gameObject);

        bool isBeingDragged = (player.GetActiveDraggable() == myInteractable);

        if (!isHovered && !isBeingDragged)
        {
            uiContainer.SetActive(false);
            return;
        }

        uiContainer.SetActive(true);

        if (isBeingDragged)
        {
            mainText.text = "[E] stop";
            if (dragSlider != null) dragSlider.gameObject.SetActive(false);
            return;
        }

        float progress = player.GetHoldProgress();

        if (progress > 0.15f)
        {
            mainText.text = "[E] ";
            if (dragSlider != null)
            {
                dragSlider.gameObject.SetActive(true);
                dragSlider.value = (progress - 0.15f) / 0.85f;
            }
        }
        else
        {
            mainText.text = "[E] light";
            if (dragSlider != null) dragSlider.gameObject.SetActive(false);
        }
    }
}