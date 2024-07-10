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

  internal class LotteryHistory_Text {
    private static LotteryHistory_Text? instance = null;
    public static LotteryHistory_Text Instance { get => instance is null ? Create() : instance; }

    private readonly List<uint> history_ = [];
    public List<uint> History { get => history_; }

    private LotteryHistory_Text() {
    }

    private LotteryHistory_Text Init() {
      return this;
    }

    public static LotteryHistory_Text Create() {
      instance = instance is null ? new LotteryHistory_Text().Init() : instance;
      return instance;
    }

    public LotteryHistory_Text AddHist(uint UID) {
      history_.Add(UID);
      return instance ?? Create();
    }
  }

  /// <summary>
  /// PageDrawLottery.xaml 的交互逻辑
  /// </summary>
  public partial class PageDrawLottery : Page {

    public PageDrawLottery() {
      InitializeComponent();

      var dict = new Dictionary<int, int>();
      for (var i = 0; i < MainWindow.CorrectAnswers.Length; i++) {
        var val = MainWindow.CorrectAnswers[i];
        if (val is not null) {
          if (dict.ContainsKey(val.Weight ?? 0)) {
            dict[val.Weight ?? 0] += val.Ratio;
          } else {
            dict.Add(val.Weight ?? 0, val.Ratio);
          }
        }
        MainWindow.CorrectAnswers[i] = null;
      }

      var prizeEventually = MainWindow.Priz.GenPrizeItem(
        LotteryEngine.GenSumSet(
          MainWindow.Priz.AdjustRatio(
            MainWindow.Priz.GenUIDRatioDictByWeight(
                LotteryEngine.GenPrizeWeigt(
                    LotteryEngine.GenSumSet(
                        LotteryEngine.AdjustRatio(
                            MainWindow.Priz.GenWeightRatioDict(
                                false), dict
                            )))))));

      DebugUtils.WriteLine("{0}: {1}", prizeEventually.UID, prizeEventually.Name);
      LotteryHistory_Text.Instance.AddHist(prizeEventually.UID);

      MainWindow.Priz.DecreasePrize(prizeEventually.UID);

      Utils.InitialObj(new Dictionary<System.Windows.Threading.DispatcherObject, Action> {
        {LotteryAnimation, Utils.GenSourceInitializer(LotteryAnimation, PathTool.GenResRelativePath("/Res/res/Animation/1920x1080/" + prizeEventually.Weight switch{ -1=>"Star3.avi",0 => "Star5.avi", _=>"Star4.avi"})) },

        { LotteryBackgroundImage, Utils.GenSourceInitializer(LotteryBackgroundImage, PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/bg-catalyst.png")) },
        { LotteryReslutBackground,Utils.GenSourceInitializer(LotteryReslutBackground, PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/Result.png")) },
      });

      if (prizeEventually.Url is null) {
        LotteryText.Text = prizeEventually.Name;
        string s = prizeEventually.Name;
        LotteryText.Text = "";
        int a = s.Length;
        for (int i = 0; i < a; i++) {
          LotteryText.Text += string.Concat(s.AsSpan(i, 1), "\n");
        }
      } else {
        Utils.InitialObj(new Dictionary<System.Windows.Threading.DispatcherObject, Action> {
          {LotteryImage,Utils.GenSourceInitializer(LotteryBackgroundImage, PathTool.GenResRelativePath(prizeEventually.Url)) }
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