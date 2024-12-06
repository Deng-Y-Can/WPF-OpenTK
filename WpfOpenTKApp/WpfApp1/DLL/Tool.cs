using Assimp;
using Assimp.Unmanaged;
using LearnOpenTK.Common;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing.Text;
using System.Security.Cryptography.X509Certificates;

namespace WpfApp
{
    public class Tool()
    {
        public static List<Vector3D> BVH(float maxX, float minX, float maxY, float minY, float maxZ, float minZ)
        {
            float k = 0f;
            maxX += k;
            maxY += k;
            maxZ += k;
            minX -= k;
            minY -= k;
            minZ -= k;
            List<Vector3D> bvhList = new List<Vector3D>();
            bvhList.Add(new Vector3D(minX, minY, minZ));
            bvhList.Add(new Vector3D(maxX, minY, minZ));
            bvhList.Add(new Vector3D(minX, maxY, minZ));
            bvhList.Add(new Vector3D(maxX, maxY, minZ));
            bvhList.Add(new Vector3D(minX, minY, maxZ));
            bvhList.Add(new Vector3D(maxX, minY, maxZ));
            bvhList.Add(new Vector3D(minX, maxY, maxZ));
            bvhList.Add(new Vector3D(maxX, maxY, maxZ));
            return bvhList;
        }
        public static Dictionary<int, Vector3D> AfterConversiondDic(Dictionary<int, Vector3D> originVertexDic, Matrix4 inverseMatrix,bool isVisible=false)
        {
            Dictionary<int, Vector3D> keyValueVertexList = new Dictionary<int, Vector3D>();

            foreach (KeyValuePair<int, Vector3D> kvp in originVertexDic)
            {
                Vector4 vector4 = new Vector4(kvp.Value.X, kvp.Value.Y, kvp.Value.Z, 1f) * inverseMatrix;
                Vector3D vector3D = new Vector3D(vector4.X / vector4.W, vector4.Y / vector4.W, vector4.Z / vector4.W);
                if (isVisible&&(Math.Abs(vector3D.X)>1|| Math.Abs(vector3D.Y) > 1|| Math.Abs(vector3D.Z) > 1))
                {
                    continue;
                }
                else
                {
                    keyValueVertexList.Add(kvp.Key, vector3D);
                } 
            }
            return keyValueVertexList;
        }

        public static List<Vector3D> AfterConversionList(List<Vector3D> vector3s, Matrix4 inverseMatrix)
        {
            List<Vector3D> result = new List<Vector3D>();
           // Matrix4 inverseMatrix = localModel * _camera.GetViewMatrix() * _camera.GetProjectionMatrix();
            for (int i = 0; i < vector3s.Count(); i++)
            {
                Vector4 vector4 = new Vector4(vector3s[i].X, vector3s[i].Y, vector3s[i].Z, 1f) * inverseMatrix;
                Vector3D vector3 = new Vector3D(vector4.X / vector4.W, vector4.Y / vector4.W, vector4.Z / vector4.W);
                result.Add(vector3);
            }
            return result;
        }
        public static float[] Vector3iToIntArray(List<Vector3D> vectorList)
        {
            float[] vertices = new float[vectorList.Count() * 3];
            int index = 0;
            for (int i = 0; i < vectorList.Count(); i++)
            {
                vertices[index] = vectorList[i].X;
                index++;
                vertices[index] = vectorList[i].Y;
                index++;
                vertices[index] = vectorList[i].Z;
                index++;
            }
            return vertices;
        }

        public static Dictionary<int, Vector3D> Vector3iToDictionary(List<Vector3D> vectorList )
        {
            Dictionary<int, Vector3D> keyValueVertexList =new Dictionary<int, Vector3D>();
            int index = 1;
           
            for (int i = 0; i < vectorList.Count(); i++)
            {
                keyValueVertexList.Add(index, vectorList[i]);
                index++;
            }
            return keyValueVertexList;
        }

        public static int[] FaceToIntArray(List<Face> faceList)
        {
            int[] indices = new int[faceList.Count() * 3];
            int index = 0;
            for (int i = 0; i < faceList.Count(); i++)
            {
                indices[index] = faceList[i].Indices[0];
                index++;
                indices[index] = faceList[i].Indices[1];
                index++;
                indices[index] = faceList[i].Indices[2];
                index++;
            }
            return indices;
        }
        /// <summary>
        /// 查看最大绝对值
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float FindMaxAbsoluteValue(params float[] numbers)
        {
            if (numbers == null || numbers.Length == 0)
            {
                throw new ArgumentException("输入的数值不能为空");
            }

            float maxAbs = Math.Abs(numbers[0]);
            foreach (float num in numbers)
            {
                float absValue = Math.Abs(num);
                if (absValue > maxAbs)
                {
                    maxAbs = absValue;
                }
            }
            return maxAbs;
        }
        public static List<Vector3> ArrayToList(float[] vertices, int n)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 point;
            for (int i = 0; i < vertices.Length; i++)
            {
                point = new Vector3();
                point.X = vertices[i];
                i++;
                point.Y = vertices[i];
                i++;
                point.Z = vertices[i];
                i += n;
                points.Add(point);
            }
            return points;
        }
        /// <summary>
        /// 选择文件打开路径
        /// </summary>
        /// <returns></returns>
        public static string SelectFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\"; // 设置初始目录
                //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"; // 设置文件过滤器
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 获取选中文件的路径
                    return openFileDialog.FileName;
                }
            }
            return null; // 用户没有选择文件或者点击了取消
        }

        public static float unReach = -999999;
        /// <summary>
        /// 查看点是否在点集合中，或者附近
        /// </summary>
        /// <param name="list"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3D ContainsVector3D(List<Vector3D> list, Vector3D point)
        {
            Vector3D near = new Vector3D();
            double mindistance = 9999999999;
            foreach (Vector3D v in list)
            {
                if (v.Equals(point))//|| CalculateDistance3D(v, point) <2
                {
                    return near;
                }
                if (CalculateDistance3D(v, point) < mindistance)
                {
                    near = v;
                    mindistance = CalculateDistance3D(v, point);
                }

            }
            if (mindistance < 5)//兼容误差
            {
                return near;
            }
            return new Vector3D(unReach, unReach, unReach);
        }
        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double CalculateDistance3D(Vector3D point1, Vector3D point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.Z - point1.Z, 2));
        }


        /// <summary>
        /// 查看最近的一个点
        /// </summary>
        /// <param name="center"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 CalculateNearestPointOnList(List<float> center, Vector3 point)
        {
            Vector3 nearestPoint = new Vector3();
            float length = -1;
            for (int i = 0; i < center.Count; i++)
            {
                Vector3 vector3 = new Vector3(center[i], center[i + 1], center[i + 2]);
                float f = Vector3.DistanceSquared(point, vector3);
                if ((length >= 0 && f < length) || length < 0)
                {
                    nearestPoint = vector3;
                    length = f;
                }
                i += 2;
            }

            return nearestPoint;
        }
        /// <summary>
        /// 生成文字纹理
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontFilePath"></param>
        /// <param name="fontSize"></param>
        /// <param name="textColor"></param>
        /// <returns></returns>
        public static Bitmap GenerateBitmapFromString(string text, string fontFilePath, float fontSize, System.Drawing.Color textColor)
        {
            //text += "。";//最后一个字不显示，暂处理
            // 创建一个新的Bitmap，初始大小设为1x1像素
            Bitmap bitmap = new Bitmap(1, 1);
            Graphics graphics = Graphics.FromImage(bitmap);
            try
            {
                // 应用字体
                PrivateFontCollection pfc = new PrivateFontCollection();
                pfc.AddFontFile(fontFilePath);
                Font font = new Font(pfc.Families[0], fontSize);

                // 测量文本大小
                SizeF textSize = graphics.MeasureString(text, font);
                int width = (int)textSize.Width;
                int height = (int)textSize.Height;
                width = width < 1 ? 1 : width;
                height = height < 1 ? 1 : height;
                // 创建实际大小的Bitmap
                bitmap = new Bitmap(bitmap, new Size(width, height));
                graphics = Graphics.FromImage(bitmap);

                // 设置文字格式（例如居中对齐）
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.None;
                format.FormatFlags = StringFormatFlags.NoWrap;

                //Brush redBrush = new SolidBrush(System.Drawing.Color.Red);
                //Brush greenBrush = new SolidBrush(System.Drawing.Color.Green);
                //graphics.DrawString("Hello, Red!", new Font(FontFamily.GenericSerif, 12), redBrush, new RectangleF(0, 0, width, height), format);
                //graphics.DrawString("Hello, Green!", new Font(FontFamily.GenericSerif, 12), greenBrush, new RectangleF(0, height, width, height), format);

                // 使用Graphics对象来填充背景颜色
                graphics.FillRectangle(new SolidBrush(System.Drawing.Color.Orange), 0, 0, width, height);

                // 绘制文本
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                graphics.DrawString(text, font, new SolidBrush(textColor), new RectangleF(0, 0, width, height), format);
            }
            finally
            {
                graphics.Dispose();
            }

            return bitmap;
        }
        /// <summary>
        /// 取两点的中点
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector3 CalculateMidpoint(Vector3 A, Vector3 B)
        {
            float midX = (A.X + B.X) / 2;
            float midY = (A.Y + B.Y) / 2;
            float midZ = (A.Z + B.Z) / 2;
            return new Vector3(midX, midY, midZ);
        }
        /// <summary>
        /// 判断点的值是否全等于一个值
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsZero(Vector3 vector, float value)
        {
            bool result = false;
            if (vector.X == value && vector.Y == value && vector.Z == value)
            {
                return true;
            }
            return result;
        }
        /// <summary>
        /// 当视角无限接近于1和90度时，减小步进值的量纲，以达到无线接近的目的
        /// </summary>
        /// <param name="change"></param>
        /// <param name="fov"></param>
        /// <returns></returns>
        public float FovChange(int change, float fov)
        {
            float variation = change > 0 ? fov - 1 : 90 - fov;
            float fovSacle = 1;
            int dimension = (int)Math.Floor(Math.Log10(Math.Abs(variation)));
            fovSacle = (float)Math.Pow(10, 3 - dimension);
            return fovSacle;
        }
        /// <summary>
        /// 自变量于因变量的关系属于分段函数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float SacleFuction(float x)
        {
            float y = 1;
            if (x > 0)
            {
                y = 1 + x;
            }
            else if (x < 0)
            {
                y = (x + 1) / x;
            }
            else
            {
                return 1;
            }
            return y;
        }

        public static float SquareRootSum(float Xa, float Xb, float Ya, float Yb)
        {
            float sumOfSquares = (Xa - Ya) * (Xa - Ya) + (Xb - Yb) * (Xb - Yb);
            return (float)Math.Sqrt(sumOfSquares);
        }

        // 已知y求x
        // 已知x求y
        public static float SolveForY(float x, float a, double b, double c)
        {
            // y = a + b / (e(cx))
            double result = a + b / Math.Exp(c * x);
            return (float)result;
        }

        // 已知y求x（需要注意，这个方法可能返回多个解，因为y=a+b/(e(cx))不是单调函数）
        public static float SolveForX(float y, float a, double b, double c)
        {
            // y = a + b / (e(cx))
            // 改写为: b / (e(cx)) = y - a
            // 进一步改写为: e(cx) = b / (y - a)
            // 最后得到: x = ln(b / (y - a)) / c

            if (y <= a)
            {
                // 当y <= a时，方程无解，因为e(cx)总是正的，所以b / (e(cx))不可能等于或小于0
                return 0;
            }

            double result = Math.Log(b / (y - a)) / c;
            return (float)result;
        }


        public static Vector3D FindNearestPointOnList(List<Vector3D> points, Vector3D P)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("点集合不能为空");
            }

            Vector3D nearestPoint = points[0];
            double minDistance = DistanceToScreen(points[0],P);

            foreach (var point in points)
            {
                double distance = DistanceToScreen(points[0], P);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }
        public static int FindNearestPointOnDictory(Dictionary<int, Vector3D> points, Vector3D P)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("点集合不能为空");
            }
            int index = -1;
            float minz = 1;
            float maxz = -1;
            Vector3D near = new Vector3D();
            double minDistance = -Tool.unReach;
            foreach (KeyValuePair<int, Vector3D> kvp in points)
            {
                double distance = DistanceToScreen(kvp.Value, P);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    //near = kvp.Value;
                    index = kvp.Key;
                }
                //if (kvp.Value.X > maxz)
                //{
                //    maxz = kvp.Value.X;
                //}
                //if (kvp.Value.X < minz)
                //{
                //    minz = kvp.Value.X;
                //}
            }
            if (minDistance > 0.3)
            {
                return -1;
            }

            return index;
        }

        public static double DistanceToScreen(Vector3D other, Vector3D P)
        {
            return Math.Sqrt(Math.Pow(other.X - P.X, 2) + Math.Pow(other.Y - P.Y, 2));
        }

        public static Vector3 DTo3(Vector3D P)
        {
            return new Vector3(P.X, P.Y, P.Z);
        }

        public static Vector3D To3D( Vector3 P)
        {
            return new Vector3D(P.X, P.Y, P.Z);
        }

    }


    public class Plane
    {
        public Vector3D Normal { get; private set; }
        public float D { get; private set; }  //平面到原点的距离

        public Plane(Vector3D normal, Vector3D pointA)
        {
            Normal = normal;
            D = -(Normal.X * pointA.X + Normal.Y * pointA.Y + Normal.Z * pointA.Z);
        }

        public Plane()
        {
        }

        public override string ToString()
        {
            return $"{Normal.X}x + {Normal.Y}y + {Normal.Z}z + {D} = 0";
        }
        public static double CalculateDistance(Vector3D point, Vector3D planeNormal, double D)
        {
            // 计算点到平面的距离
            double numerator = Math.Abs(planeNormal.X * point.X + planeNormal.Y * point.Y + planeNormal.Z * point.Z + D);
            double denominator = Math.Sqrt(planeNormal.X * planeNormal.X + planeNormal.Y * planeNormal.Y + planeNormal.Z * planeNormal.Z);
            return numerator / denominator;
        }
    }


}
