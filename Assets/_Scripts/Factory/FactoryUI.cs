using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class FactoryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text currentStoredText;
    [SerializeField] private TMP_Text productionQueueText;
    [SerializeField] private TMP_Text remainingTimeText;
    [SerializeField] private Slider remainingTimeSlider;
    [SerializeField] private GameObject productionButtonsParent;
    [SerializeField] private Button productionButton;
    [SerializeField] private Button removeProductionButton;

    public event Action OnAddProductionOrder;
    public event Action OnRemoveProductionOrder;

    private void Awake()
    {
        if (productionButton != null)
            productionButton.onClick.AddListener(() => OnAddProductionOrder?.Invoke());

        if (removeProductionButton != null)
            removeProductionButton.onClick.AddListener(() => OnRemoveProductionOrder?.Invoke());
    }

    public void UpdateUI(int currentStored, int productionQueue, int capacity, ResourceType requiredResource, int requiredAmount, int availableResources)
    {
        // Update texts
        currentStoredText.text = $"{currentStored}";
        productionQueueText.text = $"{productionQueue}/{capacity}";
        remainingTimeText.text = currentStored == capacity ? "Full" : "Idle";

        // Update buttons
        if (productionButton != null)
            productionButton.interactable = productionQueue < capacity
                && productionQueue + currentStored < capacity
                && availableResources >= requiredAmount;     

        if (removeProductionButton != null)
            removeProductionButton.interactable = productionQueue > 0;
    }

    public async UniTask UpdateSlider(float duration, CancellationToken ct)
    {
        float timeRemaining = duration;
        remainingTimeSlider.value = 1;

        while (timeRemaining > 0)
        {
            if (ct.IsCancellationRequested)
                return;

            timeRemaining -= Time.deltaTime;
            remainingTimeSlider.value = timeRemaining / duration;
            remainingTimeText.text = $"{Mathf.CeilToInt(timeRemaining)}s";
            await UniTask.Yield();
        }

        remainingTimeSlider.value = 0;
    }

    public void ToggleProductionButtons(bool active)
    {
        if (productionButtonsParent != null)
            productionButtonsParent.SetActive(active);
    }

    public bool IsButtonsActive()
    {
        return productionButtonsParent.activeSelf;
    }
}