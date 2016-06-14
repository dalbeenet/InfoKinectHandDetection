using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectHandTracker
{
    /// <summary>
    /// Interaction logic for StatusPanel.xaml
    /// </summary>
    public partial class StatusPanel : UserControl
    {
        public StatusPanel()
        {
            InitializeComponent();
        }

        private ushort[] lHandBuffer = new ushort[Constants.croppedRegionWidth * Constants.croppedRegionHeight];

        public ushort[] LeftHand
        {
            get
            {
                return lHandBuffer;
            }
            set
            {
                var img = Utilites.DepthToBitmap(value, Constants.croppedRegionWidth, Constants.croppedRegionHeight);
                LeftHandView.Source = img;
                lHandBuffer = value;
            }
        }

        private ushort[] rHandBuffer = new ushort[Constants.croppedRegionWidth * Constants.croppedRegionHeight];
        public ushort[] RightHand
        {
            get
            {
                return rHandBuffer;
            }
            set
            {
                var img = Utilites.DepthToBitmap(value, Constants.croppedRegionWidth, Constants.croppedRegionHeight);
                RightHandView.Source = img;
                rHandBuffer = value;
            }
        }
         
        private void BtnLHandSave_OnClick(object sender, RoutedEventArgs e)
        {
            Utilites.DepthToFile(FormClassName.Text, lHandBuffer, 90, 90);
        }

        private void BtnRHandSave_OnClick(object sender, RoutedEventArgs e)
        {
            Utilites.DepthToFile(FormClassName.Text, rHandBuffer, 90, 90);  
        }
    }
}
