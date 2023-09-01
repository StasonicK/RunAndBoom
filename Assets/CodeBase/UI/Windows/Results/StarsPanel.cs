using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.UI.Windows.Results
{
    public class StarsPanel : MonoCache
    {
        [SerializeField] private Star _star1;
        [SerializeField] private Star _star2;
        [SerializeField] private Star _star3;

        public void ShowStars(int count)
        {
            switch (count)
            {
                case 1:
                    _star1.ShowReached();
                    _star2.ShowUnreached();
                    _star3.ShowUnreached();
                    break;
                case 2:
                    _star1.ShowReached();
                    _star2.ShowReached();
                    _star3.ShowUnreached();
                    break;
                case 3:
                    _star1.ShowReached();
                    _star2.ShowReached();
                    _star3.ShowReached();
                    break;
                default:
                    _star1.ShowUnreached();
                    _star2.ShowUnreached();
                    _star3.ShowUnreached();
                    break;
            }
        }
    }
}