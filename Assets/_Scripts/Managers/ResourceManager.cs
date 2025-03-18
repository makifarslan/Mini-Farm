using UniRx;
using UnityEngine;

public enum ResourceType
{
    Wheat,
    Flour,
    Bread
}

public class ResourceManager : Singleton<ResourceManager>
{
    // Store resource amounts
    public ReactiveDictionary<ResourceType, int> resources = new ReactiveDictionary<ResourceType, int>();

    private void Awake()
    {
        resources[ResourceType.Wheat] = 0;
        resources[ResourceType.Flour] = 0;
        resources[ResourceType.Bread] = 0;
    }

    public void AddResource(ResourceType resourceType, int amount)
    {
        if (!resources.ContainsKey(resourceType))
            resources[resourceType] = 0;
        resources[resourceType] += amount;
    }

    public bool ConsumeResource(ResourceType resourceType, int amount)
    {
        if (resources.ContainsKey(resourceType) && resources[resourceType] >= amount)
        {
            resources[resourceType] -= amount;
            return true;
        }
        return false;
    }
}