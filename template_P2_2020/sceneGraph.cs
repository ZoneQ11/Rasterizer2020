using System.Collections.Generic;
using OpenTK;

namespace Template
{
    public class SceneGraph
    {
        public void Render(List<Mesh> parent, Shader shader, Matrix4 transform, Texture texture)
        {
            foreach (var child in parent)
            {
                Matrix4 trans = child.local * transform;
                Render(child.meshes, shader, trans, texture);
                child.Render(shader, trans);
            }
        }
    }
}