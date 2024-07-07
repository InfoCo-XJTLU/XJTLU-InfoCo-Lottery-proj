using LotteryCore;
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
using static System.Net.Mime.MediaTypeNames;

namespace UI {

  /// <summary>
  /// PageDrawLottery.xaml 的交互逻辑
  /// </summary>
  public partial class PageDrawLottery : Page {

    public PageDrawLottery() {
      InitializeComponent();

      var val = MainWindow.Priz.GenPrizeItem(
        LotteryEngine.GenSumSet(
          MainWindow.Priz.AdjustRatio(
            MainWindow.Priz.GenUIDRatioDictByWeight(
                LotteryEngine.GenPrizeWeigt(
                    LotteryEngine.GenSumSet(
                        LotteryEngine.AdjustRatio(
                            MainWindow.Priz.GenWeightRatioDict(
                                false)
                           )))))));

      DebugUtils.WriteLine("{0}: {1}", val.UID, val.Name);

      MainWindow.Priz.DecreasePrize(val.UID);

      Utils.InitialObj(new Dictionary<System.Windows.Threading.DispatcherObject, Action> {
        { LotteryAnimation, Utils.GenSourceInitializer(LotteryAnimation, PathTool.GenResRelativePath("/Res/res/Animation/1920x1080/" + val.Weight switch{ -1=>"Star3.avi",0 => "Star5.avi", _=>"Star4.avi"})) },
        {LotteryBackgroundImage, Utils.GenSourceInitializer(LotteryBackgroundImage, PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/bg-catalyst.png")) },
        {LotteryReslutBackground,Utils.GenSourceInitializer(LotteryReslutBackground, PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/Result.png")) },
        {ExitLotteryButtonIcon,Utils.GenSourceInitializer(ExitLotteryButtonIcon, PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/primogem.png")) }
      });

      if (val.Url is null) {
        LotteryText.Text = val.Name;
        string s = val.Name;
        LotteryText.Text = "";
        int a = s.Length;
        for (int i = 0; i < a; i++) {
          LotteryText.Text += s.Substring(i, 1) + "\n";
        }
      } else {
        Utils.InitialObj(new Dictionary<System.Windows.Threading.DispatcherObject, Action> {
          {LotteryImage,Utils.GenSourceInitializer(LotteryBackgroundImage, PathTool.GenResRelativePath(val.Url)) }
        });
      }

      LotteryAnimation.Play();
    }

    private void MediaElement_MediaEnded(object sender, RoutedEventArgs e) {
      LotteryReslutBackground.Visibility = Visibility.Visible;
      LotteryBackgroundImage.Visibility = Visibility.Visible;
      ExitLotteryButton.Visibility = Visibility.Visible;
      LotteryText.Visibility = Visibility.Visible;
      LotteryImage.Visibility = Visibility.Visible;
    }

    private void ExitLotteryButton_Click(object sender, RoutedEventArgs e) {
      MainWindow.Instance.SwitchMenu("PageForeground");
    }
  }
}