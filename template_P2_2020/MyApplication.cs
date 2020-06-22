using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;

namespace Template
{
	class MyApplication
	{
        const int bulletDespawnDistance = 16;
        const float PI = 3.1415926535f,
            bulletSpeed = 0.05f,
            bulletSpawnDistanceFromTurretCenter = 1.0f;

        // member variables
        public Surface screen;                  // background surface for printing etc.
        public Light[] lights;                  // array with the lights

        SceneGraph scene = new SceneGraph();    // the scenegraph
        List<Mesh> world = new List<Mesh>();    // the list of meshes where all objects reside; the world

        Mesh turret, floor;                     // a mesh to draw using OpenGL
        float cam_x = 0.0f,
            cam_y = 0.0f,
            cam_z = 0.0f,
            cam_rot = 0.0f;                     // camera positions and rotations
        float turret_rot = 0.0f;                // turret rotation

		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader,                          // shader to use for rendering
		    postproc;                           // shader to use for post processing
		Texture wood;                           // texture to use for rendering
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing

        KeyboardState keyState, lastKeyState;   // keyboard states
        MouseState mouseState, lastMouseState;  // mouse states

        bool useRenderTarget = true;            // post-processing

		// initialize
		public void Init()
		{
            // load texture
            wood = new Texture("../../assets/wood.jpg");

            // load lights
            List<Light> ls = new List<Light>();
            ls.Add(new Light(new Vector4(0, 2, 0, 1), new Vector3(8.0f, 8.0f, 8.0f), Vector3.Zero, Key.Number0));
            ls.Add(new Light(new Vector4(1, 1, 1, 1), new Vector3(8.0f, 8.0f, 8.0f), new Vector3(-1.0f, -1.0f, -1.0f), Key.Number1, 12.5f, 15.0f));
            ls.Add(new Light(new Vector4(1, 1, -1, 1), new Vector3(8.0f, 0.0f, 0.0f), new Vector3(-1.0f, -1.0f, 1.0f), Key.Number2, 12.5f, 15.0f));
            ls.Add(new Light(new Vector4(-1, 1, 1, 1), new Vector3(0.0f, 8.0f, 0.0f), new Vector3(1.0f, -1.0f, -1.0f), Key.Number3, 12.5f, 15.0f));
            ls.Add(new Light(new Vector4(-1, 1, -1, 1), new Vector3(0.0f, 0.0f, 8.0f), new Vector3(1.0f, -1.0f, 1.0f), Key.Number4, 12.5f, 15.0f));
            lights = ls.ToArray();

            // load turret teapot and floor
            turret = new Mesh("../../assets/teapot.obj", wood, lights);
            floor = new Mesh("../../assets/floor.obj", wood, lights);

            // the floor is base of the world, with the turret as its first child
            world.Add(floor);
            floor.children.Add(turret);

            // initialize stopwatch
            timer = new Stopwatch();
			timer.Reset();
            timer.Start();
			// create shaders
			shader = new Shader( "../../shaders/vs.glsl", "../../shaders/fs.glsl", BuildULightArray() );
			postproc = new Shader( "../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl", BuildULightArray() );
			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();
		}

        // build array containing the location IDs of the light variables in the glsl files
		private UniformLight[] BuildULightArray()
        {
            List<UniformLight> list = new List<UniformLight>();
			foreach (Light l in lights)
                list.Add(new UniformLight());
            return list.ToArray();
        }

		// tick for background surface
		public void Tick()
        {
            UpdateBullets();

            mouseState = Mouse.GetState();
            keyState = Keyboard.GetState();

            // shoot bullets by clicking the left mouse button or by pressing space
            if (LeftMouseKeyPressed() || KeyPressed(Key.Space))
                MakeBullet();

            // check if lights should be switched off
            for (int i = 0; i < lights.Length; i++)
            {
                if (KeyPressed(lights[i].key))
                    lights[i].Switch();
            }

            if (KeyPressed(Key.M))
                if (useRenderTarget)
                    useRenderTarget = false;                        // Switch post-proc
                else useRenderTarget = true;

            if (KeyDown(Key.A)) cam_x += 0.2f;                      // Move left
            if (KeyDown(Key.D)) cam_x -= 0.2f;                      // Move right
            if (KeyDown(Key.W)) cam_z += 0.2f;                      // Move up
            if (KeyDown(Key.S)) cam_z -= 0.2f;                      // Move down
            if (KeyDown(Key.Z)) { cam_x = 0.0f; cam_z = 0.0f; }     // Reset Position

            if (KeyDown(Key.Q)) cam_y += 0.2f;                      // Zoom in
            if (KeyDown(Key.E)) cam_y -= 0.2f;                      // Zoom out
            if (KeyDown(Key.X)) cam_y = 0.0f;                       // Reset Zoom

            if (KeyDown(Key.R)) cam_rot -= 0.03f;                   // Rotate camera left
            if (KeyDown(Key.F)) cam_rot += 0.03f;                   // Rotate camera right
            if (KeyPressed(Key.C)) cam_rot = 0.0f;                  // Reset camera rotation

            if (KeyDown(Key.Left)) turret_rot -= 0.05f;             // Rotate turret left
            if (KeyDown(Key.Right)) turret_rot += 0.05f;            // Rotate turret right
            if (KeyPressed(Key.Down)) turret_rot = 0.0f;            // Reset turret rotation

            lastMouseState = mouseState;
            lastKeyState = keyState;
        }

        public bool LeftMouseKeyPressed()
        { return mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released; }

        public bool KeyPressed(Key key)
        { return keyState[key] && (keyState[key] != lastKeyState[key]); }

        public bool KeyDown(Key key)
        { return keyState[key]; }

        // tick for OpenGL rendering code
        public void RenderGL()
		{
			// measure frame duration
			timer.Reset();
			timer.Start();

			// prepare matrix for vertex shader
			float angle90degrees = PI / 2;

            Matrix4 turretRotation = new Matrix4(
                new Vector4((float)Math.Cos(turret_rot), 0, (float)Math.Sin(turret_rot), 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(-(float)Math.Sin(turret_rot), 0, (float)Math.Cos(turret_rot), 0),
                new Vector4(0, 0, 0, 1));

            turret.local = Matrix4.CreateScale(.125f) * Matrix4.CreateTranslation(new Vector3(0, -2.0f, 0)) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), 0) * turretRotation;
            floor.local = Matrix4.CreateScale(4.0f) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), 0);

            Matrix4 camRotation = new Matrix4(
                new Vector4((float)Math.Cos(cam_rot), 0, (float)Math.Sin(cam_rot), 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(-(float)Math.Sin(cam_rot), 0, (float)Math.Cos(cam_rot), 0),
                new Vector4(0, 0, 0, 1));

            Matrix4 Tcamera = Matrix4.CreateTranslation( new Vector3( cam_x, cam_y - 14.5f, cam_z ) ) * Matrix4.CreateFromAxisAngle( new Vector3( 1, 0, 0 ), angle90degrees );
			Matrix4 Tview = Matrix4.CreatePerspectiveFieldOfView( 1.2f, 1.3f, .1f, 1000 );

            Vector3 viewPos = -Tcamera.ExtractTranslation();

            // post-production stuff
			if( useRenderTarget )
			{
				// enable render target
				target.Bind();

                // render scene to render target
                scene.Render(world, shader, camRotation * Tcamera * Tview, viewPos);

                // render quad
                target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
                // render scene directly to the screen
                scene.Render( world, shader, camRotation * Tcamera * Tview, viewPos );
            }
        }

        // spawn a bullet along with its orbital
        void MakeBullet()
        {
            Matrix4 turretRotation = new Matrix4(
                new Vector4((float)Math.Cos(-turret_rot), 0, (float)Math.Sin(-turret_rot), 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(-(float)Math.Sin(-turret_rot), 0, (float)Math.Cos(-turret_rot), 0),
                new Vector4(0, 0, 0, 1));

            Matrix4 turretRotation2 = new Matrix4(
                new Vector4((float)Math.Cos(turret_rot), 0, (float)Math.Sin(turret_rot), 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(-(float)Math.Sin(turret_rot), 0, (float)Math.Cos(turret_rot), 0),
                new Vector4(0, 0, 0, 1));

            Vector3 velocity = (turretRotation * new Vector4(bulletSpeed, 0,0, 1)).Xyz;
            Bullet bullet = new Bullet ("../../assets/teapot.obj", wood, lights, velocity);
            floor.children.Add(bullet);

            bullet.local *= Matrix4.CreateTranslation(new Vector3(bulletSpawnDistanceFromTurretCenter, -2.0f, 0)) * turretRotation2;

            // every bullet will spawn an orbital
            Mesh orbital = new Mesh("../../assets/teapot.obj", wood, lights);
            bullet.children.Add(orbital);
        }

        void UpdateBullets()
        {
            List<int> indexBullets = new List<int>();
            foreach (Mesh child in floor.children)
            {
                if (child.GetType() == typeof(Bullet))
                {
                    // get the indices of the bullets that should be removed from the world
                    if (Math.Abs(child.local[3, 0]) + Math.Abs(child.local[3, 2]) > bulletDespawnDistance)
                    {
                        indexBullets.Add(floor.children.IndexOf(child));
                        continue;
                    }

                    // update rotation of the orbitals
                    foreach (Mesh orbital in child.children)
                    {
                        float frameDuration = timer.ElapsedMilliseconds;
                        child.a += 0.004f * frameDuration;
                        if (child.a > 2 * PI) child.a -= 2 * PI;
                        orbital.local = Matrix4.CreateScale(.4f) * Matrix4.CreateTranslation(new Vector3(0, 0, 10)) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), child.a);
                    }
                }
            }

            // reverse the list so the indices go from high to low
            indexBullets.Reverse();

            // remove the bullet at each index in the list
            foreach (int index in indexBullets)
            {
                floor.children.RemoveAt(index);
            }
        }
    }
}