using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Keiwando.Evolution.UI;

namespace Keiwando.UI {

    public class LabelledSlider: MonoBehaviour {

        public event System.Action<float> onValueChanged;
        public event System.Action onDragWillBegin;

        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField] private ClickableTooltip tooltip;

        public string Description {
            get { return descriptionLabel?.text ?? ""; }
            set { if (descriptionLabel != null) descriptionLabel.text = value; }
        }

        public TooltipData TooltipData { get; set; }

        void Start() {

            this.slider.onValueChanged.AddListener(delegate (float value) {
                if (this.onValueChanged != null) {
                    onValueChanged(value);
                }
            });
            if (TooltipData == null) {
                this.tooltip.gameObject.SetActive(false);
            }
        }

        public void OnDragWillBegin() {
            if (this.onDragWillBegin != null) {
                this.onDragWillBegin();
            }
        }

        public void Refresh(float value, string valueText = null) {
            slider.value = value;
            if (valueText == null) {
                valueLabel.text = value.ToString();
            } else {
                valueLabel.text = valueText;
            }

            tooltip?.SetData(TooltipData);
        }
    }
}