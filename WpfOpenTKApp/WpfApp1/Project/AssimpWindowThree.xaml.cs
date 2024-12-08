﻿using Assimp;
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
using System.IO;

namespace WpfApp
{
    /// <summary>
    /// AssimpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AssimpWindowThree : Window
    {
        public AssimpWindowThree()
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
            Screeen_pick,
            Z_pick,
            Color_pick,
        }
        public enum PickPointCount
        {
            One,//取点个数
            Two

        }
        private readonly uint[] _indices =
        {
            0, 1, 3,//文本框加载索引
            1, 2, 3
        };

        private int _vertexBufferObject;//主模型VBO
        private int _vertexArrayObject;//主模型VAO
        private int _elementBufferObject;//主模型EBO
        private int framebuffer;        //帧缓冲
        private int[] framebufferlist = new int[1];
        private int colorTexture;      //帧缓冲纹理
        private int depthBuffer;      //帧缓冲深度缓冲
        int _vertexBufferObject2;     //文本框VBO
        int _vertexArrayObject2;      //文本框VAO
          
        private float height;         //模型高度
        private bool _firstMove = true;   //第一次移动鼠标
        private bool moveModel = true;
        private bool leftMouseButtonDown;
        private bool rightMouseButtonDown;
        private float cameraSpeed;  //摄像机移动速度（按键）
        private float sensitivity;  //右键平移因子
        float change;                //按键改变因子
        float mousechange;       //左键旋转因子（摄像机）
        private float rotatefactor;  //左键旋转因子（模型）
        private float _scalingEnlargeFactor;  //放大因子
        private float _scalingReduceFactor;  //缩小因子
        private float _scalingPositionFactor;
        float hight;                         //文本框高度因子
        private int decimalPlaces; //文本框小数位数
        private float _radians = 0;         //旋转角度
        string fontPath = Directory.GetCurrentDirectory() + "\\Fonts\\FRADMCN.ttf";  //字体路径

        Color4 backgroundColor;  //背景颜色
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

        private VertexList vertexList;
        public void KeyParam()
        {
            backgroundColor = new Color4(0.75f, 1f, 0.75f, 1.0f);
            cameraSpeed = 0.15f;            //摄像机移动速度（按键）
            sensitivity = 0.02f;            //右键平移因子
            change = 0.01f;                  //按键改变因子
            mousechange = 0.045f;           //左键旋转因子（摄像机）
            rotatefactor = 0.45f;           //左键旋转因子（模型）
            _scalingEnlargeFactor = 1.1f;  //缩小因子
            _scalingReduceFactor = 0.9091f;   //放大因子
            _scalingPositionFactor = 1f;    //摄像机放缩因子
            hight = 0.08f;                  //文本框高度因子
            decimalPlaces = 5;              //文本框小数位数
        }

        private float scaling = 1;
        public void scalingFuction(float value)
        {
            localModel = localModel * Matrix4.CreateScale(1 / scaling);
            if (scaling > 1)
            {
                scaling = value > 0 ? (scaling + 1) : (scaling - 1);

            }
            else if (scaling == 1)
            {
                scaling = value > 0 ? scaling + 1 : 1 / (scaling + 1);
            }
            else
            {
                float reciprocal = value < 0 ? (1 / scaling) + 1 : (1 / scaling) - 1;
                scaling = 1 / reciprocal;
            }
            localModel = localModel * Matrix4.CreateScale(scaling);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
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
            optkGL.MouseDown += new System.Windows.Forms.MouseEventHandler(Chart_MouseDown);
            optkGL.MouseUp+= new System.Windows.Forms.MouseEventHandler(Chart_MouseUp);

            // 指定这个GLControl作为chart控件的child
            chart.Child = optkGL;
            KeyParam();
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
            chart.Height = 800;
            chart.Width = 1000;

            GL.Viewport(0, 0, 1000, 800);

            // 设置视口大小

            // GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f); // 设置清除颜色为灰色

        }
        public void Import(string filePath)
        {
            ResizeParam();
            // 创建Assimp场景
            var importer = new AssimpContext();
            try
            {              
               // importer.ImportFile(filePath, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);
                var scene = importer.ImportFile(filePath, PostProcessSteps.None);//E:\\OpenTK\\数据\\20241022202442.ply    mesh3.ply
                //导出模型
                //bool success = importer.ExportFile(scene, Directory.GetCurrentDirectory() + $@"\model\kd.obj", "obj");
                

                if (scene.HasMeshes)
                {
                    foreach (var mesh in scene.Meshes)
                    {
                        //Material material = scene.Materials[0];
                        InstantiateModel(mesh);
                    }
                }
                else
                {
                    Console.WriteLine("No meshes found in the file.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($@"{ex.ToString()}");
            }
            finally
            {
                // 释放资源
                importer.Dispose();
            }                    
        }

        public void Import2(string filePath)
        {
            ClearColor(backgroundColor);
            _shader = new Shader(vertModelShader, fragModelShader, 2);
            _camera = new Camera(Vector3.UnitZ * 20, 1.2f);
            SetMVP();

            ModelT modelT = new ModelT(filePath);
            modelT.Draw(_shader);
            optkGL.SwapBuffers();
        }
        private void InstantiateModel(Mesh mesh)
        {
            if (mesh.Vertices.Count() > 0)
            {           
                vertexList = new VertexList(mesh.Vertices);            
                orginVerctors = vertexList._vector3s;
                List<Face> face3s = mesh.Faces;
                int[] _indices = Tool.FaceToIntArray(face3s);
                primitiveType = (PrimitiveType)mesh.PrimitiveType;
                primitiveType = PrimitiveType.Points;               

                float[] maxlist = new float[] { vertexList._maxX, vertexList._minX, vertexList._maxY, vertexList._minY, vertexList._maxZ, vertexList._minZ };
                float maxEdge = Tool.FindMaxAbsoluteValue(maxlist);

                Vector3 newOrigin = new Vector3(-(vertexList._maxX + vertexList._minX) / 2, -(vertexList._maxY + vertexList._minY) / 2, -(vertexList._maxZ + vertexList._minZ) / 2);
                TranslateModel(vertexList._vector3s, newOrigin);

                height =0.5f* maxEdge;
                ClearColor(backgroundColor);
                _vertexBufferObject = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexList._vertices.Length * sizeof(float), vertexList._vertices, BufferUsageHint.StaticDraw);

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
            string filePath = Directory.GetCurrentDirectory() + "\\model\\candy.ply";       //20241022202442.ply";mesh3
            //Import2(filePath);
            
            Import(filePath);
            Render();
        }


        //放大缩小
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {

            Scaling(_scalingPositionFactor);
        }
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            Scaling(-_scalingPositionFactor);
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
            float[] arr = { vertexList._minX + (vertexList._maxX - vertexList._minX) / 3, vertexList._minX + (vertexList._maxX - vertexList._minX) * 2 / 3 };
            _shader = new Shader(vertGradientShader, GetGradientfrag("0,0,1,1", "0,1,0,1", "1,0,0,0", "x", arr), 0);
            _shader.Use();
            Render();
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            float[] arr = { vertexList._minY + (vertexList._maxY - vertexList._minY) / 3, vertexList._minY + (vertexList._maxY - vertexList._minY) * 2 / 3 };
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
            pickState = (PickState)(((int)pickState + 1) % 4);
            if (pickState == PickState.No_pick)
            {
                GL.DeleteFramebuffer(framebufferlist[0]);
                ClearColor(backgroundColor);
                //GL.DeleteFramebuffer(framebuffer);
                ResizeParam();

                _shader = new Shader(vertMainShader, fragMainShader, 2);

                //_vertexBufferObject = GL.GenBuffer();
                //GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                //GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
                //GL.BindVertexArray(_vertexArrayObject);
                //_shader.Use();
                //SetMVP();
                //GL.DrawArrays(primitiveType, 0, verts);
                //glc.SwapBuffers();
                Render();
                return;
            }

            if (pickState == PickState.Z_pick)
            {
                this.label.Content = $@"";
                this.getPoint.Content = "深度取点";
                ResizePoint();
                //Render();
            }
            if (pickState == PickState.Screeen_pick)
            {
                this.label.Content = $@"";
                this.getPoint.Content = "屏幕取点";
                ResizePoint();
                //Render();
            }
            if (pickState == PickState.Color_pick)
            {
                this.label.Content = $@"";
                this.getPoint.Content = "颜色取点";
                ResizePoint();
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
                    MessageBox.Show($"Framebuffer is not complete: {status}");
                    //throw new Exception($"Framebuffer is not complete: {status}");
                }
                _shader.Use();
                SetMVP();
                GL.DrawArrays(primitiveType, 0, vertexList._count);
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

        
        private void Button_Click_17(object sender, RoutedEventArgs e)
        {
            if (_camera == null)
            {
                MessageBox.Show("请先加载模型");
                return;
            }
            ParameterDialog dialog = new ParameterDialog();
            if (dialog.ShowDialog() == true && dialog.paramX.Text != "")
            {
                float paramX = float.Parse(dialog.ParamX);
                float paramY = float.Parse(dialog.ParamY);
                float paramZ = float.Parse(dialog.ParamZ);

                var newOrigin = new Vector3(-paramX, -paramY, -paramZ); // 例如，将原点移动到 (-1, 0, 0)

                TranslateModel(vertexList._vector3s, newOrigin);
                ClearColor(backgroundColor);
                ResizeParam();
                MessageBox.Show($"原点变换成功！坐标原点已更新为：X:{paramX};Y:{paramY};Z:{paramZ}");
            }
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

            vertexList = new VertexList(newvertices);
            ClearColor(backgroundColor);
            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexList._vertices.Length * sizeof(float), vertexList._vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);

            _shader = new Shader(vertMainShader, fragMainShader, 2);
            _shader.Use();
            //MessageBox.Show("原点变换成功！");

        }

       
        private void ResizePoint()
        {
            lastPickPoint = new Vector3(Tool.unReach, Tool.unReach, Tool.unReach);
            linePoints.Clear();
            _firstMove = true;
        }
        private void chart_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            optkGL.Focus();
        }
        private void Chart_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

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
                float mouseX = (float)e.X;
                float mouseY = (float)e.Y;
                float deltaX = Tool.unReach;
                float deltaY = Tool.unReach;
                if (_firstMove) // This bool variable is initially set to true.
                {
                    _lastPos = new Vector2(mouseX, mouseY);
                    _firstMove = false;
                }
                else
                {
                    deltaX = mouseX - _lastPos.X;
                    deltaY = mouseY - _lastPos.Y;
                    _lastPos = new Vector2(mouseX, mouseY);
                }
                if (Math.Abs(deltaX) < 40 && Math.Abs(deltaY) < 40)
                {
                    //右键平移
                    if (e.Button == MouseButtons.Right && rightMouseButtonDown)
                    {
                        if (pickState != PickState.No_pick)
                        {
                            if (moveModel)
                            {
                                //localModel = localModel * Matrix4.CreateTranslation(deltaX * sensitivity, -deltaY * sensitivity, 0);
                                _camera.Position -= _camera.Right * deltaX * sensitivity;
                                _camera.Position -= _camera.Up * -deltaY * sensitivity;
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
                        else
                        {
                            #region

                            //localModel = localModel * Matrix4.CreateTranslation(deltaX * sensitivity, -deltaY * sensitivity, 0);

                            _camera.Position -= _camera.Right * deltaX * sensitivity;
                            _camera.Position -= _camera.Up * -deltaY * sensitivity;
                            ClearColor(backgroundColor);
                            GL.BindVertexArray(_vertexArrayObject);
                            _shader.Use();
                            _shader.SetMatrix4("model", localModel);

                            _shader.SetMatrix4("view", _camera.GetViewMatrix());
                            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                            GL.DrawArrays(primitiveType, 0, vertexList._count);
                            
                            optkGL.SwapBuffers();

                            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                            //_camera.Yaw -= deltaX * sensitivity;
                            //_camera.Pitch += deltaY * sensitivity ; // Reversed since y-coordinates range from bottom to top


                            #endregion
                        }

                    }
                    if (e.Button == MouseButtons.Left && leftMouseButtonDown)
                    {
                        DateTime newDateTime = DateTime.Now;
                        TimeSpan timeDifference = newDateTime - dateTime;
                        double secondsDifference = timeDifference.TotalSeconds;

                        if (secondsDifference < 0.01)
                        {
                            return;
                        }
                        #region   左键旋转  物体移动
                       
                        localModel = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(deltaX * rotatefactor)) *
                                Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(deltaY * rotatefactor))* localModel;
                        if (pickState == PickState.No_pick)
                        {
                            Render();
                        }
                        else
                        {
                            if (pickPointCount == PickPointCount.One)
                            {
                                PickupPoint(lastPickPoint, 0f, -0f);
                            }
                            else if (pickPointCount == PickPointCount.Two)
                            {
                                PickupLine(lastPickPoint, 0f, -0f);
                            }

                        }

                        #endregion

                        #region    摄像头移动

                        //    _camera.Position -= _camera.Right * deltaX * mousechange; // RIGHT
                        //    _camera.Position += _camera.Up * deltaY * mousechange; // RIGHT
                        //    Render();
                        //}
                        #endregion

                    }

                }

            }

        }
       
        public float FovChange(float change)
        {
            Matrix4 changeMatrix = localModel * _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
            vertexList.changeViwVertexDic = Tool.AfterConversiondDic(vertexList.originVertexDic, changeMatrix, true);
            if (vertexList.changeViwVertexDic.Count >0)
            {
                int farIndex = vertexList.changeViwVertexDic.OrderBy(kvp => kvp.Value.Z).FirstOrDefault().Key;
                int nearIndex = vertexList.changeViwVertexDic.OrderByDescending(kvp => kvp.Value.Z).FirstOrDefault().Key;
                float far = vertexList.originVertexDic[farIndex].Z;
                float near = vertexList.originVertexDic[nearIndex].Z;

                far = Math.Abs(_camera.Position.Z - far) > Math.Abs(_camera.Position.Z - near) ? far : near;
                if (_camera.Position.Z - far > 0)
                {
                    while (_camera.Position.Z - change - far < 0)
                    {
                        change = change * 0.1f;
                    }
                }
                if (_camera.Position.Z - far < 0)
                {
                    while (_camera.Position.Z - change - far > 0)
                    {
                        change = change * 0.1f;
                    }
                }
                float variation = Math.Abs(_camera.Position.Z - far);
                int dimension = (int)Math.Floor(Math.Log10(Math.Abs(variation)));
                sensitivity = (float)(0.02f * Math.Pow(10,dimension));
            }
           
            
            return change;
        }

        public float FarScreen()
        {
            Vector3D normal =Tool.To3D( _camera.Position - _camera.Front);
            List<Vector3D> bvhList = Tool.BVH(vertexList._maxX, vertexList._minX, vertexList._maxY, vertexList._minY, vertexList._maxZ, vertexList._minZ);

            double fardistance = 0;
            Vector3D farvector3D = new Vector3D();
            Plane farPlane = new Plane();

            foreach (Vector3D vector3D in bvhList)
            {
                Plane plane = new Plane(normal, vector3D);
                double distance = Plane.CalculateDistance(Tool.To3D(_camera.Position), plane.Normal, plane.D);
                if (distance >fardistance)
                {
                    fardistance = distance;
                    farvector3D = vector3D;
                    farPlane = plane;
                }
            }
            return farvector3D.Z;
        }

       

        private void Chart_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            FarScreen();
            if (_camera != null)
            {
                //滚轮事件
                //float dimension = Tool.FovChange(e.Delta,_camera.Fov);
                //_camera.Fov -= e.Delta / dimension;//摄像机改变

                //float scale = Math.Abs(e.Delta / 100 > 0 ? e.Delta / 100 * _scalingEnlargeFactor : e.Delta / 100 * _scalingReduceFactor);
                //localModel = localModel * Matrix4.CreateScale(scale);

                float frontLength = e.Delta / 100 * _scalingPositionFactor;

                //指数函数变换，变换过大
                //frontX += cameraSpeed * frontLength;
                //float y = Tool.SolveForY(frontX, vertexList._maxZ, 1,1);
                //float change =   _camera.Position.Z- Tool.SolveForY(frontX, vertexList._maxZ, 1, 1);
                // _camera.Position += _camera.Front * (change);

                //步进值逐减，但不确定最远点
                float change = cameraSpeed * frontLength;
                float i = FovChange(change);
                _camera.Position += _camera.Front * i;
                //_camera.Position += _camera.Front * cameraSpeed * frontLength;
                _firstMove = true;
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

        DateTime dateTime = DateTime.Now;
        private void Chart_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                leftMouseButtonDown = true;
                dateTime = DateTime.Now;
            }
            if (e.Button == MouseButtons.Right)
            {
                rightMouseButtonDown = true;
            }
        }
        private void Chart_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && pickState != PickState.No_pick && leftMouseButtonDown)
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

                    Vector3D vector3D = new Vector3D(depthin.X / depthin.W, depthin.Y / depthin.W, depthin.Z / depthin.W);
                    Vector3D vector3Dnear = Tool.ContainsVector3D(vertexList._vector3s, vector3D);
                    Vector3 vector3near = new Vector3(vector3Dnear.X, vector3Dnear.Y, vector3Dnear.Z);
                    if (!Tool.IsZero(vector3near, Tool.unReach))
                    {
                        this.label.Content = $@"x:{vector3Dnear.X};
y:{vector3Dnear.Y};
z:{vector3Dnear.Z}";
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
                    if (id < vertexList._vertices.Length / 3 && id > 0)
                    {
                        Vector3 depcolor = new Vector3(vertexList._vertices[3 * id], vertexList._vertices[3 * id + 1], vertexList._vertices[3 * id + 2]);
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
                _firstMove = true;

                #region
                //1.进行拾取操作 先将所有点转换成变换后的坐标，取与捕获点最近的四个，再从中找出Z值最小的一个作为捕获点Z值，然后通过逆矩阵求出原来的点
                if (pickState == PickState.Screeen_pick)
                {
                    //比较屏幕中的最近点
                    Matrix4 changeMatrix = localModel * _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
                   // vertexList.changeVertexDic = Tool.AfterConversiondDic(vertexList.originVertexDic, changeMatrix);
                    vertexList.changeViwVertexDic = Tool.AfterConversiondDic(vertexList.originVertexDic, changeMatrix,true);
                    int pointIndex = Tool.FindNearestPointOnDictory(vertexList.changeViwVertexDic, new Vector3D(ndcX, ndcY, 1));


                    if (pointIndex != -1)
                    {
                        Vector3D vector3nearPoint = vertexList.originVertexDic[pointIndex];
                        this.label.Content = $@"x:{vector3nearPoint.X};
y:{vector3nearPoint.Y}; 
z:{vector3nearPoint.Z}";
                        if (pickPointCount == PickPointCount.One)
                        {
                            PickupPoint(new Vector3(vector3nearPoint.X, vector3nearPoint.Y, vector3nearPoint.Z), 0, 0, true);
                        }
                        if (pickPointCount == PickPointCount.Two)
                        {
                            PickupLine(new Vector3(vector3nearPoint.X, vector3nearPoint.Y, vector3nearPoint.Z), 0, 0, true);
                        }

                    }
                    else
                    {
                        this.label.Content = $@"no pick";
                    }
                }

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
                #endregion
            }
            leftMouseButtonDown = false;
            rightMouseButtonDown = false;

        }

        private void PickupPoint(Vector3 vector, float mouseX = 0, float mouseY = 0, bool isNewPoint = false)
        {
            try 
            {
                ClearColor(backgroundColor);


            float centerndcX = 0f;
            float centerndcY = 0f;
            float centerndcZ = 0f;


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
            GL.BufferData(BufferTarget.ArrayBuffer, vertexList._vertices.Length * sizeof(float), vertexList._vertices, BufferUsageHint.StaticDraw);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);
            //GL.DrawArraysInstanced(PrimitiveType.Points, 0, _vertices.Length, 1);
            GL.DrawArrays(PrimitiveType.Points, 0, vertexList._vertices.Length / 3);

            if (Tool.IsZero(lastPickPoint, Tool.unReach) && Tool.IsZero(vector, Tool.unReach))
            {
                optkGL.SwapBuffers();
                return;
            }

            //文本框
            string label = $@"Point
X: {SizeFormat(vector.X)}
Y: {SizeFormat(vector.Y)}
Z: {SizeFormat(vector.Z)}";
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
            GL.PointSize(5f);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            GL.PointSize(1f);
            lastPickPoint = vector;
            optkGL.SwapBuffers();
            }
            catch
            {
            }
        }

        private void PickupLine(Vector3 vector, float mouseX = 0, float mouseY = 0, bool IsNewPoint = false)
        {
            ClearColor(backgroundColor);

            float centerndcX = 0f;
            float centerndcY = 0f;
            float centerndcZ = 0f;


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
                    ResizePoint();
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
            Vector3 centerRightT = new Vector3(centerndcX + 5 * hight, centerndcY + hight, centerndcZ);
            Vector3 centerRightB = new Vector3(centerndcX + 5 * hight, centerndcY - hight, centerndcZ);
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
            GL.BufferData(BufferTarget.ArrayBuffer, vertexList._vertices.Length * sizeof(float), vertexList._vertices, BufferUsageHint.StaticDraw);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);
            //GL.DrawArraysInstanced(PrimitiveType.Points, 0, _vertices.Length, 1);
            GL.DrawArrays(PrimitiveType.Points, 0, vertexList._vertices.Length / 3);

            if (linePoints.Count == 0 && Tool.IsZero(vector, Tool.unReach))
            {
                optkGL.SwapBuffers();
                return;
            }
            Vector3 changeVector3 = TransformVector(vector, GetMVP());
            if (linePoints.Count < 2)
            {
                if ((linePoints.Count == 1 && linePoints[0] == vector) || Tool.IsZero(vector, Tool.unReach))
                {
                }
                else
                {
                    linePoints.Add(vector);
                }

            }

            foreach (var item in linePoints)
            {
                RendPoint(TransformVector(item, GetMVP()));
            }

            if (linePoints.Count == 2)
            {
                Vector3 changeVector3First = TransformVector(linePoints[0], GetMVP());
                Vector3 changeVector3Second = TransformVector(linePoints[1], GetMVP());


                Vector3 vector3Mid = TransformVector(Tool.CalculateMidpoint(linePoints[0], linePoints[1]), GetMVP());

                float[] _vertices3 = {
                changeVector3First.X,changeVector3First.Y,changeVector3First.Z,
               changeVector3Second.X,changeVector3Second.Y,changeVector3Second.Z,
               vector3Mid.X,vector3Mid.Y,vector3Mid.Z,
               nearNode.X,nearNode.Y,nearNode.Z
               };

                //文本框
                string text = $@"Distance: {SizeFormat(Vector3.Distance(linePoints[0], linePoints[1]))}
▲X: {SizeFormat(linePoints[0].X - linePoints[1].X)},▲XY: {SizeFormat(Tool.SquareRootSum(linePoints[0].X, linePoints[1].X, linePoints[0].Y, linePoints[1].Y))}
▲Y: {SizeFormat(linePoints[0].Y - linePoints[1].Y)},▲XZ: {SizeFormat(Tool.SquareRootSum(linePoints[0].X, linePoints[1].X, linePoints[0].Z, linePoints[1].Z))} 
▲Z: {SizeFormat(linePoints[0].Z - linePoints[1].Z)},▲ZY: {SizeFormat(Tool.SquareRootSum(linePoints[0].Z, linePoints[1].Z, linePoints[0].Y, linePoints[1].Y))}
";
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
                GL.PointSize(5f);
                GL.DrawArrays(PrimitiveType.Lines, 0, 4);
                GL.PointSize(1f);
                lastPickPoint = vector;

            }
            optkGL.SwapBuffers();

        }

        private void ResizeParam()
        {
            pickState = PickState.No_pick;
            this.getPoint.Content = "未取点模式";
            lastPickPoint = new Vector3(Tool.unReach, Tool.unReach, Tool.unReach);
            linePoints.Clear();
            _firstMove = true;
            this.label.Content = $@"";
        }
        public string SizeFormat(float value)
        {
            string result = Math.Round(value, decimalPlaces).ToString("0.000000");
            result = value > 0 ? result.PadRight(14, ' ') : result.PadRight(12, ' ');
            return result;
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
            var bmp2 = Tool.GenerateBitmapFromString(text, fontPath, 12f, Color.Blue);
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

        private readonly uint[] _indicebhv =
        {
            0, 1,2,
            1, 2,3,
            4, 5, 6,
            5, 6,7,
            0,1,4,
            1,4,5,
            2,3,6,
            3,6,7,
            0,2,4,
            2,4,6,
            1,3,5,
            3,5,7
        };
        public void RenerBHV()
        {
            List<Vector3D> bvhList = Tool.BVH(vertexList._maxX, vertexList._minX, vertexList._maxY, vertexList._minY, vertexList._maxZ, vertexList._minZ);
            float[] _vertices4 = Tool.Vector3iToIntArray(bvhList);
            _shader = new Shader(vertPickLineShader, GetfragPickPointShader("0.6,0.5,0.52,1"), 0);
            _shader.Use();
            var _vertexBufferObject5 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject5);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices4.Length * sizeof(float), _vertices4, BufferUsageHint.StaticDraw);

            var _vertexArrayObject5 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject5);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indicebhv.Length * sizeof(uint), _indicebhv, BufferUsageHint.StaticDraw);
            SetMVP();
            GL.PointSize(5);
            GL.DrawElements(PrimitiveType.Triangles, _indicebhv.Length, DrawElementsType.UnsignedInt, 0);

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

                GL.DrawArrays(primitiveType, 0, vertexList._count);
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
                _camera.Position += _camera.Front * cameraSpeed * scalingFactor;
                // localModel = localModel * Matrix4.CreateScale(scalingFactor);
                //_camera.Fov -= scalingFactor;//改变摄像机角度
                SetMVP();
                GL.DrawArrays(primitiveType, 0, vertexList._count);

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
                GL.DrawArrays(primitiveType, 0, vertexList._count);
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
                GL.DrawArrays(primitiveType, 0, vertexList._count);
                optkGL.SwapBuffers();
            }
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
        public Vector3 GetcoordinateOrigin()
        {
            Vector4 oldOrigin = new Vector4(0, 0, 0, 1);
            oldOrigin = oldOrigin * localModel * _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
            Vector3 newOrigin = new Vector3(oldOrigin.X / oldOrigin.W, oldOrigin.Y / oldOrigin.W, oldOrigin.Z / oldOrigin.W);
            return newOrigin;
        }
        public Vector3 GetModelOrigin()
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


        #region  着色器脚本 
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


        public string GetfragPickPointShader(string color1)
        {
            return $@"
            #version 330
            out vec4 outputColor;
            void main()
            {{
                outputColor = vec4({color1});
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

        public readonly static string vertModelShader = $@"
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aNormal;
        layout (location = 2) in vec2 aTexCoords;

        out vec2 TexCoords;

        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;

        void main()
        {{
        TexCoords = aTexCoords;    
        gl_Position =  vec4(aPos, 1.0) * model * view * projection ;
        }}
         ";


        public readonly static string fragModelShader = $@"
        #version 330 core
        out vec4 FragColor;

        in vec2 TexCoords;
          
        uniform sampler2D material_texture_diffuse1;

        void main()
        {{    
        FragColor = texture(material_texture_diffuse1, TexCoords);
        }}  
        ";

        #endregion


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
                ResizePoint();
                PickupPoint(lastPickPoint, 0, 0);
            }
            if (pickPointCount == PickPointCount.Two)
            {
                this.PointName.Content = "直线";
                ResizePoint();
                PickupLine(lastPickPoint, 0, 0);
            }
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width > 0 && e.PreviousSize.Height > 0)
            {
                double chartWidth, chartHeight;
                chartWidth = e.NewSize.Width * chart.Width / e.PreviousSize.Width;
                chartHeight = e.NewSize.Height * chart.Height / e.PreviousSize.Height;
                chart.Width = chartWidth;
                chart.Height = chartHeight;

                //this.InvalidateVisual();

                UpdateLayout();
                GL.Viewport(0, 0, (int)chartWidth, (int)chartHeight);
                //ClearColor(backgroundColor);
                Render();
            }
        }
    }
}
