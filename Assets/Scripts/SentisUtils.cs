using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace DuluxVisualizer
{
      /// <summary>
      /// Utility class for Unity Sentis functionality
      /// Provides helper methods for working with Sentis models
      /// </summary>
      public static class SentisUtils
      {
            /// <summary>
            /// Loads a model from a file path (supports direct path or streaming assets)
            /// </summary>
            public static UnityEngine.ScriptableObject LoadModel(string modelPath)
            {
                  try
                  {
                        // Try direct path
                        if (File.Exists(modelPath))
                        {
                              var model = Unity.Sentis.ModelLoader.Load(modelPath);
                              Debug.Log($"Model loaded from path: {modelPath}");
                              return model;
                        }

                        // Try StreamingAssets path
                        string streamingPath = Path.Combine(Application.streamingAssetsPath, modelPath);
                        if (File.Exists(streamingPath))
                        {
                              var model = Unity.Sentis.ModelLoader.Load(streamingPath);
                              Debug.Log($"Model loaded from StreamingAssets: {streamingPath}");
                              return model;
                        }

                        Debug.LogError($"Model file not found at path: {modelPath} or {streamingPath}");
                        return null;
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error loading model: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Creates a worker for the given model with the specified backend
            /// </summary>
            public static IWorker CreateWorker(UnityEngine.ScriptableObject model, BackendType backendType = BackendType.CPU)
            {
                  if (model == null)
                  {
                        Debug.LogError("Cannot create worker: model is null");
                        return null;
                  }

                  try
                  {
                        var sentisModel = model as Unity.Sentis.ModelAsset;
                        if (sentisModel == null)
                        {
                              Debug.LogError("Invalid model type: expected Unity.Sentis.ModelAsset");
                              return null;
                        }

                        var sentisBackendType = SentisShim.ToSentisBackendType(backendType);
                        var worker = Unity.Sentis.WorkerFactory.CreateWorker(sentisBackendType, sentisModel);
                        return SentisShim.FromSentisWorker(worker);
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
            public static TensorFloat TextureToTensor(Texture2D texture, int width, int height, bool normalizeInput = true, bool useNCHW = true)
            {
                  if (texture == null)
                  {
                        Debug.LogError("Cannot create tensor: texture is null");
                        return null;
                  }

                  try
                  {
                        var textureTransform = new Unity.Sentis.TextureTransform()
                            .SetDimensions(width, height)
                            .SetTensorLayout(useNCHW ? Unity.Sentis.TensorLayout.NCHW : Unity.Sentis.TensorLayout.NHWC);

                        if (normalizeInput)
                        {
                              textureTransform.SetNormalization(new Color(0.5f, 0.5f, 0.5f), new Color(0.5f, 0.5f, 0.5f));
                        }

                        var tensor = Unity.Sentis.TextureConverter.ToTensor(texture, textureTransform);
                        return SentisShim.FromSentisTensor(tensor);
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error creating tensor from texture: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Execute model inference with the given input tensor
            /// </summary>
            public static void ExecuteModel(IWorker worker, string inputName, TensorFloat inputTensor)
            {
                  if (worker == null || inputTensor == null)
                  {
                        Debug.LogError("Cannot execute model: worker or input tensor is null");
                        return;
                  }

                  try
                  {
                        var sentisWorker = SentisShim.ToSentisWorker(worker);
                        var sentisTensor = SentisShim.ToSentisTensor(inputTensor);

                        sentisWorker.Execute(new Dictionary<string, Unity.Sentis.Tensor> { { inputName, sentisTensor } });
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error executing model: {e.Message}");
                  }
            }

            /// <summary>
            /// Get the output tensor from the worker
            /// </summary>
            public static TensorFloat GetOutputTensor(IWorker worker, string outputName)
            {
                  if (worker == null)
                  {
                        Debug.LogError("Cannot get output: worker is null");
                        return null;
                  }

                  try
                  {
                        var sentisWorker = SentisShim.ToSentisWorker(worker);
                        var tensor = sentisWorker.PeekOutput(outputName) as Unity.Sentis.TensorFloat;
                        return SentisShim.FromSentisTensor(tensor);
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError($"Error getting output tensor: {e.Message}");
                        return null;
                  }
            }
      }
}