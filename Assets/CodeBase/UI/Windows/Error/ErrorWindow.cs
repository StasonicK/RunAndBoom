using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows.Error
{
    public class ErrorWindow : WindowBase
    {
        [SerializeField] private TextMeshProUGUI _errorText;

        // protected override void Initialize() =>
        //     _errorText.text = CurrentError;
    }
}