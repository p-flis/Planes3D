using Assimp;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Planes3D
{
    class Skybox
    {
        SkyboxTexture day;
        SkyboxTexture night;
        Shader shader;
        Model cube;

        public Skybox(string vertShader, string fragShader, string model, List<string> facesDay, List<string> facesNight)
        {
            cube = ModelLoader.LoadFromFile(model,
                PostProcessSteps.Triangulate,
                null);
            shader = new Shader(vertShader, fragShader);
            day = new SkyboxTexture(facesDay);
            night = new SkyboxTexture(facesNight);
        }

        public void Draw(Matrix4 view, Matrix4 projection, double time, Vector3 fogColour, float upperLimit)
        {
            uint texture1 = night.handle;
            uint texture2 = day.handle;
            double blendFactor;
            time %= 24000;
            if (time >= 0 && time < 5000)
            {
                texture1 = night.handle;
                texture2 = night.handle;
                blendFactor = (time - 0) / (5000 - 0);
            }
            else if (time >= 5000 && time < 8000)
            {
                texture1 = night.handle;
                texture2 = day.handle;
                blendFactor = (time - 5000) / (8000 - 5000);
            }
            else if (time >= 8000 && time < 21000)
            {
                texture1 = day.handle;
                texture2 = day.handle;
                blendFactor = (time - 8000) / (21000 - 8000);
            }
            else
            {
                texture1 = day.handle;
                texture2 = night.handle;
                blendFactor = (time - 21000) / (24000 - 21000);
            }


            GL.DepthMask(false);
            shader.Use();
            shader.SetVector3("fogColour", fogColour);
            shader.SetFloat("upperLimit", upperLimit);
            shader.SetInt("skybox", 0);
            shader.SetInt("skybox2", 1);
            shader.SetMatrix4("view", new Matrix4(new Matrix3(view*Matrix4.CreateRotationY((float)time/100000))));
            shader.SetMatrix4("projection", projection);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture2);
            shader.SetFloat("blendFactor", (float)blendFactor);
            foreach (var m in cube.meshes)
            {
                m.Render();
            }
            GL.DepthMask(true);
        }
    }
}
