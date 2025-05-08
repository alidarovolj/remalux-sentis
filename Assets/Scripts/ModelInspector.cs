using System.IO;
using UnityEngine;
using System.Linq;
using DuluxVisualizer; // Use our compatibility namespace instead of Unity.Sentis

/// <summary>
/// Инструмент для анализа ONNX-моделей и инспекции их входных и выходных тензоров
/// </summary>
public class ModelInspector : MonoBehaviour
{
      [Header("Model Source")]
      [SerializeField] private DuluxVisualizer.ModelAsset modelAsset;
      [SerializeField] private string modelPath = "Assets/StreamingAssets/Models/model.onnx";

      [Header("Debug Options")]
      [SerializeField] private bool logOnStart = true; // Выводить информацию при запуске
      [SerializeField] private bool logTensorShapes = true; // Выводить формы тензоров

      void Start()
      {
            if (logOnStart)
            {
                  InspectModel();
            }
      }

      public void InspectModel()
      {
            DuluxVisualizer.ModelAsset model = modelAsset;

            // Пробуем загрузить модель из пути напрямую
            try
            {
                  if (File.Exists(modelPath))
                  {
                        // Use our compatibility layer's reflection-based loading
                        model = DuluxVisualizer.SentisShim.LoadModel(modelPath);
                        Debug.Log("Модель загружена из файла: " + modelPath);
                  }
                  else
                  {
                        Debug.LogError($"Файл модели не найден по пути: {modelPath}");

                        // Попробуем также искать в Application.streamingAssetsPath во время выполнения
                        string runtimePath = Path.Combine(Application.streamingAssetsPath, "Models/model.onnx");
                        if (File.Exists(runtimePath))
                        {
                              model = DuluxVisualizer.SentisShim.LoadModel(runtimePath);
                              Debug.Log("Модель загружена из StreamingAssets: " + runtimePath);
                        }
                        else
                        {
                              Debug.LogError($"Файл модели не найден и в StreamingAssets: {runtimePath}");
                        }
                  }
            }
            catch (System.Exception e)
            {
                  Debug.LogError("Ошибка при загрузке модели из файла: " + e.Message);
            }

            // Если не удалось загрузить из файла, пробуем использовать сериализованную модель
            if (model == null && modelAsset != null)
            {
                  model = modelAsset;
                  Debug.Log("Модель загружена из сериализованного ассета");
            }
            else if (model == null)
            {
                  Debug.LogError("Не удалось загрузить модель ни из файла, ни из ассета");
                  return;
            }

            // Выводим общую информацию о модели
            Debug.Log("=== Информация о модели ===");
            Debug.Log($"Имя: {model.name}");
            Debug.Log($"Всего входов: {model.inputs.Count}");
            Debug.Log($"Всего выходов: {model.outputs.Count}");

            // Выводим информацию о входах
            Debug.Log("\n=== Входы модели ===");
            foreach (var input in model.inputs)
            {
                  Debug.Log($"Имя: {input.name}");
                  Debug.Log($"Форма: {string.Join(",", input.shape)}");
                  Debug.Log($"Тип данных: {input.dataType}");
            }

            // Выводим информацию о выходах
            Debug.Log("\n=== Выходы модели ===");
            foreach (var output in model.outputs)
            {
                  Debug.Log($"Имя: {output}");
            }

            Debug.Log("Инспекция модели завершена!");
      }

      /// <summary>
      /// Запускает инспекцию модели из кода
      /// </summary>
      public void InspectModelFromButton()
      {
            InspectModel();
      }
}