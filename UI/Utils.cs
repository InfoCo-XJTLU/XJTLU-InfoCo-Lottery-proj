using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UI {

  internal class Utils {

    public static void InitialObj(Dictionary<DispatcherObject, Action> objset) {
      foreach (var obj in objset) {
        Task.Run(() => { obj.Key.Dispatcher.Invoke(obj.Value); });
      }
    }

    public static Action GenSourceInitializer(Image obj, string sourceDir) {
      return () => { obj.Source = new BitmapImage(new Uri(sourceDir, UriKind.Absolute)); };
    }

    public static Action GenSourceInitializer(MediaElement obj, string sourceDir) {
      return () => { obj.Source = new Uri(sourceDir, UriKind.Absolute); };
    }
  }
}