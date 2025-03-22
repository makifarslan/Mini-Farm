using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

public class SaveManager : Singleton<SaveManager>
{
    [Inject] private ResourceManager resourceManager;

    private string saveFilePath; // C:\Users\ABC\AppData\LocalLow\DefaultCompany\Mini-Farm

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Save timestamp

        // Save resource data
        saveData.resources = new List<ResourceData>();
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            ResourceData rd = new ResourceData();
            rd.resourceType = type;
            rd.amount = resourceManager.resources.ContainsKey(type) ? resourceManager.resources[type] : 0;
            saveData.resources.Add(rd);
        }

        // Save factory data
        saveData.factories = new List<FactorySaveData>();
        foreach (Factory factory in FactoryManager.Instance.allFactories)
        {
            FactorySaveData fsd = new FactorySaveData();
            fsd.factoryID = factory.GetInstanceID();
            fsd.currentStored = factory.GetCurrentStored();
            fsd.productionQueue = factory.GetProductionQueue();
            fsd.productionTimer = factory.GetProductionTimer();
            saveData.factories.Add(fsd);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game Saved:\n" + json);
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("No save file found.");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long elapsedTime = currentTime - saveData.lastSaveTime; // Calculate elapsed time

        // Load resources
        foreach (ResourceData rd in saveData.resources)
        {
            resourceManager.resources[rd.resourceType] = rd.amount;
        }

        // Load factories
        foreach (FactorySaveData fsd in saveData.factories)
        {
            Factory factory = FactoryManager.Instance.GetFactoryByID(fsd.factoryID);
            if (factory != null)
            {
                factory.LoadFromSaveData(fsd, elapsedTime);
            }
        }

        Debug.Log("Game Loaded");
    }
}