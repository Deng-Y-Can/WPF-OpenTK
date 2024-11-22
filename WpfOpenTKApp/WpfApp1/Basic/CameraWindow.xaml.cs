using OpenTK.GLControl;
using System.Windows;
using OpenTK.Graphics.OpenGL4;
using LearnOpenTK.Common;
using OpenTK.Mathematics;

namespace WpfApp
{
    /// <summary>
    /// CameraWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CameraWindow : Window
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
       
        private float _time;


        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

      
        public CameraWindow()
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

            _shader = new Shader(vertMainShader, fragMainShader,0);
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

            _camera = new Camera(Vector3.UnitZ * 3, 1.2f);

           
            _shader.Use();

            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(0));
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

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

            optkGL.MouseWheel += new System.Windows.Forms.MouseEventHandler(MousewEventHandler);
            optkGL.KeyUp += new System.Windows.Forms.KeyEventHandler(KeywEventHandler);
            optkGL.MouseMove += new System.Windows.Forms.MouseEventHandler(MousemEventHandler);
            chart.Child = optkGL;
            this.optkGL.Focus();
        }

        private void Render()
        {
            GL.ClearColor(0.5f, 0.2f, 0.5f, 1.0f);//背景颜色
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);

           
            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(0));
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            optkGL.SwapBuffers();
            this.optkGL.Focus();
        }
        const float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;

        void KeywEventHandler(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            _time = 0.5f;
           
            switch(e.KeyCode.ToString())
            {
                case "W":
                    _camera.Position -= _camera.Front * cameraSpeed * _time; // Backwards
                    break;
                case "S":
                    _camera.Position += _camera.Front * cameraSpeed * _time; // Forward
                    break;
                case "A":
                    _camera.Position += _camera.Right * cameraSpeed * _time; // Left
                    break;
                case "D":
                    _camera.Position -= _camera.Right * cameraSpeed * _time; // Right

                    break;
                case "Q":
                    _camera.Position -= _camera.Up * cameraSpeed * _time; // Up
                    break;
                case "E":
                    _camera.Position += _camera.Up * cameraSpeed * _time; // down
                    break;

                    
            }
            Render();

        }
        void MousemEventHandler(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_firstMove) 
            {
                _lastPos = new Vector2(e.X, e.Y);
                _firstMove = false;
            }
            else
            {
                
                var deltaX = e.X - _lastPos.X;
                var deltaY = e.Y - _lastPos.Y;
                _lastPos = new Vector2(e.X, e.Y);

               
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
            Render();
        }

        void MousewEventHandler(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            _camera.Fov -= e.X * 0.01f;
            Render();
        }

        public readonly static string vertMainShader = $@"
        #version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{{
    texCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}}
         ";


        public readonly static string fragMainShader = $@"
         #version 330

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

void main()
{{
    outputColor = mix(texture(texture0, texCoord), texture(texture1, texCoord), 0.2);
}}
        ";
    }
}
