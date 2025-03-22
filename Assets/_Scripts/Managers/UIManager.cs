using UnityEngine;
using UniRx;
using TMPro;
using Zenject;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text wheatText;
    [SerializeField] private TMP_Text flourText;
    [SerializeField] private TMP_Text breadText;

    [Inject] private ResourceManager resourceManager;

    private void Start()
    {
        // Subscribe to resource changes to update UI
        resourceManager.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Wheat) ? dict[ResourceType.Wheat] : 0)
            .Subscribe(value => wheatText.text = value.ToString())
            .AddTo(this);

        resourceManager.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Flour) ? dict[ResourceType.Flour] : 0)
            .Subscribe(value => flourText.text = value.ToString())
            .AddTo(this);

        resourceManager.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Bread) ? dict[ResourceType.Bread] : 0)
            .Subscribe(value => breadText.text = value.ToString())
            .AddTo(this);
    }
}