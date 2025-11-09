#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MadeYellow.InputBus.Components
{
    [CustomEditor(typeof(InputController))]
    public class InputControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Отрисовываем стандартный инспектор
            DrawDefaultInspector();

            InputController controller = (InputController)target;

            GUILayout.Space(20);

            // Большая кнопка для генерации схем
            if (GUILayout.Button("Сгенерировать InputScheme", GUILayout.Height(40)))
            {
                controller.GenerateInputSchemesAndAddToService();
            }

            GUILayout.Space(10);

            // Поясняющий текст
            EditorGUILayout.HelpBox(
                "Нажмите кнопку выше чтобы:\n" +
                "1. Создать InputScheme ассеты для всех схем управления\n" +
                "2. Автоматически добавить их в InputService",
                MessageType.Info
                );
        }
    }
}
#endif
