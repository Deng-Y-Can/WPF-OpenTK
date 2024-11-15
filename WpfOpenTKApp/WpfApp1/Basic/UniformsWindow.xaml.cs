using OpenTK.GLControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.GLControl;
using System.Windows;
using OpenTK.Graphics.OpenGL4;
using LearnOpenTK.Common;
using System.Diagnostics;
using System.Windows.Threading;

namespace WpfApp
{
    /// <summary>
    /// UniformsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UniformsWindow : Window
    {
        private DispatcherTimer timer;
        private double count;
        private GLControl optkGL;

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };

        // So we're going make the triangle pulsate between a color range.
        // In order to do that, we'll need a constantly changing value.
        // The stopwatch is perfect for this as it is constantly going up.
        private Stopwatch _timer;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;
        public UniformsWindow()
        {
            InitializeComponent();
            
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            count+=0.1 ; // 每次计时器触发，增加计数
            if (_shader!=null){
                float greenValue = (float)Math.Sin(count)/2f+0.5f;

                // This gets the uniform variable location from the frag shader so that we can 
                // assign the new green value to it.
                int vertexColorLocation = GL.GetUniformLocation(_shader.Handle, "ourColor");

                // Here we're assigning the ourColor variable in the frag shader 
                // via the OpenGL Uniform method which takes in the value as the individual vec values (which total 4 in this instance).
                GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);

                // You can alternatively use this overload of the same function.
                // GL.Uniform4(vertexColorLocation, new OpenTK.Mathematics.Color4(0f, greenValue, 0f, 0f));

                // Bind the VAO
                GL.BindVertexArray(_vertexArrayObject);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

                optkGL.SwapBuffers();
            }

        }

        void optk_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

            GL.ClearColor(0.5f, 0.2f, 0.5f, 1.0f);//背景颜色
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            _shader = new Shader("Shaders/shader4.vert", "Shaders/shader4.frag");
            _shader.Use();

            timer.Start(); // 启动计时器

           
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



        private void Window_Loaded_1(object sender, RoutedEventArgs e)
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
    }
}
