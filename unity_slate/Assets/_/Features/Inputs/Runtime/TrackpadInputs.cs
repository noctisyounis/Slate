using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs.Runtime
{
    public class TrackpadInputs : MonoBehaviour
    {
        [SerializeField] private float panSpeed = 0.2f;

        private void Update()
        {
            if (Mouse.current == null)
                return;

            // Two-finger swipe (trackpad scroll gesture)
            Vector2 scrollDelta = Mouse.current.scroll.ReadValue();

            if (scrollDelta.sqrMagnitude <= 0.0f || scrollDelta.y >= 0.05f)
                return;

            GetMousePosDelta();
            _posLastFrame = Mouse.current.position.value;
            // Convert the delta to a "pan" vector — note that
            // trackpads often invert Y relative to user expectation
            Vector2 panDelta = new Vector2(scrollDelta.x, scrollDelta.y) * panSpeed;

            // Apply to your camera or scene panning system
            Debug.Log($"Panning by {panDelta}");
        }

        private void GetMousePosDelta()
        {
            Vector2 dir = Mouse.current.position.value - _posLastFrame;
        }

        private Vector2 _posLastFrame;
    }
}
