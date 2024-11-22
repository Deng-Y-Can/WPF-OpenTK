using OpenTK.Windowing.Desktop;
using System.Windows.Media.Media3D;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = System.Windows.Window;
using OpenTK.GLControl;
using LearnOpenTK.Common;
using System.Windows;
using OpenTK.Graphics.OpenGL4;

namespace WpfApp
{
    /// <summary>
    /// GeometryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GeometryWindow : Window
    {
        public GeometryWindow()
        {
            InitializeComponent();
        }
        private GLControl openTK;
        
        private readonly float[] _vertices =
      {
             -0.5f,  0.5f, 0.0f,1.0f,0f,0f, // top right
             0.5f, 0.5f, 0.0f,0f,1f,0f, // bottom right
             0.5f, -0.5f, 0.0f,0f,0f,1f ,// bottom left
             -0.5f,  -0.5f, 0.0f,1f,1f,0f // top left
        };


        private readonly uint[] _indices =
        {
            // Note that indices start at 0!
            0,1,2,3
        };


        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        // Add a handle for the EBO
        private int _elementBufferObject;
        void glc_Load(object sender, EventArgs e)
        {
            // 启用深度测试
            GL.Enable(EnableCap.DepthTest);

            // 设置深度测试函数
            // GL.DepthFunc(DepthFunction.Less);

            // 启用背面填充
            //GL.Enable(EnableCap.CullFace);

            // 设置视口大小
            GL.Viewport(0, 0, 400, 300);

            // GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f); // 设置清除颜色为灰色

        }
        void glc_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);// 设置清除颜色为灰色
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _shader = new Shader(vertShader, geometryShader2, fragShader, 0);
            _shader.Use();
            var texCoordLocation = _shader.GetAttribLocation("aColor");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);




            GL.DrawElements(PrimitiveType.Points, _indices.Length, DrawElementsType.UnsignedInt, 0);
            openTK.SwapBuffers();

        }      

        public readonly static string geometryShader = $@"
      #version 330 core
layout (points) in;
layout (line_strip, max_vertices = 4) out;

void main() {{    
    gl_Position = gl_in[0].gl_Position + vec4(-0.1, 0.0, 0.0, 0.0); 
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4( 0.1, 0.0, 0.0, 0.0);
    EmitVertex();
    gl_Position = gl_in[0].gl_Position + vec4(0, 0.1, 0.0, 0.0); 
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4(-0.1, 0.0, 0.0, 0.0); 
    EmitVertex();

    EndPrimitive();
}}
         ";


        public readonly static string geometryShader2 = $@"
      #version 330 core
layout (points) in;
layout (triangle_strip, max_vertices = 5) out;
in VS_OUT {{
    vec3 color;
}} gs_in[];
out vec3 fColor;
void build_house(vec4 position)
{{    
    fColor = gs_in[0].color; 
    gl_Position = position + vec4(-0.1, -0.1, 0.0, 0.0);   
    EmitVertex();   
    gl_Position = position + vec4( 0.1, -0.1, 0.0, 0.0);  
    EmitVertex();
    gl_Position = position + vec4(-0.1,  0.1, 0.0, 0.0);   
    EmitVertex();
    gl_Position = position + vec4( 0.1,  0.1, 0.0, 0.0); 
    EmitVertex();
    gl_Position = position + vec4( 0.0,  0.2, 0.0, 0.0);   
    fColor = vec3(1.0, 1.0, 1.0);
    EmitVertex();
    EndPrimitive();
}}

void main() {{    
    build_house(gl_in[0].gl_Position);
}}
";


        public readonly static string vertShader = $@"
        #version 330 core
layout(location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;

out VS_OUT {{
    vec3 color;
}} vs_out;
void main(void)
{{
    gl_Position = vec4(aPosition, 1.0);
    vs_out.color = aColor;
}}

         ";

        public readonly static string fragShader = $@"
#version 330

out vec4 outputColor;
in vec3 fColor;
void main()
{{
    outputColor = vec4(fColor, 1.0);
}}       

";

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _shader.Use();

            GL.DrawElements(PrimitiveType.Points, _indices.Length, DrawElementsType.UnsignedInt, 0);
            openTK.SwapBuffers();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

            // 创建 GLControl.
            openTK = new GLControl();

            // 指定GLControl的Load 和 Paint 事件
            openTK.Load += new EventHandler(glc_Load);
            openTK.Paint += new PaintEventHandler(glc_Paint);


            // 指定这个GLControl作为chart控件的child
            chart.Child = openTK;
        }
    }
}
