using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649 // never assigned warning

namespace TheraBytes.BetterUi.Examples.ResolutionTracking
{
    public class SetDpiText : MonoBehaviour, IResolutionDependency
    {
        [SerializeField] Text screenDpiValueLabel;
        [SerializeField] Text betterUiDpiValueLabel;
        [SerializeField] Text resolutionValueLabel;


        void Start()
        {
            OnResolutionChanged();
        }

        public void OnResolutionChanged()
        {
            screenDpiValueLabel.text = Screen.dpi.ToString();
            betterUiDpiValueLabel.text = ResolutionMonitor.CurrentDpi.ToString();
            resolutionValueLabel.text = $"{Screen.width} x {Screen.height}";
        }

    }
}

#pragma warning restore 0649 // never assigned warning
