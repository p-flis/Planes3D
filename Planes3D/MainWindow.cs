using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Assimp.Configs;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;
using System.Windows.Forms;
using Planes3D.Move;
using Planes3D.Cameras;

namespace Planes3D
{
    public class MainWindow : GameWindow
    {
        private const string plane = "plane";
        private Shader _lightingShader;
        private Shader _lampShader;

        private Dictionary<string, Model> models = new Dictionary<string, Model>();
        private Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

        ICamera Camera = new StationaryTrackingCamera();

        private Vector2 _spotLight = new Vector2(0f, 0f);
        private Vector2 _fog = new Vector2(0.007f, 1.5f);
        private Vector3 _fogColour = new Vector3(0.1f, 0.5f, 0.4f);
        private float _upperLimit = 0.6f;
        
        private Vector3 _lightPos = new Vector3(4f, 20f, -22f);
        private Vector3 _lightColor = new Vector3(1f, 1f, 1f);
        IMoveModule moveModule = new EightMove();

        public MainWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
        }

        List<string> facesNight = new List<string>()
        {
            "../../textures/skybox/night/right.png",
            "../../textures/skybox/night/left.png",
            "../../textures/skybox/night/top.png",
            "../../textures/skybox/night/bottom.png",
            "../../textures/skybox/night/front.png",
            "../../textures/skybox/night/back.png",
        };

        List<string> facesDay = new List<string>()
        {
            "../../textures/skybox/day/right.bmp",
            "../../textures/skybox/day/left.bmp",
            "../../textures/skybox/day/top.bmp",
            "../../textures/skybox/day/bottom.bmp",
            "../../textures/skybox/day/front.bmp",
            "../../textures/skybox/day/back.bmp",
        };

        Skybox skybox;

        protected override void OnLoad(EventArgs e)
        {
            models["map"] = ModelLoader.LoadFromFile("../../models/mpmap2.x",
                PostProcessSteps.Triangulate,
                null);
            models["sun"] = ModelLoader.LoadFromFile("../../models/sphere.obj",
                PostProcessSteps.Triangulate,
                null);

            models[plane] = (ModelLoader.LoadFromFile("../../models/VLJ19.blend",
                PostProcessSteps.Triangulate,
                null));

            skybox = new Skybox("../../shaders/skybox/shader.vert", "../../shaders/skybox/shader.frag",
                "../../models/cube.obj",
                facesDay, facesNight);

            _lampShader = new Shader("../../shaders/shader.vert", "../../shaders/shader.frag");
            _lampShader.Use();

            shaders["blinn"] = new Shader("../../shaders/blinn/shader.vert", "../../shaders/blinn/lighting.frag");
            shaders["gouraud"] = new Shader("../../shaders/gouraud/shader.vert", "../../shaders/gouraud/lighting.frag");
            shaders["phong"] = new Shader("../../shaders/phong/shader.vert", "../../shaders/phong/lighting.frag");

            _lightingShader = shaders["phong"];
            _lightingShader.Use();

            models["map"].texture = new Texture("../../textures/mpmap2.jpg");
            models["map"].matrix = Matrix4.Identity * Matrix4.CreateScale(0.1f) * Matrix4.CreateTranslation(5, -10, 3);
            models[plane].texture = new Texture("../../textures/a.jpg");
            models[plane].matrix = Matrix4.Identity *
                                   Matrix4.CreateScale(0.2f) *
                                   Matrix4.CreateRotationZ((float) Math.PI / 2) *
                                   Matrix4.CreateRotationX(-(float) Math.PI / 2) * Matrix4.CreateTranslation(0, -1f, 0);

            models["sun"].matrix = Matrix4.Identity * Matrix4.CreateScale(0.05f);
            models["sun"].texture = new Texture("../../textures/sun.jpg");


            GL.Enable(EnableCap.DepthTest);

            base.OnLoad(e);
            _timer.Interval = 1000;
            _timer.Start();
        }

        Timer _timer = new Timer();
        float _time = 0.0f;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _time += (float) e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var deltaPlane = moveModule.Move(_time % 20 / 10);
            var planeModel = (models[plane].matrix * deltaPlane.matrix);

            SetSpotLight(_lightingShader, 50 + _spotLight.X, deltaPlane.angle + _spotLight.Y,
                planeModel.ExtractTranslation());
            SetFog(_lightingShader, _fogColour, _fog.X, _fog.Y);
            
            skybox.Draw(Camera.GetViewMatrix(planeModel, deltaPlane.angle),
                Camera.GetProjectionMatrix(planeModel, deltaPlane.angle),
                _time * 1000, _fogColour, _upperLimit);

            foreach (var model in models)
            {
                var m = model.Value;
                m.texture.Use();
                var mm = m.matrix;

                switch (model.Key)
                {
                    case "sun":
                        _lampShader.Use();

                        _lampShader.SetMatrix4("model", mm * Matrix4.CreateTranslation(_lightPos));
                        _lampShader.SetMatrix4("view", Camera.GetViewMatrix(planeModel, deltaPlane.angle));
                        _lampShader.SetMatrix4("projection", Camera.GetProjectionMatrix(planeModel, deltaPlane.angle));
                        foreach (var mesh in m.meshes)
                        {
                            mesh.Render();
                        }

                        break;

                    case "map":
                        _lightingShader.Use();

                        _lightingShader.SetMatrix4("model", mm);
                        _lightingShader.SetMatrix4("view", Camera.GetViewMatrix(planeModel, deltaPlane.angle));
                        _lightingShader.SetMatrix4("projection",
                            Camera.GetProjectionMatrix(planeModel, deltaPlane.angle));

                        _lightingShader.SetVector3("light.position", _lightPos);
                        _lightingShader.SetVector3("light.ambient", _lightColor * new Vector3(1f));
                        _lightingShader.SetVector3("light.diffuse", _lightColor * new Vector3(1.5f));
                        _lightingShader.SetVector3("light.specular", _lightColor * new Vector3(1f));

                        foreach (var mesh in m.meshes)
                        {
                            _lightingShader.SetVector3("material.ambient", new Vector3(0.1f));
                            _lightingShader.SetVector3("material.diffuse", mesh.Material.getDiffuseColour());
                            _lightingShader.SetVector3("material.specular", new Vector3(0.2f));
                            _lightingShader.SetFloat("material.shininess", mesh.Material.getReflectance());
                            mesh.Render();
                        }

                        break;
                    case plane:
                        _lightingShader.Use();

                        mm = mm * moveModule.Move(_time % 20 / 10).matrix;

                        _lightingShader.SetMatrix4("model", mm);
                        _lightingShader.SetMatrix4("view", Camera.GetViewMatrix(planeModel, deltaPlane.angle));
                        _lightingShader.SetMatrix4("projection",
                            Camera.GetProjectionMatrix(planeModel, deltaPlane.angle));

                        _lightingShader.SetVector3("light.position", _lightPos);
                        _lightingShader.SetVector3("light.ambient", _lightColor * new Vector3(1f));
                        _lightingShader.SetVector3("light.diffuse", _lightColor * new Vector3(1f));
                        _lightingShader.SetVector3("light.specular", _lightColor * new Vector3(1f));

                        foreach (var mesh in m.meshes)
                        {
                            _lightingShader.SetVector3("material.ambient", new Vector3(0.1f));
                            _lightingShader.SetVector3("material.diffuse", mesh.Material.getDiffuseColour());
                            _lightingShader.SetVector3("material.specular", mesh.Material.getSpecularColour());
                            _lightingShader.SetFloat("material.shininess", mesh.Material.getReflectance());
                            mesh.Render();
                        }

                        break;

                    default:
                        break;
                }
            }

            SwapBuffers();

            base.OnRenderFrame(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused)
            {
                return;
            }

            var input = Keyboard.GetState();
            models["sun"].matrix = models["sun"].matrix *
                                   Matrix4.CreateRotationY((float) MathHelper.DegreesToRadians(-0.3f)) *
                                   Matrix4.CreateRotationZ((float) MathHelper.DegreesToRadians(0.1f));

            const float lightSpeed = 0.1f;
            const float spotLightSpeed = 0.02f;
            const float fogDensitySpeed = 0.001f;
            const float fogGradientSpeed = 0.01f;
            const float upperLimitSpeed = 0.01f;

            var ratio = Width / (float) Height;
            if (input.IsKeyDown(Key.Minus))
            {
                _upperLimit -= upperLimitSpeed;
                if (_upperLimit < 0) _upperLimit = 0;
            }
            if (input.IsKeyDown(Key.Plus))
            {
                _upperLimit += upperLimitSpeed;
            }
            if (input.IsKeyDown(Key.Number0))
            {
                _fog.X += fogDensitySpeed;
            }

            if (input.IsKeyDown(Key.Number9))
            {
                _fog.X -= fogDensitySpeed;
                if (_fog.X < 0) _fog.X = 0;
            }

            if (input.IsKeyDown(Key.Number8))
            {
                _fog.Y += fogGradientSpeed;
            }

            if (input.IsKeyDown(Key.Number7))
            {
                _fog.Y -= fogGradientSpeed;
                if (_fog.Y < 0) _fog.Y = 0;
            }

            if (input.IsKeyDown(Key.Up))
                _spotLight.X += spotLightSpeed;
            if (input.IsKeyDown(Key.Down))
                _spotLight.X -= spotLightSpeed;
            if (input.IsKeyDown(Key.Right))
                _spotLight.Y += spotLightSpeed;
            if (input.IsKeyDown(Key.Left))
                _spotLight.Y -= spotLightSpeed;
            if (input.IsKeyDown(Key.Number1))
                Camera = CameraFactory.Produce(CameraMode.StationaryObserving, ratio);
            if (input.IsKeyDown(Key.Number2))
                Camera = CameraFactory.Produce(CameraMode.StationaryTracking, ratio);
            if (input.IsKeyDown(Key.Number3))
                Camera = CameraFactory.Produce(CameraMode.Tracking, ratio);
            if (input.IsKeyDown(Key.Number4))
                _lightingShader = shaders["phong"];
            if (input.IsKeyDown(Key.Number5))
                _lightingShader = shaders["gouraud"];
            if (input.IsKeyDown(Key.Number6))
                _lightingShader = shaders["blinn"];
            if (input.IsKeyDown(Key.W))
                _lightPos.Y += lightSpeed;
            if (input.IsKeyDown(Key.S))
                _lightPos.Y -= lightSpeed;

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }


        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            Camera.SetRatio(Width / (float) Height);
            base.OnResize(e);
        }
        
        private static void SetSpotLight(Shader shader, float pitch, float yaw, Vector3 translation)
        {
            var front = new Vector3
            {
                X = (float) Math.Cos(pitch) * (float) Math.Cos(yaw),
                Y = (float) Math.Sin(pitch),
                Z = (float) Math.Cos(pitch) * (float) Math.Sin(yaw)
            };
            shader.Use();
            shader.SetVector3("spotLight.position", translation);
            shader.SetVector3("spotLight.direction", front);
            shader.SetVector3("spotLight.ambient", new Vector3(0.0f, 0.0f, 0.0f));
            shader.SetVector3("spotLight.diffuse", new Vector3(5.0f, 5.0f, 5.0f));
            shader.SetVector3("spotLight.specular", new Vector3(1.0f, 1.0f, 1.0f));
            shader.SetFloat("spotLight.constant", 1.0f);
            shader.SetFloat("spotLight.linear", 0.09f);
            shader.SetFloat("spotLight.quadratic", 0.032f);
            shader.SetFloat("spotLight.cutOff", (float) Math.Cos(MathHelper.DegreesToRadians(20.5f)));
            shader.SetFloat("spotLight.outerCutOff", (float) Math.Cos(MathHelper.DegreesToRadians(25.5f)));
        }

        private static void SetFog(Shader shader, Vector3 skyColour, float density, float gradient)
        {
            shader.SetVector3("skyColour", skyColour);
            shader.SetFloat("fogDensity", density);
            shader.SetFloat("fogGradient", gradient);
        }
    }
}