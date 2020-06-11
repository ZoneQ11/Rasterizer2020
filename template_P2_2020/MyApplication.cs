using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	class MyApplication
	{
		// member variables
		public Surface screen;                  // background surface for printing etc.
        public Light[] lights = new Light[1];   // add amount of lights here

        SceneGraph scene = new SceneGraph();
        List<Mesh> world = new List<Mesh>();

        Mesh mesh, floor;                       // a mesh to draw using OpenGL
		const float PI = 3.1415926535f;         // PI
		float a = 0;                            // teapot rotation angle

		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader;                          // shader to use for rendering
		Shader postproc;                        // shader to use for post processing
		Texture wood;                           // texture to use for rendering
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing

		bool useRenderTarget = false;           // let's not use postproduct for now...

		// initialize
		public void Init()
		{
            // load texture and light
            wood = new Texture("../../assets/wood.jpg");
            lights[0] = new Light();

            // load teapot
            mesh = new Mesh( "../../assets/teapot.obj", wood, lights );
			floor = new Mesh( "../../assets/floor.obj", wood, lights );

			// initialize stopwatch
			timer = new Stopwatch();
			timer.Reset();
            timer.Start();
			// create shaders
			shader = new Shader( "../../shaders/vs.glsl", "../../shaders/fs.glsl" );
			postproc = new Shader( "../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl" );
			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();
		}

		// tick for background surface
		public void Tick()
		{
		}

		// tick for OpenGL rendering code
		public void RenderGL()
		{
			// measure frame duration
			float frameDuration = timer.ElapsedMilliseconds;
			timer.Reset();
			timer.Start();

			// prepare matrix for vertex shader
			float angle90degrees = PI / 2;
			Matrix4 Tpot = Matrix4.CreateScale( 0.5f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), a );
			Matrix4 Tfloor = Matrix4.CreateScale( 4.0f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), a );
			Matrix4 Tcamera = Matrix4.CreateTranslation( new Vector3( 0, -14.5f, 0 ) ) * Matrix4.CreateFromAxisAngle( new Vector3( 1, 0, 0 ), angle90degrees );
			Matrix4 Tview = Matrix4.CreatePerspectiveFieldOfView( 1.2f, 1.3f, .1f, 1000 );

			// update rotation
			a += 0.001f * frameDuration;
			if( a > 2 * PI ) a -= 2 * PI;

            // post-production stuff
			if( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene to render target
				mesh.Render( shader, Tpot * Tcamera * Tview );
				floor.Render( shader, Tfloor * Tcamera * Tview );

				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene directly to the screen
				mesh.Render( shader, Tpot * Tcamera * Tview );
				floor.Render( shader, Tfloor * Tcamera * Tview );
			}
		}
	}
}