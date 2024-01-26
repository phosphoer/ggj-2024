// The MIT License (MIT)
// Copyright (c) 2023 David Evans @festivevector
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EditorSelectionHistory : EditorWindow
{
  [SerializeField]
  private List<HistoryItem> _historyList = new();

  [System.Serializable]
  private class HistoryItem
  {
    public Object Object;
  }

  private Vector2 _scrollPosition;

  [MenuItem("Window/Selection History")]
  public static void ShowWindow()
  {
    EditorWindow.GetWindow<EditorSelectionHistory>("Selection History");
  }

  private void OnSelectionChanged()
  {
    HistoryItem newHistoryItem = new();
    newHistoryItem.Object = Selection.activeObject;
    _historyList.Insert(0, newHistoryItem);
    if (_historyList.Count > 100)
      _historyList.RemoveAt(_historyList.Count - 1);

    Repaint();
  }

  private void OnEnable()
  {
    Selection.selectionChanged += OnSelectionChanged;
  }

  private void OnDisable()
  {
    Selection.selectionChanged -= OnSelectionChanged;
  }

  private void OnGUI()
  {
    EditorGUILayout.BeginHorizontal();

    if (GUILayout.Button("Clear", GUILayout.MaxWidth(80)))
    {
      _historyList.Clear();
    }

    GUILayout.BeginVertical();
    GUILayout.Label("Left click: Select/Ping/Drag");
    GUILayout.Label("Right click: Edit properties");
    GUILayout.EndVertical();

    EditorGUILayout.EndHorizontal();


    // Contain list in a scrollview
    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

    for (int i = 0; i < _historyList.Count; i++)
    {
      HistoryItem historyItem = _historyList[i];
      if (historyItem.Object == null)
      {
        _historyList.RemoveAt(i);
        continue;
      }

      try
      {
        EditorGUILayout.BeginHorizontal();
        DrawDraggableObjectField(historyItem, i);
        if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
        {
          _historyList.RemoveAt(i);
        }
      }
      catch (System.Exception)
      {
        // Ignore weird internal GUI errors?
      }
      finally
      {
        EditorGUILayout.EndHorizontal();
      }
    }

    GUILayout.EndScrollView();
  }

  private void DrawDraggableObjectField(HistoryItem historyItem, int index)
  {
    Rect rect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);

    // Get the content (icon and text) for the object
    GUIContent content = EditorGUIUtility.ObjectContent(historyItem.Object, typeof(Object));

    // Draw the icon
    GUI.Label(new Rect(rect.x, rect.y + 2, 20, EditorGUIUtility.singleLineHeight), content.image);

    // Draw the object field
    EditorGUI.LabelField(new Rect(rect.x + 25, rect.y + 2, rect.width - 25, EditorGUIUtility.singleLineHeight),
        new GUIContent(historyItem.Object.name, "Drag to another field"), EditorStyles.objectField);

    Event evt = Event.current;
    switch (evt.type)
    {
      // Allow dragging from field
      case EventType.MouseDrag:
        if (rect.Contains(evt.mousePosition))
        {
          DragAndDrop.PrepareStartDrag();
          DragAndDrop.objectReferences = new Object[] { historyItem.Object };
          DragAndDrop.StartDrag(historyItem.Object.name);
          Event.current.Use();
        }
        break;

      // Ping objects on click
      case EventType.MouseUp:
        if (rect.Contains(evt.mousePosition) && evt.button == 0)
        {
          Selection.activeObject = historyItem.Object;
          EditorGUIUtility.PingObject(historyItem.Object);
        }
        else if (rect.Contains(evt.mousePosition) && evt.button == 1)
        {
          EditorUtility.OpenPropertyEditor(historyItem.Object);
        }
        break;
    }
  }
}
