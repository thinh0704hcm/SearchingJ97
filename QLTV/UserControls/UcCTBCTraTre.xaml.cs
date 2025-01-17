﻿using QLTV.Models;
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

namespace QLTV.UserControls
{
    /// <summary>
    /// Interaction logic for UcCTBCTraTre.xaml
    /// </summary>
    public partial class UcCTBCTraTre : UserControl
    {
        public BCTRATRE BaoCaoTraTre { get; private set; }

        public UcCTBCTraTre(BCTRATRE baoCaoTraTre)
        {
            InitializeComponent();
            BaoCaoTraTre = baoCaoTraTre;
            DataContext = BaoCaoTraTre;
            dgLateReturnReportDetails.ItemsSource = BaoCaoTraTre.CTBCTRATRE;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
