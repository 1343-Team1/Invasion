/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Entering this object's collider wins the game.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WinTrigger : MonoBehaviour { }
}
