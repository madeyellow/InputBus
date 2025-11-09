using System;
using System.Collections.Generic;
using System.Linq;
using MadeYellow.InputBus.Schemes;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using MadeYellow.InputBus.Services.Abstractions;

namespace MadeYellow.InputBus.Services
{
    /// <summary>
    /// Сервис-шина, которая выполняет маршрутизацию <see cref="InputAction"/> к зарегистрированным для них методам-обработчикам
    /// </summary>
    [CreateAssetMenu(fileName = "New Input Service", menuName = "Input Bus/Input Service")]
    public class InputService : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// Указывается нужно ли показывать в консоли предупрежедния о том, что <see cref="InputAction"/> не маршрутизирован
        /// </summary>
        [SerializeField]
        private bool _showNotMappedWarnings = true;

        /// <summary>
        /// Нужно ли выводить в консоль предупреждение о том, что схема не найдена (при смене схемы)
        /// </summary>
        [SerializeField]
        private bool _showSchemeNotFoundWarnings = true;
#endif

        /// <summary>
        /// Схемы управления
        /// </summary>
        public IReadOnlyCollection<InputScheme> InputSchemes => _inputSchemes;
        [SerializeField]
        private List<InputScheme> _inputSchemes = new();
        
        /// <summary>
        /// Текущая схема управления в <see cref="PlayerInput"/>
        /// </summary>
        public InputScheme CurrentScheme { get; private set; }

        /// <summary>
        /// Маршрутизатор, строящий карту маппингов <see cref="InputAction"/> и маршрутизирует их к методам-обработчикам
        /// </summary>
        InputRouter _router;
        
        /// <summary>
        /// Набор <see cref="InputAction"/>
        /// </summary>
        private InputActionAsset _inputActionAsset;

        /// <summary>
        /// Контроллер инута, для которого инициализирована эта шина
        /// </summary>
        private PlayerInput _playerInput;
        
#region События

        /// <summary>
        /// Событие смены схемы управления в <see cref="PlayerInput"/>
        /// </summary>
        /// <remarks>
        /// В качестве аргумента указывается какая именно схема была выбрана
        /// </remarks>
        public event Action<InputScheme> OnSchemeChanged;

#endregion

        /// <summary>
        /// Подготовка шины для работы с определённым <see cref="PlayerInput"/>
        /// </summary>
        /// <param name="inputController">Контроллер, с которого будут считываться события вызова <see cref="InputAction"/></param>
        public void Initilize(PlayerInput inputController)
        {
            // Предварительная проверка - переданы ли нам необходимые данные
            if (inputController == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"Не удалось инициализировать '{nameof(InputService)}' так как переданный {nameof(PlayerInput)} был равен null!");
#endif

                return;
            }

            // Отписка от событий PlayerInput, инициализированного ранее
            if (_playerInput != null)
            {
                _playerInput.onControlsChanged -= SchemeChangedCallback;
                _playerInput.onActionTriggered -= RouteAction;
            }

            _playerInput = inputController;

            // Подготовка карты маршрутизации
            _inputActionAsset = inputController.actions;

            _router = new InputRouter(_inputActionAsset, _inputSchemes);
            
            // Подписка на события PlayerInput (для маршрутизации)
            _playerInput.onControlsChanged += SchemeChangedCallback; // При смене схемы управления - попробуем обработать и оповестить об этом
            _playerInput.onActionTriggered += RouteAction; // При срабатывании InputAction - попробуем маршрутизировать его к методам-обработчикам

            // Установка текущей схемы управления сразу после инициализации
            SchemeChangedCallback(inputController);
        }

        /// <summary>
        /// Реакция на событие смены схемы инпута
        /// </summary>
        private void SchemeChangedCallback(PlayerInput playerInput)
        {
            var newScheme = FindScheme(playerInput.currentControlScheme);

            if (newScheme == null)
            {
#if UNITY_EDITOR
                if (_showSchemeNotFoundWarnings)
                    Debug.LogWarning($"Схема управления '{playerInput.currentControlScheme}' не найдена среди схем управления инициализированного ассета.");
#endif
                return;
            }

            // Если схема не изменилась - не публикуем событие
            if (CurrentScheme != null && !CurrentScheme .Equals(newScheme))
                return;

            // Запоминаем новую схему и публикуем событие
            CurrentScheme = newScheme;

            OnSchemeChanged?.Invoke(CurrentScheme);
        }

        /// <summary>
        /// Найти схему управления по её имени
        /// </summary>
        private InputScheme FindScheme(string schemeName)
        {
            foreach (var scheme in InputSchemes)
            {
                if (scheme.name.Equals(schemeName))
                    return scheme;
            }

            return null;
        }

#region API
        /// <summary>
        /// Добавить метод, который должен быть выполнен при срабатывании определённого <see cref="InputAction"/>. Ожидается, что это будет <see cref="InputAction"/> с указанным названием
        /// </summary>
        /// <param name="inputActionName">Название <see cref="InputAction"/>, при срабатывании которого нужно вызвать функцию-обработчик</param>
        /// <param name="callback">Метод-обработчик <see cref="InputAction"/></param>
        /// <param name="limitWithSchemes">Список <see cref="InputScheme"/> для которых будет вызываться этот метод. Оставьте поле пустым, если хотите выполнять метод для любой схемы</param>
        /// <returns>
        /// Возвращает эту шину, чтобы выстраивать цепочку вызовов
        /// </returns>
        public InputService Subscribe(string inputActionName, Action<CallbackContext> callback, params InputScheme[] limitWithSchemes)
        {
            SubscribeHandle(inputActionName, callback, limitWithSchemes);

            return this;
        }

        public void AddScheme(InputScheme inputScheme)
        {
            if (_inputSchemes.Contains(inputScheme))
                return;
            
            _inputSchemes.Add(inputScheme);
        }

        public void RemoveScheme(InputScheme inputScheme)
        {
            _inputSchemes.Remove(inputScheme);
        }
#endregion

        /// <summary>
        /// Логика подписки метода-обработчика на <see cref="InputAction"/>
        /// </summary>
        private void SubscribeHandle(string inputActionName, Action<CallbackContext> callback, IEnumerable<InputScheme> limitWithSchemes)
        {
            // Убедимся, что предоставлено название метода
            if (string.IsNullOrWhiteSpace(inputActionName))
                throw new ArgumentNullException(nameof(inputActionName));

            // Убедимся, что предоставлен callback для маршрутизации
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            // Найдём в _inputActionAsset InputAction с нужным названием
            var action = _inputActionAsset.FindAction(inputActionName);

            if (action != null)
            {
                _router.Append(action, callback, limitWithSchemes);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError($"Действие '{inputActionName}' не найдено в {nameof(InputActionAsset)}.");
            }
#endif
        }


        /// <summary>
        /// Маршрутизация вызванного игроком <see cref="InputAction"/> к методам-обработчикам для этого метода
        /// </summary>
        private void RouteAction(CallbackContext context)
        {
            _router.RouteAction(context, CurrentScheme);
        }
    }
}
