using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

namespace UI {

  /// <summary>
  /// PageForeground.xaml 的交互逻辑
  /// </summary>
  public partial class PageForeground : Page {
    private readonly Window parentWindow;
    public Window ParentWindow { get => parentWindow; }

    public PageForeground(in MainWindow windowobj) {
      InitializeComponent();
      parentWindow = windowobj;

      //BackgroundImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Res/res/UI/Scene/StartScene/background/1920x1080/Background.png", UriKind.Absolute));
      Utils.InitialObj(new Dictionary<DispatcherObject, Action> {
        {BackgroundObject, Utils.GenSourceInitializer(BackgroundObject, LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/background/1920x1080/Background.png")) },
        {Background_Image_Constant, Utils.GenSourceInitializer(Background_Image_Constant, LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/background/1920x1080/Background.png")) }
      });
    }

    public PageForeground() {
      InitializeComponent();
      parentWindow = Application.Current.MainWindow;

      Loaded += (s, e) => LotteryCore.DebugUtils.WriteLine("PageForeground/Loaded");
      Unloaded += (s, e) => LotteryCore.DebugUtils.WriteLine("PageForeground/Unloaded");

      Utils.InitialObj(new Dictionary<DispatcherObject, Action> {
        {BackgroundObject, Utils.GenSourceInitializer(BackgroundObject, LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/background/1920x1080/Background.png")) },
        {Background_Image_Constant, Utils.GenSourceInitializer(Background_Image_Constant, LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/background/1920x1080/Background.png")) },
        {ForegroundInfoMenual, Utils.GenSourceInitializer(ForegroundInfoMenual,LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/Menu.png")) },
        {PrimogemIcon,Utils.GenSourceInitializer(PrimogemIcon,LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/Primogem.png")) },
        {AcquaintFateIcon, Utils.GenSourceInitializer(AcquaintFateIcon,LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/acquaint-fate.png")) },
        {Button1x,Utils.GenSourceInitializer(Button1x,LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/shop-nav-bg.png")) },
        {Button10x,Utils.GenSourceInitializer(Button10x,LotteryCore.PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/shop-nav-bg.png")) }
      });
    }

    public void QuestionButtonClicked(object sender, RoutedEventArgs e) {
    }

    private void Wish_x1_Click(object sender, RoutedEventArgs e) {
      MainWindow.Instance.SwitchMenu("PageDrawLottery");
      return;
    }

    private void Wish_x10_Click(object sender, RoutedEventArgs e) {
      MainWindow.Instance.SwitchMenu("PageDrawLottery");
      return;
    }
  }
}