using OpenTK.GLControl;
using System.Windows;
using OpenTK.Graphics.OpenGL4;
using LearnOpenTK.Common;
using System.Diagnostics;
using OpenTK.Mathematics;

namespace WpfApp
{
    /// <summary>
    /// TransformationsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TransformationsWindow : Window
    {
        private GLControl optkGL;
        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

        private Texture _texture2;
        public TransformationsWindow()
        {
            InitializeComponent();
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

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // shader.frag has been modified yet again, take a look at it as well.
            _shader = new Shader("Shaders/shader7.vert", "Shaders/shader7.frag");
            _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile("Resources/rsb.jpg");
            // Texture units are explained in Texture.cs, at the Use function.
            // First texture goes in texture unit 0.
            _texture.Use(TextureUnit.Texture0);

            // This is helpful because System.Drawing reads the pixels differently than OpenGL expects.
            _texture2 = Texture.LoadFromFile("Resources/cww.jpg");
            // Then, the second goes in texture unit 1.
            _texture2.Use(TextureUnit.Texture1);

            // Next, we must setup the samplers in the shaders to use the right textures.
            // The int we send to the uniform indicates which texture unit the sampler should use.
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);


            //纹理设置之后要重新使用
            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);

            // Note: The matrices we'll use for transformations are all 4x4.

            // We start with an identity matrix. This is just a simple matrix that doesn't move the vertices at all.
            var transform = Matrix4.Identity;

            // The next few steps just show how to use OpenTK's matrix functions, and aren't necessary for the transform matrix to actually work.
            // If you want, you can just pass the identity matrix to the shader, though it won't affect the vertices at all.

            // A fact to note about matrices is that the order of multiplications matter. "matrixA * matrixB" and "matrixB * matrixA" mean different things.
            // A VERY important thing to know is that OpenTK matrices are so called row-major. We won't go into the full details here, but here is a good place to read more about it:
            // https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/geometry/row-major-vs-column-major-vector
            // What it means for us is that we can think of matrix multiplication as going left to right.
            // So "rotate * translate" means rotate (around the origin) first and then translate, as opposed to "translate * rotate" which means translate and then rotate (around the origin).

            // To combine two matrices, you multiply them. Here, we combine the transform matrix with another one created by OpenTK to rotate it by 20 degrees.
            // Note that all Matrix4.CreateRotation functions take radians, not degrees. Use MathHelper.DegreesToRadians() to convert to radians, if you want to use degrees.
            transform = transform * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(25f));

            // Next, we scale the matrix. This will make the rectangle slightly larger.
            transform = transform * Matrix4.CreateScale(1.1f);

            // Then, we translate the matrix, which will move it slightly towards the top-right.
            // Note that we aren't using a full coordinate system yet, so the translation is in normalized device coordinates.
            // The next tutorial will be about how to set one up so we can use more human-readable numbers.
            transform = transform * Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);

            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            // Now that the matrix is finished, pass it to the vertex shader.
            // Go over to shader.vert to see how we finally apply this to the vertices.
            _shader.SetMatrix4("transform", transform);


            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
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
