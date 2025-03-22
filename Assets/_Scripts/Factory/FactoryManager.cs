using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FactoryManager : Singleton<FactoryManager>
{
    public List<Factory> allFactories = new List<Factory>();
    private Factory currentFactory;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Return if it's an UI element
            if (EventSystem.current.IsPointerOverGameObject()) return;

            // Close all factory UIs if clicked outside
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                if (currentFactory != null)
                {
                    currentFactory.factoryUI.ToggleProductionButtons(false);
                    currentFactory = null;
                }
                return;
            }
        }
    }

    public void RegisterFactory(Factory factory)
    {
        if (!allFactories.Contains(factory))
        {
            allFactories.Add(factory);
        }
    }

    public Factory GetFactoryByID(int id)
    {
        return allFactories.Find(factory => factory.GetInstanceID() == id);
    }

    public void OpenFactoryUI(Factory factory)
    {
        if (currentFactory != null)
            currentFactory.factoryUI.ToggleProductionButtons(false);

        currentFactory = factory;
        currentFactory.factoryUI.ToggleProductionButtons(true);
    }
}