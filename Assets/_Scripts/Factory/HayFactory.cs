using UnityEngine;
using Cysharp.Threading.Tasks;

public class HayFactory : Factory
{
    public override ResourceType producedResource => ResourceType.Wheat;

    protected override async void Start()
    {
        base.Start();
        await GameManager.Instance.LoadGameAsync(); // Wait to load save
        AutoProduce().Forget();
    }

    protected override void OnMouseDown()
    {
        CollectResources();
        FactoryManager.Instance.OpenFactoryUI(this);
    }

    private async UniTaskVoid AutoProduce()
    {
        while (!productionCTS.Token.IsCancellationRequested)
        {
            if (currentStored < capacity)
            {
                float waitTime = (productionTimerRemaining > 0) ? productionTimerRemaining : productionTime;
                await factoryUI.UpdateSlider(waitTime, productionCTS.Token);
                currentStored++;
                Debug.Log($"HayFactory: 1 Wheat produced. Storage: {currentStored}");
                UpdateUI();
            }
            else
            {
                await UniTask.Delay(500, cancellationToken: productionCTS.Token);
            }
        }
    }

    public override void LoadFromSaveData(FactorySaveData fsd, long elapsedTime)
    {
        currentStored = fsd.currentStored;
        productionTimerRemaining = fsd.productionTimer;

        float timePassed = productionTimerRemaining + elapsedTime;
        int completedCycles = (int)(timePassed / productionTime);
        float remainder = timePassed % productionTime;

        currentStored = Mathf.Min(currentStored + completedCycles, capacity);
        productionTimerRemaining = (currentStored < capacity) ? remainder : 0;

        UpdateUI();
    }
}