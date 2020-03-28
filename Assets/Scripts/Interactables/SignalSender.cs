/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// This parent class sends a signal to all SignalReceivers in its list.
    /// </summary>
    public class SignalSender : MonoBehaviour
    {
        /********************
         * =- Functions -=
         ********************/

        // Send a signal to a SignalReceiver.
        protected virtual void SendSignal(SignalReceiver destination)
        {
            destination.TryReceiveSignal(this); // currently does nothing with the returned bool.
        }
    }
}
