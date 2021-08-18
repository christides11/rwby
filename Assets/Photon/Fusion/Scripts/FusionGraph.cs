using System;
using Fusion.Collections;
using UnityEngine;

public class FusionGraph : MonoBehaviour {
  float   _min;
  float   _max;
  float[] _values;

  [SerializeField]
  public UnityEngine.UI.Image Image;

  [SerializeField]
  public UnityEngine.UI.Text LabelMin;

  [SerializeField]
  public UnityEngine.UI.Text LabelMax;

  [SerializeField]
  public UnityEngine.UI.Text LabelAvg;
  
  [SerializeField]
  public UnityEngine.UI.Text LabelLast;
  
  [SerializeField]
  public int Fractions = 0;
  
  [SerializeField]
  public float Height = 50; 
  
  [SerializeField]
  public float Multiplier = 1;

  [NonSerialized]
  public Func<Fusion.Simulation.Statistics.StatsBuffer<float>> DataSource;

  public void Clear() {
    if (_values != null && _values.Length > 0) {
      Array.Clear(_values, 0, _values.Length);
    }
  }
  
  public void Refresh() {
    var data = DataSource();
    if (data == null || data.Count == 0) {
      return;
    }

    if (_values == null || _values.Length < data.Capacity) {
      _values = new float[data.Capacity];
    }

    var min  = float.MaxValue;
    var max  = float.MinValue;
    var avg  = 0f;
    var last = 0f;

    for (int i = 0; i < data.Count; ++i) {
      var v = Multiplier * data[i];
      
      min = Math.Min(v, min);
      max = Math.Max(v, max);

      _values[i] = last = v;

      avg += v;
    }

    avg /= data.Count;
    
    if (min > 0) {
      min = 0;
    }

    if (max > _max) {
      _max = max;
    }

    if (min < _min) {
      _min = min;
    }
    
    {
      var r = _max - _min;

      for (int i = 0; i < data.Count; ++i) {
        _values[i] = Mathf.Clamp01((_values[i] - _min) / r);
      }
    }

    if (LabelMin) {
      LabelMin.text = Math.Round(min, Fractions).ToString();
    }
    
    if (LabelMax) {
      LabelMax.text = Math.Round(max, Fractions).ToString();
    }
    
    if (LabelAvg) {
      LabelAvg.text = Math.Round(avg, Fractions).ToString();
    }

    if (LabelLast) {
      LabelLast.text = Math.Round(last, Fractions).ToString();
    }

    Image.material.SetFloatArray("_Data", _values);
    Image.material.SetFloat("_Count", _values.Length);
    Image.material.SetFloat("_Height", Height);
    
    _min = Mathf.Lerp(_min, 0, Time.deltaTime);
    _max = Mathf.Lerp(_max, 1, Time.deltaTime);
  }

}