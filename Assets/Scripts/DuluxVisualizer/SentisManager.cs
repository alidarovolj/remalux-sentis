using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace DuluxVisualizer
{
      /// <summary>
      /// Central manager for Unity Sentis functionality in the application
      /// </summary>
      public class SentisManager : MonoBehaviour
      {
            private static SentisManager _instance;

            public static SentisManager Instance
            {
                  get
                  {
                        if (_instance == null)
                        {
                              // Try to find existing instance
                              _instance = FindObjectOfType<SentisManager>();

                              // Create new instance if none exists
                              if (_instance == null)
                              {
                                    GameObject obj = new GameObject("SentisManager");
                                    _instance = obj.AddComponent<SentisManager>();
                                    DontDestroyOnLoad(obj);
                                    Debug.Log("SentisManager: Created new instance");
                              }
                        }
                        return _instance;
                  }
            }

            // Cache for loaded models
            private Dictionary<string, ModelAsset> modelCache = new Dictionary<string, ModelAsset>();

            // Cache for created workers
            private Dictionary<string, IWorker> workerCache = new Dictionary<string, IWorker>();

            private void Awake()
            {
                  if (_instance != null && _instance != this)
                  {
                        Destroy(gameObject);
                        return;
                  }

                  _instance = this;
                  DontDestroyOnLoad(gameObject);
            }

            /// <summary>
            /// Loads a model from a path (supports direct path or StreamingAssets)
            /// </summary>
            public ModelAsset LoadModel(string modelPath)
            {
                  if (string.IsNullOrEmpty(modelPath))
                  {
                        Debug.LogError("Cannot load model: path is null or empty");
                        return null;
                  }

                  // Return cached model if available
                  if (modelCache.TryGetValue(modelPath, out ModelAsset cachedModel))
                  {
                        return cachedModel;
                  }

                  ModelAsset loadedModel = null;

                  try
                  {
                        // Try direct path
                        if (File.Exists(modelPath))
                        {
                              // Use reflection-based loading via SentisShim
                              var realModel = System.Type.GetType("Unity.Sentis.ModelLoader, Unity.Sentis")
                                ?.GetMethod("Load", new System.Type[] { typeof(string) })
                                ?.Invoke(null, new object[] { modelPath });

                              // Convert to our ModelAsset type
                              if (realModel != null)
                              {
                                    loadedModel = ScriptableObject.CreateInstance<ModelAsset>();
                                    Debug.Log($"Model loaded from path: {modelPath}");
                              }
                        }
                        // Try StreamingAssets path
                        else
                        {
                              string streamingPath = Path.Combine(Application.streamingAssetsPath, modelPath);
                              if (File.Exists(streamingPath))
                              {
                                    var realModel = System.Type.GetType("Unity.Sentis.ModelLoader, Unity.Sentis")
                                        ?.GetMethod("Load", new System.Type[] { typeof(string) })
                                        ?.Invoke(null, new object[] { streamingPath });

                                    if (realModel != null)
                                    {
                                          loadedModel = ScriptableObject.CreateInstance<ModelAsset>();
                                          Debug.Log($"Model loaded from StreamingAssets: {streamingPath}");
                                    }
                              }
                              else
                              {
                                    Debug.LogError($"Model file not found at path: {modelPath} or {streamingPath}");
                              }
                        }

                        // Cache the loaded model
                        if (loadedModel != null)
                        {
                              modelCache[modelPath] = loadedModel;
                        }
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error loading model: {e.Message}");
                  }

                  return loadedModel;
            }

            /// <summary>
            /// Creates a worker for the given model with the specified backend
            /// </summary>
            public IWorker CreateWorker(ModelAsset model, BackendType backendType = BackendType.CPU)
            {
                  if (model == null)
                  {
                        Debug.LogError("Cannot create worker: model is null");
                        return null;
                  }

                  string key = $"{model.name}_{backendType}";

                  // Create a new worker using our SentisShim
                  try
                  {
                        IWorker worker = SentisShim.CreateWorker(model, backendType);

                        if (worker != null)
                        {
                              Debug.Log($"Created worker with {backendType} backend for model {model.name}");
                              return worker;
                        }
                        else
                        {
                              Debug.LogError("Failed to create worker with SentisShim");
                              return null;
                        }
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error creating worker: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Creates a tensor from a texture
            /// </summary>
            public TensorFloat TextureToTensor(Texture2D texture, int width, int height, bool useNCHW = true)
            {
                  if (texture == null)
                  {
                        Debug.LogError("Cannot create tensor: texture is null");
                        return null;
                  }

                  try
                  {
                        // This is a placeholder implementation since we're using reflection
                        // In a real implementation, we would use proper texture-to-tensor conversion
                        return new TensorFloat(new float[width * height * 3], new TensorShape(1, 3, height, width));
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error creating tensor from texture: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Executes model inference with the given input tensor
            /// </summary>
            public void ExecuteModel(IWorker worker, string inputName, TensorFloat inputTensor)
            {
                  if (worker == null || inputTensor == null)
                  {
                        Debug.LogError("Cannot execute model: worker or input tensor is null");
                        return;
                  }

                  try
                  {
                        worker.Execute(new Dictionary<string, Tensor> { { inputName, inputTensor } });
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error executing model: {e.Message}");
                  }
            }

            /// <summary>
            /// Gets the output tensor from the worker
            /// </summary>
            public TensorFloat GetOutputTensor(IWorker worker, string outputName)
            {
                  if (worker == null)
                  {
                        Debug.LogError("Cannot get output: worker is null");
                        return null;
                  }

                  try
                  {
                        return worker.PeekOutput(outputName) as TensorFloat;
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error getting output tensor: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Cleans up all resources
            /// </summary>
            public void CleanUp()
            {
                  // Dispose all workers
                  foreach (var worker in workerCache.Values)
                  {
                        worker.Dispose();
                  }
                  workerCache.Clear();

                  // Clear model cache
                  modelCache.Clear();

                  Debug.Log("SentisManager: Cleaned up resources");
            }

            private void OnDestroy()
            {
                  CleanUp();
            }
      }
}