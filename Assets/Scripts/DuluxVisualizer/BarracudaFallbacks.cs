using UnityEngine;
using System;
using System.Collections.Generic;

// This file provides fallbacks for Unity.Barracuda types when the package is not installed
// These types will only be used when UNITY_BARRACUDA_PRESENT is NOT defined

#if !UNITY_BARRACUDA_PRESENT
namespace Unity.Barracuda
{
      // Basic Barracuda types
      public class NNModel : ScriptableObject
      {
      }

      public class Model
      {
            public Model(string name = "") { }
            public Model(NNModel asset) { }

            public List<string> inputs = new List<string>();
            public List<string> outputs = new List<string>();
      }

      public class Worker : IDisposable
      {
            public Worker(Model model, bool verbose = false) { }

            public void Execute(Tensor input) { }
            public Tensor PeekOutput() { return null; }
            public Tensor PeekOutput(string name) { return null; }

            public void Dispose() { }
      }

      public class Tensor : IDisposable
      {
            public Tensor(string name, int[] dimensions) { }
            public Tensor(string name, int n, int h, int w, int c) { }

            public void Dispose() { }
      }
}
#endif