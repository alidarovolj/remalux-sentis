using UnityEngine;
#if UNITY_AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
#endif
using UnityEngine.UI;
#if UNITY_XR_CORE_UTILS_PRESENT
using Unity.XR.CoreUtils;
#endif
using System.Collections;
using System.Reflection;
using DuluxVisualizer;

/// <summary>
/// Скрипт для настройки демо-сцены с SentisWallSegmentation
/// </summary>
public class SentisWallSegmentationDemo : MonoBehaviour
{
      [Header("AR Components")]
#if UNITY_AR_FOUNDATION_PRESENT
      public UnityEngine.XR.ARFoundation.ARSession arSession;
#else
      public DuluxVisualizer.ARSession arSession;
#endif

#if UNITY_XR_CORE_UTILS_PRESENT
      public Unity.XR.CoreUtils.XROrigin xrOrigin;
#else
      public GameObject xrOrigin;
#endif

#if UNITY_AR_FOUNDATION_PRESENT
      public UnityEngine.XR.ARFoundation.ARCameraManager cameraManager;
      public UnityEngine.XR.ARFoundation.ARPlaneManager planeManager;
#else
      public DuluxVisualizer.ARCameraManager cameraManager;
      public DuluxVisualizer.ARPlaneManager planeManager;
#endif

      [Header("Segmentation")]
      public SentisWallSegmentation wallSegmentation;
      public RenderTexture segmentationMask;

      [Header("Wall Paint")]
      public WallPaintBlit wallPaintBlit;
      public Color paintColor = new Color(0.2f, 0.5f, 0.9f, 0.8f);
      public float paintOpacity = 0.7f;

      [Header("UI")]
      public Canvas debugCanvas;
      public RawImage debugImage;
      public Text infoText;

      private void Start()
      {
            // Проверяем и настраиваем компоненты
            SetupARComponents();
            SetupSegmentation();
            SetupWallPainting();
            SetupDebugUI();
      }

      private void SetupARComponents()
      {
            // Находим компоненты, если они не назначены
#if UNITY_AR_FOUNDATION_PRESENT
            if (arSession == null)
                  arSession = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSession>();
#else
            if (arSession == null)
                  arSession = FindObjectOfType<DuluxVisualizer.ARSession>();
#endif

#if UNITY_XR_CORE_UTILS_PRESENT
            if (xrOrigin == null)
                  xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
#else
            if (xrOrigin == null)
                  xrOrigin = FindObjectOfType<GameObject>();
#endif

#if UNITY_AR_FOUNDATION_PRESENT
            if (cameraManager == null)
                  cameraManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARCameraManager>();

            if (planeManager == null)
                  planeManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARPlaneManager>();

            // Настраиваем распознавание вертикальных поверхностей (стен)
            if (planeManager != null)
            {
                  // Use extension method to set vertical detection mode
                  // Comment out this line as SetVerticalDetectionMode is not available
                  // planeManager.SetVerticalDetectionMode();
            }
#else
            if (cameraManager == null)
                  cameraManager = FindObjectOfType<DuluxVisualizer.ARCameraManager>();

            if (planeManager == null)
                  planeManager = FindObjectOfType<DuluxVisualizer.ARPlaneManager>();
#endif
      }

      private void SetupSegmentation()
      {
            // Находим компонент сегментации, если он не назначен
            if (wallSegmentation == null)
            {
                  // Ищем в сцене
                  wallSegmentation = FindObjectOfType<SentisWallSegmentation>();

                  // Если не найден, создаем новый
                  if (wallSegmentation == null && cameraManager != null)
                  {
                        GameObject cameraObject = cameraManager.gameObject;
                        wallSegmentation = cameraObject.AddComponent<SentisWallSegmentation>();
                  }
            }

            // Настраиваем компонент сегментации
            if (wallSegmentation != null)
            {
                  // Устанавливаем приватное поле arCamera через рефлексию
                  Camera arCamera = null;

                  // Пытаемся найти AR камеру в иерархии
                  if (cameraManager != null)
                  {
                        arCamera = cameraManager.GetComponent<Camera>();
                  }

                  // Если не нашли, ищем mainCamera
                  if (arCamera == null)
                  {
                        arCamera = Camera.main;
                  }

                  // Устанавливаем приватное поле через рефлексию
                  if (arCamera != null)
                  {
                        var arCameraField = typeof(SentisWallSegmentation).GetField("arCamera",
                              BindingFlags.NonPublic |
                              BindingFlags.Instance);

                        if (arCameraField != null)
                        {
                              arCameraField.SetValue(wallSegmentation, arCamera);
                              Debug.Log("Установлено приватное поле arCamera через рефлексию");
                        }
                  }

                  // Создаем маску сегментации, если не назначена
                  if (segmentationMask == null)
                  {
                        segmentationMask = new RenderTexture(256, 256, 0, RenderTextureFormat.R8);
                        segmentationMask.Create();
                  }

                  wallSegmentation.outputRenderTexture = segmentationMask;

                  // Включаем отладочную визуализацию
                  wallSegmentation.EnableDebugVisualization(true);
            }
      }

      private void SetupWallPainting()
      {
            // Находим компонент перекраски стен, если он не назначен
            if (wallPaintBlit == null)
            {
                  // Ищем в сцене
                  wallPaintBlit = FindObjectOfType<WallPaintBlit>();

                  // Если не найден, создаем новый
                  if (wallPaintBlit == null && cameraManager != null)
                  {
                        GameObject cameraObject = cameraManager.gameObject;
                        wallPaintBlit = cameraObject.AddComponent<WallPaintBlit>();
                  }
            }

            // Настраиваем компонент перекраски
            if (wallPaintBlit != null && segmentationMask != null)
            {
                  wallPaintBlit.maskTexture = segmentationMask;
                  wallPaintBlit.paintColor = paintColor;
                  wallPaintBlit.opacity = paintOpacity;
            }
      }

      private void SetupDebugUI()
      {
            // Создаем канвас для отладки, если он не назначен
            if (debugCanvas == null)
            {
                  // Ищем существующий канвас
                  debugCanvas = FindObjectOfType<Canvas>();

                  // Если канвас не найден, создаем новый
                  if (debugCanvas == null)
                  {
                        GameObject canvasObject = new GameObject("Debug Canvas");
                        debugCanvas = canvasObject.AddComponent<Canvas>();
                        canvasObject.AddComponent<CanvasScaler>();
                        canvasObject.AddComponent<GraphicRaycaster>();

                        // Настраиваем канвас
                        debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                  }
            }

            // Создаем отладочное изображение, если оно не назначено
            if (debugImage == null && debugCanvas != null)
            {
                  GameObject imageObject = new GameObject("Debug Image");
                  imageObject.transform.SetParent(debugCanvas.transform, false);
                  debugImage = imageObject.AddComponent<RawImage>();

                  // Настраиваем изображение
                  RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
                  rectTransform.anchorMin = new Vector2(0, 0.7f);
                  rectTransform.anchorMax = new Vector2(0.3f, 1f);
                  rectTransform.offsetMin = new Vector2(10, 10);
                  rectTransform.offsetMax = new Vector2(-10, -10);

                  // Назначаем изображение для отладки сегментации
                  if (wallSegmentation != null)
                  {
                        // Задаем ссылку на RawImage для отображения отладочной информации
                        var field = typeof(SentisWallSegmentation).GetField("debugImage",
                            BindingFlags.NonPublic |
                            BindingFlags.Instance);

                        if (field != null)
                        {
                              field.SetValue(wallSegmentation, debugImage);
                        }
                  }
            }

            // Создаем текст с информацией, если он не назначен
            if (infoText == null && debugCanvas != null)
            {
                  GameObject textObject = new GameObject("Info Text");
                  textObject.transform.SetParent(debugCanvas.transform, false);
                  infoText = textObject.AddComponent<Text>();

                  // Настраиваем текст
                  infoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                  infoText.fontSize = 14;
                  infoText.color = Color.white;
                  infoText.text = "Wall Segmentation Demo\nTouch screen to change color";

                  RectTransform rectTransform = textObject.GetComponent<RectTransform>();
                  rectTransform.anchorMin = new Vector2(0, 0);
                  rectTransform.anchorMax = new Vector2(1, 0.1f);
                  rectTransform.offsetMin = new Vector2(10, 10);
                  rectTransform.offsetMax = new Vector2(-10, -10);
            }
      }

      public void Update()
      {
            // Обновляем информационный текст
            if (infoText != null && wallSegmentation != null)
            {
                  string mode = wallSegmentation.IsUsingDemoMode() ? "Demo" : "ML Model";
                  infoText.text = $"Mode: {mode}\nFPS: {(int)(1.0f / Time.deltaTime)}";
            }

            // Изменение цвета по нажатию на экран
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && wallPaintBlit != null)
            {
                  // Генерируем новый случайный цвет
                  wallPaintBlit.paintColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
            }
      }
}