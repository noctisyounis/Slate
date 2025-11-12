using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs.Runtime
{
    public class TrackpadInputs : MonoBehaviour
    {
        [SerializeField] private float panSpeed = 0.2f;
        private Transform _mainCamTransform;

        private void OnGUI()
        {
            float test = Input.GetAxis("Horizontal");
            GUILayout.Button(test.ToString());
        }

        private void Awake()
        {
            _mainCamTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (Mouse.current == null)
                return;

            // unfortunately 2 fingers gesture is embedded in mouse.scroll.value
            Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
            float magnitude = scrollDelta.magnitude;
            if (magnitude <= 0.0f)
                return;

            // we ended up opting for Amplify way of handling gestures (trackpad)
            // on touchpad we would have : 
            // - Move inside Plate : RMB + mouse Delta
            // - Move Plate Window : Hover top of Plate window +  LMB + Mousedelta

            bool movingHorizontally = scrollDelta.x != 0 && Mathf.Abs(scrollDelta.y) <= 0.02f;
            bool movingVertically = scrollDelta.y != 0 && Mathf.Abs(scrollDelta.x) <= 0.02f;
            if (movingHorizontally)
            {
                _mainCamTransform.position += transform.up * scrollDelta.y + transform.right * scrollDelta.x;

            }
            else if (movingVertically)
            {
                _mainCamTransform.position += transform.up * scrollDelta.y;
            }
        }
    }
}
