﻿using Assimp;
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
            if (mindistance < 0.05)//兼容误差
            {
                return near;
            }
            return new Vector3D(-99999, -9999, -99999);
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
            text += "。";//最后一个字不显示，暂处理
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
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

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
    }
}