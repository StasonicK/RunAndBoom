using UnityEngine;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel
{
    public class LookByTouch : MonoBehaviour
    {
        [SerializeField] private GameObject _joystick;
        [SerializeField] private GameObject _joystickBG;

        public Vector2 JoystickVec;
        private Vector2 _joystickTouchPos;
        private Vector2 _joystickOriginalPos;
        private float _joystickRadius;

        private Touch _touch;

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                _touch = Input.GetTouch(0);

                if (_touch.phase == TouchPhase.Moved)
                {
                    // if (_touch.deltaPosition.sqrMagnitude > Constants.RotationEpsilon)
                    JoystickVec = -_touch.deltaPosition;
                    Debug.Log($"delta pos {_touch.deltaPosition}");
                }
                else
                {
                    JoystickVec = Vector2.zero;
                }
            }
        }

        // private void Start()
        // {
        //     _joystickOriginalPos = _joystickBG.transform.position;
        //     _joystickRadius = _joystickBG.GetComponent<RectTransform>().sizeDelta.y / 4;
        // }
        //
        // public void PointerDown()
        // {
        //     _joystick.transform.position = Input.mousePosition;
        //     _joystickBG.transform.position = Input.mousePosition;
        //     _joystickTouchPos = Input.mousePosition;
        // }
        //
        // public void Drag(BaseEventData baseEventData)
        // {
        //     PointerEventData pointerEventData = baseEventData as PointerEventData;
        //     Vector2 dragPos = pointerEventData.position;
        //     JoystickVec = (dragPos - _joystickTouchPos).normalized;
        //
        //     float joystickDist = Vector2.Distance(dragPos, _joystickTouchPos);
        //
        //     if (joystickDist < _joystickRadius)
        //     {
        //         _joystick.transform.position = _joystickTouchPos + JoystickVec * joystickDist;
        //     }
        //
        //     else
        //     {
        //         _joystick.transform.position = _joystickTouchPos + JoystickVec * _joystickRadius;
        //     }
        // }
        //
        // public void PointerUp()
        // {
        //     JoystickVec = Vector2.zero;
        //     _joystick.transform.position = _joystickOriginalPos;
        //     _joystickBG.transform.position = _joystickOriginalPos;
        // }
    }
}