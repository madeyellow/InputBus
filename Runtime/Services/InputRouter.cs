using System;
using System.Collections.Generic;
using System.Linq;
using MadeYellow.InputBus.Schemes;
using UnityEngine.InputSystem;

namespace MadeYellow.InputBus.Services
{
    /// <summary>
    /// Маршрутизатор инпута в методы-обработчики
    /// </summary>
    /// <remarks>
    /// Содержит методы для хранения маппингов InputAction к InputAction.CallbackContext
    /// </remarks>
    internal class InputRouter
    {
        /// <summary>
        /// Карта маппингов InputAction к InputAction.CallbackContext
        /// </summary>
        readonly Dictionary<InputAction, List<InputRouterMap>> _map;

        readonly int ActionsCount;
        readonly int SchemesCount;
        
        public InputRouter(IEnumerable<InputAction> inputActions, IEnumerable<InputScheme> schemes)
        {
            if (inputActions == null)
                throw new ArgumentNullException(nameof(inputActions));
            
            if (schemes == null)
                throw new ArgumentNullException(nameof(schemes));
            
            ActionsCount = inputActions.Count();
            SchemesCount = schemes.Count();
            
            _map = new Dictionary<InputAction, List<InputRouterMap>>(ActionsCount);
        }
        
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
                mappings = new List<InputRouterMap>(SchemesCount + 1);
                
                _map.Add(inputAction, mappings);
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
            var inputAction = context.action;
            
            if (inputAction == null)
                return;
            
            // Если для этого действия нет маппингов - то пропускаем
            if (!_map.TryGetValue(inputAction, out var mappings))
                return;

            // Для каждого маппинга найдём те, куда можно маршрутилизровать это действие
            foreach (var mapping in mappings)
            {
                // Если это действие не применимо к текущей схеме
                if (!mapping.IsApplicable(currentScheme))
                    continue;

                // Вызовем все callback'и
                foreach (var callback in mapping.Callbacks)
                    callback.Invoke(context);
            }
        }

        internal class InputRouterMap
        {
            public readonly InputScheme Scheme;
            readonly HashSet<Action<InputAction.CallbackContext>> _callbacks;
            public IReadOnlyCollection<Action<InputAction.CallbackContext>> Callbacks => _callbacks;
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

            /// <summary>
            /// Применим ли этот маппинг к схеме?
            /// </summary>
            public bool IsApplicable(InputScheme currentScheme)
            {
                if (Scheme == null)
                    return true;
                
                return Scheme.Equals(currentScheme);
            }
        }
    }
}