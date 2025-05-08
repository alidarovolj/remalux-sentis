# Unity Sentis Migration Guide

This guide explains how to migrate from Unity Barracuda to Unity Sentis 2.1.2 for machine learning in Unity projects.

## Key Differences Between Barracuda and Sentis

| Barracuda | Sentis | Notes |
|-----------|--------|-------|
| `NNModel` | `ModelAsset` | Main model asset type |
| `Model` | `Model` (internal) | Runtime model representation |
| `IWorker` | `IWorker` | Worker interface remains similar |
| `BarracudaWorkerFactory` | `WorkerFactory` | Factory for creating workers |
| `Tensor` | `TensorFloat`, `TensorInt`, etc. | Strongly typed tensors |
| `TextureAsTensorData` | `TextureConverter` | Converting textures to tensors |

## Step-by-Step Migration

### 1. Update Package References

Ensure your project has the Unity Sentis package (at least 2.1.2) installed via the Package Manager:

```
com.unity.sentis: 2.1.2 or newer
```

### 2. Update Assembly References

Make sure your assembly definition files reference `Unity.Sentis`:

```json
{
  "references": [
    "Unity.Sentis"
  ]
}
```

### 3. Update Using Statements

Change:
```csharp
using Unity.Barracuda;
```

To:
```csharp
using Unity.Sentis;
```

### 4. Update Model Loading

Barracuda:
```csharp
public NNModel modelAsset;
var model = ModelLoader.Load(modelAsset);
```

Sentis:
```csharp
public ModelAsset modelAsset;
// ModelAsset is already the loaded model in Sentis
```

Or load from file:
```csharp
ModelAsset model = ModelLoader.Load(modelPath);
```

### 5. Update Worker Creation

Barracuda:
```csharp
IWorker worker = BarracudaWorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
```

Sentis:
```csharp
IWorker worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, modelAsset);
```

### 6. Update Tensor Creation from Textures

Barracuda:
```csharp
Tensor inputTensor = new Tensor(texture, 3); // 3 channels (RGB)
```

Sentis:
```csharp
var textureTransform = new TextureTransform()
    .SetDimensions(width, height)
    .SetTensorLayout(TensorLayout.NCHW);
    
TensorFloat inputTensor = TextureConverter.ToTensor(texture, textureTransform);
```

### 7. Update Model Execution

Barracuda:
```csharp
worker.Execute(inputTensor);
Tensor outputTensor = worker.PeekOutput();
```

Sentis:
```csharp
worker.Execute(new Dictionary<string, Tensor> { { inputName, inputTensor } });
TensorFloat outputTensor = worker.PeekOutput(outputName) as TensorFloat;
```

### 8. Update Tensor Access

Barracuda:
```csharp
float[] data = outputTensor.data.Download(outputTensor.shape);
```

Sentis:
```csharp
ReadOnlySpan<float> data = outputTensor.ToReadOnlySpan();
```

### 9. Update Resource Cleanup

Both libraries:
```csharp
inputTensor.Dispose();
outputTensor.Dispose();
worker.Dispose();
```

## Helper Classes

For easier migration, consider using these helper classes:

1. `SentisManager.cs` - A singleton manager for Sentis operations
2. `SentisAdapter.cs` - Adapter with simplified API for common operations
3. `SentisUtils.cs` - Static utility methods

## Common Issues

- **Type Errors**: Sentis uses strongly typed tensors (TensorFloat, TensorInt) instead of generic Tensor
- **API Changes**: Some method signatures and parameter orders have changed
- **Shape Handling**: Tensor shape access works slightly differently
- **Memory Management**: Still requires explicit Dispose() calls as in Barracuda

## Example: Processing an Image for Segmentation

```csharp
using Unity.Sentis;
using UnityEngine;
using System.Collections.Generic;

public class SegmentationExample : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private string inputName = "input";
    [SerializeField] private string outputName = "output";
    
    private IWorker worker;
    
    void Start()
    {
        // Create worker
        BackendType backend = SystemInfo.supportsComputeShaders ?
            BackendType.GPUCompute : BackendType.CPU;
        worker = WorkerFactory.CreateWorker(backend, modelAsset);
    }
    
    public Texture2D ProcessImage(Texture2D sourceImage)
    {
        // Create input tensor
        var textureTransform = new TextureTransform()
            .SetDimensions(256, 256)
            .SetTensorLayout(TensorLayout.NCHW);
        
        var inputTensor = TextureConverter.ToTensor(sourceImage, textureTransform);
        
        // Execute model
        worker.Execute(new Dictionary<string, Tensor> { { inputName, inputTensor } });
        
        // Get output tensor
        var outputTensor = worker.PeekOutput(outputName) as TensorFloat;
        
        // Process results (simplified)
        Texture2D resultTexture = new Texture2D(256, 256);
        // ... process outputTensor and fill resultTexture ...
        
        // Clean up
        inputTensor.Dispose();
        outputTensor.Dispose();
        
        return resultTexture;
    }
    
    void OnDestroy()
    {
        worker?.Dispose();
    }
} 