using UnityEngine;
using UnityEngine.UI;

// This file provides fallbacks for TextMeshPro types when the package is not installed
// These types will only be used when a special define symbol TEXTMESH_PRO_PRESENT is NOT defined

#if !TEXTMESH_PRO_PRESENT
namespace TMPro
{
      // Fallback implementation for TextMeshProUGUI, inheriting from Text for compatibility
      public class TextMeshProUGUI : Text
      {
            public new string text
            {
                  get => base.text;
                  set => base.text = value;
            }
      }

      // Fallback implementation for TMP_InputField, inheriting from InputField for compatibility
      public class TMP_InputField : InputField
      {
            public new string text
            {
                  get => base.text;
                  set => base.text = value;
            }
      }
}
#endif