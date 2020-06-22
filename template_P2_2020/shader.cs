using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	public class Shader
	{
        // data members
        public int programID, vsID, fsID, viewID,               // Identifications
            attribute_vpos, attribute_vnrm, attribute_vuvs,     // Attributes
            uniform_mview, uniform_2wrld, numLights;            // Uniforms
        public UniformLight[] uLights;                          // Light(s)

		// constructor
		public Shader( String vertexShader, String fragmentShader, UniformLight[] uLights )
		{
            this.uLights = uLights;

            // compile shaders
            programID = GL.CreateProgram();
			Load( vertexShader, ShaderType.VertexShader, programID, out vsID );
			Load( fragmentShader, ShaderType.FragmentShader, programID, out fsID );
			GL.LinkProgram( programID );
			Console.WriteLine( GL.GetProgramInfoLog( programID ) );

			// get locations of shader parameters
			attribute_vpos = GL.GetAttribLocation( programID, "vPosition" );
			attribute_vnrm = GL.GetAttribLocation( programID, "vNormal" );
			attribute_vuvs = GL.GetAttribLocation( programID, "vUV" );
			uniform_mview = GL.GetUniformLocation( programID, "transform" );
            uniform_2wrld = GL.GetUniformLocation(programID, "toWorld");
            numLights = GL.GetUniformLocation(programID, "numLights");
            viewID = GL.GetUniformLocation(programID, "viewPos");

            for (int i = 0; i < uLights.Length; i++)
            {
                uLights[i].lpos = GL.GetUniformLocation(programID, $"lights[{i}].pos");
                uLights[i].lcol = GL.GetUniformLocation(programID, $"lights[{i}].col");
                uLights[i].ldir = GL.GetUniformLocation(programID, $"lights[{i}].dir");
                uLights[i].cutOff = GL.GetUniformLocation(programID, $"lights[{i}].cutOff");
                uLights[i].outerCutOff = GL.GetUniformLocation(programID, $"lights[{i}].outerCutOff");
            }
        }

        // loading shaders
        void Load( String filename, ShaderType type, int program, out int ID )
		{
			// source: http://neokabuto.blogspot.nl/2013/03/opentk-tutorial-2-drawing-triangle.html
			ID = GL.CreateShader( type );
			using( StreamReader sr = new StreamReader( filename ) ) GL.ShaderSource( ID, sr.ReadToEnd() );
			GL.CompileShader( ID );
			GL.AttachShader( program, ID );
			Console.WriteLine( GL.GetShaderInfoLog( ID ) );
		}
	}
}
