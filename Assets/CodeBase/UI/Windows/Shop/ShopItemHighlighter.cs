using NTC.Global.Cache;
using NTC.Global.System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeBase.UI.Windows.Shop
{
    public class ShopItemHighlighter : MonoCache, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject _outline;

        protected override void OnDisabled() =>
            _outline.Disable();

        public void OnPointerEnter(PointerEventData eventData) =>
            _outline.Enable();

        public void OnPointerExit(PointerEventData eventData) =>
            _outline.Disable();
    }
}