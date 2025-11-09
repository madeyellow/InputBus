using System;
using System.Collections.Generic;
using System.Linq;
using MadeYellow.InputBus.Schemes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MadeYellow.InputBus.Services
{
    /// <summary>
    /// Роутер инпута в методы-обработчики
    /// </summary>
    /// <remarks>
    /// Содержит методы для хранения маппингов InputAction к InputAction.CallbackContext
    /// </remarks>
    internal class InputRouter : MonoBehaviour
    {
        /// <summary>
        /// Карта маппингов InputAction к InputAction.CallbackContext
        /// </summary>
        readonly Dictionary<InputAction, List<InputRouterMap>> _map;
        
        /// <summary>
        /// Добавить в карту метод-обработчик <see cref="callback"/> для действия <see cref="inputAction"/>. Опционально: ограничить выполнение действия ТОЛЬКО для определённых <see cref="limitWithSchemes"/>
        /// </summary>
        /// <param name="inputAction">К какому действию нужно будет маршрутизировать метод-обработчик</param>
        /// <param name="callback">Метод-обработчик действия</param>
        /// <param name="limitWithSchemes">Если указать - то метод-обработчик будет вызываться только для указанных схем</param>
        public void Append(InputAction inputAction, Action<InputAction.CallbackContext> callback, IEnumerable<InputScheme> limitWithSchemes)
        {
            // Если не указан inputAction - нужно выдать ошибку
            if (inputAction == null)
                throw new ArgumentNullException(nameof(inputAction));
            
            // Если не указан callback - нужно выдать ошибку
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            // Если не указаны - схемы
            if (limitWithSchemes != null && !limitWithSchemes.Any())
                limitWithSchemes = new InputScheme[] { null };
            else 
                limitWithSchemes = limitWithSchemes.Where(x => x != null).ToArray();
            
            // Если для указанного действия ещё не было добавлено набора маппингов - созадть новый
            if (!_map.TryGetValue(inputAction, out var mappings))
            {
                mappings = new List<InputRouterMap>(); // ToDo Вероятно стоит указать какую-то вместительность
                
                _map[inputAction] = mappings;
            }

            // Для каждой предоставленной схемы (или для её отсутствия) - добавить callback в маппинг
            foreach (var scheme in limitWithSchemes)
            {
                // Получить или создать новый маппинг для InputScheme
                var mapping = mappings.FirstOrDefault(x => x.Scheme.Equals(scheme));

                if (mapping == null)
                {
                    mapping = new InputRouterMap(scheme);
                    mappings.Add(mapping);
                }

                // Попробовать добавить в маппинг предоставленный callback
                mapping.Append(callback);
            }
        }

        public void RouteAction(InputAction.CallbackContext context, InputScheme currentScheme)
        {
            // ToDo Маршрутизировать действие в нужные callback'и
        }

        internal class InputRouterMap
        {
            public readonly InputScheme Scheme;
            readonly HashSet<Action<InputAction.CallbackContext>> _callbacks;

            public InputRouterMap(InputScheme scheme)
            {
                Scheme = scheme;
                _callbacks = new HashSet<Action<InputAction.CallbackContext>>();
            }

            public void Append(Action<InputAction.CallbackContext> callback)
            {
                // Если не указан callback - нужно выдать ошибку
                if (callback == null)
                    throw new ArgumentNullException(nameof(callback));

                // Если это действие уже добавлено в карту - не добавлять
                if (_callbacks.Contains(callback))
                    return;

                _callbacks.Add(callback);
            }
        }
    }
}