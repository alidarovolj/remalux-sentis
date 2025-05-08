using UnityEngine;
using Unity.Sentis;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Sample script demonstrating Unity Sentis usage for model inspection and inference
/// </summary>
public class SentisExampleScene : MonoBehaviour
{
      [Header("Model Settings")]
      [SerializeField] private ModelAsset modelAsset;
      [SerializeField] private string modelPath = "Models/model.onnx";
      [SerializeField] private string inputName = "input";
      [SerializeField] private string outputName = "output";

      [Header("UI References")]
      [SerializeField] private RawImage inputDisplay;
      [SerializeField] private RawImage outputDisplay;
      [SerializeField] private Button processButton;
      [SerializeField] private Text statusText;

      [Header("Processing Settings")]
      [SerializeField] private int inputWidth = 256;
      [SerializeField] private int inputHeight = 256;
      [SerializeField] private bool useDemoMode = false;

      // Private fields
      private IWorker worker;
      private Texture2D inputTexture;
      private Texture2D sampleImage;
      private bool isInitialized = false;

      void Start()
      {
            // Set up UI event handlers
            if (processButton != null)
            {
                  processButton.onClick.AddListener(ProcessImage);
            }

            // Load and initialize the model
            InitializeModel();

            // Load sample image
            LoadSampleImage();

            // Display status
            UpdateStatus("Ready. Click Process to run inference.");
      }

      void OnDestroy()
      {
            worker?.Dispose();

            if (inputTexture != null)
                  Destroy(inputTexture);

            if (sampleImage != null)
                  Destroy(sampleImage);
      }

      /// <summary>
      /// Initialize the Sentis model and worker
      /// </summary>
      private void InitializeModel()
      {
            try
            {
                  // Try to load model from asset or path
                  if (modelAsset == null)
                  {
                        // Try to load from StreamingAssets
                        string streamingPath = Path.Combine(Application.streamingAssetsPath, modelPath);
                        if (File.Exists(streamingPath))
                        {
                              modelAsset = ModelLoader.Load(streamingPath);
                              Debug.Log($"Model loaded from StreamingAssets: {streamingPath}");
                        }
                        else
                        {
                              Debug.LogWarning($"Model not found at path: {streamingPath}");
                              useDemoMode = true;
                        }
                  }

                  if (modelAsset != null && !useDemoMode)
                  {
                        // Select backend based on device capabilities
                        BackendType backend = SystemInfo.supportsComputeShaders ?
                            BackendType.GPUCompute : BackendType.CPU;

                        // Create worker
                        worker = WorkerFactory.CreateWorker(backend, modelAsset);

                        // Log model info
                        Debug.Log($"Model initialized with {backend} backend");
                        Debug.Log($"Inputs: {string.Join(", ", modelAsset.inputs)}");
                        Debug.Log($"Outputs: {string.Join(", ", modelAsset.outputs)}");

                        isInitialized = true;
                        UpdateStatus("Model loaded successfully");
                  }
                  else if (useDemoMode)
                  {
                        Debug.Log("Using demo mode - no actual inference will be performed");
                        UpdateStatus("Demo Mode Activated (No model)");
                        isInitialized = true;
                  }
            }
            catch (System.Exception e)
            {
                  Debug.LogError($"Error initializing model: {e.Message}");
                  UpdateStatus($"Error: {e.Message}");
                  useDemoMode = true;
            }
      }

      /// <summary>
      /// Load a sample image for processing
      /// </summary>
      private void LoadSampleImage()
      {
            // Create a sample image (colored gradient)
            sampleImage = new Texture2D(inputWidth, inputHeight, TextureFormat.RGBA32, false);

            Color[] pixels = new Color[inputWidth * inputHeight];
            for (int y = 0; y < inputHeight; y++)
            {
                  for (int x = 0; x < inputWidth; x++)
                  {
                        float r = (float)x / inputWidth;
                        float g = (float)y / inputHeight;
                        float b = ((float)x + y) / (inputWidth + inputHeight);
                        pixels[y * inputWidth + x] = new Color(r, g, b, 1.0f);
                  }
            }

            sampleImage.SetPixels(pixels);
            sampleImage.Apply();

            // Display the sample image
            if (inputDisplay != null)
            {
                  inputDisplay.texture = sampleImage;
            }
      }

      /// <summary>
      /// Process the image using the Sentis model
      /// </summary>
      public void ProcessImage()
      {
            if (!isInitialized)
            {
                  UpdateStatus("Model not initialized");
                  return;
            }

            UpdateStatus("Processing image...");

            if (useDemoMode)
            {
                  // Create a fake output in demo mode
                  CreateDemoOutput();
                  UpdateStatus("Demo processing complete");
                  return;
            }

            try
            {
                  // Create input tensor from texture
                  var textureTransform = new TextureTransform()
                      .SetDimensions(inputWidth, inputHeight)
                      .SetTensorLayout(TensorLayout.NCHW);

                  TensorFloat inputTensor = TextureConverter.ToTensor(sampleImage, textureTransform);

                  // Execute the model
                  worker.Execute(new Dictionary<string, Tensor> { { inputName, inputTensor } });

                  // Get the output tensor
                  TensorFloat outputTensor = worker.PeekOutput(outputName) as TensorFloat;

                  if (outputTensor != null)
                  {
                        // Create a visualization of the output tensor
                        Texture2D outputTexture = CreateOutputVisualization(outputTensor);

                        // Display the result
                        if (outputDisplay != null)
                        {
                              outputDisplay.texture = outputTexture;
                        }

                        UpdateStatus("Processing complete");
                  }
                  else
                  {
                        UpdateStatus("Error: No output tensor produced");
                  }

                  // Clean up resources
                  inputTensor.Dispose();
                  outputTensor?.Dispose();
            }
            catch (System.Exception e)
            {
                  Debug.LogError($"Error processing image: {e.Message}");
                  UpdateStatus($"Error: {e.Message}");
            }
      }

      /// <summary>
      /// Create a visualization of the output tensor
      /// </summary>
      private Texture2D CreateOutputVisualization(TensorFloat tensor)
      {
            Texture2D outputTexture = new Texture2D(inputWidth, inputHeight, TextureFormat.RGBA32, false);

            try
            {
                  // Access tensor data
                  var data = tensor.ToReadOnlySpan();
                  var shape = tensor.shape;

                  // Log shape info
                  Debug.Log($"Output tensor shape: {shape}");

                  // Create texture based on the tensor data (simplified example)
                  Color[] pixels = new Color[inputWidth * inputHeight];

                  // Handle different shapes (this is just a simple visualization)
                  if (shape.rank == 4) // NCHW format (batch, channels, height, width)
                  {
                        int channels = shape[1];
                        int height = shape[2];
                        int width = shape[3];

                        for (int y = 0; y < inputHeight; y++)
                        {
                              for (int x = 0; x < inputWidth; x++)
                              {
                                    // Scale to tensor dimensions
                                    int tx = Mathf.Min(x * width / inputWidth, width - 1);
                                    int ty = Mathf.Min(y * height / inputHeight, height - 1);

                                    // Get RGB values from the first 3 channels (or fewer if less available)
                                    float r = channels > 0 ? data[ty * width + tx] : 0;
                                    float g = channels > 1 ? data[height * width + ty * width + tx] : 0;
                                    float b = channels > 2 ? data[2 * height * width + ty * width + tx] : 0;

                                    // Normalize values
                                    r = Mathf.Clamp01(r);
                                    g = Mathf.Clamp01(g);
                                    b = Mathf.Clamp01(b);

                                    pixels[y * inputWidth + x] = new Color(r, g, b, 1.0f);
                              }
                        }
                  }
                  else
                  {
                        // For other shapes, create a grayscale visualization
                        for (int i = 0; i < pixels.Length; i++)
                        {
                              float value = i < data.Length ? data[i] : 0;
                              value = Mathf.Clamp01(value);
                              pixels[i] = new Color(value, value, value, 1.0f);
                        }
                  }

                  outputTexture.SetPixels(pixels);
                  outputTexture.Apply();
            }
            catch (System.Exception e)
            {
                  Debug.LogError($"Error creating output visualization: {e.Message}");

                  // Create error pattern
                  Color[] errorPixels = new Color[inputWidth * inputHeight];
                  for (int i = 0; i < errorPixels.Length; i++)
                  {
                        errorPixels[i] = Color.red;
                  }
                  outputTexture.SetPixels(errorPixels);
                  outputTexture.Apply();
            }

            return outputTexture;
      }

      /// <summary>
      /// Create a demo output texture
      /// </summary>
      private void CreateDemoOutput()
      {
            Texture2D demoOutput = new Texture2D(inputWidth, inputHeight, TextureFormat.RGBA32, false);

            // Create a fake segmentation mask
            Color[] pixels = new Color[inputWidth * inputHeight];
            for (int y = 0; y < inputHeight; y++)
            {
                  for (int x = 0; x < inputWidth; x++)
                  {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(inputWidth / 2, inputHeight / 2));
                        float radius = Mathf.Min(inputWidth, inputHeight) * 0.4f;

                        if (dist < radius)
                        {
                              pixels[y * inputWidth + x] = new Color(0, 1, 0, 0.7f); // Green segmentation
                        }
                        else
                        {
                              pixels[y * inputWidth + x] = new Color(0, 0, 0, 0); // Transparent
                        }
                  }
            }

            demoOutput.SetPixels(pixels);
            demoOutput.Apply();

            // Display the result
            if (outputDisplay != null)
            {
                  outputDisplay.texture = demoOutput;
            }
      }

      /// <summary>
      /// Update the status text
      /// </summary>
      private void UpdateStatus(string message)
      {
            Debug.Log(message);
            if (statusText != null)
            {
                  statusText.text = message;
            }
      }
}