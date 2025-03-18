using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class HayFactory : Factory
{
    public override ResourceType producedResource => ResourceType.Wheat;

    protected override void Start()
    {
        base.Start();
        AutoProduce().Forget();
    }

    private async UniTaskVoid AutoProduce()
    {
        while (!productionCTS.Token.IsCancellationRequested)
        {
            if (currentStored < capacity)
            {
                await UpdateSlider(productionTime);
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
}