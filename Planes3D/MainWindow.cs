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

        private Camera _camera;
        ICamera Camera = new StationaryTrackingCamera();

        private bool _firstMove = true;
        private Vector2 _lastPos;

        private readonly Vector3 _lightPos = new Vector3(4f, 18f, -22f);
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

            _lightingShader = new Shader("../../shaders/shader.vert", "../../shaders/lighting.frag");
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

            _camera = new Camera(Vector3.UnitZ * 5, Width / (float) Height);

            GL.Enable(EnableCap.DepthTest);

            base.OnLoad(e);
            _timer.Interval = 1000;
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(
                $"{_camera.Position.X} {_camera.Position.Y} {_camera.Position.Z} {_camera.Yaw} {_camera.Pitch}");
        }

        Timer _timer = new Timer();
        float _time = 0.0f;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _time += (float) e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var shit = moveModule.Move(_time % 20 / 10);
            var fajny = (models[plane].matrix * shit.matrix);

            skybox.Draw(Camera.GetViewMatrix(fajny, shit.angle), Camera.GetProjectionMatrix(fajny, shit.angle),
                _time * 1000);

            var _front = new Vector3();
            float pitch = 50;
            _front.X = (float)Math.Cos(pitch) * (float)Math.Cos(shit.angle);
            _front.Y = (float)Math.Sin(pitch);
            _front.Z = (float)Math.Cos(pitch) * (float)Math.Sin(shit.angle);
            
            // Spot light
            _lightingShader.Use();
            _lightingShader.SetVector3("spotLight.position", fajny.ExtractTranslation());
            _lightingShader.SetVector3("spotLight.direction", _front);
            _lightingShader.SetVector3("spotLight.ambient", new Vector3(0.0f, 0.0f, 0.0f));
            _lightingShader.SetVector3("spotLight.diffuse", new Vector3(1.0f, 1.0f, 1.0f));
            _lightingShader.SetVector3("spotLight.specular", new Vector3(1.0f, 1.0f, 1.0f));
            _lightingShader.SetFloat("spotLight.constant", 1.0f);
            _lightingShader.SetFloat("spotLight.linear", 0.09f);
            _lightingShader.SetFloat("spotLight.quadratic", 0.032f);
            _lightingShader.SetFloat("spotLight.cutOff", (float) Math.Cos(MathHelper.DegreesToRadians(10.5f)));
            _lightingShader.SetFloat("spotLight.outerCutOff", (float) Math.Cos(MathHelper.DegreesToRadians(12.5f)));

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
                        _lampShader.SetMatrix4("view", Camera.GetViewMatrix(fajny, shit.angle));
                        _lampShader.SetMatrix4("projection", Camera.GetProjectionMatrix(fajny, shit.angle));
                        foreach (var mesh in m.meshes)
                        {
                            mesh.Render();
                        }

                        break;

                    case "map":
                        _lightingShader.Use();

                        _lightingShader.SetMatrix4("model", mm);
                        _lightingShader.SetMatrix4("view", Camera.GetViewMatrix(fajny, shit.angle));
                        _lightingShader.SetMatrix4("projection", Camera.GetProjectionMatrix(fajny, shit.angle));

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
                    case plane:
                        _lightingShader.Use();

                        mm = mm * moveModule.Move(_time % 20 / 10).matrix;

                        _lightingShader.SetMatrix4("model", mm);
                        _lightingShader.SetMatrix4("view", Camera.GetViewMatrix(fajny, shit.angle));
                        _lightingShader.SetMatrix4("projection", Camera.GetProjectionMatrix(fajny, shit.angle));

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
            ;

            const float cameraSpeed = 15f;
            const float sensitivity = 0.2f;
            float ratio = Width / (float) Height;
            if (input.IsKeyDown(Key.Number1))
                Camera = CameraFactory.Produce(CameraMode.StationaryObserving, ratio);
            if (input.IsKeyDown(Key.Number2))
                Camera = CameraFactory.Produce(CameraMode.StationaryTracking, ratio);
            if (input.IsKeyDown(Key.Number3))
                Camera = CameraFactory.Produce(CameraMode.Tracking, ratio);
            if (input.IsKeyDown(Key.W))
                _camera.Position += _camera.Front * cameraSpeed * (float) e.Time; // Forward 
            if (input.IsKeyDown(Key.S))
                _camera.Position -= _camera.Front * cameraSpeed * (float) e.Time; // Backwards
            if (input.IsKeyDown(Key.A))
                _camera.Position -= _camera.Right * cameraSpeed * (float) e.Time; // Left
            if (input.IsKeyDown(Key.D))
                _camera.Position += _camera.Right * cameraSpeed * (float) e.Time; // Right
            if (input.IsKeyDown(Key.Space))
                _camera.Position += _camera.Up * cameraSpeed * (float) e.Time; // Up 
            if (input.IsKeyDown(Key.LShift))
                _camera.Position -= _camera.Up * cameraSpeed * (float) e.Time; // Down

            var mouse = Mouse.GetState();

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }


        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            _camera.AspectRatio = Width / (float) Height;
            Camera.SetRatio(Width / (float) Height);
            base.OnResize(e);
        }


        protected override void OnUnload(EventArgs e)
        {
            // GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // GL.BindVertexArray(0);
            // GL.UseProgram(0);
            //
            // GL.DeleteBuffer(_vertexBufferObject);
            // GL.DeleteVertexArray(_vertexArrayObject);
            //
            // GL.DeleteProgram(_shader.Handle);
            // // Don't forget to dispose of the texture too!
            // GL.DeleteTexture(_texture.Handle);
            // base.OnUnload(e);
        }
    }
}