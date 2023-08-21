using System.Collections.Generic;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    public class SwapObjects : MonoBehaviour
    {
        public List<GameObject> ListA;

        public List<GameObject> ListB;

        public KeyCode SwapKey = KeyCode.Space;

        private bool _activeListIsA = true;

        private void Update()
        {
            if (!Input.GetKeyUp(SwapKey))
                return;

            _activeListIsA = !_activeListIsA;

            foreach (GameObject o in ListA)
                o.SetActive(_activeListIsA);

            foreach (GameObject o in ListB)
                o.SetActive(!_activeListIsA);
        }
    }
}