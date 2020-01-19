using System;
using System.Collections.Generic;
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

//using Assimp;

namespace Planes3D
{
    public class MainWindow : GameWindow
    {
        private Shader _shader;
       
        private Dictionary<string, Model> models = new Dictionary<string, Model>();

        private Model teapot;

        private Matrix4 projection;
        private Matrix4 view;

        private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

        public MainWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        
        protected override void OnLoad(EventArgs e)
        {
            models["sun"] = ModelLoader.LoadFromFile("../../models/sphere.obj", 
                PostProcessSteps.Triangulate, 
                null);

            models["teapot"] = (ModelLoader.LoadFromFile("../../models/teapot.obj",
                PostProcessSteps.Triangulate,
                null));

            _shader = new Shader("../../shaders/shader.vert", "../../shaders/shader.frag");
            _shader.Use();

            models["teapot"].matrix = Matrix4.Identity;
            models["teapot"].texture = new Texture("../../textures/bricks.jpg");

            models["sun"].matrix = Matrix4.Identity* Matrix4.CreateScale(0.3f);
            models["sun"].texture = new Texture("../../textures/sun.jpg");

            view = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f),
                Width / Height,
                0.1f,
                100.0f);

            GL.Enable(EnableCap.DepthTest);

            base.OnLoad(e);
        }

        double _time = 0.0d;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _time += 10*e.Time;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach (var model in models)
            {
                var m = model.Value;
                m.texture.Use();
                _shader.Use();
                var mm = m.matrix;

                if(model.Key == "sun")
                {
                    mm *= Matrix4.CreateTranslation(_lightPos);
                }

                _shader.SetMatrix4("model", mm);
                _shader.SetMatrix4("view", view);
                _shader.SetMatrix4("projection", projection);
                foreach (var mesh in m.meshes)
                {
                    mesh.Render();
                }
            }
            
            SwapBuffers();

            base.OnRenderFrame(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = Keyboard.GetState();

            models["teapot"].matrix = models["teapot"].matrix* Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(1f)) * Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(0.23f)); ;

            models["sun"].matrix = models["sun"].matrix * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(-0.3f)) * Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(0.1f)); ;

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }


        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f),
               Width / (float) Height,
               0.1f,
               100.0f);
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