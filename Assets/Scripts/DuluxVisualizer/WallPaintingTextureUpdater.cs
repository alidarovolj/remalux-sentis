using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Component for handling wall painting texture updates
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class WallPaintingTextureUpdater : MonoBehaviour
{
      [Header("Texture Settings")]
      [SerializeField] private int textureWidth = 512;
      [SerializeField] private int textureHeight = 512;
      [SerializeField] private Color defaultColor = new Color(1, 1, 1, 0);

      [Header("Brush Settings")]
      [SerializeField] private float brushSize = 0.1f;
      [SerializeField] private float brushHardness = 0.5f;
      [SerializeField] private Color brushColor = new Color(0.8f, 0.2f, 0.3f, 1.0f);
      [SerializeField] private Texture2D brushTexture;

      [Header("Painting Settings")]
      public Color paintColor = Color.white;
      public float paintOpacity = 0.8f;
      public bool preserveShadows = true;
      public bool useTemporaryMask = false;

      [Header("References")]
      public WallSegmentation2D wallSegmentation2D;

      [Header("Debug")]
      [SerializeField] private bool debugMode = false;

      // Private variables
      private Texture2D paintTexture;
      private Color[] pixelBuffer;
      private MeshRenderer meshRenderer;
      private Material material;
      private bool isInitialized = false;
      private Texture2D currentTexture;
      private RenderTexture renderTarget;

      private void Awake()
      {
            meshRenderer = GetComponent<MeshRenderer>();
      }

      /// <summary>
      /// Initialize the painting texture
      /// </summary>
      public void Initialize(RenderTexture target)
      {
            renderTarget = target;

            if (currentTexture == null)
            {
                  currentTexture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            }

            textureWidth = target.width;
            textureHeight = target.height;

            // Create painting texture
            paintTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            pixelBuffer = new Color[textureWidth * textureHeight];

            // Initialize with transparent pixels
            for (int i = 0; i < pixelBuffer.Length; i++)
            {
                  pixelBuffer[i] = Color.clear;
            }

            paintTexture.SetPixels(pixelBuffer);
            paintTexture.Apply();

            // Setup material
            if (meshRenderer != null && meshRenderer.material != null)
            {
                  material = meshRenderer.material;
                  material.mainTexture = paintTexture;
            }
            else
            {
                  Debug.LogError("WallPaintingTextureUpdater: MeshRenderer or material not found");
            }

            // Create default brush texture if none is assigned
            if (brushTexture == null)
            {
                  CreateDefaultBrushTexture();
            }

            isInitialized = true;
      }

      /// <summary>
      /// Create a default circular brush texture
      /// </summary>
      private void CreateDefaultBrushTexture()
      {
            // Create a simple circular brush texture
            int brushTextureSize = 64;
            brushTexture = new Texture2D(brushTextureSize, brushTextureSize, TextureFormat.RGBA32, false);

            Color[] pixels = new Color[brushTextureSize * brushTextureSize];
            float center = brushTextureSize / 2f;

            for (int y = 0; y < brushTextureSize; y++)
            {
                  for (int x = 0; x < brushTextureSize; x++)
                  {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                        float alpha = Mathf.Clamp01(1f - distance / center);

                        // Smooth falloff
                        alpha = Mathf.SmoothStep(0, 1, alpha);

                        pixels[y * brushTextureSize + x] = new Color(1, 1, 1, alpha);
                  }
            }

            brushTexture.SetPixels(pixels);
            brushTexture.Apply();
      }

      /// <summary>
      /// Clear the painting texture with the default color
      /// </summary>
      public void ClearTexture()
      {
            if (paintTexture == null) return;

            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                  pixels[i] = defaultColor;
            }

            paintTexture.SetPixels(pixels);
            paintTexture.Apply();
      }

      /// <summary>
      /// Paint at UV coordinates with the current brush settings
      /// </summary>
      public void PaintAtUV(Vector2 uvPosition, Color color, float size)
      {
            if (!isInitialized || paintTexture == null) return;

            // Convert UV to texture coordinates
            int x = Mathf.FloorToInt(uvPosition.x * textureWidth);
            int y = Mathf.FloorToInt(uvPosition.y * textureHeight);

            // Calculate brush size in pixels
            int brushRadius = Mathf.FloorToInt(size * textureWidth * 0.5f);
            int brushDiameter = brushRadius * 2;

            // Get pixels from the paint texture in the brush area
            int xStart = Mathf.Clamp(x - brushRadius, 0, textureWidth - 1);
            int yStart = Mathf.Clamp(y - brushRadius, 0, textureHeight - 1);
            int xEnd = Mathf.Clamp(x + brushRadius, 0, textureWidth - 1);
            int yEnd = Mathf.Clamp(y + brushRadius, 0, textureHeight - 1);

            int width = xEnd - xStart;
            int height = yEnd - yStart;

            // Get current pixels
            Color[] pixels = paintTexture.GetPixels(xStart, yStart, width, height);

            // Apply brush
            for (int py = 0; py < height; py++)
            {
                  for (int px = 0; px < width; px++)
                  {
                        // Calculate distance from brush center
                        float distance = Vector2.Distance(new Vector2(px, py), new Vector2(width / 2f, height / 2f));
                        float normalizedDistance = distance / brushRadius;

                        if (normalizedDistance <= 1)
                        {
                              // Get brush alpha based on distance and brush texture
                              float brushAlpha = 1.0f;
                              if (brushTexture != null)
                              {
                                    // Sample from brush texture
                                    float u = px / (float)width;
                                    float v = py / (float)height;
                                    brushAlpha = brushTexture.GetPixelBilinear(u, v).a;
                              }
                              else
                              {
                                    // Simple falloff if no brush texture
                                    brushAlpha = 1.0f - normalizedDistance;
                              }

                              // Apply brush color with alpha
                              int pixelIndex = py * width + px;
                              if (pixelIndex < pixels.Length)
                              {
                                    Color existingColor = pixels[pixelIndex];
                                    Color newColor = Color.Lerp(existingColor, color, brushAlpha * color.a);
                                    pixels[pixelIndex] = newColor;
                              }
                        }
                  }
            }

            // Apply the modified pixels back to the texture
            paintTexture.SetPixels(xStart, yStart, width, height, pixels);
            paintTexture.Apply();
      }

      /// <summary>
      /// Set the brush color
      /// </summary>
      public void SetBrushColor(Color color)
      {
            brushColor = color;
      }

      /// <summary>
      /// Set the brush size
      /// </summary>
      public void SetBrushSize(float size)
      {
            brushSize = Mathf.Clamp01(size);
      }

      /// <summary>
      /// Paint at a world position by raycasting
      /// </summary>
      public bool PaintAtWorldPosition(Vector3 worldPosition, Color color, float size)
      {
            if (!isInitialized) return false;

            // Get mesh and convert world position to UV
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.mesh == null) return false;

            // Convert world position to local position
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            // Simple conversion for quad (assumes quad is on local XY plane, centered at origin)
            Vector2 uv = new Vector2(localPos.x + 0.5f, localPos.y + 0.5f);

            // Check if UV is within bounds
            if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return false;

            // Paint at UV
            PaintAtUV(uv, color, size);
            return true;
      }

      /// <summary>
      /// Get the painting material
      /// </summary>
      public Material GetMaterial()
      {
            return material;
      }

      /// <summary>
      /// Get the painting texture
      /// </summary>
      public Texture2D GetTexture()
      {
            return paintTexture;
      }

      /// <summary>
      /// Updates the wall texture with the current paint color
      /// </summary>
      public void UpdateTexture(Texture2D maskTexture)
      {
            if (renderTarget == null || maskTexture == null)
                  return;

            // Apply painting
            Graphics.Blit(maskTexture, renderTarget);

            if (debugMode)
            {
                  Debug.Log($"Updated wall texture with color {paintColor}, opacity {paintOpacity}");
            }
      }

      /// <summary>
      /// Sets the paint color
      /// </summary>
      public void SetPaintColor(Color color)
      {
            paintColor = color;
      }

      /// <summary>
      /// Sets the paint opacity
      /// </summary>
      public void SetPaintOpacity(float opacity)
      {
            paintOpacity = Mathf.Clamp01(opacity);
      }

      /// <summary>
      /// Toggles shadow preservation
      /// </summary>
      public void SetPreserveShadows(bool preserve)
      {
            preserveShadows = preserve;
      }

      /// <summary>
      /// Clear the current painting
      /// </summary>
      public void ClearPainting()
      {
            if (renderTarget != null)
            {
                  RenderTexture.active = renderTarget;
                  GL.Clear(true, true, Color.clear);
                  RenderTexture.active = null;
            }
      }
}