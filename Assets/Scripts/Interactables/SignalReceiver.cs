/// Author: Jeremy Anderson, March 22, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// This parent class can receive a signal.
    /// </summary>
    public class SignalReceiver : MonoBehaviour
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== PRIVATE ==========
        protected bool receivedSignal = false;          // used to track whether the SignalReceiver is busy.

        // ========== PUBLIC ==========
        [Header("Trigger Settings")]
        public bool canBeTriggeredMultipleTimes = false; // whether to destroy itself after one trigger.


        /********************
         * =- Methods -=
         ********************/

        // Try to receive a signal, report success or failure.
        public virtual bool TryReceiveSignal(SignalSender origin)
        {
            return false;
        }

        // Destroy or reinitialize the SignalReceiver.
        protected virtual void ClearSignal()
        {
            if (!canBeTriggeredMultipleTimes)           // this object can only be triggered once.
                Destroy(this);                          // destroy this SignalReceiver.

            receivedSignal = false;                     // this object can be triggered multiple times, register that it's ready for another signal.
        }
    }
}