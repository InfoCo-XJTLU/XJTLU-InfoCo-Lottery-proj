using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UI {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    private static MainWindow instance = null!;
    public static MainWindow Instance { get => instance; set => instance = (instance is null) ? value : instance; }
    private static LotteryCore.QuestionEngine quiz = null!;
    private static LotteryCore.LotteryEngine priz = null!;
    public static LotteryCore.QuestionEngine Quiz { get => quiz; }
    public static LotteryCore.LotteryEngine Priz { get => priz; }
    public static LotteryCore.QuestionEntry?[] CorrectAnswers { get; set; } = new LotteryCore.QuestionEntry[3];

    public MainWindow() {
      InitializeComponent();

      Instance = this;

      SourceInitialized += (s, e) => LotteryCore.DebugUtils.WriteLine("MainWindow/SourceInitialized");
      Activated += (s, e) => LotteryCore.DebugUtils.WriteLine("MainWindow/Activated");
      Loaded += (s, e) => LotteryCore.DebugUtils.WriteLine("MainWindow/Loaded");
      ContentRendered += (s, e) => LotteryCore.DebugUtils.WriteLine("MainWindow/ContentRendered");
      Deactivated += (s, e) => LotteryCore.DebugUtils.WriteLine("MainWindow/Deactivated");
      Closing += (s, e) => LotteryCore.DebugUtils.WriteLine("MainWindow/Closing");
      Closed += (s, e) => LotteryCore.DebugUtils.WriteLine("MainWindow/Closed");
      Unloaded += (s, e) => LotteryCore.DebugUtils.WriteLine("MainWindow/Unloaded");

      quiz = new LotteryCore.QuestionEngine(LotteryCore.PathTool.GenResRelativePath("cfg/questionDatabase.csv"));
      priz = new LotteryCore.LotteryEngine(LotteryCore.PathTool.GenResRelativePath("cfg/prizeDatabase.csv"));

      frmLogical.Navigate(new Uri("pack://application:,,,/PageForeground.xaml"));
    }

    public void SwitchMenu(object? menu) {
      frmLogical.Navigate(new Uri("pack://application:,,,/" + menu?.ToString() + ".xaml"));
    }
  }
}