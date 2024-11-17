using OpenTK.GLControl;
using System.Windows;
using OpenTK.Graphics.OpenGL4;
using LearnOpenTK.Common;
using System.Diagnostics;
using OpenTK.Mathematics;
using System.Windows.Threading;
using System.Windows.Input;
using Microsoft.VisualBasic.Devices;

namespace WpfApp
{
    /// <summary>
    /// ColorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ColorWindow : Window
    {
        private GLControl optkGL;
        private DispatcherTimer timer;

        private readonly float[] _vertices =
        {
            // Position
            -0.5f, -0.5f, -0.5f, // Front face
             0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f, -0.5f,  0.5f, // Back face
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,

            -0.5f,  0.5f,  0.5f, // Left face
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,

             0.5f,  0.5f,  0.5f, // Right face
             0.5f,  0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,

            -0.5f, -0.5f, -0.5f, // Bottom face
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f,  0.5f, -0.5f, // Top face
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f
        };

        // This is the position of both the light and the place the lamp cube will be drawn in the scene
        private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

        private int _vertexBufferObject;

        // I renamed the vertex array object since we now want two VAO's one for the model (the big cube for testing light shaders),
        // and one for the lamp so we can see where the light source comes from.
        // In an actual application you would probably either not draw the lamp at all or draw it with a model of a lamp of some sort.
        private int _vaoModel;

        private int _vaoLamp;

        // We also need two shaders, one for the lamp and one for our lighting material.
        // The lighting shader is where most of this chapter will take place as this is where a lot of the lighting "magic" happens.
        private Shader _lampShader;

        private Shader _lightingShader;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        public ColorWindow()
        {
            InitializeComponent();
        }

        void optk_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

            GL.ClearColor(0.5f, 0.2f, 0.5f, 1.0f);//背景颜色
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Load the two different shaders, they use the same vertex shader program. However they have two different fragment shaders.
            // This is because the lamp only uses a basic shader to turn it white, it wouldn't make sense to have the lamp lit in other colors.
            // The lighting shaders uses the lighting.frag shader which is what a large part of this chapter will be about
            _lightingShader = new Shader("Shaders/shader10.vert", "Shaders/lighting10.frag");
            _lampShader = new Shader("Shaders/shader10.vert", "Shaders/shader10.frag");

            {
                // Initialize the vao for the model
                _vaoModel = GL.GenVertexArray();
                GL.BindVertexArray(_vaoModel);

                var vertexLocation = _lightingShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(vertexLocation);
                GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            }

            {
                // Initialize the vao for the lamp, this is mostly the same as the code for the model cube
                _vaoLamp = GL.GenVertexArray();
                GL.BindVertexArray(_vaoLamp);

                // Set the vertex attributes (only position data for our lamp)
                var vertexLocation = _lampShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(vertexLocation);
                GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            }

            _camera = new Camera(Vector3.UnitZ * 3,1f);
            Render();
            //timer.Start();

        }
        void optk_Load(object sender, EventArgs e)
        {
            // 启用深度测试
            GL.Enable(EnableCap.DepthTest);

            //// 设置深度测试函数
            //GL.DepthFunc(DepthFunction.Less);

            //// 启用背面填充
            //GL.Enable(EnableCap.CullFace);

            // 设置视口大小
            GL.Viewport(0, 0, 400, 300);

            // 清除颜色缓冲和深度缓冲
            // GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f); // 设置清除颜色为灰色
            // GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private float _time = 0;
        private void Timer_Tick(object sender, EventArgs e)
        {
            _time += 5; // 每次计时器触发，增加计数
            if (_lightingShader != null&& _lampShader!=null)
            {
                
            }

        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.optkGL.Focus();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 创建 GLControl.
            optkGL = new GLControl();

            // 指定GLControl的Load 和 Paint 事件
            optkGL.Load += new EventHandler(optk_Load);
            optkGL.Paint += new System.Windows.Forms.PaintEventHandler(optk_Paint);
            optkGL.KeyDown += new System.Windows.Forms.KeyEventHandler(Chart_KeyDown);
            optkGL.MouseMove += new System.Windows.Forms.MouseEventHandler(Chart_MouseMove);
            optkGL.MouseWheel += new System.Windows.Forms.MouseEventHandler(Chart_MouseWheel);
            
            // 指定这个GLControl作为chart控件的child
            chart.Child = optkGL;
            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(1); // 设置间隔时间为1秒
            //timer.Tick += Timer_Tick; // 订阅Tick事件
        }

        private void Chart_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_lightingShader != null && _lampShader != null)
            {
                float mouseX = (float)e.X;
                float mouseY = (float)e.Y;
                if (_firstMove) // This bool variable is initially set to true.
                {
                    _lastPos = new Vector2(mouseX, mouseY);
                    _firstMove = false;
                }
                else
                {
                    // Calculate the offset of the mouse position
                    var deltaX = mouseX - _lastPos.X;
                    var deltaY = mouseY - _lastPos.Y;
                    _lastPos = new Vector2(mouseX, mouseY);

                    _camera.Position -= _camera.Right * deltaX * mousechange; // RIGHT
                    _camera.Position += _camera.Up * deltaY * mousechange; // RIGHT

                    Render();
                }
            }
               
        }

        const float cameraSpeed = 0.15f;
        const float sensitivity = 0.05f;  //右键平移因子
        float change = 3.0f;
        float mousechange = 0.045f;//左键旋转因子
        private void Chart_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //if (!IsFocused) // Check to see if the window is focused
            //{
            //    return;
            //}
            change += 0.01f;

            Key keyPressed = (Key)e.KeyCode;
            switch (e.KeyCode.ToString())
            {
                //case Key.Escape:
                //    Close();
                //    break;
                case "Q":
                    _camera.Position += _camera.Front * cameraSpeed * change; // enlarge
                    break;
                case "E":
                    _camera.Position -= _camera.Front * cameraSpeed * change; // narrow
                    break;
                case "D":
                    _camera.Position -= _camera.Right * cameraSpeed * change; // RIGHT
                    break;
                case "A":
                    _camera.Position += _camera.Right * cameraSpeed * change; // LEFT
                    break;
                case "S":
                    _camera.Position += _camera.Up * cameraSpeed * change; // TOP
                    break;
                case "W":
                    _camera.Position -= _camera.Up * cameraSpeed * change; // BUTTOM
                    break;
                default:

                    break;
            }
            Console.WriteLine($@"执行" + e.KeyCode.ToString());
            Render();
        }
        private void Chart_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_camera != null)
            {
                //滚轮事件
                _camera.Fov -= e.Delta / 100;
                Render();
            }

        }

        public void Render()
        {
            if (_lightingShader != null && _lampShader != null)
            {

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                
                // Draw the model/cube with the lighting shader
                GL.BindVertexArray(_vaoModel);

                _lightingShader.Use();

                // Matrix4.Identity is used as the matrix, since we just want to draw it at 0, 0, 0
                _lightingShader.SetMatrix4("model", Matrix4.Identity);
                _lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
                _lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                _lightingShader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
                _lightingShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

                // Draw the lamp, this is mostly the same as for the model cube
                GL.BindVertexArray(_vaoLamp);

                _lampShader.Use();

                Matrix4 lampMatrix = Matrix4.CreateScale(0.2f); // We scale the lamp cube down a bit to make it less dominant
                lampMatrix = lampMatrix * Matrix4.CreateTranslation(_lightPos);

                _lampShader.SetMatrix4("model", lampMatrix);
                _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
                _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

                optkGL.SwapBuffers();
            }

        }
    }
}
