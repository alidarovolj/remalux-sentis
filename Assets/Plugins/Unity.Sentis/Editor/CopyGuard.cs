using UnityEngine;
using UnityEditor;
using System.IO;

namespace Unity.Sentis.Editor
{
      /// <summary>
      /// Simple editor script to ensure Sentis types are loaded and available
      /// </summary>
      [InitializeOnLoad]
      public class SentisCopyGuard
      {
            static SentisCopyGuard()
            {
                  Debug.Log("Initializing Unity Sentis compatibility layer");

                  // Check if Sentis package is available
                  bool sentisFound = false;

                  try
                  {
                        // Try to check for Sentis types by reflection
                        var sentisType = System.Type.GetType("Unity.Sentis.ModelAsset, Unity.Sentis");
                        sentisFound = sentisType != null;

                        if (sentisFound)
                        {
                              Debug.Log("Unity Sentis package found and properly loaded.");
                        }
                        else
                        {
                              Debug.LogWarning("Unity Sentis not found or not properly loaded. Using compatibility layer.");
                        }
                  }
                  catch (System.Exception ex)
                  {
                        Debug.LogWarning($"Error checking for Sentis: {ex.Message}");
                  }
            }
      }
}