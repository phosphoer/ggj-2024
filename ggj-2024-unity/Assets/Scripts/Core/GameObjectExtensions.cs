using UnityEngine;
using System.Collections.Generic;

public static class GameObjectExtensions
{
  private static List<MeshRenderer> _cachedRenderers = new List<MeshRenderer>();

  public static void SetLayerRecursively(this GameObject gameObject, int layer)
  {
    gameObject.layer = layer;
    foreach (Transform t in gameObject.transform)
    {
      SetLayerRecursively(t.gameObject, layer);
    }
  }

  public static Bounds GetHierarchyBounds(this GameObject gameObject)
  {
    _cachedRenderers.Clear();
    gameObject.GetComponentsInChildren<MeshRenderer>(_cachedRenderers);

    Bounds bounds = _cachedRenderers.Count > 0 ? _cachedRenderers[0].bounds : new Bounds(gameObject.transform.position, Vector3.zero);
    foreach (Renderer r in _cachedRenderers)
    {
      bounds.Encapsulate(r.bounds);
    }

    return bounds;
  }

  public static void SetIdentityTransformLocal(this Transform transform)
  {
    transform.localScale = Vector3.one;
    transform.localPosition = Vector3.zero;
    transform.localRotation = Quaternion.identity;
  }

  public static Transform GetRootParent(this Transform transform)
  {
    if (ReferenceEquals(transform.parent, null))
      return transform;

    Transform node = transform.parent;
    while (!ReferenceEquals(node.parent, null))
      node = node.parent;

    return node;
  }

  public static void DestroyAllChildren(this Transform transform)
  {
    for (int i = transform.childCount - 1; i >= 0; --i)
    {
      GameObject.Destroy(transform.GetChild(i).gameObject);
    }
  }

#if UNITY_EDITOR
  [UnityEditor.MenuItem("GameObject/Mirror Global X")]
  public static void MirrorSelectedGlobalX()
  {
    foreach (var obj in UnityEditor.Selection.gameObjects)
    {
      obj.transform.localPosition = obj.transform.localPosition.WithX(-obj.transform.localPosition.x);
      obj.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(obj.transform.forward, Vector3.left), obj.transform.up);
    }
  }

  [UnityEditor.MenuItem("GameObject/Randomize Rotation")]
  public static void RandomizeRotation()
  {
    foreach (var obj in UnityEditor.Selection.gameObjects)
    {
      obj.transform.rotation = Random.rotationUniform;
    }
  }
#endif
}