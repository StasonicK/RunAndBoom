﻿using CodeBase.UI.Services;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Results
{
    public class Star : MonoBehaviour
    {
        [SerializeField] private Image _reached;
        [SerializeField] private Image _unreached;

        public void ShowReached()
        {
            _reached.ChangeImageAlpha(Constants.Visible);
            _unreached.ChangeImageAlpha(Constants.Invisible);
        }

        public void ShowUnreached()
        {
            _unreached.ChangeImageAlpha(Constants.Visible);
            _reached.ChangeImageAlpha(Constants.Invisible);
        }
    }
}