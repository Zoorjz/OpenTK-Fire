using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Fire_v1.Components
{
    class Background
    {



        struct VertexData
        {
            public Vector3 position;
            public Vector2 tex_coord;
            public Vector3 normal;
        }

        VertexData[] vertexes = { new VertexData { position = new Vector3(500.0f, 0.0f, 500.0f), normal = Vector3.UnitY, tex_coord = new Vector2(30.0f, 30.0f) },
                                  new VertexData { position = new Vector3(-500.0f, 0.0f, 500.0f),normal = Vector3.UnitY, tex_coord = new Vector2(0.0f, 30.0f) },
                                  new VertexData { position = new Vector3(500.0f, 0.0f, -500.0f), normal = Vector3.UnitY, tex_coord = new Vector2(30.0f, 0.0f) },
                                  new VertexData { position = new Vector3(-500.0f, 0.0f, -500.0f),  normal = Vector3.UnitY, tex_coord = new Vector2(0.0f, 0.0f) } };

        int vao;
        int vbo;
        public int shader;
        public int texture;
        public int ShadowTexture;
        public int ShadowTexture2;
        public int ShadowTexture3;
        public int ShadowTexture4;
        public int ShadowTexture5;
        public int ShadowTexture_GLOB;


        public Vector3[] Lights = new Vector3[4];

        public Vector3 GlobalLightDir;

        public Matrix4 ShadowMatrix = Matrix4.Identity;
        public Matrix4 ShadowMatrix2 = Matrix4.Identity;
        public Matrix4 ShadowMatrix3 = Matrix4.Identity;
        public Matrix4 ShadowMatrix4 = Matrix4.Identity;
        public Matrix4 ShadowMatrix5 = Matrix4.Identity;
        public Matrix4 ShadowMatrix_GLOB = Matrix4.Identity;

        string vertex_shader_src = @"
#version 450 core
in vec3 vertex;
in vec2 tex_coord;
in vec3 normal;

uniform mat4 modelview;
uniform mat4 projection;
uniform mat4 shadowMat1;
uniform mat4 shadowMat2;
uniform mat4 shadowMat3;
uniform mat4 shadowMat4;
uniform mat4 shadowMat_GLOB;

out vec2 tc;
out vec3 norm;
out vec3 pos;
out vec4 shadow_coord1;
out vec4 shadow_coord2;
out vec4 shadow_coord3;
out vec4 shadow_coord4;
out vec4 shadow_coord_GLOB;

void main() {
  gl_Position = projection*modelview*vec4(vertex, 1.0);
  tc = tex_coord;
  shadow_coord1 = shadowMat1 * vec4(vertex, 1.0);
  shadow_coord2 = shadowMat2 * vec4(vertex, 1.0);
  shadow_coord3 = shadowMat3 * vec4(vertex, 1.0);
  shadow_coord4 = shadowMat4 * vec4(vertex, 1.0);
  shadow_coord_GLOB = shadowMat_GLOB * vec4(vertex, 1.0);
  norm = normal;
  pos = vertex.xyz;


}
";

        string fragment_shader_src = @"
#version 450 core
in vec2 tc;
in vec4 shadow_coord1;
in vec4 shadow_coord2;
in vec4 shadow_coord3;
in vec4 shadow_coord4;
in vec4 shadow_coord_GLOB;
in vec3 norm;
in vec3 pos;

uniform sampler2D tex;
uniform sampler2D shadow1;
uniform sampler2D shadow2;
uniform sampler2D shadow3;
uniform sampler2D shadow4;
uniform sampler2D shadow_GLOB;

uniform vec3 light[4];
uniform vec3 global_light;

out vec4 FragColor;

void main() {
  float visibility = 1.0;
  vec3 sc1 = vec3((shadow_coord1.xy*0.5)/shadow_coord1.w+vec2(0.5), shadow_coord1.z*0.5/shadow_coord1.w+0.49995);
  vec3 sc2 = vec3((shadow_coord2.xy*0.5)/shadow_coord2.w+vec2(0.5), shadow_coord2.z*0.5/shadow_coord2.w+0.49995);
  vec3 sc3 = vec3((shadow_coord3.xy*0.5)/shadow_coord3.w+vec2(0.5), shadow_coord3.z*0.5/shadow_coord3.w+0.49995);
  vec3 sc4 = vec3((shadow_coord4.xy*0.5)/shadow_coord4.w+vec2(0.5), shadow_coord4.z*0.5/shadow_coord4.w+0.49995);

  vec4 rescolor = vec4(1.0);

float shadow_shade = 1.0;


  if (sc1.x >= 0.0 && sc1.y >= 0.0 && sc1.x <= 1.0 && sc1.y <= 1.0 && shadow_coord1.z > 0 &&
      texture2D(shadow1, sc1.xy).x < sc1.z)
    shadow_shade = 0.6;

  if (sc2.x >= 0.0 && sc2.y >= 0.0 && sc2.x <= 1.0 && sc2.y <= 1.0 && shadow_coord2.z > 0 &&
      texture2D(shadow2, sc2.xy).x < sc2.z)
    shadow_shade = 0.6;

  if (sc3.x >= 0.0 && sc3.y >= 0.0 && sc3.x <= 1.0 && sc3.y <= 1.0 && shadow_coord3.z > 0 &&
      texture2D(shadow3, sc3.xy).x < sc3.z)
    shadow_shade = 0.6;

  if (sc4.x >= 0.0 && sc4.y >= 0.0 && sc4.x <= 1.0 && sc4.y <= 1.0 && shadow_coord4.z > 0 &&
      texture2D(shadow4, sc4.xy).x < sc4.z)
    shadow_shade = 0.6;


 float global_shade = 1.0;
  if (texture2D(shadow_GLOB, (shadow_coord_GLOB.xy*0.5)/shadow_coord_GLOB.w+vec2(0.5)).x < shadow_coord_GLOB.z*0.5/shadow_coord_GLOB.w+0.49995){
    global_shade = 0.4;
  }

float kl[4];

  for (int i = 0; i < 4; i++)
    kl[i] = dot(norm, normalize(light[i]-pos));

 float light_shade;
  if (length(global_light) > 0)
  {
   light_shade = (0.5+max(0.0, dot(norm, global_light))) * 2;
   shadow_shade = 1.0-(1.0-shadow_shade)/2.2;
   // global_shade = 0.2;
  }
  else
  {
    light_shade = 0.5;
    global_shade = 0.5;
  }

 float fireness = (max(0.0, kl[0])+max(0.0, kl[1])+max(0.0, kl[2])+max(0.0, kl[3])) * 2;

float total_shade = min(min(light_shade, shadow_shade), global_shade);

  vec4 base_color = texture2D(tex, tc);

//  FragColor = rescolor;
   // FragColor = texture2D(tex, tc) * light_shade ;
  
    //FragColor = vec4(norm, 1.0);  



if (length(global_light) > 0)
  {
     FragColor = (base_color*total_shade+base_color*vec4(1.0, 0.5, 0.5, 1.0)*fireness)/max(total_shade+fireness, 1.0);

  }
else
{
   FragColor = texture2D(tex, tc) * fireness * shadow_shade; 
}


   //FragColor = texture2D(tex, tc) *  global_shade;

//FragColor = base_color*min(light_shade, shadow_shade);
 

 // gl_FragColor = texture2D(tex, tc)*visibility;
}
";


        public void Buffer()
        {
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            GL.BindVertexArray(vao);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.IndexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.IndexArray);

            GL.BufferData(BufferTarget.ArrayBuffer, Marshal.SizeOf(typeof(VertexData)) * vertexes.Length, vertexes, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (int)Marshal.SizeOf(typeof(VertexData)), (int)Marshal.OffsetOf(typeof(VertexData), "position"));
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (int)Marshal.SizeOf(typeof(VertexData)), (int)Marshal.OffsetOf(typeof(VertexData), "tex_coord"));
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (int)Marshal.SizeOf(typeof(VertexData)), (int)Marshal.OffsetOf(typeof(VertexData), "normal"));

            GL.BindVertexArray(0);

            shader = GL.CreateProgram();
            int vertex_shader = GL.CreateShader(ShaderType.VertexShader);
            int fragment_shader = GL.CreateShader(ShaderType.FragmentShader);

            string result;

            GL.ShaderSource(vertex_shader, vertex_shader_src);
            GL.CompileShader(vertex_shader);
            result = GL.GetShaderInfoLog(vertex_shader);


            GL.ShaderSource(fragment_shader, fragment_shader_src);
            GL.CompileShader(fragment_shader);
            result = GL.GetShaderInfoLog(fragment_shader);

            GL.AttachShader(shader, vertex_shader);
            GL.AttachShader(shader, fragment_shader);

            GL.BindAttribLocation(shader, 0, "vertex");
            GL.BindAttribLocation(shader, 1, "tex_coord");
            GL.BindAttribLocation(shader, 2, "normal");

            GL.LinkProgram(shader);
            result = GL.GetProgramInfoLog(shader);

            int LinkRes = 0;
            GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out LinkRes);
            if (LinkRes != 1)
                return;
        }

        public void Render(Matrix4 projection, Matrix4 modelview)
        {
            GL.BindVertexArray(vao);
            GL.UseProgram(shader);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "modelview"), false, ref modelview);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "projection"), false, ref projection);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "shadowMat1"), false, ref ShadowMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "shadowMat2"), false, ref ShadowMatrix2);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "shadowMat3"), false, ref ShadowMatrix3);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "shadowMat4"), false, ref ShadowMatrix4);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "shadowMat_GLOB"), false, ref ShadowMatrix_GLOB);

            GL.Uniform3(GL.GetUniformLocation(shader, "global_light"), ref GlobalLightDir);

            float[] lights = { Lights[0].X, Lights[0].Y, Lights[0].Z,
                               Lights[1].X, Lights[1].Y, Lights[1].Z,
                               Lights[2].X, Lights[2].Y, Lights[2].Z,
                               Lights[3].X, Lights[3].Y, Lights[3].Z };

            GL.Uniform3(GL.GetUniformLocation(shader, "light"), 4, lights);

            GL.Uniform1(GL.GetUniformLocation(shader, "tex"), 0);
            GL.Uniform1(GL.GetUniformLocation(shader, "shadow1"), 1);
            GL.Uniform1(GL.GetUniformLocation(shader, "shadow2"), 2);
            GL.Uniform1(GL.GetUniformLocation(shader, "shadow3"), 3);
            GL.Uniform1(GL.GetUniformLocation(shader, "shadow4"), 4);
            GL.Uniform1(GL.GetUniformLocation(shader, "shadow_GLOB"), 5);



            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, ShadowTexture);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, ShadowTexture2);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, ShadowTexture3);

            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, ShadowTexture4);

            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, ShadowTexture_GLOB);


            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, 0);


            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, 0);


            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindVertexArray(0);
        }
    }
}
