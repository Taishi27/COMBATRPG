using UnityEngine;
using UnityEngine.UI;
public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject fireIcon;
    [SerializeField] private GameObject iceIcon;
    [SerializeField] private GameObject poisonIcon;
    [SerializeField] private GameObject lightningIcon;

    private StatusEffectReceiver boundReceiver;

    public Slider HealthSlider => healthSlider;

    public void Bind(StatusEffectReceiver receiver)
    {
        Unbind();

        boundReceiver = receiver;

        if (boundReceiver == null)
        {
            HideAllIcons();
            return;
        }

        boundReceiver.OnStatusEffectStarted += HandleStatusStarted;
        boundReceiver.OnStatusEffectEnded += HandleStatusEnded;

        RefreshAllIcons();
    }

    public void Unbind()
    {
        if (boundReceiver != null)
        {
            boundReceiver.OnStatusEffectStarted -= HandleStatusStarted;
            boundReceiver.OnStatusEffectEnded -= HandleStatusEnded;
            boundReceiver = null;
        }

        HideAllIcons();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void HandleStatusStarted(StatusEffectType type)
    {
        SetIcon(type, true);
    }

    private void HandleStatusEnded(StatusEffectType type)
    {
        SetIcon(type, false);
    }

    private void RefreshAllIcons()
    {
        if (boundReceiver == null)
        {
            HideAllIcons();
            return;
        }

        SetIcon(StatusEffectType.Fire, boundReceiver.IsEffectActive(StatusEffectType.Fire));
        SetIcon(StatusEffectType.Ice, boundReceiver.IsEffectActive(StatusEffectType.Ice));
        SetIcon(StatusEffectType.Poison, boundReceiver.IsEffectActive(StatusEffectType.Poison));
        SetIcon(StatusEffectType.Lightning, boundReceiver.IsEffectActive(StatusEffectType.Lightning));
    }

    private void SetIcon(StatusEffectType type, bool enabled)
    {
        GameObject target = type switch
        {
            StatusEffectType.Fire => fireIcon,
            StatusEffectType.Ice => iceIcon,
            StatusEffectType.Poison => poisonIcon,
            StatusEffectType.Lightning => lightningIcon,
            _ => null
        };

        if (target != null)
            target.SetActive(enabled);
    }

    private void HideAllIcons()
    {
        if (fireIcon != null) fireIcon.SetActive(false);
        if (iceIcon != null) iceIcon.SetActive(false);
        if (poisonIcon != null) poisonIcon.SetActive(false);
        if (lightningIcon != null) lightningIcon.SetActive(false);
    }
}

