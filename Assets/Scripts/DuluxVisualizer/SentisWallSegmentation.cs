using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Sentis;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

namespace DuluxVisualizer
{
      /// <summary>
      /// Component for wall segmentation using Unity Sentis neural network
      /// </summary>
      [RequireComponent(typeof(ARCameraManager))]
      public class SentisWallSegmentation : MonoBehaviour
      {
            // Режим работы сегментации стен
            public enum SegmentationMode
            {
                  Demo,             // Простая демонстрация без нейросети
                  EmbeddedModel,    // Используется модель, привязанная через инспектор
                  ExternalModel     // Используется модель из StreamingAssets
            }

            [Header("AR Components")]
            [SerializeField] public ARCameraManager cameraManager;
            [SerializeField] private Camera arCamera;

            [Header("Segmentation Mode")]
            [SerializeField] private SegmentationMode currentMode = SegmentationMode.ExternalModel;
            [SerializeField] private string externalModelPath = "Models/model.onnx"; // Путь к ONNX модели

            [Header("Sentis Model")]
            [SerializeField] public ModelAsset modelAsset; // Модель ONNX через asset
            [SerializeField] public bool forceUseEmbeddedModel = true; // Принудительно использовать встроенную модель

            [Header("Segmentation Parameters")]
            [SerializeField] private int inputWidth = 256; // Размер ширины входного изображения для модели
            [SerializeField] private int inputHeight = 256; // Размер высоты входного изображения для модели
            [SerializeField] private int inputChannels = 3; // RGB входное изображение (3 канала)
            [SerializeField] private string inputName = "input"; // Имя входа модели
            [SerializeField] private string outputName = "output"; // Имя выхода модели
            [SerializeField, Range(0, 1)] private float threshold = 0.5f; // Порог для бинаризации маски
            [SerializeField] private int wallClassIndex = 9; // Индекс класса стены в выходном тензоре

            [Header("Alternative Model Names")]
            [SerializeField] private string altInputName = "image"; // Альтернативное имя входа
            [SerializeField] private string altOutputName = "predict"; // Альтернативное имя выхода
            [SerializeField] private int altWallClassIndex = 9; // Альтернативный индекс класса стены

            [Header("Debug & Performance")]
            [SerializeField] private bool forceDemoMode = false; // Отключаем принудительный демо-режим
            [SerializeField] private bool showDebugVisualisation = true;
            [SerializeField] private RawImage debugImage;
            [SerializeField] private float processingInterval = 0.3f;
            [SerializeField] private bool enableDebugLogs = true; // Добавлен флаг включения отладочных логов
            [SerializeField] private bool debugPositioning = true; // Флаг для отладки позиционирования стен (включен по умолчанию)
            [SerializeField] private bool useARPlaneController = true; // Использовать ARPlaneController для управления плоскостями
            [SerializeField] private bool useSmoothInterpolation = true; // Сглаживание результатов между кадрами для устранения мерцания
            [SerializeField, Range(0f, 1f)] private float smoothingFactor = 0.5f; // Коэффициент сглаживания (0 - нет сглаживания, 1 - полное сглаживание)
            [SerializeField, Range(0, 3)] private int stabilizationBufferSize = 2; // Размер буфера стабилизации (0 - отключено)

            [Header("Wall Visualization")]
            [SerializeField] private Material wallMaterial; // Материал для стен
            [SerializeField] private Color wallColor = new Color(1, 1, 1, 0.8f); // Цвет для визуализации стен
            [SerializeField] private RenderTexture _outputRenderTexture; // Выходная маска сегментации для внешнего использования
            [SerializeField] private bool showDebugInfo = false;
            [SerializeField] private RawImage debugDisplay;

            // Property to access the output render texture
            public RenderTexture outputRenderTexture
            {
                  get { return _outputRenderTexture; }
                  set { _outputRenderTexture = value; }
            }

            // Приватные переменные
            private Texture2D cameraTexture;
            private Texture2D segmentationTexture;
            private Model runtimeModel;
            private Worker worker;
            private bool isProcessing;
            private Tensor inputTensor;
            private bool useDemoMode = false; // Используем демо-режим при ошибке модели
            private int errorCount = 0; // Счетчик ошибок нейросети
            private ModelAsset currentModelAsset; // Текущая используемая модель
            private List<GameObject> currentWalls = new List<GameObject>(); // Список текущих стен
            private List<GameObject> demoWalls = new List<GameObject>(); // Список демо-стен
            private bool isModelInitialized = false; // Флаг инициализации модели
            private float lastProcessTime = 0f; // Время последней обработки
            private bool useNCHW = true; // Флаг формата тензора

            // Буфер для сглаживания результатов
            private List<Texture2D> resultBuffer = new List<Texture2D>();
            private int currentBufferIndex = 0;
            private bool bufferInitialized = false;

            private WallSegmentation2D _segmentation2D;

            private void Awake()
            {
                  // Get required components
                  if (cameraManager == null)
                  {
                        cameraManager = GetComponent<ARCameraManager>();
                  }

                  if (arCamera == null && cameraManager != null)
                  {
                        arCamera = cameraManager.GetComponent<Camera>();
                  }

                  // Create segmentation 2D component
                  _segmentation2D = gameObject.AddComponent<WallSegmentation2D>();
            }

            private void Start()
            {
                  InitializeSegmentation();
                  StartCoroutine(ProcessFrames());
            }

            private void OnDestroy()
            {
                  // Release resources
                  ReleaseResources();
            }

            /// <summary>
            /// Initializes the segmentation system
            /// </summary>
            private void InitializeSegmentation()
            {
                  try
                  {
                        // Create output texture if not assigned
                        if (outputRenderTexture == null)
                        {
                              outputRenderTexture = new RenderTexture(inputWidth, inputHeight, 0, RenderTextureFormat.ARGB32);
                              outputRenderTexture.Create();
                        }

                        // Subscribe to camera frame received event
                        if (cameraManager != null)
                        {
                              cameraManager.frameReceived += OnCameraFrameReceived;
                        }

                        isModelInitialized = true;
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error initializing segmentation: {e.Message}");
                        useDemoMode = true;
                  }
            }

            /// <summary>
            /// Camera frame received event handler
            /// </summary>
            private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
            {
                  // Update camera texture - this provides the input for the neural network
                  if (cameraTexture == null || cameraTexture.width != inputWidth || cameraTexture.height != inputHeight)
                  {
                        if (cameraTexture != null)
                        {
                              Destroy(cameraTexture);
                        }

                        cameraTexture = new Texture2D(inputWidth, inputHeight, TextureFormat.RGBA32, false);
                  }

                  // The actual texture update is done in the ProcessFrames coroutine
            }

            /// <summary>
            /// Processes frames at regular intervals
            /// </summary>
            private IEnumerator ProcessFrames()
            {
                  // Wait for initialization
                  yield return new WaitForSeconds(0.5f);

                  while (true)
                  {
                        if (isModelInitialized && !isProcessing && cameraTexture != null)
                        {
                              isProcessing = true;

                              try
                              {
                                    // Capture camera image
                                    if (arCamera != null)
                                    {
                                          RenderTexture tempRT = RenderTexture.GetTemporary(inputWidth, inputHeight, 0, RenderTextureFormat.ARGB32);
                                          RenderTexture prevRT = RenderTexture.active;

                                          // Render camera to temporary texture
                                          arCamera.targetTexture = tempRT;
                                          arCamera.Render();
                                          arCamera.targetTexture = null;

                                          // Copy to texture
                                          RenderTexture.active = tempRT;
                                          cameraTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
                                          cameraTexture.Apply();

                                          // Restore RT state
                                          RenderTexture.active = prevRT;
                                          RenderTexture.ReleaseTemporary(tempRT);

                                          // Process the image using segmentation2D
                                          if (_segmentation2D != null)
                                          {
                                                _segmentation2D.ProcessImage(cameraTexture);

                                                // Update output texture
                                                RenderTexture segTexture = _segmentation2D.GetSegmentationTexture();
                                                if (segTexture != null)
                                                {
                                                      Graphics.Blit(segTexture, outputRenderTexture);
                                                }
                                          }
                                    }
                              }
                              catch (Exception e)
                              {
                                    Debug.LogError($"Error processing frame: {e.Message}");
                              }

                              isProcessing = false;
                        }

                        // Wait before processing next frame
                        yield return new WaitForSeconds(processingInterval);
                  }
            }

            /// <summary>
            /// Gets the current segmentation result as a texture
            /// </summary>
            public Texture2D GetSegmentationTexture()
            {
                  if (_segmentation2D != null)
                  {
                        RenderTexture rt = _segmentation2D.GetSegmentationTexture();
                        if (rt != null)
                        {
                              if (segmentationTexture == null ||
                                    segmentationTexture.width != rt.width ||
                                    segmentationTexture.height != rt.height)
                              {
                                    if (segmentationTexture != null)
                                          Destroy(segmentationTexture);

                                    segmentationTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                              }

                              RenderTexture.active = rt;
                              segmentationTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                              segmentationTexture.Apply();
                              RenderTexture.active = null;

                              return segmentationTexture;
                        }
                  }
                  return null;
            }

            /// <summary>
            /// Creates a demo segmentation texture for testing
            /// </summary>
            private Texture2D CreateDemoSegmentation(int width, int height)
            {
                  Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                  Color[] pixels = new Color[width * height];

                  // Create a simple pattern
                  for (int y = 0; y < height; y++)
                  {
                        for (int x = 0; x < width; x++)
                        {
                              // Circular mask in the center
                              float dx = x - width / 2;
                              float dy = y - height / 2;
                              float distance = Mathf.Sqrt(dx * dx + dy * dy);
                              float radius = Mathf.Min(width, height) * 0.4f;

                              if (distance < radius)
                              {
                                    pixels[y * width + x] = wallColor;
                              }
                              else
                              {
                                    pixels[y * width + x] = Color.clear;
                              }
                        }
                  }

                  texture.SetPixels(pixels);
                  texture.Apply();
                  return texture;
            }

            /// <summary>
            /// Releases resources
            /// </summary>
            private void ReleaseResources()
            {
                  if (cameraManager != null)
                  {
                        cameraManager.frameReceived -= OnCameraFrameReceived;
                  }

                  if (cameraTexture != null)
                  {
                        Destroy(cameraTexture);
                        cameraTexture = null;
                  }

                  if (segmentationTexture != null)
                  {
                        Destroy(segmentationTexture);
                        segmentationTexture = null;
                  }
            }

            /// <summary>
            /// Enables or disables debug visualization
            /// </summary>
            public void EnableDebugVisualization(bool enable)
            {
                  showDebugVisualisation = enable;
                  if (_segmentation2D != null)
                  {
                        // Call equivalent method in WallSegmentation2D if it exists
                        // _segmentation2D.EnableDebugVisualization(enable);
                  }
            }

            /// <summary>
            /// Checks if debug visualization is enabled
            /// </summary>
            public bool IsDebugVisualizationEnabled()
            {
                  return showDebugVisualisation;
            }

            /// <summary>
            /// Checks if using demo mode
            /// </summary>
            public bool IsUsingDemoMode()
            {
                  return useDemoMode || currentMode == SegmentationMode.Demo;
            }
      }
}