using UnityEngine;
using UniRx;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text wheatText;
    [SerializeField] private TMP_Text flourText;
    [SerializeField] private TMP_Text breadText;

    private void Start()
    {
        // Subscribe to resource changes to update UI
        ResourceManager.Instance.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Wheat) ? dict[ResourceType.Wheat] : 0)
            .Subscribe(value => wheatText.text = value.ToString())
            .AddTo(this);

        ResourceManager.Instance.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Flour) ? dict[ResourceType.Flour] : 0)
            .Subscribe(value => flourText.text = value.ToString())
            .AddTo(this);

        ResourceManager.Instance.resources.ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Bread) ? dict[ResourceType.Bread] : 0)
            .Subscribe(value => breadText.text = value.ToString())
            .AddTo(this);
    }
}