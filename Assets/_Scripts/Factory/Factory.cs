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
    protected float productionTimerRemaining = 0;

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
        FactoryManager.Instance.RegisterFactory(this);
        productionCTS = new CancellationTokenSource();

        if (productionButton != null) productionButton.onClick.AddListener(AddProductionOrder);
        if (removeProductionButton != null) removeProductionButton.onClick.AddListener(RemoveProductionOrder);

        UpdateUI();
        ResourceManager.Instance.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(requiredResource) ? dict[requiredResource] : 0)
            .Subscribe(_ => UpdateUI())
            .AddTo(this);
    }

    private void OnDestroy()
    {
        productionCTS?.Cancel();
    }

    private void OnMouseDown()
    {
        if (productionButtonsParent != null)
        {
            if (productionButtonsParent.activeSelf) CollectResources();
            else FactoryManager.Instance.OpenFactoryUI(this);
        }
        else
        {
            CollectResources();
            FactoryManager.Instance.OpenFactoryUI(this);
        }
    }

    #region Production Functions
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
                if (productionQueue == 1)
                {
                    // If the current token is canceled, create a new one
                    if (productionCTS.IsCancellationRequested) productionCTS = new CancellationTokenSource();

                    StartProduction().Forget();
                }

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
            ResourceManager.Instance.AddResource(requiredResource, requiredResourceAmount); // Refund

            Debug.Log($"{producedResource} Factory: Production order removed. Queue: {productionQueue}");
            UpdateUI();

            if (productionQueue == 0)
            {
                productionCTS.Cancel();

                productionTimerRemaining = 0;
                remainingTimeSlider.value = 0;
            }
        }
    }

    private async UniTaskVoid StartProduction()
    {
        CancellationToken ct = productionCTS.Token;
        while (productionQueue > 0 && !ct.IsCancellationRequested)
        {
            float waitTime = (productionTimerRemaining > 0) ? productionTimerRemaining : productionTime;
            await UpdateSlider(waitTime, ct);

            if (ct.IsCancellationRequested)
                break;

            productionQueue--;
            currentStored = Mathf.Min(currentStored + 1, capacity);
            productionTimerRemaining = 0;
            Debug.Log($"{producedResource} Factory: 1 {producedResource} produced. Storage: {currentStored}");

            UpdateUI();
        }
    }
    #endregion

    #region UI Functions
    protected async UniTask UpdateSlider(float duration, CancellationToken ct)
    {
        float timeRemaining = duration;
        remainingTimeSlider.value = 1; // Start full

        while (timeRemaining > 0)
        {
            if (ct.IsCancellationRequested)
                return;

            timeRemaining -= Time.deltaTime;
            remainingTimeSlider.value = timeRemaining / duration;
            remainingTimeText.text = $"{Mathf.CeilToInt(timeRemaining)}s";
            productionTimerRemaining = timeRemaining;
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

    #region Save Functions
    public virtual void LoadFromSaveData(FactorySaveData fsd, long elapsedTime)
    {
        currentStored = fsd.currentStored;
        productionQueue = fsd.productionQueue;
        productionTimerRemaining = fsd.productionTimer; // Load remaining time

        float timePassed = productionTimerRemaining + elapsedTime; // Total elapsed time
        int completedCycles = (int)(timePassed / productionTime); // How many cycles completed?
        float remainder = timePassed % productionTime; // Remaining time for the next cycle

        // Process completed productions
        int producible = Mathf.Min(completedCycles, productionQueue);
        productionQueue -= producible;
        currentStored = Mathf.Min(currentStored + producible, capacity);

        // Update the remaining timer for the next cycle
        productionTimerRemaining = (productionQueue > 0) ? remainder : 0;

        // Restart production if there's still a queue
        if (productionQueue > 0)
        {
            StartProduction().Forget();
        }

        UpdateUI();
    }

    public int GetCurrentStored()
    {
        return currentStored;
    }

    public int GetProductionQueue()
    {
        return productionQueue;
    }

    public float GetProductionTimer()
    {
        return productionTimerRemaining;
    }
    #endregion
}