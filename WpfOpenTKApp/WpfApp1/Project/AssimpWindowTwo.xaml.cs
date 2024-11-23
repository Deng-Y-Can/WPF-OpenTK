using Assimp;
using LearnOpenTK.Common;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Printing;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using Camera = LearnOpenTK.Common.Camera;
using MessageBox = System.Windows.MessageBox;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector3D = Assimp.Vector3D;
using Vector4 = OpenTK.Mathematics.Vector4;
using Window = System.Windows.Window;
using Color = System.Drawing.Color;

namespace WpfApp
{
    /// <summary>
    /// AssimpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AssimpWindowTwo : Window
    {
        public AssimpWindowTwo()
        {
            InitializeComponent();
        }
        

        private GLControl optkGL;

        public enum CoordinateAxis
        {
            Z_positive,//六视图
            Z_negative,
            X_positive,
            X_negative,
            Y_positive,
            Y_negative,
        }
        public enum PickState
        {
            No_pick,//不同取点方法
            Z_pick,
            Color_pick,
        }
        public enum PickPointCount
        {
            One,//取点个数
            Two,
            Three,
        }
        private readonly uint[] _indices =
        {
            0, 1, 3,//文本框加载索引
            1, 2, 3
        };

        private int _vertexBufferObject;//主模型VBO
        private int _vertexArrayObject;//主模型VAO
        private int _elementBufferObject;//主模型EBO
        private int verts;              //顶点总个数
        private int framebuffer;        //帧缓冲
        private int[] framebufferlist = new int[1];
        private int colorTexture;      //帧缓冲纹理
        private int depthBuffer;      //帧缓冲深度缓冲
        int _vertexBufferObject2;     //文本框VBO
        int _vertexArrayObject2;      //文本框VAO

        private float[] _vertices;    //模型顶点数组
        private float height;         //模型高度
        private float maxX;           //模型最大X
        private float minX;
        private float maxY;
        private float minY;
        private float maxZ;
        private float minZ;
        private bool _firstMove = true;   //第一次移动鼠标
        private bool moveModel = true;
        const float cameraSpeed = 0.15f;  //摄像机移动速度（按键）
        const float sensitivity = 0.05f;  //右键平移因子
        float change = 3.0f;                //按键改变因子
        float mousechange = 0.045f;       //左键旋转因子（摄像机）
        private float rotatefactor = 0.2f;  //左键旋转因子（模型）
        private float _scalingFactor = 20;  //缩放因子
        private float _radians = 0;         //旋转角度
        string fontPath = "E:\\home\\github\\WPFOpenTK\\WPF-OpenTK\\WpfOpenTKApp\\WpfApp1\\Fonts\\CALIFI.TTF";  //字体路径

        Color4 backgroundColor = new Color4(0.2f, 0.3f, 0.3f, 1.0f);  //背景颜色
        private Vector2 _lastPos;        //鼠标上次位置
        private Vector3 coordinateOrigin;   //初始坐标原点
        private Vector3 lastPickPoint;      //上次取点
        private Matrix4 localModel = Matrix4.Identity;    //模型变换矩阵
        private Matrix4 originModel = Matrix4.Identity;   //初始模型变换矩阵(原点改变)
        private Shader _shader;                           //着色器
        private PrimitiveType primitiveType;               //绘制方式
        Vector3 pickPoints = new Vector3();     //取点列表
        List<Vector3> linePoints = new List<Vector3>();     //线段列表
        List<Vector3D> orginVerctors;                       //模型原始顶点

        private PickState pickState = PickState.No_pick;     //取点状态
        private PickPointCount pickPointCount = PickPointCount.One;  //取点个数
        private Camera _camera;                                      //摄像机
        private Texture _texture;                                   //文本框文字纹理
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            // 创建 GLControl.
            optkGL = new GLControl();

            // 指定GLControl的Load 和 Paint 事件
            optkGL.Load += new EventHandler(glc_Load);
            //glc.Paint += new System.Windows.Forms.PaintEventHandler(glc_Paint);

            this.MouseEnter += new System.Windows.Input.MouseEventHandler(chart_MouseEnter);
            optkGL.KeyDown += new System.Windows.Forms.KeyEventHandler(Chart_KeyDown);
            optkGL.MouseMove += new System.Windows.Forms.MouseEventHandler(Chart_MouseMove);
            optkGL.MouseWheel += new System.Windows.Forms.MouseEventHandler(Chart_MouseWheel);
            optkGL.MouseDown += new System.Windows.Forms.MouseEventHandler(MouseDown);

            // 指定这个GLControl作为chart控件的child
            chart.Child = optkGL;
        }
        void glc_Load(object sender, EventArgs e)
        {
            // 启用深度测试
            GL.Enable(EnableCap.DepthTest);
            //禁用深度缓冲的写入  深度掩码(Depth Mask)设置
            // GL.DepthMask(false);
            GL.PointSize(1f);
            GL.LineWidth(1f);
            // 设置深度测试函数
            // GL.DepthFunc(DepthFunction.Less);

            // 启用背面填充
            //GL.Enable(EnableCap.CullFace);

            //GL.Enable(EnableCap.StencilTest);//模板测试
            //GL.StencilMask(0xFF);//模板掩码 
            // 设置视口大小
            GL.Viewport(0, 0, 1000, 800);

            // GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f); // 设置清除颜色为灰色

        }
        public void Import(string filePath)
        {
            pickState = PickState.No_pick;
            this.getPoint.Content = "未取点模式";
            // 创建Assimp场景
            var importer = new AssimpContext();
            var scene = importer.ImportFile(filePath, PostProcessSteps.None);//
            if (scene.HasMeshes)
            {
                foreach (var mesh in scene.Meshes)
                {
                    if (mesh.Vertices.Count() > 0)
                    {
                        List<Vector3D> vector3s = mesh.Vertices;
                        _vertices = Tool.Vector3iToIntArray(vector3s);
                        orginVerctors = vector3s;
                        List<Face> face3s = mesh.Faces;
                        int[] _indices = Tool.FaceToIntArray(face3s);
                        primitiveType = (PrimitiveType)mesh.PrimitiveType;
                        primitiveType = PrimitiveType.Points;

                        verts = _vertices.Length;

                        float _verticeMax = _vertices.Max();
                        float _verticeMin = _vertices.Min();
                        maxX = vector3s.Max(v => v.X);
                        minX = vector3s.Min(v => v.X);
                        maxY = vector3s.Max(v => v.Y);
                        minY = vector3s.Min(v => v.Y);
                        maxZ = vector3s.Max(v => v.Z);
                        minZ = vector3s.Min(v => v.Z);

                        float[] maxlist = new float[] { maxX, minX, maxY, minY, maxZ, minZ };
                        float maxEdge = Tool.FindMaxAbsoluteValue(maxlist);

                        height = 1.2f * maxEdge;
                        ClearColor(backgroundColor);
                        _vertexBufferObject = GL.GenBuffer();

                        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

                        _vertexArrayObject = GL.GenVertexArray();
                        GL.BindVertexArray(_vertexArrayObject);
                        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
                        GL.EnableVertexAttribArray(0);

                        _shader = new Shader(vertMainShader, fragMainShader, 2);
                        _shader.Use();
                        _camera = new Camera(Vector3.UnitZ * height, 1.2f);

                    }
                    else
                    {
                        MessageBox.Show("No model data available for loading!");
                    }
                }
            }
            else
            {
                Console.WriteLine("No meshes found in the file.");
            }
            // 释放资源
            importer.Dispose();

        }

        //六个视图
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.Z_positive);
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.Z_negative);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.Y_positive);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.Y_negative);
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.X_positive);
        }
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.X_negative);
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //string filePath = Tool.SelectFile();
            string filePath = "E:\\home\\github\\WPFOpenTK\\WPF-OpenTK\\WpfOpenTKApp\\Model\\dragonBk5_0.ply";
            Import(filePath);
            Render();
        }


        //放大缩小
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            Scaling(_scalingFactor);
        }
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            Scaling(-_scalingFactor);
        }
        //旋转     

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            ModelRotation("X", 30);
        }
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            ModelRotation("Y", 30);
        }
        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            float[] arr = { minX + (maxX - minX) / 3, minX + (maxX - minX) * 2 / 3 };
            _shader = new Shader(vertGradientShader, GetGradientfrag("0,0,1,1", "0,1,0,1", "1,0,0,0", "x", arr), 0);
            _shader.Use();
            Render();
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            float[] arr = { minY + (maxY - minY) / 3, minY + (maxY - minY) * 2 / 3 };
            _shader = new Shader(vertGradientShader, GetGradientfrag("0,0,1,1", "0,1,0,1", "1,0,0,0", "y", arr), 0);
            _shader.Use();
            Render();
        }

        private void getPoint_Click(object sender, RoutedEventArgs e)
        {
            if (_camera == null)
            {
                MessageBox.Show("请先加载模型");
                return;
            }
            pickState = (PickState)(((int)pickState + 1) % 3);
            if (pickState == PickState.No_pick)
            {
                GL.DeleteFramebuffer(framebufferlist[0]);
                ClearColor(backgroundColor);
                GL.DeleteFramebuffer(framebuffer);
                this.getPoint.Content = "未取点模式";
                this.label.Content = $@"";
                _shader = new Shader(vertMainShader, fragMainShader, 2);

                _vertexBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
                GL.BindVertexArray(_vertexArrayObject);
                _shader.Use();
                SetMVP();
                GL.DrawArrays(primitiveType, 0, verts);
                optkGL.SwapBuffers();
                return;
            }

            if (pickState == PickState.Z_pick)
            {
                this.label.Content = $@"";
                this.getPoint.Content = "深度取点";
                //Render();
            }
            if (pickState == PickState.Color_pick)
            {
                this.label.Content = $@"";
                this.getPoint.Content = "颜色取点";
                ClearColor(backgroundColor);
                _shader = new Shader(vertPickRenderShader, fragPickRenderShader, 2);

                //framebuffer = GL.GenFramebuffer();
                GL.CreateFramebuffers(1, framebufferlist);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferlist[0]);

                // 创建并绑定颜色纹理
                colorTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, colorTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, optkGL.Width, optkGL.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);

                //深度模板纹理
                // GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, glc.Width, glc.Height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);
                // GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, colorTexture, 0);

                // 创建并绑定深度缓冲区
                //depthBuffer = GL.GenRenderbuffer();
                //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
                // GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, glc.Width, glc.Height);
                //GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);


                // 检查帧缓冲区状态
                var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferErrorCode.FramebufferComplete)
                {
                    throw new Exception($"Framebuffer is not complete: {status}");
                }
                _shader.Use();
                SetMVP();
                GL.DrawArrays(primitiveType, 0, verts / 3);
                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                //ClearColor(backgroundColor);
                //_shader = new Shader(vertPickRenderShader, fragPickRenderShader, 2);
                //_shader.Use();
                //SetMVP();
                //GL.DrawArrays(primitiveType, 0, verts/3);

                //_shader.Use();
                //SetMVP();

                // 解绑帧缓冲区
                optkGL.SwapBuffers();

            }

            //GL.Enable(EnableCap.ScissorTest);//剪裁操作
            //GL.Scissor(0, 0, glc.Width, glc.Height); 
        }

        private void Button_Click_16(object sender, RoutedEventArgs e)
        {
            if (_camera == null)
            {
                MessageBox.Show("请先加载模型");
                return;
            }
            pickPointCount = (PickPointCount)(((int)pickPointCount + 1) % 3);
            if (pickPointCount == PickPointCount.One)
            {
                this.PointName.Content = "单点";
            }
            if (pickPointCount == PickPointCount.Two)
            {
                this.PointName.Content = "直线";
            }
        }

        private void Button_Click_17(object sender, RoutedEventArgs e)
        {
            var newOrigin = new Vector3(-20.0f, 0.0f, 1.0f); // 例如，将原点移动到 (-1, 0, 0)
            List<Vector3D> changeVerctors = new List<Vector3D>();
            changeVerctors = orginVerctors;
            TranslateModel(changeVerctors, newOrigin);
            //originModel = originModel * Matrix4.CreateTranslation(40, 0, 0);
            //localModel = originModel;
        }
        //应用平移变换来更改模型原点
        private void TranslateModel(List<Vector3D> vertices, Vector3 translation)
        {
            List<Vector3D> newvertices = new List<Vector3D>();
            foreach (var vertex in vertices)
            {
                Vector4 vertex4 = new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f);
                vertex4 = vertex4 * Matrix4.CreateTranslation(translation.X, translation.Y, translation.Z);
                Vector3D vector3 = new Vector3D(vertex4.X / vertex4.W, vertex4.Y / vertex4.W, vertex4.Z / vertex4.W);
                newvertices.Add(vector3);
            }
            orginVerctors = newvertices;
            _vertices = Tool.Vector3iToIntArray(orginVerctors);
            ClearColor(backgroundColor);
            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);

            _shader = new Shader(vertMainShader, fragMainShader, 2);
            _shader.Use();
            MessageBox.Show("原点变换成功！");

        }


        private void chart_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            optkGL.Focus();
        }
        private void Chart_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            change += 0.01f;
            Key keyPressed = (Key)e.KeyCode;
            switch (e.KeyCode.ToString())
            {
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
            Render();
        }
        private void Chart_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_camera != null)
            {
                //右键平移
                if (e.Button == MouseButtons.Right)
                {
                    if (pickState != PickState.No_pick)
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
                            float deltaX = mouseX - _lastPos.X;
                            float deltaY = mouseY - _lastPos.Y;
                            _lastPos = new Vector2(mouseX, mouseY);
                            if (moveModel)
                            {
                                localModel = localModel * Matrix4.CreateTranslation(deltaX * sensitivity, -deltaY * sensitivity, 0);
                                if (pickPointCount == PickPointCount.One)
                                {
                                    PickupPoint(lastPickPoint, 0, 0);
                                }
                                else if (pickPointCount == PickPointCount.Two)
                                {
                                    PickupLine(lastPickPoint, 0, 0);
                                }

                            }
                            else
                            {
                                if (pickPointCount == PickPointCount.One)
                                {
                                    PickupPoint(lastPickPoint, deltaX * 0.1f, -deltaY * 0.1f);
                                }
                                else if (pickPointCount == PickPointCount.Two)
                                {
                                    PickupLine(lastPickPoint, deltaX * 0.1f, -deltaY * 0.1f);
                                }

                            }

                        }
                    }
                    else
                    {
                        #region
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

                            localModel = localModel * Matrix4.CreateTranslation(deltaX * sensitivity, -deltaY * sensitivity, 0);
                            ClearColor(backgroundColor);
                            GL.BindVertexArray(_vertexArrayObject);
                            _shader.Use();
                            _shader.SetMatrix4("model", localModel);
                            //localModel = model;
                            _shader.SetMatrix4("view", _camera.GetViewMatrix());
                            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                            GL.DrawArrays(primitiveType, 0, verts);
                            optkGL.SwapBuffers();

                            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                            //_camera.Yaw -= deltaX * sensitivity;
                            //_camera.Pitch += deltaY * sensitivity ; // Reversed since y-coordinates range from bottom to top

                        }
                        // Render();
                        #endregion
                    }

                }
                if (e.Button == MouseButtons.Left && pickState == PickState.No_pick)
                {
                    #region   左键旋转  物体移动
                    float mouseX = (float)e.X;
                    float mouseY = (float)e.Y;
                    if (_firstMove) // This bool variable is initially set to true.
                    {
                        _lastPos = new Vector2(mouseX, mouseY);
                        _firstMove = false;

                    }
                    else
                    {
                        var deltaX = mouseX - _lastPos.X;
                        var deltaY = mouseY - _lastPos.Y;
                        _lastPos = new Vector2(mouseX, mouseY);

                        localModel = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(deltaX * rotatefactor)) *
                            Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(deltaY * rotatefactor)) * localModel;
                        Render();

                    }

                    #endregion

                    #region    摄像头移动

                    Vector3 center = GetcoordinateOrigin();
                    //float mouseX = (float)e.X;
                    //float mouseY = (float)e.Y;
                    //if (_firstMove) 
                    //{
                    //    _lastPos = new Vector2(mouseX, mouseY);
                    //    _firstMove = false;
                    //}
                    //else
                    //{
                    //    // Calculate the offset of the mouse position
                    //    var deltaX = mouseX - _lastPos.X;
                    //    var deltaY = mouseY - _lastPos.Y;
                    //    _lastPos = new Vector2(mouseX, mouseY);
                    //    localModel =  


                    //    _camera.Position -= _camera.Right * deltaX * mousechange; // RIGHT
                    //    _camera.Position += _camera.Up * deltaY * mousechange; // RIGHT

                    //    Render();
                    //}
                    #endregion

                }

            }

        }

        private void Chart_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_camera != null)
            {
                //滚轮事件
                _camera.Fov -= e.Delta / 100;
                if (pickState != PickState.No_pick)
                {
                    if (pickPointCount == PickPointCount.One)
                    {
                        PickupPoint(lastPickPoint, 0f, 0f);
                    }
                    else if (pickPointCount == PickPointCount.Two)
                    {
                        PickupLine(lastPickPoint, 0f, 0f);
                    }

                }
                else
                {
                    Render();
                }
            }

        }
        private void MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && pickState != PickState.No_pick)
            {
                // 获取鼠标点击的屏幕坐标
                int x = e.X;
                int y = e.Y;
                // 将屏幕坐标转换为OpenGL的NDC坐标
                float ndcX = (float)(2.0f * x / optkGL.Width - 1.0f);
                float ndcY = (float)(1.0f - 2.0 * y / optkGL.Height);// (float)((2.0f * y) / glc.Height - 1.0f);

                //                // 计算逆矩阵
                //                // 获取当前的投影矩阵和视图矩阵
                Matrix4 projectionMatrix = _camera.GetProjectionMatrix();
                Matrix4 viewMatrix = _camera.GetViewMatrix();
                Matrix4 inverseMatrix = Matrix4.Invert(localModel * viewMatrix * projectionMatrix);

                float nearplane = 0.01f;
                float farplane = 100f;

                if (pickState == PickState.Z_pick)
                {
                    // 2.读取深度值
                    GL.Enable(EnableCap.DepthTest);
                    // GL.ReadBuffer(ReadBufferMode.Aux0);
                    float[] depth = new float[1];
                    GL.ClampColor(ClampColorTarget.ClampVertexColor, ClampColorMode.False);
                    GL.ReadPixels(x, optkGL.Height - y, 1, 1, PixelFormat.DepthComponent, PixelType.Float, depth);


                    float originalDepth = 2 * depth[0] - 1;
                    // depth[0] * (farplane - nearplane) + nearplane;
                    //float depthZ = 1 / ((depth[0] / farplane) + ((1 - depth[0]) / nearplane));
                    //float originalDepth = depth[0];                                    //originalDepth = depth[0];
                    Vector4 depthin = new Vector4(ndcX, ndcY, originalDepth, 1) * inverseMatrix;
                    Vector3 depnear = new Vector3(depthin.X / depthin.W, depthin.Y / depthin.W, depthin.Z / depthin.W);

                    Vector3D vector3D = new Vector3D(depnear.X, depnear.Y, depnear.Z);
                    Vector3D vector3Dnear = Tool.ContainsVector3D(orginVerctors, vector3D);
                    if (vector3Dnear.X != -99999)
                    {
                        this.label.Content = $@"x:{vector3Dnear.X};
y:{vector3Dnear.Y};
z:{vector3Dnear.Z}";
                        Vector3 vector3near = new Vector3((float)vector3Dnear.X, (float)vector3Dnear.Y, (float)vector3Dnear.Z);
                        if (pickPointCount == PickPointCount.One)
                        {
                            PickupPoint(vector3near, 0, 0, true);
                        }
                        if (pickPointCount == PickPointCount.Two)
                        {
                            PickupLine(vector3near, 0, 0, true);
                        }

                    }
                    else
                    {
                        this.label.Content = $@"no pick";
                    }
                }
                if (pickState == PickState.Color_pick)
                {
                    //2.2,根据颜色计算 较精确
                    uint id = ReadStageVertexId(x, y);
                    if (id < _vertices.Length / 3 && id > 0)
                    {
                        Vector3 depcolor = new Vector3(_vertices[3 * id], _vertices[3 * id + 1], _vertices[3 * id + 2]);
                        this.label.Content = $@"x:{depcolor.X};
y:{depcolor.Y}; 
z:{depcolor.Z}";
                        if (pickPointCount == PickPointCount.One)
                        {
                            PickupPoint(depcolor, 0, 0, true);
                        }
                        if (pickPointCount == PickPointCount.Two)
                        {
                            PickupLine(depcolor, 0, 0, true);
                        }
                    }
                    else
                    {
                        this.label.Content = $@"no pick";
                    }
                }


               //1.进行拾取操作 先将所有点转换成变换后的坐标，取与捕获点最近的四个，再从中找出Z值最小的一个作为捕获点Z值，然后通过逆矩阵求出原来的点
               //                 List<Vector3> originalPoints = Tool.ArrayToList(_vertices, 0);
               // List<Vector3> points = AfterConversion(originalPoints);
               // List<Vector3> vector3nearfour = PointDistanceComparer.FindNearestFourPoints(new Vector3(ndcX, ndcY, 1), points);
               // Vector3 nearFZ = vector3nearfour.OrderBy(v => v.Z).FirstOrDefault(); //Tool.FindNearPointsOnLine(points, nearvector, farvector);
               // Vector4 ndcin = new Vector4(ndcX, ndcY, nearFZ.Z, 1) * inverseMatrix;
               // Vector3 near = new Vector3(ndcin.X / ndcin.W, ndcin.Y / ndcin.W, ndcin.Z / ndcin.W);

                // //比较最近点
                // near = Tool.FindNearest3DPoint(originalPoints, near, 5, out double nearst);
                // this.label.Content = $@"x:{near.X};
                // y:{near.Y};
                // z:{near.Z}
                // 距离：{nearst}";


                // //3.以近平面和远平面组成的向量与点进行距离计算，取最近的一个点
                //                 Vector4 nearPonint = new Vector4(ndcX, ndcY, -1, 1) * inverseMatrix;
                // Vector4 farPonint = new Vector4(ndcX, ndcY, 1, 1) * inverseMatrix;
                // Vector3 near2 = new Vector3(nearPonint.X / nearPonint.W, nearPonint.Y / nearPonint.W, nearPonint.Z / nearPonint.W);
                // Vector3 far = new Vector3(farPonint.X / farPonint.W, farPonint.Y / farPonint.W, farPonint.Z / farPonint.W);
                // List<Vector3> points2 = Tool.ArrayToList(_vertices, 0);
                // Vector3 vector = Tool.FindNearPointsOnLine(points2, near2, far, out float errorvalue);
                // Vector4 tranfV4 = new Vector4(vector.X, vector.Y, vector.Z, 1) * localModel * viewMatrix * projectionMatrix;
                // Vector3 tranfvector = new Vector3(ndcX, ndcY, tranfV4.Z / tranfV4.W);
                // this.label.Content = $@"x:{vector.X};
                // y:{vector.Y};
                // z:{vector.Z}
                // 误差：{errorvalue}";

            }

        }

        private void PickupPoint(Vector3 vector, float mouseX = 0, float mouseY = 0, bool isNewPoint = false)
        {
            ClearColor(backgroundColor);

            if (lastPickPoint == null)
            {
                return;
            }
            float centerndcX = 0f;
            float centerndcY = 0f;
            float centerndcZ = 0f;

            float hight = 0.15f;
            //矩形边界限制
            if (mouseX != 0 || mouseY != 0)
            {
                centerndcX += mouseX * hight;
                centerndcY += mouseY * hight;
            }

            if (centerndcX < 4 * hight - 1)
            {
                centerndcX = 4 * hight - 1;
            }
            if (centerndcY < 2 * hight - 1)
            {
                centerndcY = 2 * hight - 1;
            }
            if (centerndcX > 1 - 4 * hight)
            {
                centerndcX = 1 - 4 * hight;
            }
            if (centerndcY > 1 - 2 * hight)
            {
                centerndcY = 1 - 2 * hight;
            }

            Vector3 center = new Vector3(centerndcX, centerndcY, centerndcZ);

            Vector3 centerLeftT = new Vector3(centerndcX, centerndcY + hight, centerndcZ);
            Vector3 centerRightT = new Vector3(centerndcX + 2 * hight, centerndcY + hight, centerndcZ);
            Vector3 centerRightB = new Vector3(centerndcX + 2 * hight, centerndcY - hight, centerndcZ);
            Vector3 centerLeftB = new Vector3(centerndcX, centerndcY - hight, centerndcZ);

            float[] _vertices2 = {
               centerRightT.X, centerRightT.Y, centerRightT.Z, 1f, 1f,
               centerRightB.X,centerRightB.Y,centerRightB.Z, 1f, 0.0f,
               centerLeftB.X,centerLeftB.Y,centerLeftB.Z, 0.0f, 0.0f,
               centerLeftT.X, centerLeftT.Y, centerLeftT.Z, 0.0f,1f
               };
            List<float> centerList = new List<float>(){centerRightT.X, centerRightT.Y, centerRightT.Z,
               centerRightB.X,centerRightB.Y,centerRightB.Z,
               centerLeftB.X,centerLeftB.Y,centerLeftB.Z,
               centerLeftT.X, centerLeftT.Y, centerLeftT.Z};
            Vector3 nearNode = Tool.CalculateNearestPointOnList(centerList, vector);

            Vector3 changeVector3 = TransformVector(vector, GetMVP());
            float[] _vertices3 = {
                changeVector3.X,changeVector3.Y,changeVector3.Z,
               nearNode.X,nearNode.Y,nearNode.Z
               };

            //主模型
            if (pickState == PickState.Color_pick)
            {
                _shader = new Shader(vertPickRenderShader, fragPickRenderShader, 2);
            }
            else
            {
                _shader = new Shader(vertMainShader, fragMainShader, 2);
            }
            _shader.Use();
            SetMVP();

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);
            //GL.DrawArraysInstanced(PrimitiveType.Points, 0, _vertices.Length, 1);
            GL.DrawArrays(PrimitiveType.Points, 0, _vertices.Length / 3);

            if (this.label.Content == "" || this.label.Content == "no pick")
            {
                optkGL.SwapBuffers();
                return;
            }

            //文本框
            string label = this.label.Content.ToString();
            RendTextBox(_vertices2, label);

            //渲染点
            RendPoint(changeVector3);

            //渲染直线
            _shader = new Shader(vertPickLineShader, fragPickLineShader, 0);
            _shader.Use();
            var _vertexBufferObject3 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject3);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices3.Length * sizeof(float), _vertices3, BufferUsageHint.StaticDraw);

            var _vertexArrayObject3 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject3);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);
            SetUnitMVP();
            GL.PointSize(2.5f);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            GL.PointSize(1f);
            lastPickPoint = vector;
            optkGL.SwapBuffers();

        }

        private void PickupLine(Vector3 vector, float mouseX = 0, float mouseY = 0, bool IsNewPoint = false)
        {
            ClearColor(backgroundColor);

            float centerndcX = 0f;
            float centerndcY = 0f;
            float centerndcZ = 0f;

            float hight = 0.15f;
            //矩形边界限制
            if (mouseX != 0 || mouseY != 0)
            {
                centerndcX += mouseX * hight;
                centerndcY += mouseY * hight;
            }
            else
            {
                if (linePoints.Count == 2 && IsNewPoint)
                {
                    linePoints.Clear();
                }

            }

            if (centerndcX < 4 * hight - 1)
            {
                centerndcX = 4 * hight - 1;
            }
            if (centerndcY < 2 * hight - 1)
            {
                centerndcY = 2 * hight - 1;
            }
            if (centerndcX > 1 - 4 * hight)
            {
                centerndcX = 1 - 4 * hight;
            }
            if (centerndcY > 1 - 2 * hight)
            {
                centerndcY = 1 - 2 * hight;
            }

            Vector3 center = new Vector3(centerndcX, centerndcY, centerndcZ);
            Vector3 centerLeftT = new Vector3(centerndcX, centerndcY + hight, centerndcZ);
            Vector3 centerRightT = new Vector3(centerndcX + 2 * hight, centerndcY + hight, centerndcZ);
            Vector3 centerRightB = new Vector3(centerndcX + 2 * hight, centerndcY - hight, centerndcZ);
            Vector3 centerLeftB = new Vector3(centerndcX, centerndcY - hight, centerndcZ);

            float[] _vertices2 = {
               centerRightT.X, centerRightT.Y, centerRightT.Z, 1f, 1f,
               centerRightB.X,centerRightB.Y,centerRightB.Z, 1f, 0.0f,
               centerLeftB.X,centerLeftB.Y,centerLeftB.Z, 0.0f, 0.0f,
               centerLeftT.X, centerLeftT.Y, centerLeftT.Z, 0.0f,1f
               };
            List<float> centerList = new List<float>(){centerRightT.X, centerRightT.Y, centerRightT.Z,
               centerRightB.X,centerRightB.Y,centerRightB.Z,
               centerLeftB.X,centerLeftB.Y,centerLeftB.Z,
               centerLeftT.X, centerLeftT.Y, centerLeftT.Z};
            Vector3 nearNode = Tool.CalculateNearestPointOnList(centerList, vector);



            //主模型
            if (pickState == PickState.Color_pick)
            {
                _shader = new Shader(vertPickRenderShader, fragPickRenderShader, 2);
            }
            else
            {
                _shader = new Shader(vertMainShader, fragMainShader, 2);
            }
            _shader.Use();
            SetMVP();

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);
            //GL.DrawArraysInstanced(PrimitiveType.Points, 0, _vertices.Length, 1);
            GL.DrawArrays(PrimitiveType.Points, 0, _vertices.Length / 3);

            if (this.label.Content == "" || this.label.Content == "no pick")
            {
                optkGL.SwapBuffers();
                return;
            }
            Vector3 changeVector3 = TransformVector(vector, GetMVP());
            if (linePoints.Count < 2)
            {
                linePoints.Add(vector);
            }

            foreach (var item in linePoints)
            {
                RendPoint(TransformVector(item, GetMVP()));
            }

            if (linePoints.Count == 2)
            {
                Vector3 changeVector3First = TransformVector(linePoints[0], GetMVP());
                Vector3 changeVector3Second = TransformVector(linePoints[1], GetMVP());
                float[] _vertices3 = {
                changeVector3First.X,changeVector3First.Y,changeVector3First.Z,
               changeVector3Second.X,changeVector3Second.Y,changeVector3Second.Z
               };

                //文本框
                string text = $@"distance: {Vector3.Distance(linePoints[0], linePoints[1])}";
                RendTextBox(_vertices2, text);

                //渲染直线
                _shader = new Shader(vertPickLineShader, fragPickLineShader, 0);
                _shader.Use();
                var _vertexBufferObject3 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject3);
                GL.BufferData(BufferTarget.ArrayBuffer, _vertices3.Length * sizeof(float), _vertices3, BufferUsageHint.StaticDraw);
                var _vertexArrayObject3 = GL.GenVertexArray();
                GL.BindVertexArray(_vertexArrayObject3);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
                GL.EnableVertexAttribArray(0);
                SetUnitMVP();
                GL.PointSize(2.5f);
                GL.DrawArrays(PrimitiveType.Lines, 0, 2);
                GL.PointSize(1f);
                lastPickPoint = vector;

            }
            optkGL.SwapBuffers();

        }


        private void RendTextBox(float[] _vertices2, string label)
        {
            _shader = new Shader(vertRectangularTextBoxShader, fragRectangularTextBoxShader, 0);
            _shader.Use();//多模型渲染需要先重新加载着色器，即切换程序附着的着色器
            _vertexBufferObject2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject2);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices2.Length * sizeof(float), _vertices2, BufferUsageHint.StaticDraw);
            _vertexArrayObject2 = GL.GenVertexArray();
            //GL.BindVertexArray(_vertexArrayObject2);
            //GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            //GL.EnableVertexAttribArray(0);
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            string png = "output.png";//Resources/container.png
            string text = label;
            var bmp2 = Tool.GenerateBitmapFromString(text, fontPath, 25f, Color.Blue);
            bmp2.Save(png, ImageFormat.Png);
            _texture = Texture.LoadFromFile(png);
            _texture.Use(TextureUnit.Texture0);
            SetUnitMVP();
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        private void RendPoint(Vector3 vector3)
        {
            float[] _vertices4 = { vector3.X, vector3.Y, vector3.Z };
            _shader = new Shader(vertPickLineShader, fragPickPointShader, 0);
            _shader.Use();
            var _vertexBufferObject4 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject4);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices4.Length * sizeof(float), _vertices4, BufferUsageHint.StaticDraw);

            var _vertexArrayObject4 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject4);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);
            SetUnitMVP();
            GL.PointSize(5);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            GL.PointSize(1);
        }

        // 计算两个Vector3D之间的距离     
        public void Render()
        {
            if (_shader != null)
            {
                ClearColor(backgroundColor);
                GL.BindVertexArray(_vertexArrayObject);
                _shader.Use();

                //var model = Matrix4.Identity;
                //localModel= model;
                _shader.SetMatrix4("model", localModel);
                _shader.SetMatrix4("view", _camera.GetViewMatrix());
                _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                GL.DrawArrays(primitiveType, 0, verts / 3);
                optkGL.SwapBuffers();

            }

        }
        public void Scaling(float scalingFactor)
        {
            if (_shader != null)
            {
                ClearColor(backgroundColor);

                GL.BindVertexArray(_vertexArrayObject);
                _shader.Use();
                _camera.Fov -= scalingFactor;
                SetMVP();
                GL.DrawArrays(primitiveType, 0, verts);

                optkGL.SwapBuffers();
            }
        }
        public Matrix4 ModelChange(CoordinateAxis coordinateAxis)
        {
            var model = originModel;
            switch (coordinateAxis)
            {
                case CoordinateAxis.Z_positive:
                    model = model * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(0));
                    break;
                case CoordinateAxis.Z_negative:
                    model = model * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(180));
                    break;

                case CoordinateAxis.Y_positive:
                    model = model * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(90));
                    break;
                case CoordinateAxis.Y_negative:
                    model = model * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(-90));
                    break;
                case CoordinateAxis.X_positive:
                    model = model * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(90));
                    break;
                case CoordinateAxis.X_negative:
                    model = model * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(-90));
                    break;

            }
            return model;
        }
        public void PlaneSwitching(CoordinateAxis coordinateAxis)
        {
            if (_shader != null)
            {
                ClearColor(backgroundColor);
                GL.BindVertexArray(_vertexArrayObject);
                _camera = new Camera(Vector3.UnitZ * height, 1.2f);
                _shader = new Shader(vertMainShader, fragMainShader, 2);
                _shader.Use();
                var model = ModelChange(coordinateAxis);
                localModel = model;
                SetMVP();
                GL.DrawArrays(primitiveType, 0, verts / 3);
                optkGL.SwapBuffers();
            }
        }
        public void ModelRotation(string axis, float radians)
        {
            if (_shader != null)
            {
                var model = originModel;
                switch (axis)
                {
                    case "X":
                        model = model * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_radians += radians));
                        break;


                    case "Y":
                        model = model * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_radians += radians));
                        break;

                }
                ClearColor(backgroundColor);
                GL.BindVertexArray(_vertexArrayObject);
                _shader.Use();
                localModel = model;
                SetMVP();
                GL.DrawArrays(primitiveType, 0, verts);
                optkGL.SwapBuffers();
            }
        }
        private List<Vector3> AfterConversion(List<Vector3> vector3s)
        {
            List<Vector3> result = new List<Vector3>();
            Matrix4 inverseMatrix = localModel * _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
            for (int i = 0; i < vector3s.Count(); i++)
            {
                Vector4 vector4 = new Vector4(vector3s[i].X, vector3s[i].Y, vector3s[i].Z, 1f) * inverseMatrix;
                Vector3 vector3 = new Vector3(vector4.X / vector4.W, vector4.Y / vector4.W, vector4.Z / vector4.W);
                result.Add(vector3);
            }
            return result;
        }
        public uint ReadStageVertexId(int x, int y)
        {
            byte[] pixels = new byte[4];

            // 调用GL.ReadPixels读取指定位置的像素数据
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.ReadPixels(x, optkGL.Height - y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            // 获取RGBA值
            Color color = Color.FromArgb(pixels[3], pixels[0], pixels[1], pixels[2]);
            uint stageVertexId = ToStageVertexId(color);
            return stageVertexId;
        }
        public uint ToStageVertexId(Color color)
        {
            uint shiftedR = (uint)color.R;
            uint shiftedG = ((uint)color.G) << 8;
            uint shiftedB = ((uint)color.B) << 16;
            uint shiftedA = ((uint)color.A) << 24;
            uint stageVertexId = shiftedR + shiftedG + shiftedB + shiftedA;

            return stageVertexId;
        }
        private Vector3 TransformVector(Vector3 vector, Matrix4 matrix)
        {
            Vector4 changeVector = new Vector4(vector.X, vector.Y, vector.Z, 1);
            changeVector = changeVector * matrix;
            return new Vector3(changeVector.X / changeVector.W, changeVector.Y / changeVector.W, changeVector.Z / changeVector.W);
        }
        private Vector3 GetcoordinateOrigin()
        {
            Vector4 oldOrigin = new Vector4(0, 0, 0, 1);
            oldOrigin = oldOrigin * localModel * _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
            Vector3 newOrigin = new Vector3(oldOrigin.X / oldOrigin.W, oldOrigin.Y / oldOrigin.W, oldOrigin.Z / oldOrigin.W);
            return newOrigin;
        }
        private Vector3 GetModelOrigin()
        {
            Vector4 oldOrigin = new Vector4(0, 0, 0, 1);
            oldOrigin = oldOrigin * localModel;
            Vector3 newOrigin = new Vector3(oldOrigin.X / oldOrigin.W, oldOrigin.Y / oldOrigin.W, oldOrigin.Z / oldOrigin.W);
            return newOrigin;
        }
        private void ClearColor(Color4 color)
        {
            GL.ClearColor(color.R, color.B, color.G, color.A); // 设置清除颜色为灰色
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }
        private void SetMVP()
        {
            _shader.SetMatrix4("model", localModel);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        }
        private void SetUnitMVP()
        {
            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetMatrix4("view", Matrix4.Identity);
            _shader.SetMatrix4("projection", Matrix4.Identity);
        }
        private Matrix4 GetMVP()
        {
            return localModel * _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
        }



        public readonly static string vertMainShader = $@"
        #version 330 core
        layout(location = 0) in vec3 aPosition;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;

        void main(void)
        {{
         gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        }}
         ";


        public readonly static string fragMainShader = $@"
        #version 330
        out vec4 outputColor;

        void main()
        {{
        outputColor = vec4(1.0, 1.0, 0.0, 1.0);
        }}        
        ";

        private string vertGradientShader = $@"
        #version 330 core
        layout(location = 0) in vec3 aPosition;
        out vec3 texCoord;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;

        void main(void)
        {{
        texCoord=aPosition;
        gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        }}
         ";
        public string GetGradientfrag(string color1, string color2, string color3, string axle, float[] waypoint)
        {
            return $@"
            #version 330
            out vec4 outputColor;
            in vec3 texCoord;
            void main()
            {{
            if(texCoord.{axle} < {waypoint[0].ToString()})
            outputColor = vec4({color1});
            else
            if(texCoord.{axle}<{waypoint[1].ToString()})
                outputColor = vec4({color2});
            else 
                outputColor = vec4({color3});
            }}
            ";
        }

        public readonly static string fragPickPointShader = $@"
        #version 330
        out vec4 outputColor;
        void main()
        {{
         outputColor = vec4(1, 0, 0, 1.0);
         }}
        ";

        public readonly static string fragPickLineShader = $@"
        #version 330
        out vec4 outputColor;
        void main()
        {{
         outputColor = vec4(0.7, 0.5, 0.8, 1.0);    
        }}
        ";

        public readonly static string vertPickLineShader = $@"
        #version 330 core
        layout(location = 0) in vec3 aPosition;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;

        void main(void)
        {{
        gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        }}
        ";

        public readonly static string vertPickRenderShader = $@"
        #version 330 core
        layout(location = 0) in vec3 aPosition;
        out vec4 verctor_Color;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        void main(void)
       {{
        gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        int objectID = gl_VertexID;
        verctor_Color = vec4(
        float(objectID & 0xFF) / 255.0, 
        float((objectID >> 8) & 0xFF) / 255.0, 
        float((objectID >> 16) & 0xFF) / 255.0, 
        float((objectID >> 24) & 0xFF) / 255.0);
        }}
        ";


        public readonly static string fragPickRenderShader = $@"
        #version 330
        in vec4 verctor_Color;
        out vec4 outputColor;

        void main()
        {{
        outputColor = verctor_Color;
        }}        
        ";


        public readonly static string vertRectangularTextBoxShader = $@"
        #version 330 core
        layout(location = 0) in vec3 aPosition;
        layout(location = 1) in vec2 aTexCoord;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        out vec2 texCoord;

        void main(void)
       {{   
        texCoord = aTexCoord;
        gl_Position = vec4(aPosition, 1.0) * model * view * projection;;
        }}
        ";

        public readonly static string fragRectangularTextBoxShader = $@"
        #version 330
        out vec4 outputColor;
        in vec2 texCoord;
        uniform sampler2D texture0;

        void main()
        {{
        outputColor = texture(texture0, texCoord);
        }}        
       ";

    }
}
