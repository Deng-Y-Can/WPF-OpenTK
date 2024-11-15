using OpenTK.GLControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.GLControl;


namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainGLWindow : Window
    {
        private GLControl optkGL;
        
        public MainGLWindow()
        {
            InitializeComponent();
        }
 

        void optk_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // 画一个小的黄色三角形
            GL.Color3(System.Drawing.Color.Yellow);
            GL.Begin(BeginMode.Triangles);
            GL.Vertex2(200, 50);
            GL.Vertex2(200, 200);
            GL.Vertex2(100, 50);
            GL.End();

            optkGL.SwapBuffers();
        }
        void optk_Load(object sender, EventArgs e)
        {
            // 设置背景色
            GL.ClearColor(System.Drawing.Color.Chocolate);

            int w = optkGL.Width;
            int h = optkGL.Height;

            // 设置初始状态
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, w, 0, h, -1, 1);
            GL.Viewport(0, 0, w, h);
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
        }
    }
}