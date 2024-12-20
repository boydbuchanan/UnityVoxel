using System.Collections.Generic;
using UnityEngine;

public struct ColorScope : System.IDisposable
{
    Color oldColor;
    public ColorScope(Color color)
    {
        oldColor = Gizmos.color;
        Gizmos.color = color == default(Color) ? oldColor : color;
    }

    public void Dispose()
    {
        Gizmos.color = oldColor;
    }
}

public static class DrawGizmo
{
    
    public static bool IsEditorFocus(Transform transform){
#if !UNITY_EDITOR
        return false;
#else
        return UnityEditor.Selection.activeTransform == transform;
#endif
    }

    public static bool HasEditorFocus(GameObject gameObject){
        return IsInSelectedHierarchy(gameObject.transform);
    }
    public static bool HasEditorFocus(Transform transform){
#if !UNITY_EDITOR
        return false;
#else
        return IsInSelectedHierarchy(transform);
#endif
    }
    public static bool IsInSelectedHierarchy(Transform transform)
    {
#if !UNITY_EDITOR
        return false;
#else
        if (transform == null)
            return false;
        if (UnityEditor.Selection.Contains(transform.gameObject))
            return true;
        return IsInSelectedHierarchy(transform.parent);
#endif
    }
    
    public static void Ray(Ray ray, Color color){
#if UNITY_EDITOR
        using (new ColorScope(color)){
            Gizmos.DrawRay(ray);
        }
#endif
    }
    
    public static void Ray(Vector3 ray, Vector3 direction, float distance, Color color){
    #if UNITY_EDITOR
            using (new ColorScope(color)){
                Gizmos.DrawRay(ray, direction.normalized * distance);
            }
    #endif
    }
    public static void Ray(Vector3 ray, Vector3 direction, Color color){
#if UNITY_EDITOR
        using (new ColorScope(color)){
            Gizmos.DrawRay(ray, direction);
        }
#endif
    }
    public static void Line(Vector3 from, Vector3 to, Color color){
#if UNITY_EDITOR
        using (new ColorScope(color)){
            Gizmos.DrawLine(from, to);
        }
#endif
    }
    
    public static void Sphere(Vector3 position, float radius, Color color, bool wireframe = true){
        using (new ColorScope(color)){
            Sphere(position, null, radius, wireframe);
        }
    }
    public static void Sphere(Vector3 position, float radius, string text, Color color, bool wireframe = true){
        using (new ColorScope(color)){
            Sphere(position, text, radius, wireframe);
        }
    }
    public static void Sphere(Vector3 position, string text, float radius = 0.1f, bool wireframe = true){
#if UNITY_EDITOR
            if(wireframe)
                Gizmos.DrawWireSphere(position, radius);
            else
                Gizmos.DrawSphere(position, radius);
            
            if(string.IsNullOrEmpty(text))
                return;

            FloatingText(text, position, -30, -30);
#endif
    }
    public static void ObjectPosition(GameObject gameObject){
#if UNITY_EDITOR
        var position = gameObject.transform.position;
        FloatingText($"({position.x},{position.y},{position.z})", position);
        FloatingText(gameObject.name, position, 0, 12f);
        Gizmos.DrawSphere(position, 0.1f);
#endif
    }
    public static void Vertices(Transform transform, Vector3[] vertices, Color color, Color? textColor = null, float textOffset = 12f, float debugSphereRadius = 0.01f){
#if UNITY_EDITOR
        using (new ColorScope(color))
        {
            Dictionary<Vector3, string> seen = new Dictionary<Vector3, string>();
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                var spherePosition = transform.TransformPoint(vertex);
                float radius = transform.lossyScale.magnitude * debugSphereRadius;

                Gizmos.DrawSphere(spherePosition, radius);
                bool hasSeen = seen.ContainsKey(spherePosition);
                if(!hasSeen){
                    seen.Add(spherePosition, $"{i}");
                }else{
                    seen[spherePosition] += $", {i}";
                }
                if(!hasSeen){
                    FloatingText($"({vertex.x},{vertex.y},{vertex.z})", spherePosition, 0, 0, textColor);
                }
            }
            foreach (var key in seen.Keys)
            {
                FloatingText($"[{seen[key]}]", key, 0, textOffset, textColor);
            }
        }
#endif
    }
    public static void FloatingText(Vector3 worldPos, string text, Color color) {
        FloatingText(text, worldPos, 0, 0, color);
    }
    public static void FloatingText(Vector3 worldPos, string text, float offsetX = 0, float offsetY = 0, Color? color = null) {
        FloatingText(text, worldPos, offsetX, offsetY, color);
    }
    public static void FloatingText(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? color = null) {

#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        GUI.color = color.HasValue ? color.Value : Gizmos.color;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        if(view == null || view.camera == null){
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0) {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        UnityEditor.Handles.Label(TransformByPixel(worldPos, oX, oY), text);

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();

    }

    static Vector3 TransformByPixel(Vector3 position, float x, float y) {
        return TransformByPixel(position, new Vector3(x, y));
    }

    static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy) {
        Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
        if (cam)
            return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
        else
            return position;
#endif
    }
    public static void CubeOutline(Vector3 position, float size = 0.1f, Color color = default(Color)){
#if UNITY_EDITOR
        float half = size * 0.5f;
        Vector3 p0 = position + new Vector3(-half, -half, -half);
        Vector3 p1 = position + new Vector3(half, -half, -half);
        Vector3 p2 = position + new Vector3(half, -half, half);
        Vector3 p3 = position + new Vector3(-half, -half, half);
        Vector3 p4 = position + new Vector3(-half, half, -half);
        Vector3 p5 = position + new Vector3(half, half, -half);
        Vector3 p6 = position + new Vector3(half, half, half);
        Vector3 p7 = position + new Vector3(-half, half, half);

        Gizmos.DrawRay(p0, p1 - p0);
        Gizmos.DrawRay(p1, p2 - p1);
        Gizmos.DrawRay(p2, p3 - p2);
        Gizmos.DrawRay(p3, p0 - p3);

        Gizmos.DrawRay(p4, p5 - p4);
        Gizmos.DrawRay(p5, p6 - p5);

        Gizmos.DrawRay(p6, p7 - p6);
        Gizmos.DrawRay(p7, p4 - p7);

        Gizmos.DrawRay(p0, p4 - p0);
        Gizmos.DrawRay(p1, p5 - p1);
        Gizmos.DrawRay(p2, p6 - p2);
        Gizmos.DrawRay(p3, p7 - p3);
#endif
    }
    public static void Bounds(Bounds b, Color color, float delay=0)
    {
#if UNITY_EDITOR
        // bottom
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Debug.DrawLine(p1, p2, color, delay);
        Debug.DrawLine(p2, p3, color, delay);
        Debug.DrawLine(p3, p4, color, delay);
        Debug.DrawLine(p4, p1, color, delay);

        // top
        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Debug.DrawLine(p5, p6, color, delay);
        Debug.DrawLine(p6, p7, color, delay);
        Debug.DrawLine(p7, p8, color, delay);
        Debug.DrawLine(p8, p5, color, delay);

        // sides
        Debug.DrawLine(p1, p5, color, delay);
        Debug.DrawLine(p2, p6, color, delay);
        Debug.DrawLine(p3, p7, color, delay);
        Debug.DrawLine(p4, p8, color, delay);
#endif
    }
    /// <summary>- Draws an arrow.</summary>
    /// <param name='position'>- The start position of the arrow.</param>
    /// <param name='direction'>- The direction the arrow will point in.</param>
    /// <param name='color'>- The color of the arrow.</param>
    /// <param name="angle">- The angle of arrow head.0 ~ 90f</param>
    /// <param name="headLength">- The angle length of arrow head. 0 ~ 1 in percent</param>
    public static void Arrow(Vector3 position, Vector3 direction, Color color = default(Color), float angle = 15f, float headLength = 0.3f)
    {
#if UNITY_EDITOR
        if (direction == Vector3.zero)
            return; // can't draw a thing
        if (angle < 0f)
            angle = Mathf.Abs(angle);
        if (angle > 0f)
        {
            float length = direction.magnitude;
            float arrowLength = length * Mathf.Clamp01(headLength);
            Vector3 headDir = direction.normalized * -arrowLength;
            Cone(position + direction, headDir, angle, color);
        }

        Gizmos.DrawRay(position, direction);
#endif
    }
    /// <summary>- Draws a cone.</summary>
    /// <param name='position'>- The position for the tip of the cone.</param>
    /// <param name='direction'>- The direction for the cone to get wider in.</param>
    /// <param name='color'>- The color of the cone.</param>
    /// <param name='angle'>- The angle of the cone.</param>
    public static void Cone(Vector3 position, Vector3 direction, float angle = 45, Color color = default(Color)){
        using (new ColorScope(color))
        {
            Cone(position, direction, angle);
        }
    }
    public static void Cone(Vector3 position, Vector3 direction, float angle = 45)
    {
#if UNITY_EDITOR
        float length = direction.magnitude;
        angle = Mathf.Clamp(angle, 0f, 90f);

        Vector3
            forward = direction,
            up = Vector3.Slerp(forward, -forward, 0.5f),
            right = Vector3.Cross(forward, up).normalized * length,
            slerpedVector = Vector3.Slerp(forward, up, angle / 90.0f);

        Plane farPlane = new Plane(-direction, position + forward);
        Ray distRay = new Ray(position, slerpedVector);

        float dist;
        farPlane.Raycast(distRay, out dist);

        Gizmos.DrawRay(position, slerpedVector.normalized * dist);
        Gizmos.DrawRay(position, Vector3.Slerp(forward, -up, angle / 90.0f).normalized * dist);
        Gizmos.DrawRay(position, Vector3.Slerp(forward, right, angle / 90.0f).normalized * dist);
        Gizmos.DrawRay(position, Vector3.Slerp(forward, -right, angle / 90.0f).normalized * dist);

        Circle(position + forward, (forward - (slerpedVector.normalized * dist)).magnitude, direction);
        Circle(position + (forward * 0.5f), ((forward * 0.5f) - (slerpedVector.normalized * (dist * 0.5f))).magnitude, direction);
#endif
    }
    /// <summary>- Draws a circle.</summary>
    /// <param name='position'>- Where the center of the circle will be positioned.</param>
    /// <param name='up'>- The direction perpendicular to the surface of the circle.</param>
    /// <param name='color'>- The color of the circle.</param>
    /// <param name='radius'>- The radius of the circle.</param>
    public static void Circle(Vector3 position, float radius, Vector3 up, Color color){
        using (new ColorScope(color))
        {
            Circle(position, radius, up);
        }
    }
    public static void Circle(Vector3 position, float radius = 1.0f, Vector3 up = default)
    {
#if UNITY_EDITOR
        up = ((up == default(Vector3)) ? Vector3.up : up).normalized * radius;
        
        Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
        Vector3 right = Vector3.Cross(up, forward).normalized * radius;

        Matrix4x4 matrix = new Matrix4x4()
        {
            m00 = right.x,
            m10 = right.y,
            m20 = right.z,

            m01 = up.x,
            m11 = up.y,
            m21 = up.z,

            m02 = forward.x,
            m12 = forward.y,
            m22 = forward.z
        };

        Vector3 lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
        Vector3 nextPoint = Vector3.zero;

        for (int i = 0; i <= 90; i++)
        {
            nextPoint = position + matrix.MultiplyPoint3x4(
                new Vector3(
                    Mathf.Cos((i * 4) * Mathf.Deg2Rad),
                    0f,
                    Mathf.Sin((i * 4) * Mathf.Deg2Rad)
                    )
                );
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
#endif
    }
    
    public static void CircleDebug(Vector3 position, float radius, Color color, Vector3 up = default(Vector3), float duration = 0)
    {
#if UNITY_EDITOR
        up = ((up == default(Vector3)) ? Vector3.up : up).normalized * radius;
        
        Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
        Vector3 right = Vector3.Cross(up, forward).normalized * radius;

        Matrix4x4 matrix = new Matrix4x4()
        {
            m00 = right.x,
            m10 = right.y,
            m20 = right.z,

            m01 = up.x,
            m11 = up.y,
            m21 = up.z,

            m02 = forward.x,
            m12 = forward.y,
            m22 = forward.z
        };

        Vector3 lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
        Vector3 nextPoint = Vector3.zero;

        for (int i = 0; i <= 90; i++)
        {
            nextPoint = position + matrix.MultiplyPoint3x4(
                new Vector3(
                    Mathf.Cos((i * 4) * Mathf.Deg2Rad),
                    0f,
                    Mathf.Sin((i * 4) * Mathf.Deg2Rad)
                    )
                );
            Debug.DrawLine(lastPoint, nextPoint, color, duration);
            lastPoint = nextPoint;
        }
#endif
    }
}
