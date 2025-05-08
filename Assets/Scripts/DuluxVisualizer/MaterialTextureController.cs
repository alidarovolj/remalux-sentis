using UnityEngine;
using System.Collections.Generic;

namespace DuluxVisualizer
{
      /// <summary>
      /// Компонент для управления текстурами материалов и их применения в процессе виртуального окрашивания стен
      /// </summary>
      public class MaterialTextureController : MonoBehaviour
      {
            [System.Serializable]
            public class MaterialPreset
            {
                  public string name = "Default";
                  public Texture2D albedoTexture; // Основная текстура материала
                  public Texture2D normalMap;     // Карта нормалей для рельефа
                  public Texture2D roughnessMap;  // Карта шероховатости поверхности
                  public float glossiness = 0.5f; // Глянцевость материала (0 - матовый, 1 - глянцевый)
                  public float tiling = 1.0f;     // Масштабирование текстуры на поверхности
                  [Range(0f, 1f)]
                  public float blendWithBaseColor = 0.7f; // Коэффициент смешивания с базовым цветом
            }

            [Header("Материалы")]
            [SerializeField] private List<MaterialPreset> materialPresets = new List<MaterialPreset>();
            [SerializeField] private int currentMaterialIndex = 0;

            [Header("Эффект перекраски")]
            [SerializeField] private ImprovedWallPaintBlit wallPaintEffect;
            [SerializeField] private Shader enhancedPaintShader; // Enhanced shader with material support

            // Свойства для доступа из других скриптов
            private Material enhancedMaterial;
            private RenderTexture compositedTexture;
            private bool isInitialized = false;

            // Свойства для доступа к текущему материалу
            public MaterialPreset CurrentMaterial
            {
                  get
                  {
                        if (materialPresets.Count > 0 && currentMaterialIndex >= 0 && currentMaterialIndex < materialPresets.Count)
                              return materialPresets[currentMaterialIndex];
                        return null;
                  }
            }

            private void Start()
            {
                  InitializeTextures();
                  if (wallPaintEffect != null)
                  {
                        // Подписываемся на события изменения цвета
                        var enhancedDemo = GetComponent<EnhancedWallPaintDemo>();
                        if (enhancedDemo != null)
                        {
                              // Логика для подписки на события изменения цвета в демо
                        }
                  }
            }

            private void InitializeTextures()
            {
                  if (isInitialized) return;

                  // Инициализируем текстуру для композитинга
                  if (compositedTexture == null)
                  {
                        compositedTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
                        compositedTexture.name = "MaterialCompositedTexture";
                        compositedTexture.wrapMode = TextureWrapMode.Repeat;
                        compositedTexture.filterMode = FilterMode.Bilinear;
                        compositedTexture.Create();
                  }

                  // Загружаем улучшенный шейдер, если он не задан
                  if (enhancedPaintShader == null)
                  {
                        enhancedPaintShader = Shader.Find("Hidden/EnhancedWallPaint");
                        if (enhancedPaintShader == null)
                        {
                              Debug.LogWarning("EnhancedWallPaint shader not found! Material textures may not work correctly.");
                        }
                  }

                  // Создаем материал для композитинга
                  if (enhancedPaintShader != null && enhancedMaterial == null)
                  {
                        enhancedMaterial = new Material(enhancedPaintShader);
                  }

                  // Добавляем стандартные пресеты материалов, если список пуст
                  if (materialPresets.Count == 0)
                  {
                        AddDefaultMaterialPresets();
                  }

                  isInitialized = true;
            }

            // Создание стандартных пресетов материалов
            private void AddDefaultMaterialPresets()
            {
                  // Гладкая краска (по умолчанию)
                  MaterialPreset smoothPaint = new MaterialPreset
                  {
                        name = "Гладкая краска",
                        glossiness = 0.3f,
                        blendWithBaseColor = 0.9f,
                        tiling = 1.0f
                  };
                  materialPresets.Add(smoothPaint);

                  // Матовая краска
                  MaterialPreset mattePaint = new MaterialPreset
                  {
                        name = "Матовая краска",
                        glossiness = 0.1f,
                        blendWithBaseColor = 0.95f,
                        tiling = 1.0f
                  };
                  materialPresets.Add(mattePaint);

                  // Глянцевая краска
                  MaterialPreset glossyPaint = new MaterialPreset
                  {
                        name = "Глянцевая краска",
                        glossiness = 0.8f,
                        blendWithBaseColor = 0.8f,
                        tiling = 1.0f
                  };
                  materialPresets.Add(glossyPaint);

                  // Загружаем стандартные текстуры из ресурсов, если они доступны
                  Texture2D defaultNormal = Resources.Load<Texture2D>("Textures/Materials/default_normal");
                  Texture2D defaultRoughness = Resources.Load<Texture2D>("Textures/Materials/default_roughness");

                  if (defaultNormal != null && defaultRoughness != null)
                  {
                        foreach (var preset in materialPresets)
                        {
                              preset.normalMap = defaultNormal;
                              preset.roughnessMap = defaultRoughness;
                        }
                  }
                  else
                  {
                        Debug.LogWarning("Default material textures not found in Resources. Create them at Resources/Textures/Materials/");
                  }
            }

            /// <summary>
            /// Переключает на следующий материал из списка пресетов
            /// </summary>
            public void NextMaterial()
            {
                  if (materialPresets.Count == 0) return;
                  currentMaterialIndex = (currentMaterialIndex + 1) % materialPresets.Count;
                  ApplyCurrentMaterial();
            }

            /// <summary>
            /// Устанавливает материал по индексу
            /// </summary>
            /// <param name="index">Индекс материала в списке пресетов</param>
            public void SetMaterial(int index)
            {
                  if (index >= 0 && index < materialPresets.Count)
                  {
                        currentMaterialIndex = index;
                        ApplyCurrentMaterial();
                  }
            }

            /// <summary>
            /// Применяет текущий материал к эффекту перекраски
            /// </summary>
            public void ApplyCurrentMaterial()
            {
                  if (!isInitialized)
                        InitializeTextures();

                  if (wallPaintEffect == null || CurrentMaterial == null)
                        return;

                  // Применяем текстуры и параметры материала к шейдеру
                  if (enhancedMaterial != null)
                  {
                        // Обновляем текстуры
                        if (CurrentMaterial.albedoTexture != null)
                              enhancedMaterial.SetTexture("_MaterialAlbedo", CurrentMaterial.albedoTexture);

                        if (CurrentMaterial.normalMap != null)
                              enhancedMaterial.SetTexture("_MaterialNormal", CurrentMaterial.normalMap);

                        if (CurrentMaterial.roughnessMap != null)
                              enhancedMaterial.SetTexture("_MaterialRoughness", CurrentMaterial.roughnessMap);

                        // Обновляем параметры
                        enhancedMaterial.SetFloat("_MaterialGlossiness", CurrentMaterial.glossiness);
                        enhancedMaterial.SetFloat("_MaterialTiling", CurrentMaterial.tiling);
                        enhancedMaterial.SetFloat("_BlendWithBaseColor", CurrentMaterial.blendWithBaseColor);

                        // Создаем композитную текстуру материала
                        UpdateCompositedMaterialTexture();

                        // Устанавливаем параметры для улучшенного эффекта окрашивания через рефлексию
                        System.Type paintType = wallPaintEffect.GetType();

                        // Устанавливаем ссылку на материал с поддержкой текстур
                        var materialField = paintType.GetField("_enhancedMaterial",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (materialField != null)
                        {
                              materialField.SetValue(wallPaintEffect, enhancedMaterial);
                        }

                        // Устанавливаем ссылку на композитную текстуру
                        var textureField = paintType.GetField("_compositedTexture",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (textureField != null)
                        {
                              textureField.SetValue(wallPaintEffect, compositedTexture);
                        }

                        // Активируем использование текстур материалов
                        var useMaterialsField = paintType.GetField("_useMaterialTextures",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (useMaterialsField != null)
                        {
                              useMaterialsField.SetValue(wallPaintEffect, true);
                        }

                        Debug.Log($"Применен материал: {CurrentMaterial.name}, глянцевость: {CurrentMaterial.glossiness}");
                  }
            }

            /// <summary>
            /// Создает композитную текстуру материала для использования в шейдере
            /// </summary>
            private void UpdateCompositedMaterialTexture()
            {
                  if (enhancedMaterial == null || compositedTexture == null)
                        return;

                  // Если у нас есть специальный шейдер для композитинга, используем его
                  Shader compositingShader = Shader.Find("Hidden/MaterialCompositing");
                  if (compositingShader != null)
                  {
                        Material compositingMaterial = new Material(compositingShader);

                        // Устанавливаем текстуры
                        if (CurrentMaterial.albedoTexture != null)
                              compositingMaterial.SetTexture("_AlbedoTex", CurrentMaterial.albedoTexture);

                        if (CurrentMaterial.normalMap != null)
                              compositingMaterial.SetTexture("_NormalTex", CurrentMaterial.normalMap);

                        if (CurrentMaterial.roughnessMap != null)
                              compositingMaterial.SetTexture("_RoughnessTex", CurrentMaterial.roughnessMap);

                        // Устанавливаем параметры
                        compositingMaterial.SetFloat("_Glossiness", CurrentMaterial.glossiness);
                        compositingMaterial.SetFloat("_Tiling", CurrentMaterial.tiling);

                        // Создаем временную RenderTexture
                        RenderTexture tempRT = RenderTexture.GetTemporary(
                            compositedTexture.width,
                            compositedTexture.height,
                            0,
                            compositedTexture.format);

                        // Выполняем блит с нашим материалом
                        Graphics.Blit(null, tempRT, compositingMaterial);
                        Graphics.Blit(tempRT, compositedTexture);

                        // Освобождаем ресурсы
                        RenderTexture.ReleaseTemporary(tempRT);
                        Destroy(compositingMaterial);
                  }
                  else
                  {
                        // Если нет шейдера для композитинга, просто копируем альбедо или создаем базовую текстуру
                        if (CurrentMaterial.albedoTexture != null)
                        {
                              Graphics.Blit(CurrentMaterial.albedoTexture, compositedTexture);
                        }
                        else
                        {
                              // Создаем базовую текстуру с учетом глянцевости
                              Color baseColor = new Color(0.5f, 0.5f, 0.5f, CurrentMaterial.glossiness);
                              Texture2D defaultTex = new Texture2D(1, 1);
                              defaultTex.SetPixel(0, 0, baseColor);
                              defaultTex.Apply();

                              Graphics.Blit(defaultTex, compositedTexture);
                              Destroy(defaultTex);
                        }
                  }
            }

            /// <summary>
            /// Добавляет новый пресет материала с указанными текстурами
            /// </summary>
            public void AddMaterialPreset(string name, Texture2D albedo, Texture2D normal, Texture2D roughness, float glossiness, float tiling)
            {
                  MaterialPreset newPreset = new MaterialPreset
                  {
                        name = name,
                        albedoTexture = albedo,
                        normalMap = normal,
                        roughnessMap = roughness,
                        glossiness = glossiness,
                        tiling = tiling
                  };

                  materialPresets.Add(newPreset);
                  Debug.Log($"Добавлен новый материал: {name}");
            }

            private void OnDestroy()
            {
                  // Освобождаем ресурсы
                  if (enhancedMaterial != null)
                  {
                        if (Application.isPlaying)
                              Destroy(enhancedMaterial);
                        else
                              DestroyImmediate(enhancedMaterial);
                  }

                  if (compositedTexture != null)
                  {
                        compositedTexture.Release();
                        if (Application.isPlaying)
                              Destroy(compositedTexture);
                        else
                              DestroyImmediate(compositedTexture);
                  }
            }
      }
}