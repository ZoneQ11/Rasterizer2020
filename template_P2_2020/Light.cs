using System;
using OpenTK;
using OpenTK.Input;

namespace Template
{
    public class Light
    {
        public Vector4 pos;                     // position
        public Vector3 color,                   // current color (black when the light is off)
            baseColor,                          // color of the light when it is on
            dir;                                // direction (Vector3.Zero means it is not a spotlight)
        public Key key;                         // key to switch the light on and off
        public float cutOff, outerCutOff;       // cutoff angles when the light is used as a spotlight

        public Light(Vector4 pos, Vector3 color, Vector3 dir, Key switchKey, float degreesInner = 90, float degreesOuter = 90)
        {
            this.pos = pos;
            this.color = color;
            baseColor = color;
            this.dir = dir;
            key = switchKey;

            // ensure that the angles are clamped within 0 and 180 degrees
            degreesInner %= 360;
            degreesOuter %= 360;
            if (degreesInner > 180)
                degreesInner = 360 - degreesInner;
            if (degreesOuter > 180)
                degreesOuter = 360 - degreesOuter;

            // switch values if the outer CutOff is smaller than the inner CutOff
            if (degreesOuter < degreesInner)
            {
                float temp = degreesInner;
                degreesInner = degreesOuter;
                degreesOuter = temp;
            }

            cutOff = (float)Math.Cos(MathHelper.DegreesToRadians(degreesInner));
            outerCutOff = (float)Math.Cos(MathHelper.DegreesToRadians(degreesOuter));
        }

        // Turns a light on if it is off and vice versa
        public void Switch()
        {
            if (color == Vector3.Zero)
                color = baseColor;
            else
                color = Vector3.Zero;
        }
    }

    // an intermediary for the shaders containing variable location IDs
    public struct UniformLight
    {
        public int lpos { get; set; }
        public int lcol { get; set; }
        public int ldir { get; set; }
        public int cutOff { get; set; }
        public int outerCutOff { get; set; }
    }
}
