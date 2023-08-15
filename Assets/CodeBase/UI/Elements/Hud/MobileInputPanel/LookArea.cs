using CodeBase.Hero;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel
{
    public class LookArea : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        private HeroRotating _heroRotating;

        public void Construct(HeroRotating heroRotating) =>
            _heroRotating = heroRotating;

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("OnPointerUp");
            _heroRotating.TurnOff();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown");
            _heroRotating.TurnOn();
        }
    }
}