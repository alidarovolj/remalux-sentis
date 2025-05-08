using UnityEngine;
using UnityEditor;

namespace DuluxVisualizer.Editor
{
      /// <summary>
      /// Editor script to ensure our compatibility symbols are defined
      /// </summary>
      [InitializeOnLoad]
      public class DefineSymbolsManager
      {
            static DefineSymbolsManager()
            {
                  // Get current defines
                  string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup);

                  bool changed = false;

                  // Check for Unity Sentis package
                  if (System.Type.GetType("Unity.Sentis.ModelAsset, Unity.Sentis") != null)
                  {
                        if (!currentDefines.Contains("UNITY_SENTIS"))
                        {
                              currentDefines += ";UNITY_SENTIS";
                              changed = true;
                              Debug.Log("Adding UNITY_SENTIS define symbol");
                        }
                  }

                  // Check for Unity Barracuda package
                  if (System.Type.GetType("Unity.Barracuda.Model, Unity.Barracuda") != null)
                  {
                        if (!currentDefines.Contains("UNITY_BARRACUDA"))
                        {
                              currentDefines += ";UNITY_BARRACUDA";
                              changed = true;
                              Debug.Log("Adding UNITY_BARRACUDA define symbol");
                        }
                  }

                  // Check for AR Subsystems 4.0+
                  if (System.Type.GetType("UnityEngine.XR.ARSubsystems.TrackableId, Unity.XR.ARSubsystems") != null)
                  {
                        if (!currentDefines.Contains("AR_SUBSYSTEMS_4_0_OR_NEWER"))
                        {
                              currentDefines += ";AR_SUBSYSTEMS_4_0_OR_NEWER";
                              changed = true;
                              Debug.Log("Adding AR_SUBSYSTEMS_4_0_OR_NEWER define symbol");
                        }
                  }

                  // Check for AR Foundation 4.0+
                  if (System.Type.GetType("UnityEngine.XR.ARFoundation.ARPlane, Unity.XR.ARFoundation") != null)
                  {
                        if (!currentDefines.Contains("AR_FOUNDATION_4_0_OR_NEWER"))
                        {
                              currentDefines += ";AR_FOUNDATION_4_0_OR_NEWER";
                              changed = true;
                              Debug.Log("Adding AR_FOUNDATION_4_0_OR_NEWER define symbol");
                        }
                  }

                  // Apply changes if needed
                  if (changed)
                  {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup,
                            currentDefines);
                        Debug.Log("Updated scripting define symbols: " + currentDefines);
                  }
            }
      }
}