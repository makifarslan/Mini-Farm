using System;
using System.Collections.Generic;

[Serializable]
public class FactorySaveData
{
    public int factoryID; // Take instanceID
    public int currentStored;
    public int productionQueue;
    public float productionTimer;
}

[Serializable]
public class ResourceData
{
    public ResourceType resourceType;
    public int amount;
}

[Serializable]
public class GameSaveData
{
    public long lastSaveTime;
    public List<ResourceData> resources;
    public List<FactorySaveData> factories;
}