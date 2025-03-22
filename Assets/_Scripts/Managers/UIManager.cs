using UnityEngine;
using UniRx;
using TMPro;
using Zenject;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text wheatText;
    [SerializeField] private TMP_Text flourText;
    [SerializeField] private TMP_Text breadText;

    [Inject] private ResourceManager resourceManager;

    private void Start()
    {
        // Subscribe to resource changes to update UI and trigger the scale effect
        resourceManager.resources
            .ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Wheat) ? dict[ResourceType.Wheat] : 0)
            .Subscribe(value => {
                wheatText.text = value.ToString();
                TextScaleEffect(wheatText);
            })
            .AddTo(this);

        resourceManager.resources
            .ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Flour) ? dict[ResourceType.Flour] : 0)
            .Subscribe(value => {
                flourText.text = value.ToString();
                TextScaleEffect(flourText);
            })
            .AddTo(this);

        resourceManager.resources
            .ObserveEveryValueChanged(dict => dict.ContainsKey(ResourceType.Bread) ? dict[ResourceType.Bread] : 0)
            .Subscribe(value => {
                breadText.text = value.ToString();
                TextScaleEffect(breadText);
            })
            .AddTo(this);
    }

    private void TextScaleEffect(TMP_Text text)
    {
        if (DOTween.IsTweening(text.transform.parent))
            return;

        Vector3 startScale = text.transform.parent.localScale;
        text.transform.parent.DOScale(startScale * 1.2f, 0.2f)
            .OnComplete(() => text.transform.parent.DOScale(startScale, 0.2f));
    }
}
