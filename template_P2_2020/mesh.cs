﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	// mesh and loader based on work by JTalton; http://www.opentk.com/node/642

	public class Mesh
	{
		// data members
		public ObjVertex[] vertices;            // vertex positions, model space
		public ObjTriangle[] triangles;         // triangles (3 vertex indices)
		public ObjQuad[] quads;                 // quads (4 vertex indices)
		int vertexBufferId,                     // vertex buffer
		    triangleBufferId,                   // triangle buffer
		    quadBufferId;                       // quad buffer

        public List<Mesh> children = new List<Mesh>();
        public Matrix4 local = new Matrix4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1));

        Light[] lights;
        Texture texture;    // Texture argument removed from Render() and instead put in the constructor
                            // otherwise the scenegraph would only be able to use one texture
        public float a;		// variable used for rotation based on elapsed milliseconds

        // constructor
        public Mesh(string fileName, Texture texture, Light[] lights)
        {
            MeshLoader loader = new MeshLoader();
            loader.Load(this, fileName);
            this.texture = texture;
            this.lights = lights;
            a = 0;
        }

        // initialization; called during first render
        public void Prepare( Shader shader )
		{
			if( vertexBufferId == 0 )
			{
				// generate interleaved vertex data (uv/normal/position (total 8 floats) per vertex)
				GL.GenBuffers( 1, out vertexBufferId );
				GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
				GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Marshal.SizeOf( typeof( ObjVertex ) )), vertices, BufferUsageHint.StaticDraw );

				// generate triangle index array
				GL.GenBuffers( 1, out triangleBufferId );
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, triangleBufferId );
				GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(triangles.Length * Marshal.SizeOf( typeof( ObjTriangle ) )), triangles, BufferUsageHint.StaticDraw );

				// generate quad index array
				GL.GenBuffers( 1, out quadBufferId );
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, quadBufferId );
				GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(quads.Length * Marshal.SizeOf( typeof( ObjQuad ) )), quads, BufferUsageHint.StaticDraw );
			}
		}

        // render the mesh using the supplied shader and matrix
        public virtual void Render(Shader shader, Matrix4 transform, Vector3 viewPos)
        {
            // on first run, prepare buffers
            Prepare(shader);

            // safety dance
            GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);

            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);

            // enable shader
            GL.UseProgram(shader.programID);

            GL.Uniform3(shader.viewID, viewPos);

            // pass transform to vertex shader
            GL.UniformMatrix4(shader.uniform_mview, false, ref transform);
            GL.UniformMatrix4(shader.uniform_2wrld, false, ref local);

            // pass light to shader
            GL.Uniform1(shader.numLights, shader.uLights.Length);
            for (int i = 0; i < shader.uLights.Length; i++)
            {
                GL.Uniform4(shader.uLights[i].lpos, lights[i].pos);
                GL.Uniform3(shader.uLights[i].lcol, lights[i].color);
                GL.Uniform3(shader.uLights[i].ldir, lights[i].dir);
                GL.Uniform1(shader.uLights[i].cutOff, lights[i].cutOff);
                GL.Uniform1(shader.uLights[i].outerCutOff, lights[i].outerCutOff);
            }

            // enable position, normal and uv attributes
            GL.EnableVertexAttribArray( shader.attribute_vpos );
			GL.EnableVertexAttribArray( shader.attribute_vnrm );
			GL.EnableVertexAttribArray( shader.attribute_vuvs );

			// bind interleaved vertex data
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
			GL.InterleavedArrays( InterleavedArrayFormat.T2fN3fV3f, Marshal.SizeOf( typeof( ObjVertex ) ), IntPtr.Zero );

			// link vertex attributes to shader parameters 
			GL.VertexAttribPointer( shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 32, 0 );
			GL.VertexAttribPointer( shader.attribute_vnrm, 3, VertexAttribPointerType.Float, true, 32, 2 * 4 );
			GL.VertexAttribPointer( shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 32, 5 * 4 );

			// bind triangle index data and render
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, triangleBufferId );
			GL.DrawArrays( PrimitiveType.Triangles, 0, triangles.Length * 3 );

			// bind quad index data and render
			if( quads.Length > 0 )
			{
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, quadBufferId );
				GL.DrawArrays( PrimitiveType.Quads, 0, quads.Length * 4 );
			}

			// restore previous OpenGL state
			GL.UseProgram( 0 );
			GL.PopClientAttrib();
		}

		// layout of a single vertex
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjVertex
		{
			public Vector2 TexCoord;
			public Vector3 Normal;
			public Vector3 Vertex;
		}

		// layout of a single triangle
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjTriangle
		{
			public int Index0, Index1, Index2;
		}

		// layout of a single quad
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjQuad
		{
			public int Index0, Index1, Index2, Index3;
		}
	}

	// special mesh with a velocity that updates the mesh's position at every tick
    public class Bullet : Mesh
    {
        private Vector3 velocity;

        public Bullet(string fileName, Texture texture, Light[] lights, Vector3 velocity)
			: base (fileName, texture, lights)
        {
            this.velocity = velocity;
            local = Matrix4.CreateTranslation(new Vector3(0, 12, 0)) * Matrix4.CreateScale(.025f);
        }

        public override void Render(Shader shader, Matrix4 transform, Vector3 viewPos)
        {
			// update the position using the velocity
            local *= Matrix4.CreateTranslation(velocity);
            base.Render(shader, transform, viewPos);
        }
    }
}