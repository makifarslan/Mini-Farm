using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using UniRx;

public abstract class Factory : MonoBehaviour
{
    [Header("Production Settings")]
    [SerializeField] protected float productionTime;
    [SerializeField] protected int capacity;
    [SerializeField] protected int requiredResourceAmount = 1;
    [SerializeField] protected ResourceType requiredResource;

    protected int currentStored = 0;
    protected int productionQueue = 0;
    protected CancellationTokenSource productionCTS;

    [Header("UI")]
    [SerializeField] private TMP_Text currentStoredText;
    [SerializeField] private TMP_Text productionQueueText;
    [SerializeField] private TMP_Text remainingTimeText;
    [SerializeField] private Slider remainingTimeSlider;
    [SerializeField] private GameObject productionButtonsParent;
    [SerializeField] private Button productionButton;
    [SerializeField] private Button removeProductionButton;

    public abstract ResourceType producedResource { get; }

    protected virtual void Start()
    {
        productionCTS = new CancellationTokenSource();

        if (productionButton != null) productionButton.onClick.AddListener(AddProductionOrder);
        if (removeProductionButton != null) removeProductionButton.onClick.AddListener(RemoveProductionOrder);

        UpdateUI();
        ResourceManager.Instance.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(requiredResource) ? dict[requiredResource] : 0)
            .Subscribe(_ => UpdateUI())
            .AddTo(this);
    }

    private void OnMouseDown()
    {
        if (currentStored > 0)
        {
            CollectResources();
        }

        FactoryManager.Instance.OpenFactoryUI(this);
    }

    private void CollectResources()
    {
        if (currentStored > 0)
        {
            ResourceManager.Instance.AddResource(producedResource, currentStored);
            Debug.Log($"Collected: {currentStored} {producedResource}");
            currentStored = 0;
            UpdateUI();
        }
    }

    private void AddProductionOrder()
    {
        if (productionQueue < capacity)
        {
            if (ResourceManager.Instance.ConsumeResource(requiredResource, requiredResourceAmount))
            {
                productionQueue++;
                Debug.Log($"{producedResource} Factory: Production order added. Queue: {productionQueue}");
                if (productionQueue == 1) StartProduction().Forget(); // If first order, start production
                UpdateUI();
            }
            else
            {
                Debug.Log($"Not enough {requiredResource}!");
            }
        }
        else
        {
            Debug.Log("Production queue is full!");
        }
    }

    private void RemoveProductionOrder()
    {
        if (productionQueue > 0)
        {
            productionQueue--;
            Debug.Log($"{producedResource} Factory: Production order removed. Queue: {productionQueue}");
            UpdateUI();
        }
    }

    private async UniTaskVoid StartProduction()
    {
        while (productionQueue > 0 && !productionCTS.Token.IsCancellationRequested)
        {
            await UpdateSlider(productionTime);
            productionQueue--;
            currentStored = Mathf.Min(currentStored + 1, capacity);
            Debug.Log($"{producedResource} Factory: 1 {producedResource} produced. Storage: {currentStored}");
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        productionCTS?.Cancel();
    }

    #region UI Functions
    protected async UniTask UpdateSlider(float duration)
    {
        float timeRemaining = duration;
        remainingTimeSlider.value = 1; // Start full

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            remainingTimeSlider.value = timeRemaining / duration;
            remainingTimeText.text = $"{Mathf.CeilToInt(timeRemaining)}s";
            await UniTask.Yield();
        }

        remainingTimeSlider.value = 0;
    }

    protected void UpdateUI()
    {
        // Update production texts
        currentStoredText.text = $"{currentStored}";
        productionQueueText.text = $"{productionQueue}/{capacity}";

        // Update remaining time text
        if (currentStored == capacity) remainingTimeText.text = "Full";
        else remainingTimeText.text = productionQueue > 0 ? $"Queue: {productionQueue}" : "Idle";

        // Update production buttons
        if (productionButton != null) productionButton.interactable = productionQueue < capacity && productionQueue + currentStored < capacity && ResourceManager.Instance.resources[requiredResource] >= requiredResourceAmount;
        if (removeProductionButton != null) removeProductionButton.interactable = productionQueue > 0;
    }

    public void ToggleProductionButtons(bool active)
    {
        if (productionButtonsParent != null) productionButtonsParent.SetActive(active);
    }
    #endregion
}