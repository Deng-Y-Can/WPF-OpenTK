using Assimp;
using Assimp.Unmanaged;
using LearnOpenTK.Common;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using System.Security.Cryptography.X509Certificates;

namespace WpfApp
{
    public class Scene
    {
    }

    public class Model
    {
        public Model(string path)
        {
            LoadModel(path);
        }

        public void Draw(Shader shader)
        {
            for (int i = 0; i < meshes.Count(); i++)
            {
               // meshes[i].Draw(shader);
            }
        }

        private void LoadModel(string path)
        {
            var importer = new AssimpContext();
            var scene = importer.ImportFile($@"{path}", PostProcessSteps.None);
        }
        private List<Mesh> meshes;
        private string directory;
        private void ProcessNode()
        {
        }
        private Mesh ProcessMesh()
        {
            return new Mesh();
        }

        private List<TextureA> loadMaterialTextures()
        {
            return null;
        }
    }

    //顶点
    public struct Vertex
    {
        Vector3i Position;//位置
        Vector3i Normal;//法向量
        Vector3i TexCoords;//纹理坐标
    };

    //纹理
    public struct TextureA
    {
        int id;
        string type;
    };

    //网格
    public class Mesh2()
    {
        public List<Vertex> _vertices;
        public List<int> _indices;
        public List<TextureA> _textures;

        private GLControl glc;

        public Mesh2(List<Vertex> verticeL, List<int> indiceL, List<TextureA> textureL) : this()
        {
            this._vertices = verticeL;
            this._indices = indiceL;
            this._textures = textureL;
            SetupMesh();
        }
        public void Draw(Shader shader)
        {
        }

        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int EBO;
        private void SetupMesh()
        {
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            //GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count() * sizeof(float), _vertices[], BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//解析顶点
            GL.EnableVertexAttribArray(0);
        }


    }

    public class Color3D
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public Color3D(float r, float g, float b, float a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
        public string ToString()
        {
            return $@"{R.ToString()},{G.ToString()},{B.ToString()},{A.ToString()}";
        }
    }
    public class Tool2()
    {
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

        public static double Distance(Vector3D v1, Vector3D v2)
        {
            return Math.Sqrt(Math.Pow(v2.X - v1.X, 2) + Math.Pow(v2.Y - v1.Y, 2) + Math.Pow(v2.Z - v1.Z, 2));
        }

        // 找到与目标点最近的点
        public static Vector3D FindNearestPoint(Vector3D target, List<Vector3D> points)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Points list cannot be null or empty.");
            }

            Vector3D nearestPoint = points[0];
            double minDistance = Distance(target, nearestPoint);

            foreach (var point in points)
            {
                double currentDistance = Distance(target, point);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }




        public static bool IsOnLine(Vector3 a, Vector3 b, Vector3 point)
        {
            // 计算向量ab
            Vector3 ab = new Vector3(b.X - a.X, b.Y - a.Y, b.Z - a.Z);

            // 计算向量ap
            Vector3 ap = new Vector3(point.X - a.X, point.Y - a.Y, point.Z - a.Z);

            // 计算向量bp
            Vector3 bp = new Vector3(point.X - b.X, point.Y - b.Y, point.Z - b.Z);

            // 判断点是否在直线ab上
            float dotProductApBp = ap.X * bp.X + ap.Y * bp.Y + ap.Z * bp.Z;
            if (dotProductApBp == 0)
            {
                return true;
            }

            return false;


        }

        public static double Length(Vector3 a)
        {
            return Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        }
        public static float DistanceFromPointToLine(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 vecAB = pointB - pointA;
            Vector3 vecAC = pointC - pointA;

            float dotAB = Vector3.Dot(vecAB, vecAB);
            float dotAC = Vector3.Dot(vecAC, vecAB);

            float projectionLength = dotAC / dotAB;
            float a = -1;
            float b = (float)ThreeDimensionDistanceCalculator.DistanceFromPointToLine(pointC, pointA, pointB);

            if (projectionLength < 0)
            {
                a = Vector3.Distance(pointC, pointA);
                return Vector3.Distance(pointC, pointA);
            }
            else if (projectionLength > 1)
            {
                a = Vector3.Distance(pointC, pointA);
                return Vector3.Distance(pointC, pointB);
            }
            else
            {
                Vector3 projectionPoint = pointA + projectionLength * vecAB;
                a = Vector3.Distance(pointC, projectionPoint);
                return Vector3.Distance(pointC, projectionPoint);

            }
        }

        public static List<Vector3> FindPointsOnLine(List<Vector3> points, Vector3 a, Vector3 b)
        {
            List<Vector3> result = new List<Vector3>();

            foreach (var point in points)
            {
                if (IsOnLine(a, b, point))
                {
                    result.Add(point);
                }
                //else if(DistanceFromPointToLine(a, b, point) < 0.5)
                //{
                //    result.Add(point);
                //}
            }

            return result;
        }

        public static Vector3 FindNearPointsOnLine(List<Vector3> points, Vector3 a, Vector3 b)
        {
            List<Vector3> result = new List<Vector3>();

            foreach (var point in points)
            {
                if (IsOnLine(a, b, point))
                {
                    result.Add(point);
                }
                else if (DistanceFromPointToLine(a, b, point) < 0.5)
                {
                    result.Add(point);
                }
            }
            Vector3 nearVector3 = result.OrderBy(v => v.Z).FirstOrDefault();
            return nearVector3;
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

        // 计算两点间距离
        public static double CalculateDistance(Vector3 point1, Vector3 point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.Z - point1.Z, 2));
        }

        // 查找与VectorA距离最近且距离小于5的点
        public static Vector3 FindNearest3DPoint(List<Vector3> points, Vector3 vectorA, double maxDistance, out double nearest)
        {
            Vector3 nearestPoint = new Vector3();
            double minDistance = double.MaxValue;
            nearest = -1;

            foreach (var point in points)
            {
                double distance = CalculateDistance(vectorA, point);
                if (distance < maxDistance && distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = point;
                    nearest = minDistance;
                }
            }

            return nearestPoint;
        }

    }

    public class ThreeDimensionDistanceCalculator
    {
        // 计算两向量之间的点积（即：两向量的每一对应分量的乘积之和）
        public static double DotProduct(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        // 计算向量的长度（即：向量的模）
        public static double VectorLength(Vector3 v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        // 计算点P到直线a-b的最短距离
        public static double DistanceFromPointToLine(Vector3 p, Vector3 a, Vector3 b)
        {
            // 计算向量AB和AP的方向向量
            Vector3 ab = new Vector3(b.X - a.X, b.Y - a.Y, b.Z - a.Z); // 向量AB的表示形式为 (Bx-Ax, By-Ay, Bz-Az)
            Vector3 ap = new Vector3(p.X - a.X, p.Y - a.Y, p.Z - a.Z); // 向量AP的表示形式为 (Px-Ax, Py-Ay, Pz-Az)
            double abLength = VectorLength(ab); // 计算向量AB的长度（模）
            double apDotAb = DotProduct(ap, ab); // 计算向量AP与AB的点积（夹角余弦值乘以向量的长度）
            double abProjectedOnAp = apDotAb / abLength; // 计算AP在AB方向上的投影长度（标量积）的系数值（实际上不是投影长度，但可以用于后续计算）
            Vector3 closestPointOnLine = new Vector3((float)(a.X + (abProjectedOnAp * ab.X / abLength)), (float)(a.Y + (abProjectedOnAp * ab.Y / abLength)), (float)(a.Z + (abProjectedOnAp * ab.Z / abLength))); // 计算直线上的最近点坐标（垂足）
            double distance = VectorLength(new Vector3(p.X - closestPointOnLine.X, p.Y - closestPointOnLine.Y, p.Z - closestPointOnLine.Z)); // 计算点到垂足的向量长度，即为所求距离
            return distance; // 返回所求的距离值
        }
    }

    public class ObjectPicker
    {
        public static float DistanceFromPoint(Point mouseLocation, Vector3 testPoint, Matrix4 inverseMatrix)
        {
            Vector3 near = UnProject(new Vector3(mouseLocation.X, mouseLocation.Y, 0), inverseMatrix); // start of ray
            Vector3 far = UnProject(new Vector3(mouseLocation.X, mouseLocation.Y, 1), inverseMatrix); // end of ray
            Vector3 pt = ClosestPoint(near, far, testPoint); // find point on ray which is closest to test point
            return Vector3.Distance(pt, testPoint); // return the distance
        }

        public static Vector3 FindNearPointsOnLine(List<Vector3> points, Vector3 a, Vector3 b, out float errorvalue)
        {
            Dictionary<Vector3, float> distances = new Dictionary<Vector3, float>();

            List<Vector3> result = new List<Vector3>();
            float distence = 99999f;
            float nearplane = 0.01f;
            float farplane = 100f;
            foreach (var point in points)
            {
                Vector3 pt = ClosestPoint(a, b, point);
                pt.Z = 1 / ((pt.Z / farplane) + (1 - pt.Z) / nearplane);//非线性关系

                if (Vector3.Distance(pt, point) < distence)
                {
                    if (result.Count() > 0)
                    {
                        result.RemoveAt(0);
                    }
                    distances.Add(point, Vector3.Distance(pt, point));
                    result.Add(point);
                    distence = distence < Vector3.Distance(pt, point) ? distence : Vector3.Distance(pt, point);
                }
            }
            Vector3 nearVector3 = result.OrderBy(v => v.Z).FirstOrDefault();
            Vector3 select = distances.OrderBy(v => v.Value).FirstOrDefault().Key;
            errorvalue = distence;
            return nearVector3;
        }
        public static Vector3 ClosestPoint(Vector3 A, Vector3 B, Vector3 P)
        {
            Vector3 AB = B - A;
            float ab_square = Vector3.Dot(AB, AB);
            Vector3 AP = P - A;
            float ap_dot_ab = Vector3.Dot(AP, AB);
            // t is a projection param when we project vector AP onto AB 
            float t = ap_dot_ab / ab_square;
            // calculate the closest point 
            Vector3 Q = A + Vector3.Multiply(AB, t);
            return Q;
        }
        private static Vector3 UnProject(Vector3 screen, Matrix4 inverseMatrix)
        {
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            Vector4 pos = new Vector4();

            // Map x and y from window coordinates, map to range -1 to 1 
            pos.X = (screen.X - viewport[0]) / viewport[2] * 2.0f - 1.0f;
            pos.Y = 1 - (screen.Y - viewport[1]) / viewport[3] * 2.0f;
            pos.Z = screen.Z * 2.0f - 1.0f;
            pos.W = 1.0f;

            Vector4 pos2 = pos * inverseMatrix;
            Vector3 pos_out = new Vector3(pos2.X, pos2.Y, pos2.Z);

            return pos_out / pos2.W;
        }


    }
    public class PointDistanceComparer : IComparer<Vector3>
    {
        private readonly Vector3 _referencePoint;

        public PointDistanceComparer(Vector3 referencePoint)
        {
            _referencePoint = referencePoint;
        }

        public int Compare(Vector3 x, Vector3 y)
        {
            double distanceX = CalculateDistance(x, _referencePoint);
            double distanceY = CalculateDistance(y, _referencePoint);
            return distanceX.CompareTo(distanceY);
        }

        private double CalculateDistance(Vector3 point1, Vector3 point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }
        public static List<Vector3> FindNearestFourPoints(Vector3 vectorA, List<Vector3> vectorList)
        {
            var comparer = new PointDistanceComparer(vectorA);
            var sortedPoints = vectorList.OrderBy(point => point, comparer).ToList();
            return sortedPoints.Take(4).ToList();
        }
    }
}
