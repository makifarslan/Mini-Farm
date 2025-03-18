using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FactoryManager : Singleton<FactoryManager>
{
    private Factory currentFactory;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Return if it's an UI element
            if (EventSystem.current.IsPointerOverGameObject()) return;

            // Close all factory UI's if clicked outside
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                if (currentFactory != null) 
                { 
                    currentFactory.ToggleProductionButtons(false); 
                    currentFactory = null;
                }
                return;
            }
        }
    }

    public void OpenFactoryUI(Factory factory)
    {
        if(currentFactory != null) currentFactory.ToggleProductionButtons(false);
        currentFactory = factory;
        currentFactory.ToggleProductionButtons(true);
    }
}
