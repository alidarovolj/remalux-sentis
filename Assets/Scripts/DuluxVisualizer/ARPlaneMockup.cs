using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// A simple mockup for AR planes to use in testing without AR hardware
/// </summary>
public class ARPlaneMockup : MonoBehaviour
{
      // Simplified enum for plane alignment in case ARFoundation is not available
      public enum PlaneAlignment
      {
            Horizontal = 0,
            Vertical = 1,
            NotSpecified = 2
      }

      [SerializeField] private PlaneAlignment planeAlignment = PlaneAlignment.Vertical;
      [SerializeField] private string planeId = "plane_mockup_001";
      [SerializeField] private MeshRenderer meshRenderer;
      [SerializeField] private MeshFilter meshFilter;

      // Properties to mimic ARPlane
      public PlaneAlignment alignment => planeAlignment;
      public string trackableId => planeId;
      public Vector3 center => transform.position;
      public Vector3 normal => planeAlignment == PlaneAlignment.Vertical ? transform.forward : transform.up;

      private void Awake()
      {
            // Get components if not assigned
            if (meshRenderer == null)
                  meshRenderer = GetComponent<MeshRenderer>();

            if (meshFilter == null)
                  meshFilter = GetComponent<MeshFilter>();
      }

      // Utility to get bounds (similar to ARPlane)
      public Bounds GetBounds()
      {
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                  return meshFilter.sharedMesh.bounds;
            }
            return new Bounds(transform.position, Vector3.one);
      }

      // Turn the mockup on/off
      public void SetActive(bool active)
      {
            gameObject.SetActive(active);
      }

      // Change the mockup's alignment
      public void SetAlignment(PlaneAlignment alignment)
      {
            planeAlignment = alignment;

            // Update rotation based on new alignment
            if (alignment == PlaneAlignment.Vertical)
            {
                  transform.rotation = Quaternion.Euler(0, 0, 0); // Facing forward
            }
            else if (alignment == PlaneAlignment.Horizontal)
            {
                  transform.rotation = Quaternion.Euler(90, 0, 0); // Facing up
            }
      }
}