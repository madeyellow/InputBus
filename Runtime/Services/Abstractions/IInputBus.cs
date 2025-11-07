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
        /// <summary>
        /// Добавить метод, который должен быть выполнен при срабатывании определённого <see cref="InputAction"/>. Ожидается, что это будет <see cref="InputAction"/> с указанным названием
        /// </summary>
        /// <param name="inputActionName">Название <see cref="InputAction"/>, при срабатывании которого нужно вызвать функцию-обработчик</param>
        /// <param name="callback">Метод-обработчик <see cref="InputAction"/></param>
        /// <returns>
        /// Возвращает эту шину, чтобы выстраивать цепочку вызовов
        /// </returns>
        InputService Subscribe(string inputActionName, Action<CallbackContext> callback);
    }
}