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
    /// BasicLightWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BasicLightWindow : Window
    {
        private GLControl optkGL;
        private DispatcherTimer timer;

        private readonly float[] _vertices =
         {
             // Position          Normal
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, // Front face
             0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, // Back face
             0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,

            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f, // Left face
            -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f, // Right face
             0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, // Bottom face
             0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, // Top face
             0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
        };

        private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

        private int _vertexBufferObject;

        private int _vaoModel;

        private int _vaoLamp;

        private Shader _lampShader;

        private Shader _lightingShader;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;
        public BasicLightWindow()
        {
            InitializeComponent();
        }
        void optk_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _lightingShader = new Shader("Shaders/shader11.vert", "Shaders/lighting11.frag");
            _lampShader = new Shader("Shaders/shader11.vert", "Shaders/shader11.frag");

            {
                _vaoModel = GL.GenVertexArray();
                GL.BindVertexArray(_vaoModel);

                var positionLocation = _lightingShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                // Remember to change the stride as we now have 6 floats per vertex
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

                // We now need to define the layout of the normal so the shader can use it
                var normalLocation = _lightingShader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            }

            {
                _vaoLamp = GL.GenVertexArray();
                GL.BindVertexArray(_vaoLamp);

                var positionLocation = _lampShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                // Also change the stride here as we now have 6 floats per vertex. Now we don't define the normal for the lamp VAO
                // this is because it isn't used, it might seem like a waste to use the same VBO if they dont have the same data
                // The two cubes still use the same position, and since the position is already in the graphics memory it is actually
                // better to do it this way. Look through the web version for a much better understanding of this.
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            }


            _camera = new Camera(Vector3.UnitZ * 3, 1f);
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
            if (_lightingShader != null && _lampShader != null)
            {
            }

        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.optkGL.Focus();

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

                GL.BindVertexArray(_vaoModel);

                _lightingShader.Use();

                _lightingShader.SetMatrix4("model", Matrix4.Identity);
                _lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
                _lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                _lightingShader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
                _lightingShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
                _lightingShader.SetVector3("lightPos", _lightPos);
                _lightingShader.SetVector3("viewPos", _camera.Position);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

                GL.BindVertexArray(_vaoLamp);

                _lampShader.Use();

                Matrix4 lampMatrix = Matrix4.CreateScale(0.2f);
                lampMatrix = lampMatrix * Matrix4.CreateTranslation(_lightPos);

                _lampShader.SetMatrix4("model", lampMatrix);
                _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
                _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

                optkGL.SwapBuffers();
            }

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
    }
}
