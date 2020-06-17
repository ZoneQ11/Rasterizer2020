using System.Collections.Generic;
using OpenTK;

namespace Template
{
    public class SceneGraph
    {
        public void Render(List<Mesh> parent, Shader shader, Matrix4 transform, Vector3 viewPos)
        {
            foreach (var child in parent)
            {
                Matrix4 trans = child.local * transform;
                Render(child.children, shader, trans, viewPos);
                child.Render(shader, trans, viewPos);
            }
        }
    }
}