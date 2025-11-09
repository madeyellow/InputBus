using MadeYellow.InputBus.Schemes;
using UnityEngine;
using UnityEngine.InputSystem;
using MadeYellow.InputBus.Services;
using UnityEditor;

namespace MadeYellow.InputBus.Components
{
    /// <summary>
    /// Этот компонент обрабатывает ввод <see cref="InputAction"/> от пользователя (через <see cref="PlayerInput"/>) и маршрутизирует его в зарегистрированные функции (через InputService)
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class InputController : MonoBehaviour
    {
        /// <summary>
        /// Через этот сервис регистрируются и маршрутизируются команды
        /// </summary>
        [SerializeField]
        private InputService _inputService;

        private PlayerInput _playerInput;

        private void Awake()
        {
            if (_inputService is null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning($"Вы не установили {nameof(InputService)}! Этот компонент будет выключен.");
                #endif
                
                enabled = false;
                return;
            }
            
            // Считать PlayerInput и передать его в сервис-шину для инициализации
            _playerInput = GetComponent<PlayerInput>();

            _inputService.Initilize(_playerInput);

            DontDestroyOnLoad(gameObject);
        }

#if UNITY_EDITOR
        private void BuildSchemes()
        {
            _playerInput = GetComponent<PlayerInput>();
            
            foreach (var scheme in _playerInput.actions.controlSchemes)
            {
                InputScheme.Build(scheme);
            }
        }

        /// <summary>
        /// Генерирует <see cref="InputScheme"/> ассеты и добавляет их в <see cref="InputService"/>
        /// </summary>
        public void GenerateInputSchemesAndAddToService()
        {
            if (_inputService == null)
            {
                Debug.LogError("InputService не назначен! Назначьте InputService в инспекторе.");
                return;
            }

            // Создаем схемы
            BuildSchemes();

            // Загружаем все созданные InputScheme ассеты и добавляем их в сервис
            string[] guids = AssetDatabase.FindAssets("t:InputScheme");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                InputScheme scheme = AssetDatabase.LoadAssetAtPath<InputScheme>(assetPath);
                
                if (scheme != null)
                {
                    _inputService.AddScheme(scheme);
                }
            }

            // Помечаем сервис как измененный для сохранения
            EditorUtility.SetDirty(_inputService);
            AssetDatabase.SaveAssets();

            Debug.Log($"{nameof(InputScheme)} ассеты созданы и добавлены в {_inputService.name}");
        }
#endif
    }
}