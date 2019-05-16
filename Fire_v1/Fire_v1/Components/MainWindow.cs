﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;


namespace Fire_v1.Components
{
   public sealed class MainWindow: GameWindow
    {

        float partiklesLife = 5;

        public struct Particle : IComparer<Particle>
        {
            public Vector3 pos, speed;
            public char r, g, b, a; // Color
            public float size, angle, weight;
            public float life; // Remaining life of the particle. if <0 : dead and unused.
            public float cameradistance; // *Squared* distance to the camera. if dead : -1.0f

            public DateTime LifeDT;

            public bool Sravn (Particle that)
            {
                return this.cameradistance > that.cameradistance;
            }

            public int Compare(Particle p1, Particle p2)
            {
                if (p1.cameradistance > p2.cameradistance)
                    return 1;
                if (p1.cameradistance < p2.cameradistance)
                    return -1;
                else
                    return 0;

            }
        }

        static int MaxParticles = 10;
        //List<Particle> ParticlesContainer = new List<Particle>();
        int LastUsedParticle = 0;
        Particle[] ParticlesContainer = new Particle[MaxParticles];

        int FindUnusedParticle()
        {
            int maxLife = 0;
            for (int i = LastUsedParticle; i < MaxParticles; i++)
            {
                if (ParticlesContainer[i].life < 0)
                {
                    LastUsedParticle = i;
                    return i;
                }
                if (ParticlesContainer[i].life > maxLife)
                    maxLife = i;
            }

            for (int i = 0; i < LastUsedParticle; i++)
            {
                if (ParticlesContainer[i].life < 0)
                {
                    LastUsedParticle = i;
                    return i;
                }
                if (ParticlesContainer[i].life > maxLife)
                    maxLife = i;
            }

            return maxLife; // All particles are taken, override oldes one
        }
        void SortParticles()
        {
            Array.Sort(ParticlesContainer, new Particle());
        }


       //OIT - Без сортировки прозрачность

            public MainWindow()
        : base(1280, // initial width
        720, // initial height
        GraphicsMode.Default,
        "dreamstatecoding",  // initial title
        GameWindowFlags.Default,
        DisplayDevice.Default,
        4, // OpenGL major version
        0, // OpenGL minor version
        GraphicsContextFlags.ForwardCompatible)
        {
            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);
        }

        public static float[] g_vertex_buffer_data = {
         -0.5f, -0.5f, 0.0f,
          0.5f, -0.5f, 0.0f,
         -0.5f,  0.5f, 0.0f,
          0.5f,  0.5f, 0.0f,
    };

        int prog;
        protected override void OnLoad(EventArgs e)
        {
            //CursorVisible = true;

            //Particle p1 = new Particle { cameradistance = 1f };
            //Particle p2 = new Particle { cameradistance = 2222f };
            //Particle p3 = new Particle { cameradistance = 167f };
            //Particle p4 = new Particle { cameradistance = -1f };

            //Particle[] par = new Particle[] { p1,p2,p3,p4};
            //Array.Sort(par, new Particle());

            //for (int i = 0; i < par.Length; i++)
            //{
            //    Debug.WriteLine(par[i].cameradistance.ToString());
            //}

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Enable(EnableCap.Texture2D);
            GL.GenTextures(1, out TextuirePlane);
            GL.BindTexture(TextureTarget.Texture2D, TextuirePlane);



            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(@"Components\firefly.png");
            System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits( new System.Drawing.Rectangle (0,0,bmp.Width,bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bmp.UnlockBits(bmpdata);
          

            GL.TexImage2D(TextureTarget.Texture2D,0,OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba, bmpdata.Width, bmpdata.Height,
                0,OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,bmpdata.Scan0);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            //GL.ShadeModel(ShadingModel.Smooth);
            //GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            //_verticeCount = vertices.Length;

            //_vertexArray = GL.GenVertexArray();
            //_buffer = GL.GenBuffer();

            //GL.BindVertexArray(_vertexArray);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);


            int VertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayID);

            int programID_vert = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(programID_vert, File.ReadAllText(@"Components\Shaders\Partikle.vert"));
            GL.CompileShader(programID_vert);

            int programID_frag = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(programID_frag, File.ReadAllText(@"Components\Shaders\Partikle.frag"));
            GL.CompileShader(programID_frag);

            prog = GL.CreateProgram();
            GL.AttachShader(prog, programID_vert);
            GL.AttachShader(prog, programID_frag);
            GL.LinkProgram(prog);

            //Возможно поттебуется отчистка шейдеров

             CameraRight_worldspace_ID = GL.GetUniformLocation(prog, "CameraRight_worldspace");
             CameraUp_worldspace_ID = GL.GetUniformLocation(prog, "CameraUp_worldspace");
             ViewProjMatrixID = GL.GetUniformLocation(prog, "VP");

            TexturID = GL.GetUniformLocation(prog, "myTextureSampler");

            for (int i = 0; i < MaxParticles; i++)
            {
                ParticlesContainer[i].life = -1.0f;
                ParticlesContainer[i].cameradistance = -1.0f;
            }

            //int Textur = LoadDDS(@"C:\Users\zoorj\source\repos\OpenTK_lessons\initialisation_1\initialisation_1\components\particle.DDS");
            //Debug.WriteLine("BItch" + Textur.ToString());
            //Кароч пока без текстуры потому что это ещё на 24 часа заёб
            //Надеюсь прокатит 

         //    Texture = LoadTexture(@"Components\gray.png");

            var vertexStride = sizeof(float);
             billboard_vertex_buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, billboard_vertex_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(g_vertex_buffer_data.Length * vertexStride), g_vertex_buffer_data, BufferUsageHint.StaticDraw);
            
            particles_position_buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_position_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxParticles * 4 * sizeof(float), IntPtr.Zero ,BufferUsageHint.StreamDraw);

            particles_color_buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_color_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxParticles * 4 * sizeof(byte), IntPtr.Zero, BufferUsageHint.StreamDraw);

             lastTime = DateTime.Now;


             //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
             //GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
        }

        static float[] g_particule_position_size_data = new float[MaxParticles * 4];
        static byte[] g_particule_color_data = new byte[MaxParticles * 4];

        int particles_position_buffer;
        int particles_color_buffer;
        int CameraRight_worldspace_ID;
        int CameraUp_worldspace_ID;
        int ViewProjMatrixID;
        int billboard_vertex_buffer;
        int Texture;
        int TexturID;
        int TextuirePlane;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            HandleKeyboard();
        }
        private void HandleKeyboard()
        {
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Down))
            {
                up -= 0.1f;
            }
            if (keyState.IsKeyDown(Key.Up))
            {
                up += 0.1f;
            }
            if (keyState.IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }
        float up= 1;
        Matrix4 _ViewMatrix;
        Matrix4 _projectionMatrix;
        private void CreateProjection()
        {

            var aspectRatio = (float)Width / Height;
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                60 * ((float)Math.PI / 180f), // field of view angle, in radians
                aspectRatio,                // current window aspect ratio
                1f,                       // near plane
                1000f);                     // far plane
        }

        DateTime lastTime;
        Random rand;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            int s = (int)(1f / e.Time);
            Title = "(Vsync: {VSync}) FPS: " + s.ToString() ;

            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = 0.0f;
            backColor.G = 0.1f;
            backColor.B = 0.3f;
            GL.ClearColor(backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Debug.WriteLine(e.Time.ToString());

            DateTime currentTime = DateTime.Now;
            TimeSpan Delta = currentTime - lastTime;
            lastTime = currentTime;

            CreateProjection();
            _ViewMatrix = Matrix4.LookAt(new Vector3(30, 20, 10), new Vector3(0.0f, 15.0f, 0.0f), new Vector3(0, 1, 0));


            //// We will need the camera's position in order to sort the particles
            //// w.r.t the camera's distance.
            //// There should be a getCameraPosition() function in common/controls.cpp, 
            //// but this works too.
            //glm::vec3 CameraPosition(glm::inverse(ViewMatrix)[3]);

            Vector3 CameraPosition = new Vector3(1, 1, 1);

            Matrix4 ViewProjectionMatrix =  _ViewMatrix * _projectionMatrix ;

            int newparticles = (int)(e.Time * 10000.0);
            if (newparticles > (int)(0.016f * 10000.0))
                newparticles = (int)(0.016f * 10000.0);
            rand = new Random();
            for (int i = 0; i < newparticles; i++)
            {


                //раньше работало так, что если не нашлась свободная частица которая умерла, то берётся нулевая.
                //Но таким образом никто не успевал дальше чем на один шаг отойти. 
                //так что попробуем без нулейвой сиграть. 

                int particleIndex = FindUnusedParticle();
                if (particleIndex > 0)
                {
                    ParticlesContainer[particleIndex].life = partiklesLife; // This particle will live 5 seconds.
                    ParticlesContainer[particleIndex].pos = new Vector3(0, 0, 0);

                    float spread = 1f;
                    Vector3 maindir = new Vector3(0.0f, 1.0f, 0.0f);
                    // Very bad way to generate a random direction; 
                    // See for instance http://stackoverflow.com/questions/5408276/python-uniform-spherical-distribution instead,
                    // combined with some user-controlled parameters (main direction, spread, etc)
                    Vector3 randomdir = new Vector3(0, 1, 0);

                    ParticlesContainer[particleIndex].speed = maindir + randomdir * spread;


                    // Very bad way to generate a random color
                    ParticlesContainer[particleIndex].r = (char)250;
                    ParticlesContainer[particleIndex].g = (char)0;
                    ParticlesContainer[particleIndex].b = (char)0;
                    ParticlesContainer[particleIndex].a = (char)250;

                    ParticlesContainer[particleIndex].size = 2f;
                }

            }

            // Simulate all particles

            //GL.enableAlphaTest
            //GL.AlpchaFunc


            int ParticlesCount = 0;
            for (int i = 0; i < MaxParticles; i++)
            {

                Particle p = ParticlesContainer[i]; // shortcut

                if (p.life > 0.0f)
                {

                    // Decrease life
                    p.life -= (float)e.Time;
                    if (p.life > 0.0f)
                    {

                        // Simulate simple physics : gravity only, no collisions
                        //p.speed += new Vector3(0.0f, -0.1f, 0.0f) ;
                        p.pos += p.speed;
                        Debug.WriteLine(p.pos.X.ToString() +  "  "  + p.pos.Y.ToString() + "  "+ p.pos.Z.ToString()  + "   speed: " + p.speed.X.ToString() + " " + p.speed.Y.ToString() + " " + p.speed.Z.ToString() + " Life: " + p.life.ToString());
                        p.cameradistance = new Vector3(p.pos - CameraPosition).Length;
                        //ParticlesContainer[i].pos += glm::vec3(0.0f,10.0f, 0.0f) * (float)delta;

                        // Fill the GPU buffer
                        g_particule_position_size_data[4 * ParticlesCount + 0] = p.pos.X;
                        g_particule_position_size_data[4 * ParticlesCount + 1] = p.pos.Y;
                        g_particule_position_size_data[4 * ParticlesCount + 2] = p.pos.Z;

                        g_particule_position_size_data[4 * ParticlesCount + 3] = p.size;

                        g_particule_color_data[4 * ParticlesCount + 0] = (byte)p.r;
                        g_particule_color_data[4 * ParticlesCount + 1] = (byte)p.g;
                        g_particule_color_data[4 * ParticlesCount + 2] = (byte)p.b;
                        g_particule_color_data[4 * ParticlesCount + 3] = (byte)p.a;

                    }
                    else
                    {
                        // Particles that just died will be put at the end of the buffer in SortParticles();
                        p.cameradistance = -1.0f;
                    }

                    ParticlesCount++;

                }
            }

            //SortParticles();

            //printf("%d ",ParticlesCount);


            // Update the buffers that OpenGL uses for rendering.
            // There are much more sophisticated means to stream data from the CPU to the GPU, 
            // but this is outside the scope of this tutorial.
            // http://www.opengl.org/wiki/Buffer_Object_Streaming



            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_position_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxParticles * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(ParticlesCount * sizeof(float) * 4), g_particule_position_size_data);

            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_color_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxParticles * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(ParticlesCount * sizeof(float) * 4), g_particule_color_data);


            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.UseProgram(prog);

            //// Bind our texture in Texture Unit 0
            //glActiveTexture(GL_TEXTURE0);
            //glBindTexture(GL_TEXTURE_2D, Texture);
            //// Set our "myTextureSampler" sampler to use Texture Unit 0 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //glUniform1i(TextureID, 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextuirePlane);
            GL.Uniform1(TexturID, 0);

            GL.Uniform3(CameraRight_worldspace_ID, _ViewMatrix[0, 0], _ViewMatrix[1, 0], _ViewMatrix[2, 0]);
            GL.Uniform3(CameraUp_worldspace_ID, _ViewMatrix[0, 1], _ViewMatrix[1, 1], _ViewMatrix[2, 1]);

            //            float gldd = ViewProjectionMatrix[0, 0];
            GL.UniformMatrix4(ViewProjMatrixID, false, ref ViewProjectionMatrix);


            // 1rst attribute buffer : vertices
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, billboard_vertex_buffer);
            GL.VertexAttribPointer(
                0,
                3,
                VertexAttribPointerType.Float,
                false,
                0,
                0);

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_position_buffer);
            GL.VertexAttribPointer(
                1,
                4,
                VertexAttribPointerType.Float,
                false,
                0,
                0);

            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_color_buffer);
            GL.VertexAttribPointer(
                2,
                4,
                VertexAttribPointerType.UnsignedByte,
                true,
                0,
                0);

            // These functions are specific to glDrawArrays*Instanced*.
            // The first parameter is the attribute buffer we're talking about.
            // The second parameter is the "rate at which generic vertex attributes advance when rendering multiple instances"
            // http://www.opengl.org/sdk/docs/man/xhtml/glVertexAttribDivisor.xml

            GL.VertexBindingDivisor(0, 0);
            GL.VertexBindingDivisor(1, 1);
            GL.VertexBindingDivisor(2, 1);

            // Draw the particules !
            // This draws many times a small triangle_strip (which looks like a quad).
            // This is equivalent to :
            // for(i in ParticlesCount) : glDrawArrays(GL_TRIANGLE_STRIP, 0, 4), 
            // but faster.



            GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, ParticlesCount);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);

            GL.UseProgram(0);

            //в модел вюь её закинуть просто

            //МНОЖИТЬ МАТРИЦЫ НУЖНО В ОБРАТНОМ ПОРЯДКЕ!!!!!

           

            rotx += 2.5f; roty += 1.5f; rotz += 0.4f;
            modelView = Matrix4.CreateRotationX((float)(rotx * Math.PI / 180)) * Matrix4.CreateRotationY((float)(rotx * Math.PI / 180)) * Matrix4.CreateRotationZ((float)(rotx * Math.PI / 180)) * Matrix4.CreateTranslation(0, 0, -5.0f);

            CreateProjection();
            _ViewMatrix = Matrix4.LookAt(new Vector3(30, 20, 10), new Vector3(0.0f, 15.0f, 0.0f), new Vector3(0, 1, 0));

            modelView = _ViewMatrix * _projectionMatrix;

            //Vector3 CameraPosition = new Vector3(1, 1, 1);

            //Matrix4 ViewProjectionMatrix = _ViewMatrix * _projectionMatrix;



            GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadMatrix(ref _projectionMatrix);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            //GL.Translate(1.5f,0.0f,-7.0f);
            GL.LoadMatrix(ref modelView);

            GL.Begin(PrimitiveType.Quads);

            GL.Color3(1.0f,  0.0f, 1.0f);
            GL.TexCoord2(up, 0);
            GL.Vertex3(1.0f, 0.0f, -1.0f);
            GL.TexCoord2(0, 0);
            GL.Vertex3(-1.0f, 0.0f, -1.0f);
            GL.TexCoord2(0, up  );
            GL.Vertex3(-1.0f, 0.0f, 1.0f);
            GL.TexCoord2(up, up);
            GL.Vertex3(1.0f, 0.0f, 1.0f);


            GL.End();


            //GL.UniformMatrix4(ViewProjMatrixID, false, ref ViewProjectionMatrix);

//            Debug.WriteLine(ParticlesCount);

            SwapBuffers();
        }
        Matrix4 modelView;
         public int ParticlesCount = 0;

        int LoadDDS(string imagepath)
        {
            char[] Header = new char[124];

            using (StreamReader sr = new StreamReader(imagepath, System.Text.Encoding.Default))
            {
                char[] array = new char[4];
                // считываем 4 символа
                sr.Read(array, 0, 4);

                return(array[0]);
            }

        }

        float rotx, roty, rotz;

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }

        private int LoadTexture(string filename)
        {
            int res = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, res);
            
            Bitmap teximage = Image.FromFile(filename) as Bitmap;
            BitmapData texdata = teximage.LockBits(new Rectangle(0, 0, teximage.Width, teximage.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, teximage.Width, teximage.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, texdata.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            teximage.UnlockBits(texdata);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return res;
        }


    }
}
