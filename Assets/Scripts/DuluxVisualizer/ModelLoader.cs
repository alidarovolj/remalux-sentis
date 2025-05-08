using UnityEngine;
using System.Collections.Generic;
#if UNITY_SENTIS
using Unity.Sentis;
#endif
using System;
using System.IO;

namespace DuluxVisualizer
{
      /// <summary>
      /// Компонент для загрузки и подготовки моделей Sentis ONNX
      /// </summary>
      public class ModelLoader : MonoBehaviour
      {
            [SerializeField] private ModelAsset _modelAsset;
            [SerializeField] private string _modelInputName = "input";
            [SerializeField] private string _modelOutputName = "output";
            [SerializeField] private bool _useNCHW = true;
            [SerializeField] private int _inputWidth = 128;
            [SerializeField] private int _inputHeight = 128;

#if UNITY_SENTIS
            private Model _runtimeModel;
            private Worker _engine;
            private BackendType _inferenceDevice = BackendType.CPU;
#else
            private ScriptableObject _runtimeModel;
            private object _engine;
            private object _inferenceDevice;
#endif
            private bool _isModelReady = false;

            public bool IsModelReady => _isModelReady;

            // Событие для оповещения о готовности модели
            public delegate void ModelReadyDelegate(bool success);
            public event ModelReadyDelegate OnModelReady;

            private void Awake()
            {
                  // Пытаемся загрузить модель автоматически при старте
                  if (_modelAsset != null)
                  {
                        LoadModel(_modelAsset);
                  }
                  else
                  {
                        // Пытаемся найти модель в Resources
                        ModelAsset model = Resources.Load<ModelAsset>("Models/model");
                        if (model != null)
                        {
                              LoadModel(model);
                        }
                        else
                        {
                              Debug.LogWarning("Модель сегментации не найдена в Resources/Models.");
                        }
                  }
            }

            /// <summary>
            /// Загружает ONNX модель и подготавливает движок для инференса
            /// </summary>
            public bool LoadModel(ModelAsset modelAsset)
            {
                  if (modelAsset == null)
                  {
                        Debug.LogError("ModelLoader: Не указана модель для загрузки.");
                        return false;
                  }

                  try
                  {
                        // Создаем рантайм-модель из ассета, используя класс Unity.Sentis.ModelLoader
                        _runtimeModel = Unity.Sentis.ModelLoader.Load(modelAsset);

                        // Создаем воркер для инференса
                        BackendType workerType = BackendType.CPU;

                        // Если доступно GPU, используем его
                        if (SystemInfo.supportsComputeShaders && _inferenceDevice == BackendType.GPUCompute)
                        {
                              workerType = BackendType.GPUCompute;
                              Debug.Log("ModelLoader: Используем GPU для инференса.");
                        }
                        else
                        {
                              Debug.Log("ModelLoader: Используем CPU для инференса.");
                        }

                        // Освобождаем предыдущий движок, если он был
                        if (_engine != null)
                        {
                              _engine.Dispose();
                        }

                        // Создаем новый движок в соответствии с API Sentis 2.1.2
                        _engine = new Worker(_runtimeModel, workerType);

                        // Анализируем входы-выходы модели
                        AnalyzeModel();

                        _isModelReady = true;
                        OnModelReady?.Invoke(true);
                        return true;
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"ModelLoader: Ошибка при загрузке модели: {e.Message}");
                        _isModelReady = false;
                        OnModelReady?.Invoke(false);
                        return false;
                  }
            }

            /// <summary>
            /// Анализирует модель и выводит информацию о входах и выходах
            /// </summary>
            private void AnalyzeModel()
            {
                  if (_runtimeModel == null) return;

                  Debug.Log($"ModelLoader: Загружена модель");
                  Debug.Log($"  Входы ({_runtimeModel.inputs.Count}):");
                  foreach (var input in _runtimeModel.inputs)
                  {
                        Debug.Log($"    {input.name}: shape={input.shape}");
                  }

                  Debug.Log($"  Выходы ({_runtimeModel.outputs.Count}):");
                  foreach (var output in _runtimeModel.outputs)
                  {
                        Debug.Log($"    {output.name}");
                  }
            }

            /// <summary>
            /// Выполняет инференс модели с заданным тензором входа
            /// </summary>
#if UNITY_SENTIS
            public Tensor Execute(Tensor inputTensor)
            {
                  if (!_isModelReady || _engine == null)
                  {
                        Debug.LogWarning("ModelLoader: Модель не готова для инференса.");
                        return null;
                  }

                  try
                  {
                        // Выполняем инференс
                        _engine.Schedule(inputTensor);

                        // Получаем результат
                        Tensor outputTensor = _engine.PeekOutput(_modelOutputName) as Tensor;
                        return outputTensor;
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"ModelLoader: Ошибка при инференсе: {e.Message}");
                        return null;
                  }
            }
#else
            public object Execute(object inputTensor)
            {
                  Debug.LogWarning("ModelLoader: Sentis not available for inference.");
                  return null;
            }
#endif

            /// <summary>
            /// Создает входной тензор из текстуры
            /// </summary>
#if UNITY_SENTIS
            public Tensor TextureToTensor(Texture2D texture)
            {
                  if (texture == null)
                  {
                        Debug.LogError("ModelLoader: Текстура равна null.");
                        return null;
                  }

                  try
                  {
                        // Создаем тензор из текстуры с преобразованием размера в соответствии с API Sentis 2.1.2
                        var transform = new TextureTransform()
                              .SetDimensions(_inputWidth, _inputHeight)
                              .SetTensorLayout(_useNCHW ? TensorLayout.NCHW : TensorLayout.NHWC);

                        // Создаем тензор с нужной формой
                        TensorShape shape = new TensorShape(1, 3, _inputHeight, _inputWidth); // Batch, Channels, Height, Width (NCHW format)
                        if (!_useNCHW)
                              shape = new TensorShape(1, _inputHeight, _inputWidth, 3); // Batch, Height, Width, Channels (NHWC format)

                        var tensor = new TensorFloat(shape);

                        // Конвертируем текстуру в тензор
                        TextureConverter.ToTensor(texture, tensor, transform);
                        return tensor;
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"ModelLoader: Ошибка при конвертации текстуры в тензор: {e.Message}");
                        return null;
                  }
            }
#else
            public object TextureToTensor(Texture2D texture)
            {
                  Debug.LogWarning("ModelLoader: Sentis not available for texture conversion.");
                  return null;
            }
#endif

#if UNITY_SENTIS
            /// <summary>
            /// Статический метод для загрузки модели напрямую из ModelAsset
            /// </summary>
            public static Model Load(ModelAsset modelAsset)
            {
                  if (modelAsset == null)
                  {
                        Debug.LogError("ModelLoader: Модель равна null.");
                        return null;
                  }

                  try
                  {
                        return Unity.Sentis.ModelLoader.Load(modelAsset);
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"ModelLoader: Ошибка при загрузке модели: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Loads a model from a file path (relative to StreamingAssets)
            /// </summary>
            public static Model LoadFromStreamingAssets(string relativePath)
            {
                  string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);

                  if (!File.Exists(fullPath))
                        throw new FileNotFoundException($"Model file not found at {fullPath}");

                  try
                  {
                        // First try loading as asset
                        ModelAsset modelAsset = Resources.Load<ModelAsset>(relativePath);
                        if (modelAsset != null)
                        {
                              return Unity.Sentis.ModelLoader.Load(modelAsset);
                        }

                        // File exists but couldn't load as asset - log error
                        Debug.LogError($"Found model file at {fullPath} but couldn't load it as ModelAsset");
                        return null;
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Failed to load model from path {fullPath}: {e.Message}");
                        throw;
                  }
            }

            /// <summary>
            /// Checks if a model is compatible with the current platform
            /// </summary>
            public static bool IsModelCompatible(ModelAsset modelAsset)
            {
                  if (modelAsset == null)
                        return false;

                  try
                  {
                        // Try to load the model - this will throw if incompatible
                        Model model = Unity.Sentis.ModelLoader.Load(modelAsset);
                        return model != null;
                  }
                  catch
                  {
                        return false;
                  }
            }

            /// <summary>
            /// Gets information about a model
            /// </summary>
            public static (List<string> inputs, List<string> outputs) GetModelInfo(ModelAsset modelAsset)
            {
                  if (modelAsset == null)
                        throw new ArgumentNullException(nameof(modelAsset), "Model asset cannot be null");

                  try
                  {
                        Model model = Unity.Sentis.ModelLoader.Load(modelAsset);

                        List<string> inputs = new List<string>();
                        foreach (var input in model.inputs)
                        {
                              inputs.Add(input.name);
                        }

                        List<string> outputs = new List<string>();
                        foreach (var output in model.outputs)
                        {
                              outputs.Add(output.name);
                        }

                        return (inputs, outputs);
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Failed to get model info: {e.Message}");
                        throw;
                  }
            }
#else
            /// <summary>
            /// Статический метод для загрузки модели напрямую из ModelAsset
            /// </summary>
            public static object Load(ScriptableObject modelAsset)
            {
                  Debug.LogWarning("ModelLoader: Sentis not available for model loading.");
                  return null;
            }

            /// <summary>
            /// Loads a model from a file path (relative to StreamingAssets)
            /// </summary>
            public static object LoadFromStreamingAssets(string relativePath)
            {
                  Debug.LogWarning("ModelLoader: Sentis not available for model loading from StreamingAssets.");
                  return null;
            }
#endif
      }
}