/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// This class sends a signal to all SignalReceivers in its list.
    /// </summary>
    public class SignalSender : MonoBehaviour
    {
        /********************
         * =- Functions -=
         ********************/

        public virtual void SendSignal(SignalReceiver destination)
        {
            destination.TryReceiveSignal(this);
        }
    }
}
