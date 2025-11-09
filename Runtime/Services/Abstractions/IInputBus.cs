using System;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace MadeYellow.InputBus.Services.Abstractions
{
    /// <summary>
    /// Шина обработки <see cref="InputAction"/>
    /// </summary>
    public interface IInputBus
    {
        
        InputService Subscribe(string inputActionName, Action<CallbackContext> callback);
    }
}