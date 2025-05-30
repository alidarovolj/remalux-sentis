# Dulux Visualizer Tool

This package provides AR wall segmentation and visualization tools for the Dulux Visualizer app.

## AR Wall Painting Scene Creation

There are several ways to create AR scenes with wall segmentation:

### 1. All-in-One AR Scene Creator (Recommended)

From the Unity menu bar, select:
`Tools > AR Wall Painting > 🆕 Create All-in-One AR Scene`

This option:
- Automatically creates a complete AR scene with all necessary components
- Works with or without AR Foundation packages installed
- Sets up WallSegmentation, WallPaintBlit, and UI visualization
- Configures proper tensor dimensions (128×128×3) to avoid runtime errors
- Handles both newer (XROrigin) and older (ARSessionOrigin) AR Foundation versions
- Creates a demo scene with test walls if AR Foundation is not available

### 2. Create AR Wall Painting Scene

From the Unity menu bar, select:
`Tools > AR Wall Painting > Create AR Wall Painting Scene`

This option creates an AR scene with:
- Support for the latest AR Foundation and XR Core Utils
- TrackedPoseDriver for camera tracking
- ARPlaneManager set to detect vertical planes
- UI Canvas with debug visualization
- WallSegmentation and WallPaintBlit

### 3. Create Scene With Fixed Segmentation

From the Unity menu bar, select:
`Tools > AR Wall Painting > Create Scene With Fixed Segmentation`

This option:
- Uses reflection-based component creation for better compatibility
- Fixes issues with missing AR packages
- Configures WallSegmentation with proper tensor dimensions
- Creates a fallback scene if AR Foundation isn't available

## Handling Missing AR Packages

All scene creation methods check for AR Foundation at runtime and offer a fallback:
- If AR Foundation is detected, a full AR scene is created
- If AR Foundation is missing, a basic demo scene with test walls is created
- You'll see a dialog asking whether to proceed with limited functionality

## Scene Components

The created scene includes:
- AR Session
- AR Session Origin / XR Origin (with Camera Floor Offset)
- AR Camera with ARCameraManager and ARCameraBackground
- TrackedPoseDriver for camera motion
- ARPlaneManager configured for vertical plane detection
- WallSegmentation for wall detection using ML
- WallPaintBlit for applying paint color to walls
- UI Canvas with debug visualization and color control panel

## Visualization Debug

A RawImage component is added to the UI Canvas for debugging the segmentation mask.
Look for the "Debug Segmentation View" object in the Canvas hierarchy.

## Demo Mode

In demo mode (when AR Foundation is not available):
- A standard Camera is created instead of AR Camera
- 3 test walls are added to the scene
- WallSegmentation is set to Demo mode
- DemoWallSegmentation component is added if available

## Technical Details

All scene creators use reflection to detect and create components, allowing them to work without compile-time dependencies on AR Foundation packages. This means you can create basic scenes even without AR packages installed.

## Settings and Documentation

For more information, select:
`Tools > AR Wall Painting > ⚙️ Settings and Documentation...`

## Components Overview

1. **ARWallPaintingCreator**: Main component that sets up the complete AR scene
2. **ARWallPaintingSceneCreator**: Static utility class to easily create the AR wall painting scene
3. **WallPaintBlit**: Applies the paint effect via post-processing
4. **WallSegmentation**: Handles the segmentation of walls using machine learning

## Getting Started

### Option 1: Using the Editor Tool

1. Open the Unity editor
2. Navigate to AR > Wall Painting > Open Scene Creator
3. Configure your scene settings
4. Click "Create AR Scene"

### Option 2: Using Script

Call the static method to create a scene programmatically:

```csharp
ARWallPaintingSceneCreator.CreateARWallPaintingScene();
```

## Scene Structure

The AR wall painting scene includes:

- **AR Session**: Manages the AR tracking
- **XR Origin**: Contains the AR camera and trackables
- **Wall Segmentation**: Processes camera frames to detect walls
- **UI Controls**: Color pickers and opacity slider

## How It Works

1. The AR camera captures live video frames
2. Frames are processed by WallSegmentation using a neural network model
3. The segmentation produces a mask highlighting walls
4. WallPaintBlit applies the chosen color to the masked areas
5. The UI allows users to select colors and adjust opacity

## Requirements

- Unity 2020.3 or newer
- AR Foundation 4.1.7+
- ARKit (iOS) or ARCore (Android) packages
- Barracuda package for neural network inference

## Customization

- **Colors**: Modify the color buttons in the UI
- **Segmentation Model**: Replace the segmentation model in Assets/Resources/Models
- **Shader**: Customize the WallPaint shader for different visual effects

## Troubleshooting

If the wall detection isn't working properly:

1. Check that ARFoundation is properly configured
2. Make sure the segmentation model is loaded correctly
3. Try using the Demo mode for testing without the neural network model
4. Check console for specific error messages

## File List

- `/Assets/Scripts/DuluxVisualizer/ARWallPaintingCreator.cs`: Main scene creation component
- `/Assets/Scripts/DuluxVisualizer/ARWallPaintingSceneCreator.cs`: Static utility class for scene creation
- `/Assets/Scripts/DuluxVisualizer/WallPaintBlit.cs`: Post-processing for paint effect
- `/Assets/Scripts/WallSegmentation.cs`: Wall segmentation using ML
- `/Assets/Shaders/WallPaint.shader`: Shader for painting walls

# DuluxVisualizer Compilation Fix

This directory contains scripts to help fix compilation errors in the DuluxVisualizer project, particularly related to Unity package dependencies.

## Quick Fix

The easiest way to fix all compilation issues is to:

1. In Unity Editor, go to `Tools > DuluxVisualizer > Fix Compilation (Complete Solution)`
2. Unity will apply all necessary fixes and trigger a recompilation
3. If problems persist, try `Tools > DuluxVisualizer > Clean Library And Restart`

## Additional Tools

For more granular control, you can use these specialized tools:

- **Fix Ambiguous AR References**: `Tools > DuluxVisualizer > Fix Ambiguous AR References`  
  Adds proper namespace qualifiers to AR Foundation references in your code

- **Fix Ambiguous Sentis References**: `Tools > DuluxVisualizer > Fix Ambiguous Sentis References`  
  Adds proper namespace qualifiers to Unity.Sentis references in your code

- **Fix Duplicate Type Definitions**: `Tools > DuluxVisualizer > Fix Duplicate Type Definitions`  
  Resolves duplicate class definitions by adding namespaces or conditional compilation

## How It Works

The solution works by:

1. Adding required define symbols (`UNITY_SENTIS`, `UNITY_AR_FOUNDATION_PRESENT`, etc.)
2. Providing fallback implementations for package types when actual packages aren't installed
3. Using conditional compilation to switch between real implementations and fallbacks
4. Adding proper namespace qualifiers to resolve ambiguous type references
5. Using conditional compilation for duplicate method definitions

## Files and Their Purpose

### Fallback Files:
- **SentisFallbacks.cs**: Provides fallback types for Unity.Sentis when not installed
- **ARFoundationFallbacks.cs**: Provides fallback types for AR Foundation when not installed
- **TMProFallbacks.cs**: Provides fallback types for TextMeshPro when not installed
- **BarracudaFallbacks.cs**: Provides fallback types for Unity.Barracuda when not installed

### Editor Scripts:
- **Editor/GlobalDefines.cs**: Adds required define symbols to the project
- **Editor/ForceRecompile.cs**: Forces Unity to recompile the project
- **Editor/PackageDetector.cs**: Detects installed packages and updates defines
- **Editor/FixCompilation.cs**: GUI tool to apply various fixes
- **Editor/FixARReferences.cs**: Adds proper namespace qualifiers to AR Foundation references
- **Editor/FixSentisReferences.cs**: Adds proper namespace qualifiers to Unity.Sentis references
- **Editor/FixDuplicateTypes.cs**: Fixes duplicate class and method definitions
- **Editor/UnifiedCompilationFix.cs**: Central script that manages all fixes

## Troubleshooting

If you still experience compilation errors:

1. Ensure all required packages are installed through the Package Manager:
   - Unity Sentis
   - AR Foundation
   - TextMeshPro
   - Barracuda (if needed)

2. Check for duplicate type definitions (same class defined in multiple files)
   - Use `Tools > DuluxVisualizer > Fix Duplicate Type Definitions` to resolve

3. Look for ambiguous references (e.g., same type name in different namespaces)
   - Use `Tools > DuluxVisualizer > Fix Ambiguous AR References` for AR Foundation types
   - Use `Tools > DuluxVisualizer > Fix Ambiguous Sentis References` for Sentis types

4. Try cleaning the Library folder and restarting Unity
   - Use `Tools > DuluxVisualizer > Clean Library And Restart`

## Manual Fixes

If automatic fixes don't work, you may need to:

1. Add proper namespace qualifiers where there are ambiguous references
   ```csharp
   // Instead of:
   ARPlane plane;
   
   // Use:
   UnityEngine.XR.ARFoundation.ARPlane plane;
   ```

2. Add conditional compilation directives around conflicting code
   ```csharp
   #if UNITY_AR_FOUNDATION_PRESENT
   using UnityEngine.XR.ARFoundation;
   #else
   // Use fallbacks
   #endif
   ```

3. Rename conflicting types
   ```csharp
   // Instead of:
   public class WallSegmentation { }
   
   // Use:
   public class WallSegmentationImpl { }
   ```

4. Move types to different namespaces
   ```csharp
   namespace DuluxVisualizer.Legacy
   {
       public class WallSegmentation { }
   }
   ```

## Contact

If you continue to have issues, please contact the project maintainer for assistance. 