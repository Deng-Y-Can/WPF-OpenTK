using Assimp;
using Assimp.Unmanaged;
using LearnOpenTK.Common;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace WpfApp
{
    public class SceneT
    {
    }

    //顶点
    public struct VertexT
    {
        public Vector3 Position;//位置
        public Vector3 Normal;//法向量
        public Vector2 TexCoords;//纹理坐标
    };

    //纹理
    public struct TextureT
    {
        public int id;
        public string type;
        public TextureSlot slot;
    };

    public class ModelT
    {
        public ModelT(string path)
        {
            LoadModel(path);
        }
        public void Draw(Shader shader)
        {
            for (int i = 0; i < meshTs.Count; i++)
            {
                meshTs[i].Draw(shader);
            }
        }
        private List<MeshT> meshTs = new List<MeshT>();
        private string directory;
        private List<TextureT> textureTsLoad = new List<TextureT>();
        private void LoadModel(string path)
        {
            AssimpContext importer = new AssimpContext();
            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);
            if (scene == null || scene.RootNode == null)
            {
                MessageBox.Show("Load failure!");
                return;
            }
            directory = Directory.GetParent(path).FullName;
            ProcessNode(scene.RootNode, scene);
        }
        private void ProcessNode(Node node, Scene scene)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                Mesh mesh = (Mesh)scene.Meshes[node.MeshIndices[i]];
                meshTs.Add(ProcessMesh(mesh, scene));
            }
            for (int i = 0; i < node.ChildCount; i++)
            {
                ProcessNode(node.Children[i], scene);
            }
        }
        private MeshT ProcessMesh(Mesh mesh, Scene scene)
        {
            List<VertexT> vertexTs = new List<VertexT>();
            List<int> indices = new List<int>();
            List<TextureT> textureTs = new List<TextureT>();
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                VertexT vertexT = new VertexT();
                vertexT.Position.X = mesh.Vertices[i].X;
                vertexT.Position.X = mesh.Vertices[i].Y;
                vertexT.Position.Z = mesh.Vertices[i].Z;
                if (mesh.HasNormals)
                {
                    vertexT.Normal.X = mesh.Normals[i].X;
                    vertexT.Normal.Y = mesh.Normals[i].Y;
                    vertexT.Normal.Z = mesh.Normals[i].Z;
                }

                if (mesh.HasTextureCoords(0))
                {
                    vertexT.TexCoords.X = mesh.TextureCoordinateChannels[0][i].X;
                    vertexT.TexCoords.Y = mesh.TextureCoordinateChannels[0][i].Y;

                }
                else
                {
                    vertexT.TexCoords = new Vector2(0f, 0f);
                }
                vertexTs.Add(vertexT);
            }
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                Face face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                {
                    indices.Add(face.Indices[j]);
                }
            }

            if (mesh.MaterialIndex >= 0)
            {
                Material material = scene.Materials[mesh.MaterialIndex];
                List<TextureT> diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
                foreach (var item in diffuseMaps)
                {
                    textureTs.Add(item);
                }
                List<TextureT> specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_specular");
                foreach (var item in specularMaps)
                {
                    textureTs.Add(item);
                }
            }
            return new MeshT(vertexTs, indices, textureTs);
        }
        private List<TextureT> LoadMaterialTextures(Material material, TextureType textureType, string typeName)
        {
            List<TextureT> textureTs = new List<TextureT>();
            for (int i = 0; i < material.GetMaterialTextureCount(textureType); i++)
            {
                TextureSlot str;
                material.GetMaterialTexture(textureType, i, out str);
                TextureT textureT = new TextureT();
                textureT.id = str.TextureIndex;
                textureT.type = typeName;
                textureT.slot = str;
                textureTs.Add(textureT);
                //bool skip = false;
                //for(int j = 0; j < textureTsLoad.Count; j++)
                //{
                //    if (textureTsLoad[j].slot.FilePath == str.FilePath)
                //    {
                //        textureTs.Add(textureTsLoad[j]);
                //        skip = true;
                //        break;
                //    }
                //}
                //if (!skip)
                //{

                //}

            }
            return textureTs;

        }
    }
    public class MeshT
    {
        public List<VertexT> _vertexTs;
        public List<int> _indices;
        public List<TextureT> _textureTs;
        public float[] _vertexTList;
        public int[] _indicesTList;

        public MeshT(List<VertexT> vertexTs, List<int> indices, List<TextureT> textureTs)
        {
            this._vertexTs = vertexTs;
            this._indices = indices;
            this._textureTs = textureTs;
            _indicesTList = _indices.ToArray();
            _vertexTList = VertexTsToArray(_vertexTs);
            SetupMesh();
        }

        public float[] VertexTsToArray(List<VertexT> vertexTs)
        {
            float[] vertexTList = new float[vertexTs.Count() * 8];
            int index = 0;
            for (int i = 0; i < vertexTs.Count; i++)
            {
                vertexTList[index] = vertexTs[i].Position.X;
                index++;
                vertexTList[index] = vertexTs[i].Position.Y;
                index++;
                vertexTList[index] = vertexTs[i].Position.Z;
                index++;
                vertexTList[index] = vertexTs[i].Normal.X;
                index++;
                vertexTList[index] = vertexTs[i].Normal.Y;
                index++;
                vertexTList[index] = vertexTs[i].Normal.Z;
                index++;
                vertexTList[index] = vertexTs[i].TexCoords.X;
                index++;
                vertexTList[index] = vertexTs[i].TexCoords.Y;
                index++;
            }
            return vertexTList;

        }
        public void Draw(Shader shader)
        {
            uint diffuerNr = 1;
            uint specularNr = 1;
            for (int i = 0; i < _textureTs.Count; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                string number;
                string name = _textureTs[i].type;
                if (name == "texture_diffuse")
                {
                    number = diffuerNr++.ToString();
                }
                else
                {
                    number = specularNr++.ToString();
                }
                shader.SetInt("material_" + name + number, i);
                GL.BindTexture(TextureTarget.Texture2D, _textureTs[i].id);
            }
            SetupMesh();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);

        }

        private int VAO, VBO, EBO;
        private void SetupMesh()
        {
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexTList.Length * sizeof(float), _vertexTList, BufferUsageHint.StaticDraw);

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);


            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);//顶点位置
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3);//顶点法线
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6);//顶点纹理坐标
            GL.EnableVertexAttribArray(2);

        }


    }

    //-------------------------------------------------------------工具类---------------------------------------
    public class VertexList
    {
        public VertexList(List<Vector3D> vector3Ds)
        {
            _vector3s = vector3Ds;
            _vertices = Tool.Vector3iToIntArray(_vector3s);
            originVertexDic = Tool.Vector3iToDictionary(_vector3s);
            _maxX = _vector3s.Max(v => v.X);
            _minX = _vector3s.Min(v => v.X);
            _maxY = _vector3s.Max(v => v.Y);
            _minY = _vector3s.Min(v => v.Y);
            _maxZ = _vector3s.Max(v => v.Z);
            _minZ = _vector3s.Min(v => v.Z);
            _verticeMax = _vertices.Max();
            _verticeMin = _vertices.Min();
            _count = _vector3s.Count();

        }
        public float[] _vertices;
        public List<Vector3D> _vector3s;

        public float _maxX;
        public float _minX;
        public float _maxY;
        public float _minY;
        public float _maxZ;
        public float _minZ;
        public float _verticeMax;
        public float _verticeMin;
        public int _count;
        public Dictionary<int, Vector3D> originVertexDic;
        public Dictionary<int, Vector3D> changeViwVertexDic;//可见点
        public Dictionary<int, Vector3D> changeVertexDic;


    }


    public class ColorT
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public ColorT(float r, float g, float b, float a)
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
