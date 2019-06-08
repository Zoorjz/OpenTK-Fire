using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Fire_v1.Components;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace Fire_v1
{
    public partial class Form1 : Form
    {

        float up = 1;
        Matrix4 _ViewMatrix;
        Matrix4 _projectionMatrix;

        int FireVAO;
        int prog;
        int shadow_texture;
        int shadow_texture2;
        int shadow_texture3;
        int shadow_texture4;
        int shadow_texture5;
        int shadow_texture_GLOB;

        int shadow_size = 2048 ;

        bool EnableRadiusUse = true;

        float partiklesLifeMAX = 1;
        float partiklesLifeMIN = 0.2f;
        //--------------------------------Unity-----------------------------------------

        static FastNoise _fastNoise;
        public Vector3 _gridSize;
        public float _increment = 1.0f;
        public float ParctikleScale;


        public float CentrX = 0;
        public float CentrY = 0;

        public float dirt;

        public float rangSpawn;


        public float TimeToLive;

        public int ParticlePerFrame = 500;

        public float PowerNoise = 0.07f;
        public float PowerSmokeNoise = 0.01f;

        public int CountPartikle;

        public Vector2 Force;

        public float radiusSpawn;

        //public Vector3 Range;
        public int CountParticlesl = 0;

        public float[] RandomfX;

        public float[] RandomfZ;

        float stepOfsetY = 10;

        public Vector3 _offset, _offsetSpeed;

        Vector3 Step0 = new Vector3(252, 255, 172);// Обязательно что бы все занчения УБЫВАЛИ 
        Vector3 Step1 = new Vector3(251, 164, 66);
        Vector3 Step2 = new Vector3(212, 48, 66);
        Vector3 Step3 = new Vector3(5, 0, 0);

        Vector3 ColorSmoke = new Vector3(17, 17, 17);
        //int RandomItemX = 0;
        //int RandomItemZ = 0;
        public float LastRadius;
        Matrix4 Flach;
        //float RandomX;
        //float RandomZ;
        public float[] RandomTheta;
        public int RandomItemTheta = 0;
        public float[] RandomForLife;
        int RandomItemLife = 0;
        static int MaxParticles = 25000;
        float PressureMAX = 0.6f;
        float PressureMIN = 0.3f;
        float bornSize = 4f;
        float sizeSmoke = 0.3f;
        float LifeSmoke = 4;
        float TransparentSmoke = 10;
        float TransparentMIN = 255;

        int LastUsedParticle = 0;

        Particle[] ParticlesContainer = new Particle[MaxParticles];

        int count_flash =0;

        Background back;
        Model bone, pep;
        static float[] g_particule_position_size_data = new float[MaxParticles * 4];
        static byte[] g_particule_color_data = new byte[MaxParticles * 4];

        int particles_position_buffer;
        int particles_color_buffer;
        int CameraRight_worldspace_ID;
        int CameraUp_worldspace_ID;
        int ViewProjMatrixID;
        int billboard_vertex_buffer;
        //int Texture;
        int TexturID;
        int TextuirePlane;
        bool test = false;


        DateTime lastTime;
        //bool test = false;
        static readonly Random rand = new Random();

        Matrix4 modelView;
        public int ParticlesCount = 0;

        public Form1()
        {
            InitializeComponent();
        }

        void ChekControl ()
        {
            bool Error = false;
            EnableRadiusUse = checkBox1.Checked;
            float BackUp_MIN = partiklesLifeMIN;
            float BackUp_MAX = partiklesLifeMAX;
            float.TryParse(textBox2.Text, out partiklesLifeMAX);
            float.TryParse(textBox1.Text, out partiklesLifeMIN);
            if (partiklesLifeMAX < partiklesLifeMIN)
            {
                partiklesLifeMAX = BackUp_MAX;
                partiklesLifeMIN = BackUp_MIN;
            }

            float BackUp_MIN_Speed = PressureMIN;
            float BackUp_MAX_Speed = PressureMAX;
            float.TryParse(textBox6.Text, out PressureMIN);
            float.TryParse(textBox5.Text, out PressureMAX);
            if (PressureMAX < PressureMIN)
            {
                PressureMAX = BackUp_MAX;
                PressureMIN = BackUp_MIN;
            }

            int.TryParse(textBox3.Text, out ParticlePerFrame);
             radiusSpawn = trackBar1.Value;
            float.TryParse(textBox4.Text, out bornSize);
            float.TryParse(textBox7.Text, out sizeSmoke);
            float.TryParse(textBox8.Text, out LifeSmoke);
            float.TryParse(textBox9.Text, out PowerNoise);
            float.TryParse(textBox10.Text, out PowerSmokeNoise);
            float.TryParse(textBox11.Text, out stepOfsetY);
            float.TryParse(textBox15.Text, out _increment);
            // MessageBox.Show(partiklesLifeMAX.ToString() + "  " + partiklesLifeMIN.ToString());

            RandomForLife = MakeRandom(partiklesLifeMIN, partiklesLifeMAX, 50);
            RandomItemLife = 0;
        }

        private void HandleKeyboard()
        {
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Down))
            {
                up -= 1f;
            }
            if (keyState.IsKeyDown(Key.Up))
            {
                up += 1f;
            }
            if (keyState.IsKeyDown(Key.Escape))
            {
                this.Close();
            }
            if (keyState.IsKeyDown(Key.T))
            {
                test = true;
            }
            else
                test = false;
        }

        int shadow_buffer;

        private void glControl1_Load(object sender, EventArgs e)
        {

            bone = Model.FromFile(@"Components\12.fbx");
            pep = Model.FromFile(@"Components\tree.fbx");
            back = new Background();
            int bgtex = Textures.AddTexture(@"Components\grass_low.jpg");
            Textures.Load();
            back.texture = Textures.GetTexture(bgtex).gl_id;
            back.Buffer();
            bone.Buffer();
            pep.Buffer();


            shadow_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadow_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadow_size, shadow_size, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);



            GL.Enable(EnableCap.Texture2D);

            GL.GenTextures(1, out TextuirePlane);
            GL.BindTexture(TextureTarget.Texture2D, TextuirePlane);



            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(@"Components\firefly.png");
            System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bmp.UnlockBits(bmpdata);


            GL.TexImage2D(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba, bmpdata.Width, bmpdata.Height,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpdata.Scan0);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

          


            FireVAO = GL.GenVertexArray();
            GL.BindVertexArray(FireVAO);

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


            CameraRight_worldspace_ID = GL.GetUniformLocation(prog, "CameraRight_worldspace");
            CameraUp_worldspace_ID = GL.GetUniformLocation(prog, "CameraUp_worldspace");
            ViewProjMatrixID = GL.GetUniformLocation(prog, "VP");

            TexturID = GL.GetUniformLocation(prog, "myTextureSampler");

            for (int i = 0; i < MaxParticles; i++)
            {
                ParticlesContainer[i].life = -1.0f;
                ParticlesContainer[i].cameradistance = -1.0f;
            }



            //    Texture = LoadTexture(@"Components\gray.png");

            var vertexStride = sizeof(float);
            billboard_vertex_buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, billboard_vertex_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(g_vertex_buffer_data.Length * vertexStride), g_vertex_buffer_data, BufferUsageHint.StaticDraw);

            particles_position_buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_position_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxParticles * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);

            particles_color_buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_color_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxParticles * 4 * sizeof(byte), IntPtr.Zero, BufferUsageHint.StreamDraw);

            shadow_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadow_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadow_size, shadow_size, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            shadow_texture2 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadow_texture2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadow_size, shadow_size, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            shadow_texture3 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadow_texture3);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadow_size, shadow_size, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);


            shadow_texture4 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadow_texture4);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadow_size, shadow_size, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            shadow_texture5 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadow_texture5);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadow_size, shadow_size, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            shadow_texture_GLOB = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadow_texture_GLOB);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, shadow_size, shadow_size, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);


            pep.ShadowTexture = shadow_texture;
            back.ShadowTexture = shadow_texture;

            pep.ShadowTexture2 = shadow_texture2;
            back.ShadowTexture2 = shadow_texture2;

            pep.ShadowTexture3 = shadow_texture3;
            back.ShadowTexture3 = shadow_texture3;

            pep.ShadowTexture4 = shadow_texture4;
            back.ShadowTexture4 = shadow_texture4;

            pep.ShadowTexture5 = shadow_texture5;
            back.ShadowTexture5 = shadow_texture5;

            pep.ShadowTexture_GLOB = shadow_texture_GLOB;
            back.ShadowTexture_GLOB = shadow_texture_GLOB;

            shadow_buffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadow_buffer);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            comboBox1.SelectedIndex = 0;
            lastTime = DateTime.Now;


            radiusSpawn = 4;
            LastRadius = radiusSpawn;
          

            _fastNoise = new FastNoise();

            //--------------------------------------------------------


            RandomForLife = MakeRandom(partiklesLifeMIN, partiklesLifeMAX, 50);

            _increment = 15f;


            _offset = new Vector3(678, 3489, -700);

            GL.BindVertexArray(0);

            // GL.Enable(EnableCap.Lighting);
            // GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
            //GL.Enable(EnableCap.Normalize);
            Flach = Matrix4.CreateTranslation(0,0.5f,0);

            button4.BackColor = Color.FromArgb((int)Step0.X, (int)Step0.Y, (int)Step0.Z); ;
            button3.BackColor = Color.FromArgb((int)Step1.X, (int)Step1.Y, (int)Step1.Z); ;
            button2.BackColor = Color.FromArgb((int)Step2.X, (int)Step2.Y, (int)Step2.Z); ;
            button1.BackColor = Color.FromArgb((int)Step3.X, (int)Step3.Y, (int)Step3.Z); ;
        }




        public struct Particle : IComparer<Particle>
        {
            public Vector3 pos, speed;
            public char r, g, b, a; // Color
            public float size, bornSize, angle, weight;
            public float life, TotalLife;// Remaining life of the particle. if <0 : dead and unused.
            public float cameradistance; // *Squared* distance to the camera. if dead : -1.0f
            public float calcLife;
            public DateTime LifeDT;
            public bool smoke;
            public bool IniSmoke;

            public bool GLOWWORM;
            public bool initGLOWWORM;


            public bool Sravn(Particle that)
            {
                return this.cameradistance > that.cameradistance;
            }

            public void ChangePos(float powerN, Vector3 offset, float incr)
            {

                float dirX_Plus = _fastNoise.GetSimplex((pos.X + 1) * incr + offset.X, (pos.Y) * incr + offset.Y, (pos.Z) * incr + offset.Z);
                float dirX_Minus = _fastNoise.GetSimplex((pos.X - 1) * incr + offset.X, (pos.Y) * incr + offset.Y, (pos.Z) * incr + offset.Z);
                float dirZ_Plus = _fastNoise.GetSimplex((pos.X) * incr + offset.X, (pos.Y) * incr + offset.Y, (pos.Z + 1) * incr + offset.Z);
                float dirZ_Minus = _fastNoise.GetSimplex((pos.X) * incr + offset.X, (pos.Y) * incr + offset.Y, (pos.Z - 1) * incr + offset.Z);
                float dirY = _fastNoise.GetSimplex((pos.X) * incr + offset.X, (pos.Y + 1) * incr + offset.Y, (pos.Z) * incr + offset.Z);
              
                speed.X += (dirX_Minus + dirX_Plus) * powerN;
                speed.Z += (dirZ_Minus + dirZ_Plus) * powerN;
                speed.Y += dirY * powerN;
                //Vector3 newPos = new Vector3(, , Z);
                var position = pos;
                position.X = pos.X + speed.X;
                position.Y = pos.Y + speed.Y;
                position.Z = pos.Z + speed.Z;
                //Debug.WriteLine("positionBefore: " + pos.X.ToString() + " " + pos.Y.ToString() + " " + pos.Z.ToString() + " Life: " + life.ToString() + " Position: ");
                pos = position;
                //Debug.WriteLine("positionAfter: " + pos.X.ToString() + " " + pos.Y.ToString() + " " + pos.Z.ToString() + " Life: " + life.ToString() + " Position: ");

            }
            public int Compare(Particle p1, Particle p2)
            {
                if (p1.cameradistance < p2.cameradistance)
                    return 1;
                if (p1.cameradistance > p2.cameradistance)
                    return -1;
                else
                    return 0;

            }
        }


        private void glControl1_Resize(object sender, EventArgs e)
        {
            CreateProjection();
        }

       
        private void CreateProjection()
        {

            var aspectRatio = (float)Width / Height;
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                60 * ((float)Math.PI / 180f), // field of view angle, in radians
                aspectRatio,                // current window aspect ratio
                1f,                       // near plane
                1000f);                     // far plane
        }

     

        private void timer1_Tick(object sender, EventArgs e)
        {
            HandleKeyboard();
            ChekControl();
            int s = (int)(1f / ((float)timer1.Interval / 1000));
            this.Text = "(Vsync: {VSync}) FPS: " + s.ToString();

            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = 0.0f;
            backColor.G = 0.0f;
            backColor.B = 0.0f;
            GL.ClearColor(backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Debug.WriteLine(e.Time.ToString());

            //DateTime currentTime = DateTime.Now;
            //TimeSpan Delta = currentTime - lastTime;
            //lastTime = currentTime;
            Vector3 CameraPosition = new Vector3(70 + up, 60 + up, 50 + up);
            CreateProjection();
            _ViewMatrix = Matrix4.LookAt(CameraPosition, new Vector3(0.0f, 5.0f, 0.0f), new Vector3(0, 1, 0));
            //// We will need the camera's position in order to sort the particles
            //// w.r.t the camera's distance.
            //// There should be a getCameraPosition() function in common/controls.cpp, 
            //// but this works too.
            //glm::vec3 CameraPosition(glm::inverse(ViewMatrix)[3]);

            //тут бред написан. Не знаю пока как вычестя ть это .

            Matrix4 ViewProjectionMatrix = _ViewMatrix * _projectionMatrix;

            //int newparticles = (int)(e.Time * 10000.0);
            //if (newparticles > (int)(0.016f * 10000.0))
            //    newparticles = (int)(0.016f * 10000.0);


            if (LastRadius != radiusSpawn)
            {
                RandomfX = MakeRandom(radiusSpawn, 100);
                RandomfZ = MakeRandom(radiusSpawn, 200);
            }

            for (int i = 0; i < ParticlePerFrame; i++)
            {
                int particleIndex = FindUnusedParticle();

                float theta = (float)(2f * Math.PI) * (float)random.NextDouble();
                float distanceDouble = (float)random.NextDouble();
                float distance = distanceDouble * radiusSpawn;

                float px = distance * (float)Math.Cos(theta) + CentrX;
                float py = distance * (float)Math.Sin(theta) + CentrY;

                ParticlesContainer[particleIndex].pos = new Vector3(px, 0, py);


                float spread = 0.9f;
                Vector3 maindir = new Vector3(0.01f, PressureMAX, 0.01f);
                if (EnableRadiusUse)
                    ParticlesContainer[particleIndex].life = RandomForLife[RandomItemLife] * (0.1f + 1 - distanceDouble);// This particle will live 2-5 seconds.
                else
                    ParticlesContainer[particleIndex].life = RandomForLife[RandomItemLife];
                ParticlesContainer[particleIndex].TotalLife = ParticlesContainer[particleIndex].life;
                RandomItemLife++;

                double RandPress = random.NextDouble();
                float PressureRes = PressureMIN + (PressureMAX - PressureMIN) * (float)RandPress;
                Vector3 randomdir = new Vector3(0, PressureRes, 0);

                ParticlesContainer[particleIndex].speed = (maindir + randomdir) * spread;
                //Debug.WriteLine(ParticlesContainer[particleIndex].pos.X.ToString() + "  " + ParticlesContainer[particleIndex].pos.Y.ToString() + "  " + ParticlesContainer[particleIndex].pos.Z.ToString() + "   speed: " + ParticlesContainer[particleIndex].speed.X.ToString() + " " + ParticlesContainer[particleIndex].speed.Y.ToString() + " " + ParticlesContainer[particleIndex].speed.Z.ToString() + " Life: " + ParticlesContainer[particleIndex].life.ToString() + " RandomDir: " + randomdir.X.ToString() + "  " + randomdir.Y.ToString() + "  " + randomdir.Z.ToString());

                ParticlesContainer[particleIndex].smoke = false;
                ParticlesContainer[particleIndex].IniSmoke = false;
                // Very bad way to generate a random color
                ParticlesContainer[particleIndex].r = (char)Step0.X;
                ParticlesContainer[particleIndex].g = (char)Step0.Y;
                ParticlesContainer[particleIndex].b = (char)Step0.Z;
                ParticlesContainer[particleIndex].a = (char)TransparentMIN;
                ParticlesContainer[particleIndex].cameradistance = new Vector3(ParticlesContainer[particleIndex].pos - CameraPosition).Length;
                ParticlesContainer[particleIndex].bornSize = bornSize;
                ParticlesContainer[particleIndex].size = bornSize;

                if (RandomItemLife > 48)
                    RandomItemLife = 0;

            }

            LastRadius = radiusSpawn;

            /*     if (!test)
                     SortParticles();*/

            int ParticlesCount = 0;
            for (int i = 0; i < MaxParticles; i++)
            {
                if (ParticlesContainer[i].life > 0.0f)
                {
                    Particle p = ParticlesContainer[i];
                    // Decrease life
                    p.life -= (float)timer1.Interval/ 1000;

                    if (p.life > 0.0f)
                    {


                        p.cameradistance = new Vector3(p.pos - CameraPosition).Length;
                        float calcLife = p.life / p.TotalLife;
                        p.calcLife = calcLife;
                        //Debug.WriteLine(calcLife.ToString());
                        //ParticlesContainer[i].pos += glm::vec3(0.0f,10.0f, 0.0f) * (float)delta;

                        if (!p.smoke && !p.GLOWWORM)
                        {


                            float gradientPr = 1f - calcLife;



                           

                            if (gradientPr < 0.45f)
                            {
                                if (Step0.X >= Step1.X)
                                {
                                    int razniX = (int)(Step0.X - Step1.X);
                                    p.r = (char)(Step0.X - (razniX * gradientPr));
                                }
                                else
                                {
                                    int razniX = (int)(Step1.X - Step0.X);
                                    p.r = (char)(Step0.X + (razniX * gradientPr));
                                }
                                if (Step0.Y >= Step1.Y)
                                {
                                    int razniY = (int)(Step0.Y - Step1.Y);
                                    p.g = (char)(Step0.Y - (razniY * gradientPr));
                                }
                                else
                                {
                                    int razniY = (int)(Step1.Y - Step0.Y);
                                    p.g = (char)(Step0.Y + (razniY * gradientPr));
                                }
                                if (Step0.Z >= Step1.Z)
                                {
                                    int razniZ = (int)(Step0.Z - Step1.Z);
                                    p.b = (char)(Step0.Z - (razniZ * gradientPr));
                                }
                                else
                                {
                                    int razniZ = (int)(Step1.Z - Step0.Z);
                                    p.b = (char)(Step0.Z + (razniZ * gradientPr));
                                }
                            }
                            if (gradientPr >= 0.45f && gradientPr < 0.75f && !(partiklesLifeMAX * 0.3f > p.TotalLife))
                            {
                                if (Step1.X >= Step2.X)
                                {
                                    int razniX = (int)(Step1.X - Step2.X);
                                    p.r = (char)(Step1.X - (razniX * gradientPr));
                                }
                                else
                                {
                                    int razniX = (int)(Step2.X - Step1.X);
                                    p.r = (char)(Step1.X + (razniX * gradientPr));
                                }
                                if (Step1.Y >= Step2.Y)
                                {
                                    int razniY = (int)(Step1.Y - Step2.Y);
                                    p.g = (char)(Step1.Y - (razniY * gradientPr));
                                }
                                else
                                {
                                    int razniY = (int)(Step2.Y - Step1.Y);
                                    p.g = (char)(Step1.Y + (razniY * gradientPr));
                                }
                                if (Step1.Z >= Step2.Z)
                                {
                                    int razniZ = (int)(Step1.Z - Step2.Z);
                                    p.b = (char)(Step1.Z - (razniZ * gradientPr));
                                }
                                else
                                {
                                    int razniZ = (int)(Step2.Z - Step1.Z);
                                    p.b = (char)(Step1.Z + (razniZ * gradientPr));
                                }
                              

                            }
                            if (gradientPr >= 0.9f && !(partiklesLifeMAX * 0.7f > p.TotalLife))
                            {
                                if (Step2.X > Step3.X)
                                {
                                    int razniX = (int)(Step2.X - Step3.X);
                                    p.r = (char)(Step2.X - (razniX * gradientPr));
                                }
                                else
                                {
                                    int razniX = (int)(Step3.X - Step2.X);
                                    p.r = (char)(Step2.X + (razniX * gradientPr));
                                }
                                if (Step2.Y > Step3.Y)
                                {
                                    int razniY = (int)(Step2.Y - Step3.Y);
                                    p.g = (char)(Step2.Y - (razniY * gradientPr));
                                }
                                else
                                {
                                    int razniY = (int)(Step3.Y - Step2.Y);
                                    p.g = (char)(Step2.Y + (razniY * gradientPr));
                                }
                                if (Step2.Z > Step3.Z)
                                {
                                    int razniZ = (int)(Step2.Z - Step3.Z);
                                    p.b = (char)(Step2.Z - (razniZ * gradientPr));
                                }
                                else
                                {
                                    int razniZ = (int)(Step3.Z - Step2.Z);
                                    p.b = (char)(Step2.Z + (razniZ * gradientPr));
                                }

                            }
                            p.a = (char)(TransparentMIN * p.calcLife);
                            p.size = p.bornSize * p.calcLife;
                            double randomSmokeChanse = random.NextDouble();
                            if (randomSmokeChanse > 0.98f)
                            {
                                p.size = 0.2f;
                                p.smoke = true;
                                p.GLOWWORM = false;
                                p.life = p.TotalLife * LifeSmoke;                           
                                p.TotalLife = p.life;
                              //  p.calcLife = p.life / p.TotalLife;
                                p.IniSmoke = false;
                            }
                            else
                            {
                                if (randomSmokeChanse > 0.9795f)
                                {
                                    p.size = 0.3f * (float)random.NextDouble();
                                    p.smoke = false;
                                    p.GLOWWORM = true;
                                    p.life = p.TotalLife * 2;
                                    p.TotalLife = p.life;
                                    p.initGLOWWORM = false;
                                }
                            }
                            p.ChangePos(PowerNoise, _offset, _increment);

                        }
                        else
                        {

                            if (p.GLOWWORM && !p.initGLOWWORM)
                            {
                                p.r = (char)247;
                                p.g = (char)137;
                                p.b = (char)8;
                                p.a = (char)(255 * p.calcLife);
                                p.initGLOWWORM = true;
                            }
                            if (!p.IniSmoke && p.smoke)
                            {
                                if (!checkBox2.Checked)
                                {
                                    //Debug.WriteLine("держите дым");
                                    int randomGray = random.Next(50, 70);

                                    p.r = (char)randomGray;
                                    p.g = (char)randomGray;
                                    p.b = (char)randomGray;
                                    //p.a = (char)(100);
                                    if (p.calcLife > 0.95f)
                                        p.a = (char)(trackBar2.Value);
                                    else
                                        p.a = (char)0;
                                }
                                else
                                {

                                    p.r = (char)ColorSmoke.X;
                                    p.g = (char)ColorSmoke.Y;
                                    p.b = (char)ColorSmoke.Z;
                                    //p.a = (char)(255 * (1 - p.calcLife));
                                    if (p.calcLife > 0.95f)
                                        p.a = (char)trackBar2.Value;
                                    else
                                        p.a = (char)0;
                                }
                                p.IniSmoke = true;
                            }
                            if (p.smoke)
                                p.size += sizeSmoke;

                            p.ChangePos(PowerSmokeNoise, _offset, _increment);

                        }

                      
                    }
                    else
                    {

                        p.ChangePos(PowerNoise, _offset, _increment);
                        p.cameradistance = new Vector3(p.pos - CameraPosition).Length;
                        p.smoke = false;
                        p.IniSmoke = false;
                        p.initGLOWWORM = false;
                        p.GLOWWORM = false;
                        //                        LastB[ItemLastB].cameradistance = p.cameradistance;
                        //                        ItemLastB++;
                    }
                    ParticlesContainer[i] = p;
                    ParticlesCount++;

                }
                else
                {
                    //if (ParticlesContainer[i].smoke)
                    //    ParticlesContainer[i].ChangePos(PowerSmokeNoise, _offset, _increment);
                    //else
                    //    ParticlesContainer[i].ChangePos(PowerNoise, _offset, _increment);
                    ParticlesContainer[i].a = (char)0;
                    ParticlesCount++;
                }
            }

            SortParticles();

            for (int i = 0; i < ParticlesCount; i++)
            {
                Particle p = ParticlesContainer[i];
                g_particule_position_size_data[4 * i + 0] = p.pos.X;
                g_particule_position_size_data[4 * i + 1] = p.pos.Y;
                g_particule_position_size_data[4 * i + 2] = p.pos.Z;

                g_particule_position_size_data[4 * i + 3] = p.size;

                g_particule_color_data[4 * i + 0] = (byte)p.r;
                g_particule_color_data[4 * i + 1] = (byte)p.g;
                g_particule_color_data[4 * i + 2] = (byte)p.b;
                g_particule_color_data[4 * i + 3] = (byte)p.a;
            }


            _offset.Y -= stepOfsetY;

            GL.Disable(EnableCap.Blend);

            Matrix4 pep_transform = Matrix4.CreateScale(0.6f, 0.6f, 0.6f) * Matrix4.CreateTranslation(-10.0f, -0.2f, -25.0f);

            Matrix4 GLOBAL_Projection = Matrix4.CreateOrthographic(300, 300, -1000.0f, 1000.0f);
            Matrix4 GLOBAL_light_Pos = Matrix4.CreateRotationY((float)(azimuthTrack.Value * Math.PI / 180.0)) * Matrix4.CreateRotationX((float)(elevationTrack.Value * Math.PI / 180.0));

            Matrix4 fire_shadow_projection;

            if (!checkBox3.Checked)
            {
                 fire_shadow_projection = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 2.0), 1.0f, 2f, 300f);
            }
            else
            {
                 fire_shadow_projection = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 2.0), 1.0f, 2f, 50f);
            }
            if (count_flash % 3 == 0)
            {
                Flach = Matrix4.CreateTranslation(0.0f + (float)random.NextDouble() * 0.3f, -2.5f + (float)random.NextDouble() * 0.3f, 0.0f + (float)random.NextDouble() * 0.3f);
            }
            //Matrix4 fire_shadow_modelview = Matrix4.CreateRotationY((float)(Math.PI / 180f * trackBar3.Value)) * Matrix4.CreateTranslation(0.0f, -5f, 0.0f);
            Matrix4 fire_shadow_modelview  = Matrix4.CreateRotationY((float)((Math.PI / 2.0f) * 0)) * Flach;
            Matrix4 fire_shadow_modelview2 = Matrix4.CreateRotationY((float)((Math.PI / 2.0f) * 1)) * Flach;
            Matrix4 fire_shadow_modelview3 = Matrix4.CreateRotationY((float)((Math.PI / 2.0f) * 2)) * Flach;
            Matrix4 fire_shadow_modelview4 = Matrix4.CreateRotationY((float)((Math.PI / 2.0f) * 3)) * Flach;
            Matrix4 fire_shadow_modelview5 = Matrix4.CreateRotationX((float)((Math.PI / 2.0f) * 3)) * Matrix4.CreateRotationZ((float)((Math.PI / 2.0f) * trackBar5.Value)) * Flach;

            Matrix4 New = Matrix4.CreateScale(8.0f, 8.0f, 8.0f) * Matrix4.CreateTranslation(0, -0.5f, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadow_buffer);
            GL.Viewport(0, 0, shadow_size, shadow_size);
            GL.ColorMask(false, false, false, false);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadow_texture, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            pep.RenderShadow(fire_shadow_projection, pep_transform * fire_shadow_modelview, shadow_size);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadow_texture2, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            pep.RenderShadow(fire_shadow_projection, pep_transform * fire_shadow_modelview2, shadow_size);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadow_texture3, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            pep.RenderShadow(fire_shadow_projection, pep_transform * fire_shadow_modelview3, shadow_size);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadow_texture4, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            pep.RenderShadow(fire_shadow_projection, pep_transform * fire_shadow_modelview4, shadow_size);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadow_texture5, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            pep.RenderShadow(fire_shadow_projection, pep_transform * fire_shadow_modelview5, shadow_size);


            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadow_texture_GLOB, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            pep.RenderShadow(GLOBAL_Projection, pep_transform * GLOBAL_light_Pos, shadow_size);
            bone.RenderShadow(GLOBAL_Projection, New *   GLOBAL_light_Pos, shadow_size);

            //Matrix4 lightProjection = Matrix4.CreateOrthographic(300, 300, -1000.0f, 1000.0f);
            //Matrix4 lightPos = Matrix4.CreateRotationY((float)(0 * Math.PI / 180.0)) * Matrix4.CreateRotationX((float)(45 * Math.PI / 180.0)) * Matrix4.CreateTranslation(0.0f, 0.0f, -250.0f);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.ColorMask(true, true, true, true);




           // pep.RenderShadow(fire_shadow_projection, pep_transform * fire_shadow_modelview, shadow_texture, shadow_size);
            //back.ShadowMatrix = lightPos * lightProjection ;
            back.ShadowMatrix =  fire_shadow_modelview  * fire_shadow_projection;
            back.ShadowMatrix2 = fire_shadow_modelview2 * fire_shadow_projection;
            back.ShadowMatrix3 = fire_shadow_modelview3 * fire_shadow_projection;
            back.ShadowMatrix4 = fire_shadow_modelview4 * fire_shadow_projection;
            back.ShadowMatrix5 = fire_shadow_modelview5 * fire_shadow_projection;
            back.ShadowMatrix_GLOB = GLOBAL_light_Pos * GLOBAL_Projection;


            if (count_flash % 3 == 0)
            {
                back.Lights[0] = new Vector3(0, 0.8f, 0) + new Vector3((float)(random.NextDouble() * 0.2 ), (float)(random.NextDouble() * 0.2 ), (float)(random.NextDouble() * 0.2 ));
                back.Lights[1] = new Vector3(0, 0.8f, 0) + new Vector3((float)(random.NextDouble() * 0.2), (float)(random.NextDouble() * 0.2), (float)(random.NextDouble() * 0.2 ));
                back.Lights[2] = new Vector3(0, 0.8f, 0) + new Vector3((float)(random.NextDouble() * 0.2), (float)(random.NextDouble() * 0.2 ), (float)(random.NextDouble() * 0.2));
                back.Lights[3] = new Vector3(0, 0.8f, 0) + new Vector3((float)(random.NextDouble() * 0.2), (float)(random.NextDouble() * 0.2 ), (float)(random.NextDouble() * 0.2 ));
            }
            count_flash++;

            Vector3[] ObjLight = new Vector3[4];

            ObjLight[0] = back.Lights[0];
            ObjLight[1] = back.Lights[1];
            ObjLight[2] = back.Lights[2];
            ObjLight[3] = back.Lights[3];

            OpenTK.Vector3  GlobDir;
            //Добавить свет и для моделей !!!!!!!!!!!!!!!!!!!!
            if (checkBox3.Checked)
            {
                //objects.GlobalLightDir = (Matrix4.CreateRotationY((float)(azimuthTrack.Value * Math.PI / 180.0)) * Matrix4.CreateRotationX((float)(elevationTrack.Value * Math.PI / 180.0)) * (Vector4.UnitZ)).Xyz;
                GlobDir = (Matrix4.CreateRotationY((float)(azimuthTrack.Value * Math.PI / 180.0)) * Matrix4.CreateRotationX((float)(elevationTrack.Value * Math.PI / 180.0)) * (Vector4.UnitZ)).Xyz;
            }
            else
            {
                //objects.GlobalLightDir = Vector3.Zero;
                GlobDir = Vector3.Zero;// (Matrix4.CreateRotationY((float)(azimuthTrack.Value * Math.PI / 180.0)) * Matrix4.CreateRotationX((float)(elevationTrack.Value * Math.PI / 180.0)) * (Vector4.UnitZ)).Xyz;              
            }
            back.GlobalLightDir = GlobDir;
            // back.ShadowMatrix_GLOB = fire_shadow_modelview_GLOB * fire_shadow_projection;

            //pep.ShadowMatrix = lightPos * lightProjection;
            //pep.Light = (Matrix4.CreateRotationY((float)(70 * Math.PI / 180.0)) * Matrix4.CreateRotationX((float)(200 * Math.PI / 180.0)) * (-Vector4.UnitZ)).Xyz;


            // ДОделать к пепу тени а точнее шадоуматрикс



            GL.Viewport(0, 0, Width, Height);


            back.Render(_projectionMatrix, _ViewMatrix);
            //pep.Render(_projectionMatrix, _ViewMatrix, lightPos /** lightProjection */);

             New = Matrix4.CreateScale(8.0f, 8.0f, 8.0f) *Matrix4.CreateTranslation(0,-0.5f,0)* _ViewMatrix;

            GL.UniformMatrix4(GL.GetUniformLocation(back.shader, "modelview"), false, ref New);
           // bone.Render(_projectionMatrix, New,  fire_shadow_modelview * fire_shadow_projection);

            Matrix4 New2 = _ViewMatrix;

            //            GL.UniformMatrix4(GL.GetUniformLocation(back.shader, "modelview"), false, ref New2);
            pep.Render(_projectionMatrix, pep_transform * _ViewMatrix, pep_transform * fire_shadow_modelview * fire_shadow_projection ,pep_transform * fire_shadow_modelview2 * fire_shadow_projection,pep_transform * fire_shadow_modelview3 * fire_shadow_projection,pep_transform * fire_shadow_modelview4 * fire_shadow_projection,pep_transform * fire_shadow_modelview5 * fire_shadow_projection,pep_transform * GLOBAL_light_Pos * GLOBAL_Projection, shadow_texture, shadow_texture2, shadow_texture3, shadow_texture4, shadow_texture5, shadow_texture_GLOB, GlobDir, ObjLight);
            
            bone.Render(_projectionMatrix, New , New * fire_shadow_modelview * fire_shadow_projection, New * fire_shadow_modelview2 * fire_shadow_projection, New * fire_shadow_modelview3 * fire_shadow_projection, New * fire_shadow_modelview4 * fire_shadow_projection, New * fire_shadow_modelview5 * fire_shadow_projection, New * GLOBAL_light_Pos * GLOBAL_Projection, shadow_texture3, shadow_texture3, shadow_texture3, shadow_texture3, shadow_texture3, shadow_texture_GLOB, GlobDir, ObjLight);
            GL.UseProgram(prog);

            GL.BindVertexArray(FireVAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_position_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxParticles * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(ParticlesCount * sizeof(float) * 4), g_particule_position_size_data);

            GL.BindBuffer(BufferTarget.ArrayBuffer, particles_color_buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxParticles * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(ParticlesCount * sizeof(float) * 4), g_particule_color_data);


            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);



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
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
            GL.Enable(EnableCap.DepthTest);
            //GL.Translate(-10f, 0, 0);




            GL.UseProgram(prog);
            GL.BindTexture(TextureTarget.Texture2D, 7);  // изменяем текстуру для частиц



            GL.DepthMask(false);

            GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, ParticlesCount);

            GL.DepthMask(true);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);

            GL.UseProgram(0);

            int show = 0;
            if (comboBox1.Text == "shadow_texture")
            {
                show = shadow_texture;
            }
            if (comboBox1.Text == "shadow_texture2")
            {
                show = shadow_texture2;
            }
            if (comboBox1.Text == "shadow_texture3")
            {
                show = shadow_texture3;
            }
            if (comboBox1.Text == "shadow_texture4")
            {
                show = shadow_texture4;
            }
            if (comboBox1.Text == "shadow_texture5")
            {
                show = shadow_texture5;
            }
            if (comboBox1.Text == "shadow_texture_GLOB")
            {
                show = shadow_texture_GLOB;
            }



            if (test)
            {
                GL.UseProgram(0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D,show);
                GL.Begin(PrimitiveType.TriangleStrip);
                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex3(1.0f, -1.0f, 0.0f);

                GL.TexCoord2(1.0f, 1.0f);
                GL.Vertex3(1.0f, 1.0f, 0.0f);

                GL.TexCoord2(0.0f, 0.0f);
                GL.Vertex3(-1.0f, -1.0f, 0.0f);

                GL.TexCoord2(0.0f, 1.0f);
                GL.Vertex3(-1.0f, 1.0f, 0.0f);
                GL.End();
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            // GL.Disable(EnableCap.Light2);
            glControl1.SwapBuffers();
            CountParticlesl = 0;
        }

        static Random random = new Random();
        float[] MakeRandom(float s, int count)
        {
            float[] Randoms = new float[count];

            for (int i = 0; i < count; i++)
            {
                float r1 = random.Next((int)(s * -1.0f), (int)s);
                double d = random.NextDouble();
                Randoms[i] = r1 + (float)d;
            }
            return Randoms;

        }

        float[] MakeRandom(float s, float k, int count)
        {
            if (s < k)
            {
                float[] Randoms = new float[count];

                for (int i = 0; i < count; i++)
                {
                    double d = random.NextDouble();

                    float p = k - s;
                    Randoms[i] = s + (p * (float)d);
                }
                return Randoms;
            }
            return null;

        }

        int FindUnusedParticle()
        {
            int maxLife = 0;
            float MaxLifeFlot = partiklesLifeMAX;
            for (int i = LastUsedParticle; i < MaxParticles; i++)
            {
                if (ParticlesContainer[i].life < 0)
                {
                    LastUsedParticle = i;
                    return i;
                }
                if (ParticlesContainer[i].life < MaxLifeFlot)
                {
                    MaxLifeFlot = ParticlesContainer[i].life;
                    maxLife = i;
                }
            }

            for (int i = 0; i < LastUsedParticle; i++)
            {
                if (ParticlesContainer[i].life < 0)
                {
                    LastUsedParticle = i;
                    return i;
                }
                if (ParticlesContainer[i].life < MaxLifeFlot)
                {
                    maxLife = i;
                    MaxLifeFlot = ParticlesContainer[i].life;
                }
            }

            return maxLife; // All particles are taken, override oldes one
        }

        private void button4_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            button4.BackColor = colorDialog1.Color;
            Step0.X = colorDialog1.Color.R;
            Step0.Y = colorDialog1.Color.G;
            Step0.Z = colorDialog1.Color.B;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            button3.BackColor = colorDialog1.Color;
            Step1.X = colorDialog1.Color.R;
            Step1.Y = colorDialog1.Color.G;
            Step1.Z = colorDialog1.Color.B;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            button2.BackColor = colorDialog1.Color;
            Step2.X = colorDialog1.Color.R;
            Step2.Y = colorDialog1.Color.G;
            Step2.Z = colorDialog1.Color.B;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            button1.BackColor = colorDialog1.Color;
            Step3.X = colorDialog1.Color.R;
            Step3.Y = colorDialog1.Color.G;
            Step3.Z = colorDialog1.Color.B;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!button5.Enabled)
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;

                Step0 = new Vector3(button4.BackColor.R, button4.BackColor.G, button4.BackColor.B);
                Step1 = new Vector3(button3.BackColor.R, button3.BackColor.G, button3.BackColor.B);
                Step2 = new Vector3(button2.BackColor.R, button2.BackColor.G, button2.BackColor.B);
                Step3 = new Vector3(button1.BackColor.R, button1.BackColor.G, button1.BackColor.B);
            }
            else
            {

                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;


                Step0 = new Vector3(252, 255, 172);
                Step1 = new Vector3(251, 164, 66);
                Step2 = new Vector3(212, 48, 66);
                Step3 = new Vector3(5, 0, 0);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            button5.BackColor = colorDialog1.Color;
            ColorSmoke.X = colorDialog1.Color.R;
            ColorSmoke.Y = colorDialog1.Color.G;
            ColorSmoke.Z = colorDialog1.Color.B;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        void SortParticles()
        {
            Array.Sort(ParticlesContainer, new Particle());
        }

        public static float[] g_vertex_buffer_data = {
         -0.5f, -0.5f, 0.0f,
          0.5f, -0.5f, 0.0f,
         -0.5f,  0.5f, 0.0f,
          0.5f,  0.5f, 0.0f,
    };
    }
}
/*                        if (test)
                                              {
                                                  LastB[ItemLastB].pos = p.pos;
                                                  LastB[ItemLastB].life = p.life;
                                                  LastB[ItemLastB].size = p.size;
                                                  LastB[ItemLastB].TotalLife = p.TotalLife;
                                                  LastB[ItemLastB].bornSize = p.bornSize;
                                                  LastB[ItemLastB].calcLife = p.calcLife;
                                                  LastB[ItemLastB].cameradistance = p.cameradistance;
                                                  LastB[ItemLastB].r = p.r;
                                                  LastB[ItemLastB].g = p.g;
                                                  LastB[ItemLastB].b = p.b;
                                                  ItemLastB++;
                                              }
                                              else
                                              {
                                                  // Fill the GPU buffer
                                                  g_particule_position_size_data[4 * ParticlesCount + 0] = p.pos.X;
                                                  g_particule_position_size_data[4 * ParticlesCount + 1] = p.pos.Y;
                                                  g_particule_position_size_data[4 * ParticlesCount + 2] = p.pos.Z;

                                                  g_particule_position_size_data[4 * ParticlesCount + 3] = p.bornSize * calcLife;

                                                  g_particule_color_data[4 * ParticlesCount + 0] = (byte)p.r;
                                                  g_particule_color_data[4 * ParticlesCount + 1] = (byte)p.g;
                                                  g_particule_color_data[4 * ParticlesCount + 2] = (byte)p.b;
                                                  g_particule_color_data[4 * ParticlesCount + 3] = (byte)((char)(TransparentMIN * calcLife));
                                                  CountParticlesl++;
                                              }*/




//float Noise = _fastNoise.GetSimplex(xoff + _offset.x, yoff + _offset.y, zoff + _offset.z) + 1;
//if (dirX_Minus < dirX_Plus)               
//    speed.X += (dirX_Plus - dirX_Minus) * powerN;            
//else
//    speed.X += (dirX_Minus - dirX_Plus) * powerN;
//if (dirZ_Minus < dirZ_Plus)
//    speed.Z += (dirZ_Plus - dirZ_Minus) * powerN;
//else
//    speed.Z += (dirZ_Minus - dirZ_Plus) * powerN;



//CursorVisible = true;
//LastB = new Particle[MaxParticles];
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




//GL.ShadeModel(ShadingModel.Smooth);
//GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
//_verticeCount = vertices.Length;

//_vertexArray = GL.GenVertexArray();
//_buffer = GL.GenBuffer();

//GL.BindVertexArray(_vertexArray);
//GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);



//Vector3 newSprite = transform.position;
//newSprite.x += _gridSize.x * 0.5f;
//newSprite.z += _gridSize.z * 0.5f;
//Particals.Add(Instantiate(ParcticleSprite, newSprite, transform.rotation));
//RandomfX = MakeRandom(radiusSpawn, 200);
//RandomfZ = MakeRandom(radiusSpawn, 100);

//RandomTheta = MakeRandom(0, 2f*3.14159f , 400);
//TransformPositionX = transform.position.x;
//TransformPositionY = transform.position.y;
//TransformPositionZ = transform.position.z;
//TransformRotation = transform.rotation;



/*            if (test)
            { 
                SortParticlesLastB();


                for (int i = 0; i < ItemLastB; i++)
                {
                    if (LastB[i].life > 0.0f )
                    {
                        g_particule_position_size_data[4 * i + 0] = LastB[i].pos.X;
                        g_particule_position_size_data[4 * i + 1] = LastB[i].pos.Y;
                        g_particule_position_size_data[4 * i + 2] = LastB[i].pos.Z;

                        g_particule_position_size_data[4 * i + 3] = LastB[i].bornSize * LastB[i].calcLife;

                        g_particule_color_data[4 * i + 0] = (byte)LastB[i].r;
                        g_particule_color_data[4 * i + 1] = (byte)LastB[i].g;
                        g_particule_color_data[4 * i + 2] = (byte)LastB[i].b;
                        g_particule_color_data[4 * i + 3] = (byte)((char)(TransparentMIN * LastB[i].calcLife));
                    }
                }
            }*/

//printf("%d ",ParticlesCount);


// Update the buffers that OpenGL uses for rendering.
// There are much more sophisticated means to stream data from the CPU to the GPU, 
// but this is outside the scope of this tutorial.
// http://www.opengl.org/wiki/Buffer_Object_Streaming




//  GL.Enable(EnableCap.DepthTest);
//в модел вюь её закинуть просто

//МНОЖИТЬ МАТРИЦЫ НУЖНО В ОБРАТНОМ ПОРЯДКЕ!!!!!



//rotx += 2.5f; roty += 1.5f; rotz += 0.4f;
//modelView = Matrix4.CreateRotationX((float)(rotx * Math.PI / 180)) * Matrix4.CreateRotationY((float)(rotx * Math.PI / 180)) * Matrix4.CreateRotationZ((float)(rotx * Math.PI / 180)) * Matrix4.CreateTranslation(0, 0, -5.0f);

//CreateProjection();
//_ViewMatrix = Matrix4.LookAt(new Vector3(30 + up, 20 + up, 10 + up), new Vector3(0.0f, 15.0f, 0.0f), new Vector3(0, 1, 0));

//modelView = _ViewMatrix * _projectionMatrix;

//Vector3 CameraPosition = new Vector3(1, 1, 1);

//Matrix4 ViewProjectionMatrix = _ViewMatrix * _projectionMatrix;

//  GL.BindTexture(TextureTarget.Texture2D, 0);

//  GL.MatrixMode(MatrixMode.Projection);
//  //GL.LoadMatrix(ref _projectionMatrix);
////  GL.LoadIdentity();

//  GL.MatrixMode(MatrixMode.Modelview);
//GL.Translate(1.5f,0.0f,-7.0f);
//Matrix4 moveModel = Matrix4.CreateTranslation(0.0f, 5.0f, 0.0f);


//GL.Translate(-10f, 0,0);
//GL.Begin(PrimitiveType.Quads);

//GL.Color3(1.0f, 0.0f, 0.0f);
//GL.TexCoord2(up, 0);
//GL.Vertex3(1.0f, 0.0f, -1.0f);
//GL.TexCoord2(0, 0);
//GL.Vertex3(-1.0f, 0.0f, -1.0f);
//GL.TexCoord2(0, up);
//GL.Vertex3(-1.0f, 0.0f, 1.0f);
//GL.TexCoord2(up, up);
//GL.Vertex3(1.0f, 0.0f, 1.0f);

//GL.Color3(0.0f, 2.0f, 0.0f);
//GL.TexCoord2(up, 0);
//GL.Vertex3(1.0f, -2.0f, -1.0f);
//GL.TexCoord2(0, 0);
//GL.Vertex3(-1.0f, -2.0f, -1.0f);
//GL.TexCoord2(0, up);
//GL.Vertex3(-1.0f, -2.0f, 1.0f);
//GL.TexCoord2(up, up);
//GL.Vertex3(1.0f, -2.0f, 1.0f);

//GL.Color3(0.0f, 0.0f, 1.0f);
//GL.TexCoord2(up, 0);
//GL.Vertex3(1.0f, -2.0f, -1.0f);
//GL.TexCoord2(0, 0);
//GL.Vertex3(1.0f, 0.0f, -1.0f);
//GL.TexCoord2(0, up);
//GL.Vertex3(1.0f, 0.0f, 1.0f);
//GL.TexCoord2(up, up);
//GL.Vertex3(1.0f, -2.0f, 1.0f);

//GL.Color3(1.0f, 1.0f, 0.0f);
//GL.TexCoord2(up, 0);
//GL.Vertex3(1.0f, -2.0f, -1.0f);
//GL.TexCoord2(0, 0);
//GL.Vertex3(1.0f, 0.0f, -1.0f);
//GL.TexCoord2(0, up);
//GL.Vertex3(-1.0f, 0.0f, -1.0f);
//GL.TexCoord2(up, up);
//GL.Vertex3(-1.0f, -2.0f, -1.0f);

//GL.Color3(0.0f, 1.0f, 1.0f);
//GL.TexCoord2(up, 0);
//GL.Vertex3(1.0f, -2.0f, 1.0f);
//GL.TexCoord2(0, 0);
//GL.Vertex3(1.0f, 0.0f, 1.0f);
//GL.TexCoord2(0, up);
//GL.Vertex3(-1.0f, 0.0f, 1.0f);
//GL.TexCoord2(up, up);
//GL.Vertex3(-1.0f, -2.0f, 1.0f);

//GL.Color3(0.8f, 0.8f, 0.8f);
//GL.TexCoord2(up, 0);
//GL.Vertex3(-1.0f, -2.0f, -1.0f);
//GL.TexCoord2(0, 0);
//GL.Vertex3(-1.0f, 0.0f, -1.0f);
//GL.TexCoord2(0, up);
//GL.Vertex3(-1.0f, 0.0f, 1.0f);
//GL.TexCoord2(up, up);
//GL.Vertex3(-1.0f, -2.0f, 1.0f);
//GL.End();


//GL.UniformMatrix4(ViewProjMatrixID, false, ref ViewProjectionMatrix);

//            Debug.WriteLine(ParticlesCount);




//Matrix4 scaleModel = Matrix4.CreateScale(6, 6, 6);

//ViewProjectionMatrix = scaleModel * ViewProjectionMatrix;
//GL.LoadMatrix(ref ViewProjectionMatrix);

//float[] material_diffuse = { 1.0f, 1.0f, 1.0f, 1.0f };
//GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, material_diffuse);

//float[] light2_diffuse = { 0.9f, 0.8f, 0.09f, 1.0f };
//float[] light2_position = { 2.0f, 2.0f, 2.0f, 1.0f };
//GL.Enable(EnableCap.Light2);
//GL.Light(LightName.Light2, LightParameter.Diffuse, light2_diffuse);
//GL.Light(LightName.Light2, LightParameter.Position, light2_position);
//GL.Light(LightName.Light2, LightParameter.ConstantAttenuation, 0.0f);
//GL.Light(LightName.Light2, LightParameter.LinearAttenuation, 0.1f);
//GL.Light(LightName.Light2, LightParameter.QuadraticAttenuation, 0.2f);


//makeEnviromet();

//  back.Render(_projectionMatrix,_ViewMatrix);