using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;

namespace Template
{
    public class Light
    {
        public Vector4 pos;
        public Vector3 color, baseColor, dir;
        public Key key;

        public Light(Vector4 pos, Vector3 color/*, Vector3 dir*/, Key switchKey)
        {
            this.pos = pos;
            this.color = color;
            baseColor = color;
            //this.dir = dir;

            key = switchKey;
        }

        /// <summary>
        /// Turns a light on if it is off and vice versa
        /// </summary>
        public void Switch()
        {
            if (color == Vector3.Zero)
                color = baseColor;
            else
                color = Vector3.Zero;
        }
    }

    public struct UniformLight
    {
        public int lpos { get; set; }
        public int lcol { get; set; }
        //public int ldir { get; set; }
    }
}
