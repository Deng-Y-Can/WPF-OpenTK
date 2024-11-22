using Assimp;
using LearnOpenTK.Common;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Drawing;
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
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector3D = Assimp.Vector3D;
using Vector4 = OpenTK.Mathematics.Vector4;
using Window = System.Windows.Window;

namespace WpfApp
{
    /// <summary>
    /// AssimpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AssimpWindow : Window
    {
        public AssimpWindow()
        {
            InitializeComponent();
        }
        private LearnOpenTK.Common.Camera _camera;
        private int _vertexBufferObject;

        private int _vertexArrayObject;
        private int _elementBufferObject;

        private Shader _shader;
        private int verts;
        private PrimitiveType primitiveType;
        private float height;

        private GLControl optkGL;

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
       
        bool isGetPhint = false;
        void glc_Load(object sender, EventArgs e)
        {
            // 启用深度测试
            //GL.Enable(EnableCap.DepthTest);
            //禁用深度缓冲的写入  深度掩码(Depth Mask)设置
            // GL.DepthMask(false);

            // 设置深度测试函数
            // GL.DepthFunc(DepthFunction.Less);

            // 启用背面填充
            //GL.Enable(EnableCap.CullFace);

            // GL.Enable(EnableCap.StencilTest);//模板测试
            //GL.StencilMask(0xFF);//模板掩码 
            // 设置视口大小
            GL.Viewport(0, 0, 500, 500);

            // GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f); // 设置清除颜色为灰色

        }

        float[] _vertices;

        private float maxX;
        private float minX;
        private float maxY;
        private float minY;
        private float maxZ;
        private float minZ;
        public void Import()
        {
            // 创建Assimp场景
            var importer = new AssimpContext();
            var scene = importer.ImportFile("E:\\home\\openTK\\OpenTKUser\\WpfOpenTKApp\\Model\\candy.ply", PostProcessSteps.None);

            if (scene.HasMeshes)
            {
                foreach (var mesh in scene.Meshes)
                {
                    if (mesh.Vertices.Count() > 0)
                    {
                        var a = mesh.Vertices;
                        List<Vector3D> vector3s = a;
                        _vertices = Tool.Vector3iToIntArray(vector3s);

                        List<Face> face3s = mesh.Faces;
                        int[] _indices = Tool.FaceToIntArray(face3s);
                        primitiveType = (PrimitiveType)mesh.PrimitiveType;
                        primitiveType = PrimitiveType.Points;

                        verts = _vertices.Length / 3;
                        // verts = 1;
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
                        float[] _vertices2 =
        {
             0.5f,  0.5f, 0.0f, // top right
             0.5f, -0.5f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, // top left
        };
                        int[] _indices2 =
        {
            // Note that indices start at 0!
            0, 1, 3, // The first triangle will be the top-right half of the triangle
            1, 2, 3  // Then the second will be the bottom-left half of the triangle
        };

                        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f); // 设置清除颜色为灰色
                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                        //int[] buffer = new int[1];   //可以一次创建多个缓存对象
                        //GL.CreateBuffers(1, buffer);
                        //GL.BindBuffer(BufferTarget.ArrayBuffer, buffer[0]);
                        //GL.NamedBufferStorage(buffer[0], _vertices.Length * sizeof(float), _vertices, BufferStorageFlags.MapReadBit);向缓存输入数据
                        //GL.NamedBufferStorage(buffer[0], _vertices2.Length * sizeof(float), 0, BufferStorageFlags.DynamicStorageBit);//对顶点坐标、颜色、纹理储存在同一buffer中
                        //GL.NamedBufferSubData(buffer[0], 0, _vertices2.Length * sizeof(float), _vertices2);
                        //GL.NamedBufferSubData(buffer[0],  _vertices2.Length * sizeof(float), _vertices3.Length * sizeof(float), _vertices3);
                        //GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer, 0, 1, 4);//复制缓存
                        //float[] _vertice4 = new float[9];
                        //GL.GetNamedBufferSubData(buffer[0], 0, _vertices2.Length * sizeof(float), _vertice4);//将缓存中的数据再读出来
                        //GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadWrite);
                        //GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                        //GL.DeleteBuffer(buffer[0]);
                        //GL.InvalidateBufferData(buffer[0]);//丢弃缓存


                        _vertexBufferObject = GL.GenBuffer();
                        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

                        _vertexArrayObject = GL.GenVertexArray();
                        GL.BindVertexArray(_vertexArrayObject);
                        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
                        GL.EnableVertexAttribArray(0);

                        //_elementBufferObject = GL.GenBuffer();
                        //GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
                        //We also upload data to the EBO the same way as we did with VBOs.
                        //GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

                        _shader = new Shader(vertMainShader, fragMainShader, 0);
                        _shader.Use();


                        _camera = new Camera(Vector3.UnitZ * height, 1.2f);

                        // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
                        //System.Windows.MessageBox.Show("The model has been successfully imported!");

                        //var model = Matrix4.Identity;
                        //_shader.SetMatrix4("model", model);
                        //_shader.SetMatrix4("view", _camera.GetViewMatrix());
                        //_shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                        //GL.DrawArrays(primitiveType, 0, verts);

                        //glc.SwapBuffers();
                    }



                    //Console.WriteLine($"Mesh name: {mesh.Name}");
                    // 处理网格数据
                }
            }
            else
            {
                Console.WriteLine("No meshes found in the file.");
            }

            // 释放资源
            importer.Dispose();

        }


        private Matrix4 localModel = Matrix4.Identity;
        public Matrix4 ModelChange(CoordinateAxis coordinateAxis)
        {
            var model = Matrix4.Identity;
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
                // _time += 5;
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                GL.BindVertexArray(_vertexArrayObject);
                _camera = new Camera(Vector3.UnitZ * height, 1.2f);
                _shader = new Shader(vertMainShader, fragMainShader, 0);
                _shader.Use();

                var model = ModelChange(coordinateAxis);
                localModel = model;
                _shader.SetMatrix4("model", model);
                _shader.SetMatrix4("view", _camera.GetViewMatrix());
                _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                GL.DrawArrays(primitiveType, 0, verts);
                optkGL.SwapBuffers();
            }
        }

        public enum CoordinateAxis
        {
            Z_positive,
            Z_negative,
            X_positive,
            X_negative,
            Y_positive,
            Y_negative,
        }
       

        const float cameraSpeed = 0.15f;
        const float sensitivity = 0.05f;  //右键平移因子
        float change = 3.0f;
        float mousechange = 0.045f;//左键旋转因子


       
        private double _time;

        public void Render()
        {
            if (_shader != null)
            {
                // _time += 5;
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                GL.BindVertexArray(_vertexArrayObject);


                _shader.Use();

                //var model = Matrix4.Identity;
                //localModel= model;
                _shader.SetMatrix4("model", localModel);
                _shader.SetMatrix4("view", _camera.GetViewMatrix());
                _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());


                GL.DrawArrays(primitiveType, 0, verts);
                #region   常用图元绘制参数
                //设置点大小
                //GL.Enable(EnableCap.ProgramPointSize);
                //GL.PointSize(1.5f);
                //设置线宽
                //GL.LineWidth(1.2f);
                #endregion

                optkGL.SwapBuffers();
            }

        }
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

        private bool _firstMove = true;

        private Vector2 _lastPos;
        private float rotatefactor = 0.2f;
        private void Chart_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_camera != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (isGetPhint)
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
                            PickupPoint(lastPickPoint, deltaX * 0.1f, -deltaY * 0.1f);
                        }
                    }
                    else
                    {
                        //右键平移
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
                            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
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
                if (e.Button == MouseButtons.Left && !isGetPhint)
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

                        localModel = localModel * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(deltaX * rotatefactor)) * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(deltaY * rotatefactor));//* Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians((deltaX+deltaY)/2));

                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                        GL.BindVertexArray(_vertexArrayObject);
                        _shader.Use();
                        _shader.SetMatrix4("model", localModel);
                        //localModel = model;
                        _shader.SetMatrix4("view", _camera.GetViewMatrix());
                        _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                        GL.DrawArrays(primitiveType, 0, verts);
                        optkGL.SwapBuffers();
                    }
                    #endregion

                    #region    摄像头移动
                    //float mouseX = (float)e.X;
                    //float mouseY = (float)e.Y;
                    //if (_firstMove) // This bool variable is initially set to true.
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

                    //    _camera.Position -= _camera.Right  * deltaX * mousechange; // RIGHT
                    //    _camera.Position += _camera.Up  * deltaY * mousechange; // RIGHT

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
                Render();
            }

        }

        private void chart_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            optkGL.Focus();
        }


        private float _scalingFactor = 20;
        public void Scaling(float scalingFactor)
        {
            if (_shader != null)
            {
                // _time += 5;
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                GL.BindVertexArray(_vertexArrayObject);
                _shader.Use();
                _camera.Fov -= scalingFactor;
                _shader.SetMatrix4("model", localModel);
                _shader.SetMatrix4("view", _camera.GetViewMatrix());
                _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                GL.DrawArrays(primitiveType, 0, verts);

                optkGL.SwapBuffers();
            }
        }

       


        //旋转
        private float _radians = 0;
        public void ModelRotation(string axis, float radians)
        {

            if (_shader != null)
            {
                // _time += 5;
                var model = Matrix4.Identity;
                switch (axis)
                {
                    case "X":
                        model = model * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_radians += radians));
                        break;


                    case "Y":
                        model = model * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_radians += radians));
                        break;

                }
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                GL.BindVertexArray(_vertexArrayObject);
                _shader.Use();
                _shader.SetMatrix4("model", model);
                localModel = model;
                _shader.SetMatrix4("view", _camera.GetViewMatrix());
                _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                GL.DrawArrays(primitiveType, 0, verts);
                optkGL.SwapBuffers();
            }
        }
        
        int _vertexBufferObject2;
        int _vertexArrayObject2;
        private void MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isGetPhint)
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

                //3.以近平面和远平面组成的向量与点进行距离计算，取最近的一个点
                Vector4 nearPonint = new Vector4(ndcX, ndcY, -1, 1) * inverseMatrix;
                Vector4 farPonint = new Vector4(ndcX, ndcY, 1, 1) * inverseMatrix;
                Vector3 near = new Vector3(nearPonint.X / nearPonint.W, nearPonint.Y / nearPonint.W, nearPonint.Z / nearPonint.W);
                Vector3 far = new Vector3(farPonint.X / farPonint.W, farPonint.Y / farPonint.W, farPonint.Z / farPonint.W);
                List<Vector3> points = Tool.ArrayToList(_vertices, 0);
                Vector3 vector = ObjectPicker.FindNearPointsOnLine(points, near, far, out float errorvalue);
                Vector4 tranfV4 = new Vector4(vector.X, vector.Y, vector.Z, 1) * localModel * viewMatrix * projectionMatrix;
                Vector3 tranfvector = new Vector3(ndcX, ndcY, tranfV4.Z / tranfV4.W);
                this.label.Content = $@"x:{vector.X};
y:{vector.Y};
z:{vector.Z}
误差：{errorvalue}";


                PickupPoint(tranfvector);

                //                // 2.读取深度值
                //                GL.Enable(EnableCap.DepthTest);
                //                GL.ReadBuffer(ReadBufferMode.Aux0);
                //                float[] depth = new float[1];
                //                GL.ClampColor(ClampColorTarget.ClampVertexColor, ClampColorMode.False);
                //                GL.ReadPixels(x, glc.Height - y, 1, 1, PixelFormat.DepthComponent, PixelType.Float, depth);
                //                float depthZ = 1 / ((depth[0] / farplane) + (1 - depth[0]) / nearplane);
                //                Vector4 depthin = new Vector4(ndcX, ndcY, depth[0], 1) * inverseMatrix;
                //                Vector3 depnear = new Vector3(depthin.X / depthin.W, depthin.Y / depthin.W, depthin.Z / depthin.W);
                //                this.label.Content = $@"x:{depnear.X};
                //y:{depnear.Y};
                //z:{depnear.Z}";

                // 1.进行拾取操作 先将所有点转换成变换后的坐标，取与捕获点最近的四个，再从中找出Z值最小的一个作为捕获点Z值，然后通过逆矩阵求出原来的点
                //                List<Vector3> originalPoints = Tool.ArrayToList(_vertices, 0);
                //                List<Vector3> points = AfterConversion(originalPoints);
                //                List<Vector3> vector3nearfour = PointDistanceComparer.FindNearestFourPoints(new Vector3(ndcX, ndcY, 1), points);
                //                Vector3 nearFZ = vector3nearfour.OrderBy(v => v.Z).FirstOrDefault(); //Tool.FindNearPointsOnLine(points, nearvector, farvector);
                //                Vector4 ndcin = new Vector4(ndcX, ndcY, nearFZ.Z, 1) * inverseMatrix;
                //                Vector3 near = new Vector3(ndcin.X / ndcin.W, ndcin.Y / ndcin.W, ndcin.Z / ndcin.W);

                //                //比较最近点
                //                near = Tool.FindNearest3DPoint(originalPoints, near, 5,out double nearst);
                //                this.label.Content = $@"x:{near.X};
                //y:{near.Y};
                //z:{near.Z}
                //距离：{nearst}";

            }
            
        }
        private Vector3 lastPickPoint;
        private void PickupPoint(Vector3 vector, float mouseX = 0, float mouseY = 0)
        {

            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f); // 设置清除颜色为灰色
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            float centerndcX = 0f;
            float centerndcY = 0f;
            float centerndcZ = 0f;

            float hight = 0.05f;
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

            Vector4 centerPonint = new Vector4(centerndcX, centerndcY, centerndcZ, 1);// * inverseMatrix;
            Vector3 center = new Vector3(centerPonint.X / centerPonint.W, centerPonint.Y / centerPonint.W, centerPonint.Z / centerPonint.W);


            Vector4 centerPonintLeftT = new Vector4(centerndcX, centerndcY + hight, centerndcZ, 1);// * inverseMatrix;
            Vector3 centerLeftT = new Vector3(centerPonintLeftT.X / centerPonintLeftT.W, centerPonintLeftT.Y / centerPonintLeftT.W, centerPonintLeftT.Z / centerPonintLeftT.W);

            Vector4 centerPonintRightT = new Vector4(centerndcX + 2 * hight, centerndcY + hight, centerndcZ, 1);// * inverseMatrix;
            Vector3 centerRightT = new Vector3(centerPonintRightT.X / centerPonintRightT.W, centerPonintRightT.Y / centerPonintRightT.W, centerPonintRightT.Z / centerPonintRightT.W);
            Vector4 centerPonintRightB = new Vector4(centerndcX + 2 * hight, centerndcY - hight, centerndcZ, 1);// * inverseMatrix;
            Vector3 centerRightB = new Vector3(centerPonintRightB.X / centerPonintRightB.W, centerPonintRightB.Y / centerPonintRightB.W, centerPonintRightB.Z / centerPonintRightB.W);
            Vector4 centerPonintLeftB = new Vector4(centerndcX, centerndcY - hight, centerndcZ, 1);// * inverseMatrix;
            Vector3 centerLeftB = new Vector3(centerPonintLeftB.X / centerPonintLeftB.W, centerPonintLeftB.Y / centerPonintLeftB.W, centerPonintLeftB.Z / centerPonintLeftB.W);

            float[] _vertices2 = {
                vector.X,vector.Y,vector.Z,
               centerndcX,centerndcY,centerndcZ ,
               centerLeftT.X, centerLeftT.Y, centerLeftT.Z,
               centerRightT.X, centerRightT.Y, centerRightT.Z,
               centerRightB.X,centerRightB.Y,centerRightB.Z,
               centerLeftB.X,centerLeftB.Y,centerLeftB.Z,
               centerndcX,centerndcY,centerndcZ ,
               };
            _shader.Use();


            Matrix4 viewMatrix = _camera.GetViewMatrix();
            Matrix4 projectionMatrix = _camera.GetProjectionMatrix();
            //Matrix4 inverseMatrix = Matrix4.Invert(localModel * viewMatrix * projectionMatrix);
            _shader.SetMatrix4("model", localModel);
            _shader.SetMatrix4("view", viewMatrix);
            _shader.SetMatrix4("projection", projectionMatrix);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);
            //GL.DrawArraysInstanced(PrimitiveType.Points, 0, _vertices.Length, 1);
            GL.DrawArrays(PrimitiveType.Points, 0, verts);

            _vertexBufferObject2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject2);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices2.Length * sizeof(float), _vertices2, BufferUsageHint.StaticDraw);
            _vertexArrayObject2 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject2);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);

            _shader = new Shader(vertMainShader, fragPickShader, 1);

            GL.LineWidth(1.5f);
            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetMatrix4("view", Matrix4.Identity);
            _shader.SetMatrix4("projection", Matrix4.Identity);
            GL.DrawArrays(PrimitiveType.LineStrip, 0, _vertices2.Length / 3);

            lastPickPoint = vector;
            optkGL.SwapBuffers();
            _shader = new Shader(vertMainShader,fragMainShader, 1);
        }

        private List<Vector3> AfterConversion(List<Vector3> vector3s)
        {
            List<Vector3> result = new List<Vector3>();
            //List<Vector4> vector4s = new List<Vector4>();
            Matrix4 inverseMatrix = localModel * _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
            for (int i = 0; i < vector3s.Count(); i++)
            {
                Vector4 vector4 = new Vector4(vector3s[i].X, vector3s[i].Y, vector3s[i].Z, 1f) * inverseMatrix;
                Vector3 vector3 = new Vector3(vector4.X / vector4.W, vector4.Y / vector4.W, vector4.Z / vector4.W);
                result.Add(vector3);
                // vector4s.Add(vector4);
            }
            // Vector4 a = vector4s[590000];
            //Vector3 b= vector3s[590000];
            return result;
        }

        // 计算两个Vector3D之间的距离


        private string vertShader = $@"
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
        public string Getfrag(string color1, string color2, string color3, string axle, float[] waypoint)
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

        
        //六个视图
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.X_positive);
           
        }
        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.X_negative);
        }

        private void Button_Click_15(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.Y_positive);
        }

        private void Button_Click_16(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.Y_negative);
        }

        private void Button_Click_17(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.Z_positive);
        }

        private void Button_Click_18(object sender, RoutedEventArgs e)
        {
            PlaneSwitching(CoordinateAxis.Z_negative);
        }

        //放大缩小
        private void Button_Click_19(object sender, RoutedEventArgs e)
        {
            Scaling(_scalingFactor);
        }

        private void Button_Click_20(object sender, RoutedEventArgs e)
        {
            Scaling(-_scalingFactor);
        }
        //沿轴旋转
        private void Button_Click_21(object sender, RoutedEventArgs e)
        {
            ModelRotation("X", 30);
        }

        private void Button_Click_22(object sender, RoutedEventArgs e)
        {
            ModelRotation("Y", 30);
        }

        private void getPoint_Click(object sender, RoutedEventArgs e)
        {
            isGetPhint = !isGetPhint;
            this.getPoint.Content = isGetPhint == true ? "取消取点" : "取点";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            float[] arr = { minX + (maxX - minX) / 3, minX + (maxX - minX) * 2 / 3 };
            _shader = new Shader(vertShader, Getfrag("0,0,1,1", "0,1,0,1", "1,0,0,0", "x", arr), 1);
            _shader.Use();
            Render();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            float[] arr = { minY + (maxY - minY) / 3, minY + (maxY - minY) * 2 / 3 };
            _shader = new Shader(vertShader, Getfrag("0,0,1,1", "0,1,0,1", "1,0,0,0", "y", arr), 1);
            _shader.Use();
            Render();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Import();
        }

        public readonly static string vertMainShader = $@"
          #version 330 core

layout(location = 0) in vec3 aPosition;

//out vec3 texCoord;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{{
   //texCoord=aPosition;
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}}
         ";


        public readonly static string fragMainShader = $@"
#version 330
out vec4 outputColor;
//in vec3 texCoord;
void main()
{{
     outputColor = vec4(1.0, 1.0, 0.0, 1.0);
    //if(texCoord.z < 0.1)
    //    outputColor = vec4(0.5, 0.0, 0.0, 1.0);
    //else
    //     if(texCoord.z<0.4)
    //           outputColor = vec4(0.0, 1.0, 0.0, 1.0);
    //      else 
    //        outputColor = vec4(0.0, 0.0, 1.0, 1.0);
}}
         ";
        public readonly static string fragPickShader = $@"
#version 330
out vec4 outputColor;
//in vec3 texCoord;
void main()
{{
     outputColor = vec4(1.0, 0.0, 1.0, 1.0);
    //if(texCoord.z < 0.1)
    //    outputColor = vec4(0.5, 0.0, 0.0, 1.0);
    //else
    //     if(texCoord.z<0.4)
    //           outputColor = vec4(0.0, 1.0, 0.0, 1.0);
    //      else 
    //        outputColor = vec4(0.0, 0.0, 1.0, 1.0);
}}
         ";
    }
}
