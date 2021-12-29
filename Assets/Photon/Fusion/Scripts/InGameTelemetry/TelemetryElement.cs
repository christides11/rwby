using UnityEngine;

//[ExecuteInEditMode]
public class TelemetryElement : MonoBehaviour {

  /// <summary>
  /// The aspect ratio this element should maintain. 2 = twice as wide as tall.
  /// </summary>
  public float AspectRatio = 2;

  // cached
  RectTransform _rectTrans;
  float         _prevWidth;

  void Update() {

    if (_rectTrans == null)
      _rectTrans = GetComponent<RectTransform>();

    // Rescale the element to maintain the desired aspect ratio
    var newWidth = _rectTrans.sizeDelta.x;
    if (newWidth != _prevWidth) {
      _rectTrans.sizeDelta = new Vector2(newWidth, newWidth / AspectRatio);
      _prevWidth = newWidth;
    }

   
  }
}
