using UnityEngine;

public static class ExtDebug
{
    public static string MATERIAL_TEXTURE_IDENTIFIER = "_MaterialMap";

    public static Color[] TEAM_COLORS = new[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.magenta,
        Color.yellow,
    };
    
    public static Vector2 DeadZoner(Vector2 raw, float dead)
    {
        float d2 = raw.x * raw.x + raw.y * raw.y;

        if(d2 < dead*dead || d2 < 0)
        {
            // We're inside the deadzone, return zero
            return Vector2.zero;
        }
        else
        {
            float d = Mathf.Sqrt(d2);

            // Normalise raw input
            float nx = raw.x / d;
            float ny = raw.y / d;

            // Rescaled so (dead->1) becomes (0->1)
            d = (d - dead) / (1 - dead);

            // and clamped down so we get 0 <= d <= 1
            d = Mathf.Min(d, 1);

            // Apply a curve for a smoother feel (as suggested by (Gibgezr)[https://github.com/Gibgezr]
            // Uncomment the following line and see if your game feels better
            //d = d * d;

            return new Vector2(nx*d, ny*d);
        }
    }

    public static bool FastApproximately(float a, float b, float threshold)
    {
        return ((a < b) ? (b - a) : (a - b)) <= threshold;
    }

    public static float FastSubstractAbs(float a, float b)
    {
        return ((a < b) ? (b - a) : (a - b));
    }

    public static int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    //Draws just the box at where it is currently hitting.
    public static void DrawBoxCastOnHit(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float hitInfoDistance, Color color)
    {
        origin = CastCenterOnCollision(origin, direction, hitInfoDistance);
        DrawBox(origin, halfExtents, orientation, color);
    }

    //Draws the full box from start of cast to its end distance. Can also pass in hitInfoDistance instead of full distance
    public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
    {
        direction.Normalize();
        Box bottomBox = new Box(origin, halfExtents, orientation);
        Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);

        Debug.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft, color);
        Debug.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight, color);
        Debug.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft, color);
        Debug.DrawLine(bottomBox.backTopRight, topBox.backTopRight, color);
        Debug.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft, color);
        Debug.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight, color);
        Debug.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft, color);
        Debug.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight, color);

        DrawBox(bottomBox, color);
        DrawBox(topBox, color);
    }

    public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color, float duration = 0.0f)
    {
        DrawBox(new Box(origin, halfExtents, orientation), color, duration);
    }

    public static void DrawBox(Box box, Color color, float duration = 0.0f)
    {
        Debug.DrawLine(box.frontTopLeft, box.frontTopRight, color, duration);
        Debug.DrawLine(box.frontTopRight, box.frontBottomRight, color, duration);
        Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color, duration);
        Debug.DrawLine(box.frontBottomLeft, box.frontTopLeft, color, duration);

        Debug.DrawLine(box.backTopLeft, box.backTopRight, color, duration);
        Debug.DrawLine(box.backTopRight, box.backBottomRight, color, duration);
        Debug.DrawLine(box.backBottomRight, box.backBottomLeft, color, duration);
        Debug.DrawLine(box.backBottomLeft, box.backTopLeft, color, duration);

        Debug.DrawLine(box.frontTopLeft, box.backTopLeft, color, duration);
        Debug.DrawLine(box.frontTopRight, box.backTopRight, color, duration);
        Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color, duration);
        Debug.DrawLine(box.frontBottomLeft, box.backBottomLeft, color, duration);
    }

    public struct Box
    {
        public Vector3 localFrontTopLeft { get; private set; }
        public Vector3 localFrontTopRight { get; private set; }
        public Vector3 localFrontBottomLeft { get; private set; }
        public Vector3 localFrontBottomRight { get; private set; }
        public Vector3 localBackTopLeft { get { return -localFrontBottomRight; } }
        public Vector3 localBackTopRight { get { return -localFrontBottomLeft; } }
        public Vector3 localBackBottomLeft { get { return -localFrontTopRight; } }
        public Vector3 localBackBottomRight { get { return -localFrontTopLeft; } }

        public Vector3 frontTopLeft { get { return localFrontTopLeft + origin; } }
        public Vector3 frontTopRight { get { return localFrontTopRight + origin; } }
        public Vector3 frontBottomLeft { get { return localFrontBottomLeft + origin; } }
        public Vector3 frontBottomRight { get { return localFrontBottomRight + origin; } }
        public Vector3 backTopLeft { get { return localBackTopLeft + origin; } }
        public Vector3 backTopRight { get { return localBackTopRight + origin; } }
        public Vector3 backBottomLeft { get { return localBackBottomLeft + origin; } }
        public Vector3 backBottomRight { get { return localBackBottomRight + origin; } }

        public Vector3 origin { get; private set; }

        public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
        {
            Rotate(orientation);
        }
        public Box(Vector3 origin, Vector3 halfExtents)
        {
            this.localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
            this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

            this.origin = origin;
        }


        public void Rotate(Quaternion orientation)
        {
            localFrontTopLeft = RotatePointAroundPivot(localFrontTopLeft, Vector3.zero, orientation);
            localFrontTopRight = RotatePointAroundPivot(localFrontTopRight, Vector3.zero, orientation);
            localFrontBottomLeft = RotatePointAroundPivot(localFrontBottomLeft, Vector3.zero, orientation);
            localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
        }
    }

    //This should work for all cast types
    static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
    {
        return origin + (direction.normalized * hitInfoDistance);
    }

    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 direction = point - pivot;
        return pivot + rotation * direction;
    }

    /// <summary>
    ///   Draw a wire sphere
    /// </summary>
    /// <param name="center"> </param>
    /// <param name="radius"> </param>
    /// <param name="color"> </param>
    /// <param name="duration"> </param>
    /// <param name="quality"> Define the quality of the wire sphere, from 1 to 10 </param>
    public static void DrawWireSphere(Vector3 center, float radius, Color color, float duration, int quality = 3)
    {
        quality = Mathf.Clamp(quality, 1, 10);

        int segments = quality << 2;
        int subdivisions = quality << 3;
        int halfSegments = segments >> 1;
        float strideAngle = 360F / subdivisions;
        float segmentStride = 180F / segments;

        Vector3 first;
        Vector3 next;
        for (int i = 0; i < segments; i++)
        {
            first = (Vector3.forward * radius);
            first = Quaternion.AngleAxis(segmentStride * (i - halfSegments), Vector3.right) * first;

            for (int j = 0; j < subdivisions; j++)
            {
                next = Quaternion.AngleAxis(strideAngle, Vector3.up) * first;
                UnityEngine.Debug.DrawLine(first + center, next + center, color, duration);
                first = next;
            }
        }

        Vector3 axis;
        for (int i = 0; i < segments; i++)
        {
            first = (Vector3.forward * radius);
            first = Quaternion.AngleAxis(segmentStride * (i - halfSegments), Vector3.up) * first;
            axis = Quaternion.AngleAxis(90F, Vector3.up) * first;

            for (int j = 0; j < subdivisions; j++)
            {
                next = Quaternion.AngleAxis(strideAngle, axis) * first;
                UnityEngine.Debug.DrawLine(first + center, next + center, color, duration);
                first = next;
            }
        }
    }
}
