
using System.Reflection;
using UnityEngine;

public static class FusionScalableIMGUI
{
  private static GUISkin _scalableSkin;

  private static void InitializedGUIStyles() {

    _scalableSkin = ScriptableObject.CreateInstance<GUISkin>();
    CopySkin(GUI.skin, _scalableSkin);
    _scalableSkin.button = new GUIStyle(GUI.skin.button);
    _scalableSkin.label = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
    _scalableSkin.textField = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleCenter };
    //_scalableSkin.window = new GUIStyle(GUI.skin.window);
    _scalableSkin.button.normal.background = GUI.skin.box.normal.background;
    _scalableSkin.button.normal.background = GUI.skin.box.normal.background;
    _scalableSkin.button.normal.background = GUI.skin.box.normal.background;
    _scalableSkin.button.hover.background = GUI.skin.window.normal.background;
  }

  /// <summary>
  /// Get the custom scalable skin, already resized to the current screen.
  /// </summary>
  /// <returns></returns>
  public static GUISkin GetScaledSkin() {
    if (_scalableSkin == null)
      InitializedGUIStyles();

    ScaleGuiSkinToScreenHeight(_scalableSkin);
    return _scalableSkin;
  }

  /// <summary>
  /// Get the custom scalable skin, already resized to the current screen. Provides the height, width, padding and margin used.
  /// </summary>
  /// <returns></returns>
  public static GUISkin GetScaledSkin(out float height, out float width, out int padding, out int margin) {

    if(_scalableSkin == null)
      InitializedGUIStyles();

    var dimensions = ScaleGuiSkinToScreenHeight(_scalableSkin);
    height  = dimensions.Item1;
    width   = dimensions.Item2;
    padding = dimensions.Item3;
    margin  = dimensions.Item4;
    return _scalableSkin;
  }

  /// <summary>
  /// Modifies a skin to make it scale with screen height.
  /// </summary>
  /// <param name="skin"></param>
  /// <returns>Returns (height, width, padding, margin) values applied to the GuiSkin</returns>
  public static (float, float, int, int) ScaleGuiSkinToScreenHeight(this GUISkin skin) {

    float height = Screen.height * .1f;
    float width = Screen.width * .5f;
    var padding = (int)(height / 4);
    var margin = (int)(height / 8);

    if (skin == null)
      InitializedGUIStyles();

    int fontsize = (int)(height * .5f);

    var margins = new RectOffset(0, 0, margin, margin);

    skin.button.fontSize = fontsize;
    skin.button.margin = margins;
    skin.label.fontSize = fontsize;
    skin.textField.fontSize = fontsize;
    skin.window.padding = new RectOffset(padding, padding, padding, padding);
    skin.window.margin = new RectOffset(margin, margin, margin, margin);

    return (height, width, padding, margin);
  }


  public static void CopySkin(GUISkin skin, GUISkin newSkin) {

    foreach (PropertyInfo propertyInfo in typeof(GUISkin).GetProperties()) {
      if (propertyInfo.PropertyType == typeof(GUISettings)) {
        GUISettings settings = (GUISettings)propertyInfo.GetValue(skin, null);
        newSkin.settings.cursorColor = settings.cursorColor;
        newSkin.settings.cursorFlashSpeed = settings.cursorFlashSpeed;
        newSkin.settings.doubleClickSelectsWord = settings.doubleClickSelectsWord;
        newSkin.settings.selectionColor = settings.selectionColor;
        newSkin.settings.tripleClickSelectsLine = settings.tripleClickSelectsLine;
      } else {
        propertyInfo.SetValue(newSkin, propertyInfo.GetValue(skin, null), null);
      }

      if (propertyInfo.PropertyType == typeof(GUIStyle)) {
        GUIStyle style = (GUIStyle)propertyInfo.GetValue(skin, null);
        GUIStyle newStyle = (GUIStyle)propertyInfo.GetValue(newSkin, null);

        newStyle.normal.background = style.normal.background;
        newStyle.hover.background = style.hover.background;
        newStyle.active.background = style.active.background;
        newStyle.focused.background = style.focused.background;
        newStyle.onNormal.background = style.onNormal.background;
        newStyle.onHover.background = style.onHover.background;
        newStyle.onActive.background = style.onActive.background;
        newStyle.onFocused.background = style.onFocused.background;
      }

      if (propertyInfo.PropertyType == typeof(GUIStyle[])) {
        GUIStyle[] styles = (GUIStyle[])propertyInfo.GetValue(skin, null);
        GUIStyle[] newStyles = (GUIStyle[])propertyInfo.GetValue(newSkin, null);

        for (int i = 0; i < styles.Length; i++) {
          GUIStyle style = styles[i];
          GUIStyle newStyle = newStyles[i];

          newStyle.normal.background = style.normal.background;
          newStyle.hover.background = style.hover.background;
          newStyle.active.background = style.active.background;
          newStyle.focused.background = style.focused.background;
          newStyle.onNormal.background = style.onNormal.background;
          newStyle.onHover.background = style.onHover.background;
          newStyle.onActive.background = style.onActive.background;
          newStyle.onFocused.background = style.onFocused.background;
        }
      }
    }
  }
}

