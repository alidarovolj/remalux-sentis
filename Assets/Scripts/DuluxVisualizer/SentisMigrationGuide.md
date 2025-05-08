# Unity Sentis Migration Guide

This guide explains how to update the DuluxVisualizer project to use Unity Sentis 2.1.2 properly.

## Overview of Issues

The main problems in the current project:

1. **Type Conflicts**: There are conflicts between the custom Barracuda compatibility layer and the actual Unity Sentis package.
2. **API Differences**: The Sentis API differs from Barracuda in several ways.
3. **Missing AR References**: The project needs proper references to AR Subsystems.

## Migration Steps

### 1. Compatibility Layer Changes

The `BarracudaCompat.cs` file has been updated to prevent conflicts with the actual Sentis package. This file is now disabled by default and only activates when the `USE_BARRACUDA_COMPATIBILITY` preprocessor directive is defined.

### 2. Using the SentisAdapter

A new `SentisAdapter.cs` file has been created to help with the transition. This adapter provides methods that handle the differences between the old API and the new Sentis API.

Key methods:
- `CreateWorker`: Creates a worker using the proper Sentis API
- `TextureToTensor`: Converts textures to the Sentis tensor format
- `ExecuteModel`: Runs inference with the correct API calls
- `GetOutputTensor`: Gets output tensors with the correct API
- `CreateSegmentationTexture`: Creates segmentation textures from tensors

### 3. Key API Changes

When updating your code, be aware of these key differences:

| Barracuda/Old Approach | Unity Sentis 2.1.2 |
|------------------------|-------------------|
| `new Worker(model, backend)` | `WorkerFactory.CreateWorker(backendType, model)` |
| `worker.Schedule(inputTensor)` | `worker.Execute(new Dictionary<string, Tensor> { { inputName, inputTensor } })` |
| `worker.PeekOutput(outputName)` | `worker.PeekOutput(outputName) as TensorFloat` |
| `Tensor<float>` | `TensorFloat` |
| `tensorData = tensor.DownloadToArray()` | `tensorData = tensor.ToReadOnlySpan()` |
| `Shape definitions` | Sentis uses different shape management |

### 4. Fixing AR References

To fix the AR Subsystems references:

1. Make sure the following packages are installed in your project:
   - `com.unity.xr.arfoundation` (should be 5.2.0 or later)
   - `com.unity.xr.arsubsystems` (should be 5.0.2 or later)
   - `com.unity.xr.core-utils` (should be 2.2.3 or later)

2. Add the missing AR Subsystems reference to your project's assembly definition file.

3. If you're using a compatibility layer for AR (like in `ARBridge.cs`), you'll need to update it to use the actual AR Subsystems types.

### 5. File-by-File Migration Guide

#### WallSegmentation2D.cs (Already Updated)
- Now uses the SentisAdapter to create workers and process tensors
- Removed custom tensor processing code in favor of the adapter

#### TextureConverter.cs
- Replace all `Unity.Barracuda` references with `Unity.Sentis`
- Use `TensorFloat` instead of `Tensor<float>`
- Use proper Sentis texture conversion methods
- Use the SentisAdapter for tensor operations

#### ModelLoader.cs
- Update the `Load` method to use `ModelLoader.Load` from Unity Sentis
- Update the worker creation code to use `WorkerFactory.CreateWorker`
- Replace tensor operations with Sentis-compatible code
- Replace model introspection code with proper Sentis API calls

#### ARBridge.cs and ARRaycastExtensions.cs
- Add proper references to `Unity.XR.ARSubsystems`
- Ensure the assembly definition includes the AR Subsystems package
- Update type references to use the proper AR types

## Testing the Migration

After updating each file:

1. Check the console for any remaining compilation errors
2. Test basic functionality to ensure models load correctly
3. Test inference to make sure it produces expected results
4. Test AR functionality if used

## Additional Resources

- [Unity Sentis Documentation](https://docs.unity3d.com/Packages/com.unity.sentis@2.1/manual/index.html)
- [Unity AR Foundation Documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.2/manual/index.html) 