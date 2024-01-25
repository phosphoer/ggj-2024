// The MIT License (MIT)
// Copyright (c) 2016 David Evans @phosphoer
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// This is based off the Mathfx from Unity Wiki but I added my own stuff that I find useful and removed stuff that was redundant with Unity and added some docs

using UnityEngine;
using System.Collections.Generic;

public static class Mathfx
{
  // Interpolation functions 
  public static float Hermite(float start, float end, float value)
  {
    return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
  }

  public static float Sinerp(float start, float end, float value)
  {
    return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
  }

  public static float SmoothSin(float start, float end, float value)
  {
    return Mathf.Lerp(start, end, Mathf.Sin(value * 2 * Mathf.PI + Mathf.PI * 0.5f));
  }

  public static float Coserp(float start, float end, float value)
  {
    return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
  }

  public static float Berp(float start, float end, float value)
  {
    value = Mathf.Clamp01(value);
    value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
    return start + (end - start) * value;
  }

  public static float SmoothStep(float min, float max, float x)
  {
    x = Mathf.Clamp(x, min, max);
    float v1 = (x - min) / (max - min);
    float v2 = (x - min) / (max - min);
    return -2 * v1 * v1 * v1 + 3 * v2 * v2;
  }

  public static float Bounce(float x)
  {
    return Mathf.Abs(Mathf.Sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
  }

  /*
   * CLerp - Circular Lerp - is like lerp but handles the wraparound from 0 to 360.
   * This is useful when interpolating eulerAngles and the object
   * crosses the 0/360 boundary.  The standard Lerp function causes the object
   * to rotate in the wrong direction and looks stupid. Clerp fixes that.
*/
  public static float Clerp(float start, float end, float value)
  {
    float min = 0.0f;
    float max = 360.0f;
    float half = Mathf.Abs((max - min) / 2.0f);//half the distance between min and max
    float retval = 0.0f;
    float diff = 0.0f;

    if ((end - start) < -half)
    {
      diff = ((max - start) + end) * value;
      retval = start + diff;
    }
    else if ((end - start) > half)
    {
      diff = -((max - end) + start) * value;
      retval = start + diff;
    }
    else retval = start + (end - start) * value;

    return retval;
  }

  // Dampening functions, stateless-ly and frame independently interpolate towards a value
  // Smoothing in range from 0 (will instantly reach target) to 1 (never reaches target)
  // I like to use around 0.5
  // https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
  public static float Damp(float source, float target, float smoothing, float dt, float snapEpsilon = 0.01f)
  {
    float val = Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    if (Mathf.Abs(val - target) < snapEpsilon)
      val = target;

    return val;
  }

  public static Vector4 Damp(Vector4 source, Vector4 target, float smoothing, float dt)
  {
    return Vector4.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  public static Vector3 Damp(Vector3 source, Vector3 target, float smoothing, float dt)
  {
    return Vector3.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  public static Vector2 Damp(Vector2 source, Vector2 target, float smoothing, float dt)
  {
    return Vector2.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  public static Color Damp(Color source, Color target, float smoothing, float dt)
  {
    return Color.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  public static Quaternion Damp(Quaternion source, Quaternion target, float smoothing, float dt)
  {
    return Quaternion.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  // test if a Vector3 is close to another Vector3 (due to floating point inprecision)
  // compares the square of the distance to the square of the range as this 
  // avoids calculating a square root which is much slower than squaring the range
  public static bool Approx(Vector3 val, Vector3 about, float range)
  {
    return ((val - about).sqrMagnitude < range * range);
  }

  public enum Axis
  {
    X = 0,
    Y = 1,
    Z = 2
  }

  private static Vector3[] _axisVectors = new Vector3[]
  {
    Vector3.right,
    Vector3.up,
    Vector3.forward
  };

  public static Vector3 GetAxisVector(Axis axis)
  {
    return _axisVectors[(int)axis];
  }

  // Project a point onto a plane, not necessarily at origin
  // Vector3.ProjectOnPlane assumes origin for the plane
  public static Vector3 ProjectPointOnPlane(Vector3 point, Plane plane)
  {
    return Vector3.ProjectOnPlane(point, plane.normal) - plane.normal * plane.distance;
  }

  // Nearest point on an infinite line to a point
  public static Vector3 NearestPointLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
  {
    Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
    float closestPoint = Vector3.Dot((point - lineStart), lineDirection) / Vector3.Dot(lineDirection, lineDirection);
    return lineStart + (closestPoint * lineDirection);
  }

  // Nearest point on a line, clamped to the endpoints of the line segment
  public static Vector3 NearestPointSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
  {
    Vector3 fullDirection = lineEnd - lineStart;
    Vector3 lineDirection = Vector3.Normalize(fullDirection);
    float closestPoint = Vector3.Dot((point - lineStart), lineDirection) / Vector3.Dot(lineDirection, lineDirection);
    return lineStart + (Mathf.Clamp(closestPoint, 0.0f, Vector3.Magnitude(fullDirection)) * lineDirection);
  }

  public static int NearestPointPath(IReadOnlyList<Vector3> path, Vector3 point, out Vector3 nearestPoint)
  {
    float minDist = Mathf.Infinity;
    int nearestIndex = 0;
    nearestPoint = point;
    for (int i = 0; i < path.Count - 1; ++i)
    {
      Vector3 nearestPointSegment = NearestPointSegment(path[i], path[i + 1], point);
      float dist = Vector3.Distance(point, nearestPointSegment);
      if (dist < minDist)
      {
        minDist = dist;
        nearestPoint = nearestPointSegment;
        nearestIndex = i;
      }
    }

    return nearestIndex;
  }

  public static Vector3 ClampToRadius(Vector3 point, Vector3 center, float radius)
  {
    Vector3 offset = point - center;
    offset = Vector3.ClampMagnitude(offset, radius);
    point = center + offset;
    return point;
  }

  // 
  // Canvas / Rect Transform helpers
  //
  public static Vector3 ViewportToCanvasPosition(RectTransform canvas, Vector3 viewportPos)
  {
    viewportPos.x *= canvas.rect.size.x;
    viewportPos.y *= canvas.rect.size.y;
    viewportPos.x -= canvas.rect.size.x * canvas.pivot.x;
    viewportPos.y -= canvas.rect.size.y * canvas.pivot.y;
    return viewportPos;
  }

  public static Vector3 WorldToCanvasPosition(RectTransform canvas, Camera camera, Vector3 worldPos, bool allowOffscreen = false)
  {
    Vector3 pos = camera.WorldToViewportPoint(worldPos);
    if (pos.z < 0 && !allowOffscreen)
    {
      pos.y = 0;
      pos.x = 1 - pos.x;
    }

    pos = ViewportToCanvasPosition(canvas, pos);
    return pos;
  }

  public static Vector3 CanvasToWorldPosition(RectTransform canvas, Camera camera, Vector2 canvasLocalPos)
  {
    Vector2 viewportPoint = canvasLocalPos + canvas.rect.size * canvas.pivot;
    viewportPoint /= canvas.rect.size;
    Vector3 worldPos = camera.ViewportToWorldPoint(viewportPoint, Camera.MonoOrStereoscopicEye.Mono);
    return worldPos;
  }

  public static Vector3[] GetRectTransformCorners(RectTransform transform)
  {
    transform.GetWorldCorners(_rectWorldCorners);
    return _rectWorldCorners;
  }

  public static Vector3 GetRectTransformWorldSize(RectTransform transform)
  {
    transform.GetWorldCorners(_rectWorldCorners);
    return _rectWorldCorners[2] - _rectWorldCorners[0];
  }

  private static Vector3[] _rectWorldCorners = new Vector3[4];

  //
  // Misc math
  //
  public static float CeilNormal(float val)
  {
    if (val > 0)
      return 1.0f;
    else if (val < 0)
      return -1.0f;
    return 0.0f;
  }

  // https://answers.unity.com/questions/283192/how-to-convert-decibel-number-to-audio-source-volu.html
  public static float LinearToDecibel(float linear)
  {
    float dB;

    if (linear != 0)
      dB = 20.0f * Mathf.Log10(linear);
    else
      dB = -144.0f;

    return dB;
  }

  public static float DecibelToLinear(float dB)
  {
    float linear = Mathf.Pow(10.0f, dB / 20.0f);

    return linear;
  }

  // Inverted bounds, useful if you want to accumulate bounds without including origin
  public static Bounds NegativeBounds()
  {
    Bounds negativeBounds = new Bounds();
    negativeBounds.min = Vector3.positiveInfinity;
    negativeBounds.max = Vector3.negativeInfinity;
    return negativeBounds;
  }

  public static float GetRectRadius(Vector2 vec, Rect rect)
  {
    float vecAngle = Mathf.Deg2Rad * Vector2.Angle(vec.WithY(0), vec);
    float radiusVertical = (rect.height / 2) / Mathf.Sin(vecAngle);
    float radiusHorizontal = (rect.width / 2) / Mathf.Cos(vecAngle);
    return Mathf.Min(radiusVertical, radiusHorizontal);
  }

  // 2D Convex hull of a list of points on the XZ plane
  public static void ConvexHull(IReadOnlyList<Vector3> points, List<Vector3> hullPoints, int maxPoints = 64)
  {
    hullPoints.Clear();
    if (points.Count < 3)
    {
      return;
    }

    var minPoint = points[0];
    foreach (var p in points)
    {
      if (p.x < minPoint.x)
        minPoint = p;
      else if (Mathf.Abs(p.x - minPoint.x) < Mathf.Epsilon && p.z < minPoint.z)
        minPoint = p;
    }

    var nextHullPoint = minPoint;
    hullPoints.Add(nextHullPoint);
    do
    {
      nextHullPoint = ConvexNextPoint(points, nextHullPoint);
      if (Vector3.Distance(nextHullPoint, hullPoints[0]) > Mathf.Epsilon)
        hullPoints.Add(nextHullPoint);
    } while (Vector3.Distance(nextHullPoint, hullPoints[0]) > Mathf.Epsilon && hullPoints.Count < maxPoints);
  }

  private static int ConvexTurn(Vector3 p, Vector3 q, Vector3 r)
  {
    var val = (q.x - p.x) * (r.z - p.z) - (r.x - p.x) * (q.z - p.z);
    if (val < 0)
      return -1;
    if (val == 0)
      return 0;
    return 1;
  }

  private static Vector3 ConvexNextPoint(IReadOnlyList<Vector3> points, Vector3 p)
  {
    var q = p;
    foreach (var r in points)
    {
      var t = ConvexTurn(p, q, r);
      if (t == -1 || t == 0 && ConvexDistance(p, r) > ConvexDistance(p, q))
        q = r;
    }
    return q;
  }

  private static float ConvexDistance(Vector3 p, Vector3 q)
  {
    return Vector3.SqrMagnitude(p - q);
  }
}

public static class BoundsExtensions
{
  public static Vector3 ClosestPointOnSurface(this Bounds bounds, Vector3 v)
  {
    if (bounds.Contains(v))
    {
      Vector3 boundsLocalPoint = v - bounds.center;
      Vector3 insideDelta = bounds.extents - boundsLocalPoint.AsAbsoluteValues();
      int minComponent = 0;
      float minComponentValue = insideDelta.x;
      // QAG.ConsoleDebug.Log($"Local point is {boundsLocalPoint}");
      // QAG.ConsoleDebug.Log($"Inside delta is {insideDelta}");
      for (int i = 0; i < 3; ++i)
      {
        if (insideDelta[i] < minComponentValue)
        {
          minComponentValue = insideDelta[i];
          minComponent = i;
        }
      }

      float localPointSide = Mathf.Sign(boundsLocalPoint[minComponent]);
      Vector3 planeNormal = Vector3.zero.WithComponent(minComponent, localPointSide);
      Vector3 planePos = Vector3.zero.WithComponent(minComponent, bounds.extents[minComponent] * localPointSide);
      planePos += bounds.center;

      return Mathfx.ProjectPointOnPlane(v, new Plane(planeNormal, planePos));
    }

    Vector3 closestPoint = bounds.ClosestPoint(v);
    return closestPoint;
  }

  public static Vector3 Clamp(this Bounds bounds, Vector3 v)
  {
    v = Vector3.Max(v, bounds.min);
    v = Vector3.Min(v, bounds.max);
    return v;
  }

  public static Vector3 IntersectRayWithCenter(this Bounds bounds, Vector3 fromPoint)
  {
    if (bounds.Contains(fromPoint))
      return fromPoint;

    Vector3 toCenter = bounds.center - fromPoint;
    float hitDistance = 0;
    bounds.IntersectRay(new Ray(fromPoint, toCenter), out hitDistance);
    return fromPoint + toCenter.normalized * hitDistance;
  }
}

public static class VectorExtensions
{
  // Get a copy of this vector with a given value for x/y/z
  // My primary use case is getting a forward vector whose y is 0 
  public static Vector2 WithX(this Vector2 v, float x)
  {
    v.x = x;
    return v;
  }

  public static Vector2 WithY(this Vector2 v, float y)
  {
    v.y = y;
    return v;
  }

  public static Vector3 NormalizedSafe(this Vector3 v)
  {
    if (v.sqrMagnitude == 0)
      return Vector3.zero;

    return v.normalized;
  }

  public static Vector3 OnXZPlane(this Vector2 v)
  {
    return new Vector3(v.x, 0, v.y);
  }

  public static Vector3 OnXYPlane(this Vector2 v)
  {
    return new Vector3(v.x, v.y, 0);
  }

  public static Vector3 WithX(this Vector3 v, float x)
  {
    v.x = x;
    return v;
  }

  public static Vector3 WithY(this Vector3 v, float y)
  {
    v.y = y;
    return v;
  }

  public static Vector3 WithZ(this Vector3 v, float z)
  {
    v.z = z;
    return v;
  }

  public static Vector3 WithComponent(this Vector3 v, int i, float val)
  {
    v[i] = val;
    return v;
  }

  public static Vector3 AsAbsoluteValues(this Vector3 v)
  {
    v.x = Mathf.Abs(v.x);
    v.y = Mathf.Abs(v.y);
    v.z = Mathf.Abs(v.z);
    return v;
  }

  public static Vector2 XY(this Vector3 vector)
  {
    return new Vector2(vector.x, vector.y);
  }

  public static Vector2 XZ(this Vector3 vector)
  {
    return new Vector2(vector.x, vector.z);
  }

  public static float Component(this Vector3 vector, Mathfx.Axis axis)
  {
    return vector[(int)axis];
  }

  public static float Component(this Vector2 vector, Mathfx.Axis axis)
  {
    return vector[(int)axis];
  }

  public static float SquareDistance(this Vector3 v, Vector3 rhs)
  {
    return (rhs - v).sqrMagnitude;
  }
}

public static class ColorExtensions
{
  public static Color WithR(this Color c, float r)
  {
    c.r = r;
    return c;
  }

  public static Color WithG(this Color c, float g)
  {
    c.g = g;
    return c;
  }

  public static Color WithB(this Color c, float b)
  {
    c.b = b;
    return c;
  }

  public static Color WithA(this Color c, float a)
  {
    c.a = a;
    return c;
  }
}