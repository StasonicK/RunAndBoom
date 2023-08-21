using System;
using System.Drawing;
using UnityEngine;

namespace QuadTreeLib
{
    /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IHasRect
    {
        Rect Rectangle { get; }
    }
}
