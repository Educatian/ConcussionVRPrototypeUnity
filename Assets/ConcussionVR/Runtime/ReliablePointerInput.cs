using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ConcussionVR
{
    public sealed class ReliablePointerInput : MonoBehaviour
    {
        private readonly List<RaycastResult> _hits = new();
        private EventSystem _eventSystem;

        private void Awake() => _eventSystem = GetComponent<EventSystem>();

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            var pointer = new PointerEventData(_eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                position = Input.mousePosition
            };
            _hits.Clear();
            _eventSystem.RaycastAll(pointer, _hits);
            foreach (RaycastResult hit in _hits)
            {
                if (ExecuteEvents.ExecuteHierarchy(hit.gameObject, pointer, ExecuteEvents.pointerClickHandler) != null)
                    return;
            }
        }
    }
}
