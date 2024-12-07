using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using WpfApp.Tools.Vlc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WpfApp
{
    /// <summary>
    /// ListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ListWindow : System.Windows.Window
    {
        public ListWindow()
        {
            InitializeComponent();
        }

        private void myTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem  view  = (TreeViewItem)this.myTreeView.SelectedItem; 
            
           
            if (view != null)
            {
                string header = view.Header.ToString();
                if (header == "OpenGL")
                {
                    MainGLWindow mainGLWindow = new MainGLWindow();
                    //mainGLWindow.ShowDialog();
                    mainGLWindow.Show();
                }
                else if(header == "OpenGL4")
                {
                    MainGLFourWindow mainGLWindow = new MainGLFourWindow();
                    mainGLWindow.Show();
                }
                else if (header == "EBO")
                {
                    EBOWindow mainGLWindow = new EBOWindow();
                    mainGLWindow.Show();
                }
                else if (header == "更多特性")
                {
                    MoreAttributesWindow mainGLWindow = new MoreAttributesWindow();
                    mainGLWindow.Show();
                }
                else if (header == "纹理")
                {
                    TexturesWindow mainGLWindow = new TexturesWindow();
                    mainGLWindow.Show();
                }
                else if (header == "多纹理")
                {
                    MultipleTexturesWindow mainGLWindow = new MultipleTexturesWindow();
                    mainGLWindow.Show();
                }
                else if (header == "Uniforms")
                {
                    UniformsWindow mainGLWindow = new UniformsWindow();
                    mainGLWindow.Show();
                }
                else if (header == "变换")
                {
                    TransformationsWindow mainGLWindow = new TransformationsWindow();
                    mainGLWindow.Show();
                }
                else if (header == "摄像机")
                {
                    CameraWindow mainGLWindow = new CameraWindow();
                    mainGLWindow.Show();
                }
                else if (header == "旋转正方体")
                {
                    CubeWindow mainGLWindow = new CubeWindow();
                    mainGLWindow.Show();
                }
                else if (header == "几何着色器")
                {
                    GeometryWindow mainGLWindow = new GeometryWindow();
                    mainGLWindow.Show();
                }
               
                else if (header == "颜色")
                {
                    ColorWindow mainGLWindow = new ColorWindow();
                    mainGLWindow.Show();
                }
                else if (header == "基础光照")
                {
                    BasicLightWindow mainGLWindow = new BasicLightWindow();
                    mainGLWindow.Show();
                }
                else if (header == "金属")
                {
                    MaterialsWindow mainGLWindow = new MaterialsWindow();
                    mainGLWindow.Show();
                }
                else if (header == "VLC播放器")
                {
                    VlcPlayerWindow mainGLWindow = new VlcPlayerWindow();
                    mainGLWindow.Show();
                }
            }
        }
    }
}
