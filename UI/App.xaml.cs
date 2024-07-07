using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;

namespace UI {

  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {

    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);
      LotteryCore.DebugUtils.WriteLine("OnStartup");
    }

    protected override void OnActivated(EventArgs e) {
      base.OnActivated(e);
      LotteryCore.DebugUtils.WriteLine("OnActivated");
    }

    protected override void OnDeactivated(EventArgs e) {
      base.OnDeactivated(e);
      LotteryCore.DebugUtils.WriteLine("OnDeactivated");
    }

    protected override void OnExit(ExitEventArgs e) {
      base.OnExit(e);
      LotteryCore.DebugUtils.WriteLine("OnExit");
    }
  }
}