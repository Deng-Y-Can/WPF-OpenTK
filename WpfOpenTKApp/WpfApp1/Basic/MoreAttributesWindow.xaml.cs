using OpenTK.GLControl;
using System.Windows;
using OpenTK.Graphics.OpenGL4;
using LearnOpenTK.Common;
using System.Diagnostics;

namespace WpfApp
{
    /// <summary>
    /// MoreAttributesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MoreAttributesWindow : Window
    {
        private GLControl optkGL;
        // We're assigning three different colors at the asscoiate vertex position:
        // blue for the top, green for the bottom left and red for the bottom right.
        private readonly float[] _vertices =
        {
             // positions        // colors
             0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // bottom left
             0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f    // top 
        };

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;
        public MoreAttributesWindow()
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

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Just like before, we create a pointer for the 3 position components of our vertices.
            // The only difference here is that we need to account for the 3 color values in the stride variable.
            // Therefore, the stride contains the size of 6 floats instead of 3.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // We create a new pointer for the color values.
            // Much like the previous pointer, we assign 6 in the stride value.
            // We also need to correctly set the offset to get the color values.
            // The color data starts after the position data, so the offset is the size of 3 floats.
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            // We then enable color attribute (location=1) so it is available to the shader.
            GL.EnableVertexAttribArray(1);

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            _shader = new Shader("Shaders/shader3.vert", "Shaders/shader3.frag");
            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            optkGL.SwapBuffers();
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



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 创建 GLControl.
            optkGL = new GLControl();

            // 指定GLControl的Load 和 Paint 事件
            optkGL.Load += new EventHandler(optk_Load);
            optkGL.Paint += new System.Windows.Forms.PaintEventHandler(optk_Paint);

            // 指定这个GLControl作为chart控件的child
            chart.Child = optkGL;
        }
    }
}
