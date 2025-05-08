using UnityEngine;
using System;
using System.Collections.Generic;

// Этот файл предоставляет заглушки для Unity.Sentis API, которые будут использоваться,
// когда пакет не установлен или когда директива UNITY_SENTIS не определена

// Заглушки будут использоваться только когда UNITY_SENTIS НЕ определен
#if !UNITY_SENTIS
namespace Unity.Sentis
{
      // Базовые типы тензоров
      public class Tensor : IDisposable
      {
            public virtual void Dispose() { }
      }

      public class TensorFloat : Tensor
      {
            // Конструкторы
            public TensorFloat(TensorShape shape) { }
            public TensorFloat(float[] data, TensorShape shape) { }

            // Методы
            public ReadOnlySpan<float> ToReadOnlySpan() { return new ReadOnlySpan<float>(); }
            public float[] ToArray() { return new float[0]; }
      }

      public class TensorInt : Tensor
      {
            public TensorInt(TensorShape shape) { }
            public TensorInt(int[] data, TensorShape shape) { }
      }

      // Форма тензора
      public class TensorShape
      {
            public int rank => 0;

            public TensorShape(params int[] dimensions) { }

            public int this[int axis] => 0;
      }

      // Перечисления
      public enum BackendType
      {
            CPU,
            GPUCompute
      }

      public enum TensorLayout
      {
            NHWC,
            NCHW
      }

      // Модель и выполнение
      public class Model
      {
            public List<string> inputs = new List<string>();
            public List<string> outputs = new List<string>();
      }

      public class ModelAsset : ScriptableObject
      {
            public List<string> outputs = new List<string>();
            public List<InputDescription> inputs = new List<InputDescription>();

            [Serializable]
            public class InputDescription
            {
                  public string name;
                  public int[] shape;
                  public string dataType;
            }
      }

      public interface IWorker : IDisposable
      {
            Tensor PeekOutput(string name);
            void Execute(Dictionary<string, Tensor> inputs);
      }

      public class Worker : IWorker
      {
            public Worker(Model model, BackendType type) { }

            public Tensor PeekOutput(string name) { return null; }

            public void Execute(Dictionary<string, Tensor> inputs) { }

            public void Dispose() { }

            // Для совместимости с Sentis 2.x
            public void Schedule(Tensor inputTensor) { }
      }

      // Преобразование текстур
      public class TextureTransform
      {
            public TextureTransform SetDimensions(int width, int height) { return this; }
            public TextureTransform SetTensorLayout(TensorLayout layout) { return this; }
            public TextureTransform SetNormalization(Color mean, Color std) { return this; } 
      }

      public static class TextureConverter
      {
            public static TensorFloat ToTensor(Texture texture, TextureTransform transform) { return new TensorFloat(new TensorShape(1, 1, 1, 1)); }
            public static void ToTensor(Texture texture, TensorFloat tensor, TextureTransform transform) { }
      }

      public static class ModelLoader
      {
            public static Model Load(ModelAsset asset) { return new Model(); }
            public static ModelAsset Load(string path) { return null; }
      }
      
      public static class WorkerFactory 
      {
            public static IWorker CreateWorker(BackendType backendType, ModelAsset modelAsset) { return null; }
      }
}
#endif