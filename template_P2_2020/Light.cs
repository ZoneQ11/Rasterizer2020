using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Template
{
    public class Light
    {
        public Vector4 pos;
        public Vector3 color, baseColor, dir;

        public Light(Vector4 pos, Vector3 color/*, Vector3 dir*/)
        {
            this.pos = pos;
            this.color = color;
            baseColor = color;
            //this.dir = dir;
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
}
