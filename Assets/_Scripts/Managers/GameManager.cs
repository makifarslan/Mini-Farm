using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameManager : Singleton<GameManager>
{
    private void Start()
    {
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
        await UniTask.Delay(500);
        Debug.Log("Game Saved on Quit");
    }
}