using LotteryCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UI {

  internal class PageForegroundStaticInstance {
    private static PageForegroundStaticInstance? instance = null;
    public static PageForegroundStaticInstance Instance { get => instance is null ? Create() : instance; }

    private int prizeFlowerNum;
    private int prizeStoneNum;
    private int prizeNum;

    private EDifficulty difficulty = EDifficulty.Simple;
    public EDifficulty Difficulty { get => difficulty; set => difficulty = value; }

    public int PrizeFlowerNum { get => prizeFlowerNum; }
    public int PrizeStoneNum { get => prizeStoneNum; }

    private PageForegroundStaticInstance() {
    }

    private PageForegroundStaticInstance Init() {
      prizeNum = MainWindow.Priz.Prizedb.Values.Select(x => x.Num).Sum();
      prizeFlowerNum = prizeNum / 2;
      prizeStoneNum = ((0 == (prizeNum % 2)) ? prizeNum : prizeNum + 1) * 160 / 2;
      return this;
    }

    public static PageForegroundStaticInstance Create() {
      instance = instance is null ? new PageForegroundStaticInstance().Init() : instance;
      return instance;
    }

    public PageForegroundStaticInstance DecreaseN() {
      if (prizeFlowerNum <= 0) {
        prizeStoneNum -= 160;
      } else {
        prizeFlowerNum--;
      }
      prizeNum--;
      return instance ?? Create();
    }
  }

  /// <summary>
  /// PageForeground.xaml 的交互逻辑
  /// </summary>
  public partial class PageForeground : Page {
    private readonly Window parentWindow;

    public Window ParentWindow { get => parentWindow; }

    public PageForeground() {
      InitializeComponent();
      parentWindow = Application.Current.MainWindow;

      Loaded += (s, e) => DebugUtils.WriteLine("PageForeground/Loaded");
      Unloaded += (s, e) => DebugUtils.WriteLine("PageForeground/Unloaded");

      Utils.InitialObj(new Dictionary<DispatcherObject, Action> {
        {BackgroundObject, Utils.GenSourceInitializer(BackgroundObject, PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/background/1920x1080/Background.png")) },
        {Background_Image_Constant, Utils.GenSourceInitializer(Background_Image_Constant, PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/background/1920x1080/Background.png")) },
        {ForegroundInfoMenual, Utils.GenSourceInitializer(ForegroundInfoMenual,PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/Menu.png")) },
        {PrimogemIcon,Utils.GenSourceInitializer(PrimogemIcon,PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/Primogem.png")) },
        {AcquaintFateIcon, Utils.GenSourceInitializer(AcquaintFateIcon,PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/acquaint-fate.png")) },
        {Button1x,Utils.GenSourceInitializer(Button1x,PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/button.png")) },
        {Button10x,Utils.GenSourceInitializer(Button10x,PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/button.png")) },
        {Button1x_icon,Utils.GenSourceInitializer(Button1x_icon,PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/acquaint-fate.png")) },
        {Button10x_icon,Utils.GenSourceInitializer(Button10x_icon,PathTool.GenResRelativePath("/Res/res/UI/Scene/StartScene/Menu/1920x1080/acquaint-fate.png")) },
      });

      FlowLeft_Label.Text = PageForegroundStaticInstance.Instance.PrizeFlowerNum.ToString();
      StoneLeft_label.Text = PageForegroundStaticInstance.Instance.PrizeStoneNum.ToString();
    }

    public void QuestionButtonClicked(object sender, RoutedEventArgs e) {
    }

    private void Wish_x1_Click(object sender, RoutedEventArgs e) {
      MainWindow.Instance.SwitchMenu("PageDrawLottery");
      FlowLeft_Label.Text = PageForegroundStaticInstance.Instance.DecreaseN().PrizeFlowerNum.ToString();
      StoneLeft_label.Text = PageForegroundStaticInstance.Instance.PrizeStoneNum.ToString();
      Button1x_Text_cost.Foreground = PageForegroundStaticInstance.Instance.PrizeFlowerNum > 0 ? Brushes.Black : Brushes.Red;
      Button10x_Text_cost.Foreground = PageForegroundStaticInstance.Instance.PrizeFlowerNum >= 10 ? Brushes.Black : Brushes.Red;
      return;
    }

    private void Wish_x10_Click(object sender, RoutedEventArgs e) {
      MainWindow.Instance.SwitchMenu("PageDrawLottery");
      FlowLeft_Label.Text = PageForegroundStaticInstance.Instance.DecreaseN().PrizeFlowerNum.ToString();
      StoneLeft_label.Text = PageForegroundStaticInstance.Instance.PrizeStoneNum.ToString();
      Button1x_Text_cost.Foreground = PageForegroundStaticInstance.Instance.PrizeFlowerNum > 0 ? Brushes.Black : Brushes.Red;
      Button10x_Text_cost.Foreground = PageForegroundStaticInstance.Instance.PrizeFlowerNum >= 10 ? Brushes.Black : Brushes.Red;
      return;
    }

    private void ButtonStore_Click(object sender, RoutedEventArgs e) {
      frmDisplay.Navigate(new Uri("pack://application:,,,/PageQuestions.xaml"));
      frmDisplay.Visibility = Visibility.Visible;
    }

    private void ButtonHistory_Click(object sender, RoutedEventArgs e) {
      frmDisplay.Navigate(new Uri("pack://application:,,,/PageHistory.xaml"));
    }

    private void StorageButton_Click(object sender, RoutedEventArgs e) {
    }

    private void ButtonDifficutySimple_Click(object sender, RoutedEventArgs e) {
      PageForegroundStaticInstance.Instance.Difficulty = EDifficulty.Simple;
    }

    private void ButtonDifficutyNormal_Click(object sender, RoutedEventArgs e) {
      PageForegroundStaticInstance.Instance.Difficulty = EDifficulty.Normal;
    }

    private void ButtonDifficutyHard_Click(object sender, RoutedEventArgs e) {
      PageForegroundStaticInstance.Instance.Difficulty = EDifficulty.Hard;
    }

    private void ButtonClearAnswerStatus_Click(object sender, RoutedEventArgs e) {
      MainWindow.CorrectAnswers[0] = null;
      MainWindow.CorrectAnswers[1] = null;
      MainWindow.CorrectAnswers[2] = null;
    }
  }
}