using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class GameManager : Singleton<GameManager>
{
    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        cancellationTokenSource = new CancellationTokenSource();
        LoadGameAsync().Forget();
    }

    public async UniTask LoadGameAsync()
    {
        await UniTask.Yield(); // Ensure all managers are initialized
        SaveManager.Instance.LoadGame();
        Debug.Log("Game Loaded");
    }

    private async void OnApplicationQuit()
    {
        SaveManager.Instance.SaveGame();
        cancellationTokenSource.Cancel();
        await UniTask.Delay(500);
        Debug.Log("Game Saved on Quit");
    }
}