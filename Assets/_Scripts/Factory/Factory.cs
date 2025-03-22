using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using Zenject;

public abstract class Factory : MonoBehaviour
{
    [Header("Production Settings")]
    [SerializeField] protected float productionTime;
    [SerializeField] protected int capacity;
    [SerializeField] protected int requiredResourceAmount = 1;
    [SerializeField] protected ResourceType requiredResource;

    [Header("UI")]
    [SerializeField] public FactoryUI factoryUI;

    protected int currentStored = 0;
    protected int productionQueue = 0;
    protected float productionTimerRemaining = 0;
    protected CancellationTokenSource productionCTS;
    
    public abstract ResourceType producedResource { get; }

    [Inject] private ResourceManager resourceManager;

    protected virtual void Start()
    {
        FactoryManager.Instance.RegisterFactory(this);
        productionCTS = new CancellationTokenSource();

        factoryUI.OnAddProductionOrder += AddProductionOrder;
        factoryUI.OnRemoveProductionOrder += RemoveProductionOrder;

        UpdateUI();
        resourceManager.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(requiredResource) ? dict[requiredResource] : 0)
            .Subscribe(_ => UpdateUI())
            .AddTo(this);
    }

    private void OnDestroy()
    {
        productionCTS?.Cancel();
        factoryUI.OnAddProductionOrder -= AddProductionOrder;
        factoryUI.OnRemoveProductionOrder -= RemoveProductionOrder;
    }

    protected virtual void OnMouseDown()
    {
        if (factoryUI.IsButtonsActive()) CollectResources();
        else FactoryManager.Instance.OpenFactoryUI(this);
    }

    #region Production Functions
    protected void CollectResources()
    {
        if (currentStored > 0)
        {
            resourceManager.AddResource(producedResource, currentStored);
            currentStored = 0;
            UpdateUI();
        }
    }

    private void AddProductionOrder()
    {
        if (productionQueue < capacity)
        {
            if (resourceManager.ConsumeResource(requiredResource, requiredResourceAmount))
            {
                productionQueue++;
                if (productionQueue == 1)
                {
                    if (productionCTS.IsCancellationRequested)
                        productionCTS = new CancellationTokenSource();

                    StartProduction().Forget();
                }
                UpdateUI();
            }
        }
    }

    private void RemoveProductionOrder()
    {
        if (productionQueue > 0)
        {
            productionQueue--;
            resourceManager.AddResource(requiredResource, requiredResourceAmount);
            UpdateUI();

            if (productionQueue == 0)
            {
                productionCTS.Cancel();
                productionTimerRemaining = 0;
            }
        }
    }

    private async UniTaskVoid StartProduction()
    {
        CancellationToken ct = productionCTS.Token;
        while (productionQueue > 0 && !ct.IsCancellationRequested)
        {
            float waitTime = productionTimerRemaining > 0 ? productionTimerRemaining : productionTime;
            await factoryUI.UpdateSlider(waitTime, ct);

            if (ct.IsCancellationRequested) break;

            productionQueue--;
            currentStored = Mathf.Min(currentStored + 1, capacity);
            productionTimerRemaining = 0;
            UpdateUI();
        }
    }
    #endregion

    #region Other Functions
    protected void UpdateUI()
    {
        int resourceCount = resourceManager.resources[requiredResource];
        factoryUI.UpdateUI(
            currentStored,
            productionQueue,
            capacity,
            requiredResource,
            requiredResourceAmount,
            resourceCount
        );
    }

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