using System;
using MadeYellow.InputBus.Schemes;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace MadeYellow.InputBus.Services
{
    public static class InputServiceExtension
    {
        /// <summary>
        /// Добавить метод, который должен быть выполнен при срабатывании определённого <see cref="InputAction"/>. Ожидается, что название <see cref="InputAction"/> будет таким же, как называется этот метод
        /// </summary>
        /// <param name="callback">Метод-обработчик <see cref="InputAction"/></param>
        /// <param name="limitWithSchemes">Список <see cref="InputScheme"/> для которых будет вызываться этот метод. Оставьте поле пустым, если хотите выполнять метод для любой схемы</param>
        /// <returns>
        /// Возвращает эту шину, чтобы выстраивать цепочку вызовов
        /// </returns>
        public static InputService Subscribe(this InputService inputService, Action<CallbackContext> callback, params InputScheme[] limitWithSchemes)
        {
            return inputService?.Subscribe(callback.Method.Name, callback, limitWithSchemes);
        }
        
        /// <summary>
        /// Добавить метод, который должен быть выполнен при срабатывании определённого <see cref="InputAction"/>
        /// </summary>
        /// <param name="inputAction">При срабатывании этого <see cref="InputAction"/> данные будут маршрутизированы в предоставленный вами метода</param>
        /// <param name="callback">Метод-обработчик <see cref="InputAction"/></param>
        /// <param name="limitWithSchemes">Список <see cref="InputScheme"/> для которых будет вызываться этот метод. Оставьте поле пустым, если хотите выполнять метод для любой схемы</param>
        /// <returns>
        /// Возвращает эту шину, чтобы выстраивать цепочку вызовов
        /// </returns>
        public static InputService Subscribe(this InputService inputService, InputAction inputAction, Action<CallbackContext> callback, params InputScheme[] limitWithSchemes)
        {
            return inputService?.Subscribe(inputAction.name, callback, limitWithSchemes);
        }
    }
}