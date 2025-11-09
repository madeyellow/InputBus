#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif


using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MadeYellow.InputBus.Schemes
{
    /// <summary>
    /// Обёртка над <see cref="InputControlScheme"/>, которую можно использовать в редакторе
    /// </summary>
    public class InputScheme : ScriptableObject, IEquatable<InputScheme>, IEquatable<InputControlScheme>
    {
        public InputControlScheme Scheme;

#if UNITY_EDITOR
        public static void Build(InputControlScheme scheme)
        {
            // Определить путь к директории
            string directoryPath = "Assets/InputSchemes";
            
            // Проверить существует ли нужная директория
            if (!AssetDatabase.IsValidFolder(directoryPath))
            {
                // Создать директорию Assets/InputSchemes если её не существует
                Directory.CreateDirectory(Application.dataPath + "/InputSchemes");
                AssetDatabase.Refresh(); // Обновляем AssetDatabase, чтобы Unity увидел новую папку
            }
            
            // Создать новый объект схемы
            InputScheme instance = ScriptableObject.CreateInstance<InputScheme>();
            instance.Scheme = scheme;

            var desiredAssetPath = $"{directoryPath}/{scheme.name}.asset";
            
            // Если вы хотите предотвратить создание ассета с тем же именем, можно добавить проверку:
            if (AssetDatabase.LoadAssetAtPath<InputScheme>(desiredAssetPath) != null)
            {
                Debug.LogWarning($"{nameof(InputScheme)} '{scheme.name}' уже существует и не будет создан заново.");
                return;
            }
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(desiredAssetPath);
    
            Debug.Log($"Создаю {nameof(InputScheme)} '{scheme.name}' по адресу '{assetPath}' ...");        
            
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"{nameof(InputScheme)} '{scheme.name}' успешно создан!");      
        }
#endif

        public bool Equals(InputScheme other)
        {
            if (other == null)
               return false;

            return other.Equals(Scheme);
        }
        
        public bool Equals(InputControlScheme other)
        {
            return other.Equals(Scheme);
        }
    }
}
