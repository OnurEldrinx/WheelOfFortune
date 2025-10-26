using UnityEngine;

namespace UI
{
    public sealed class ButtonBinder : MonoBehaviour
    {
        [SerializeField] private HUDView view;
        private HUDPresenter presenter;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!view) view = GetComponentInChildren<HUDView>(true);
        }
#endif
        public void Bind(HUDPresenter p)
        {
            presenter = p;
            if (view && view.spinButton){
                view.spinButton.onClick.RemoveAllListeners();
                view.spinButton.onClick.AddListener(presenter.OnSpinPressed);
            }
            if (view && view.exitButton){
                view.exitButton.onClick.RemoveAllListeners();
                view.exitButton.onClick.AddListener(presenter.OnCashOutPressed);
            }
            if (view && view.failView.giveUpButton){
                view.failView.giveUpButton.onClick.RemoveAllListeners();
                view.failView.giveUpButton.onClick.AddListener(presenter.OnGiveUpPressed);
            }
        }
    }
}
