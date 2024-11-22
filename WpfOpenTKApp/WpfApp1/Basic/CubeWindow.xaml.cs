using OpenTK.GLControl;
using System.Windows;
using OpenTK.Graphics.OpenGL4;
using LearnOpenTK.Common;
using System.Diagnostics;
using OpenTK.Mathematics;
using System.Windows.Threading;
namespace WpfApp
{
    /// <summary>
    /// CubeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CubeWindow : Window
    {
        private GLControl optkGL;
        private DispatcherTimer timer;

        private readonly float[] _vertices=
        {
    -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
     0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
    -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

    -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
};

        

        private readonly float[] centerPosition = {
         0.0f,  0.0f,  0.0f,
         2.0f,  5.0f, -15.0f,
        -1.5f, -2.2f, -2.5f,
        -3.8f, -2.0f, -12.3f,
         2.4f, -0.4f, -3.5f,
        -1.7f,  3.0f, -7.5f,
         1.3f, -2.0f, -2.5f,
         1.5f,  2.0f, -2.5f,
         1.5f,  0.2f, -1.5f,
        -1.3f,  1.0f, -1.5f
        };

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

        private Texture _texture2;
        // We create a double to hold how long has passed since the program was opened.
        private double _time;

        // Then, we create two matrices to hold our view and projection. They're initialized at the bottom of OnLoad.
        // The view matrix is what you might consider the "camera". It represents the current viewport in the window.
        private Matrix4 _view;

        // This represents how the vertices will be projected. It's hard to explain through comments,
        // so check out the web version for a good demonstration of what this does.
        private Matrix4 _projection;
        public CubeWindow()
        {
            InitializeComponent();
        }

       
        private void grid_Loaded(object sender, RoutedEventArgs e)
        {
            // 创建 GLControl.
            optkGL = new GLControl();

            // 指定GLControl的Load 和 Paint 事件
            optkGL.Load += new EventHandler(optk_Load);
            optkGL.Paint += new System.Windows.Forms.PaintEventHandler(optk_Paint);

            // 指定这个GLControl作为chart控件的child
            chart.Child = optkGL;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 设置间隔时间为1秒
            timer.Tick += Timer_Tick; // 订阅Tick事件
        }
        void optk_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

            GL.ClearColor(0.5f, 0.2f, 0.5f, 1.0f);//背景颜色
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            

           
            _shader = new Shader("Shaders/shader8.vert", "Shaders/shader8.frag");
            _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile("Resources/rsb.jpg");
           
            _texture.Use(TextureUnit.Texture0);

           
            _texture2 = Texture.LoadFromFile("Resources/cww.jpg");
            
            _texture2.Use(TextureUnit.Texture1);

           
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);


            //纹理设置之后要重新使用
            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);

           
            _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);

            
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 1.2f, 0.1f, 100.0f);

            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

           
            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));

            
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);

            timer.Start();

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

        private void Timer_Tick(object sender, EventArgs e)
        {
            _time += 5; // 每次计时器触发，增加计数
            if (_shader != null)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.BindVertexArray(_vertexArrayObject);

                _texture.Use(TextureUnit.Texture0);
                _texture2.Use(TextureUnit.Texture1);
                _shader.Use();



                for (int i = 0; i < centerPosition.Length; i++)
                {
                    var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time)) * Matrix4.CreateTranslation(centerPosition[i], centerPosition[i + 1], centerPosition[i + 2]); ;
                    i += 2;
                    _shader.SetMatrix4("model", model);
                    _shader.SetMatrix4("view", _view);
                    _shader.SetMatrix4("projection", _projection);

                    GL.DrawArrays(PrimitiveType.Triangles,0, _vertices.Length);

                }

                optkGL.SwapBuffers();
            }

        }


       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _time += 10; // 每次计时器触发，增加计数
            if (_shader != null)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.BindVertexArray(_vertexArrayObject);

                _texture.Use(TextureUnit.Texture0);
                _texture2.Use(TextureUnit.Texture1);
                _shader.Use();

                for (int i = 0; i < centerPosition.Length; i++)
                {
                    var model = Matrix4.Identity * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_time)) * Matrix4.CreateTranslation(centerPosition[i], centerPosition[i + 1], centerPosition[i + 2]); ;
                    i += 2;
                    _shader.SetMatrix4("model", model);
                    _shader.SetMatrix4("view", _view);
                    _shader.SetMatrix4("projection", _projection);

                    GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length);

                }

                optkGL.SwapBuffers();
            }
        }
    }
}
