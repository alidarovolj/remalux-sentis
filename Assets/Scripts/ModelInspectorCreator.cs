using UnityEngine;

/// <summary>
/// Создает объект с компонентом ModelInspector при запуске сцены
/// </summary>
public class ModelInspectorCreator : MonoBehaviour
{
      private static bool isCreated = false;

      void Awake()
      {
            // Проверяем, был ли уже создан объект с ModelInspector
            if (!isCreated)
            {
                  // Создаем новый GameObject
                  GameObject inspectorObject = new GameObject("ModelInspector");

                  // Добавляем компонент ModelInspector
                  inspectorObject.AddComponent<ModelInspector>();

                  // Делаем объект постоянным между сценами
                  DontDestroyOnLoad(inspectorObject);

                  // Устанавливаем флаг, что объект уже создан
                  isCreated = true;

                  Debug.Log("ModelInspector создан и готов к использованию");
            }
      }
}