using UnityEngine;
using UnityEngine.UI;

namespace ConcussionVR
{
    public sealed class DirectButtonInput : MonoBehaviour
    {
        private Button _button;
        private RectTransform _rect;
        private bool _acceptEnter;

        public void Initialize(Button button, bool acceptEnter)
        {
            _button = button;
            _rect = button.GetComponent<RectTransform>();
            _acceptEnter = acceptEnter;
        }

        private void Update()
        {
            if (_button == null || !_button.interactable) return;
            bool pointerPressed = Input.GetMouseButtonDown(0)
                && RectTransformUtility.RectangleContainsScreenPoint(_rect, Input.mousePosition);
            bool enterPressed = _acceptEnter
                && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
            if (pointerPressed || enterPressed) _button.onClick.Invoke();
        }
    }
}
