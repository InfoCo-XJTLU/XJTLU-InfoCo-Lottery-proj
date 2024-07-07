using System.Windows.Forms;
using System.Diagnostics;

namespace LotteryCore {

  public static class PathTool {

    public static string GenResRelativePath(string res) {
      return AppDomain.CurrentDomain.BaseDirectory + res;
    }

    private static void MessageFileNotFound(in string fp) {
      // Initializes the variables to pass to the MessageBox.Show method.
      string message = $"File whose type is \"{fp}\" Not found";
      string caption = "Error File Not Found.";
      MessageBoxButtons buttons = MessageBoxButtons.YesNo;
      DialogResult result;

      // Displays the MessageBox.
      result = MessageBox.Show(message, caption, buttons);
      if (result == System.Windows.Forms.DialogResult.Yes) {
        return;
      }
    }

    public static void IinitDB<T>(ref string dbpath, out List<T> dbi) {
      TryGetPath(ref dbpath, ".csv");

      // Useless, but to prevent compiler error
      // Provide a default empty value;
      dbi = [];

      using StreamReader reader = new(dbpath);
      using CsvHelper.CsvReader csvdbreader =
          new(reader, System.Globalization.CultureInfo.InvariantCulture);
      bool flag = true;
      do {
        try {
          dbi = csvdbreader.GetRecords<T>().ToList();
          flag = false;
        } catch {
          DebugUtils.Error.WriteLine("{0}", "Not a csv or file dose not match the request.");
          dbpath = typeof(T).ToString();
          LotteryCore.PathTool.TryGetPath(ref dbpath, ".csv");
          flag = true;
        } finally {
        }
      } while (flag);

      // var modinfo = typeof(PrizeItem).GetMethod("init");
      // if(null != modinfo){
      //     for(var i = 0; i < dbi.Count(); i ++){
      //         modinfo.Invoke(dbi[i], null);
      //     }
      // }
      // Obsoleted, for it can be called in context other rather than initdb
    }

    public static void TryGetPath(ref string path, string ft) {
      if (!File.Exists(path)) {
        DebugUtils.Error.WriteLine("{0}", "No file called `" + path + "' was found.");
        MessageFileNotFound(path);
        do {
          Console.WriteLine("Try specified another one? [Y/n]:");
          string? tmppath = Console.ReadLine();
          Console.SetCursorPosition(0, Console.CursorTop - 1);
          Console.Write("{0}", new string(' ', Console.WindowWidth));
          Console.SetCursorPosition(0, Console.CursorTop);
          if (string.IsNullOrEmpty(tmppath) || string.IsNullOrWhiteSpace(tmppath)) {
            tmppath = "Y";
          }
          if (tmppath.Contains('Y') || tmppath.Contains('y')) {
            Console.Write("{0}", "Given a path:");
            tmppath = Console.ReadLine();
          } else {
            DebugUtils.Error.WriteLine("{0}", "Expect a database with extension of " + ft);
            Environment.Exit(1);
          }
          if (File.Exists(tmppath) && new FileInfo(tmppath).Extension == ft) {
            path = tmppath;
            break;
          } else {
            DebugUtils.Error.WriteLine("{0}", "File not exist or not specified request.");
          }
        } while (true);
      }
    }
  }

  public static class ConsoleWrapper {

    public static void WriteError() {
      var tmp = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("{0}", "Error: ");
      Console.ForegroundColor = tmp;
    }

    public static void WriteDebug() {
      var tmp = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write("{0}", "Debug: ");
      Console.ForegroundColor = tmp;
    }
  }

  public static class DebugUtils {

    public static void WriteInColor(in string str, ConsoleColor color) {
      var origin = Console.ForegroundColor;
      Console.ForegroundColor = color;
      Console.Write("{0}", str);
      Console.ForegroundColor = origin;

      return;
    }

    public static void WriteLine(in string format, params object?[] args) {
      WriteInColor("Debug: ", ConsoleColor.Yellow);
      Console.WriteLine(format, args);
    }

    public static void Write(in string format, params object?[] args) {
      WriteInColor("Debug: ", ConsoleColor.Yellow);
      Console.Write(format, args);
    }

    public static class Error {

      public static void WriteInColor(in string str, ConsoleColor color) {
        var origin = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Error.Write("{0}", str);
        Console.ForegroundColor = origin;

        return;
      }

      public static void WriteLine(in string format, params object?[] args) {
        WriteInColor("Error: ", ConsoleColor.Red);
        Console.Error.WriteLine(format, args);
      }

      public static void Write(in string format, params object?[] args) {
        WriteInColor("Error: ", ConsoleColor.Red);
        Console.Error.Write(format, args);
      }
    }

    public static void PrintObj(in object? obj, int level = 0) {
      static void printlevel(int level) {
        for (var i = 1; i <= level; i++) {
          Console.Write("|   ");
        }
      }

      if (obj is null) {
        return;
      }
      var type = obj.GetType();
      if (type is null) {
        return;
      }

      printlevel(level);
      Write("Type name: {0}: ", type.Name);
      if (type.IsValueType) {
        Console.WriteLine("{0}", obj);
        return;
      }

      if ((type.Namespace ?? "").StartsWith("System")) {
        Console.WriteLine("{0}", obj);
        return;
      }

      Write("");
      foreach (var val in type.GetProperties()) {
        if (val is null) {
          break;
        }
        if (val.GetGetMethod() is null) {
          Console.Write("(null)\n");
          continue;
        }
        printlevel(level);
        Console.Write("{0}: ", val.Name);
        PrintObj(val.GetValue(obj), level + 1);
        Console.Write(", ");
      }
      Console.WriteLine("");
    }

    public static void PrintObj(in object?[] obj, int level = 0) {
      static void printlevel(int level) {
        for (var i = 1; i <= level; i++) {
          Console.Write("|   ");
        }
      }

      if (obj is null) {
        return;
      }
      var type = (obj.FirstOrDefault() ?? 0).GetType();
      var properties = new List<System.Reflection.PropertyInfo>();
      if (type is null) {
        return;
      }

      if (type.IsValueType) {
        foreach (var val in obj) {
          Console.Write("{0}", val);
        }
        return;
      }

      if ((type.Namespace ?? "").StartsWith("System")) {
        foreach (var val in obj) {
          Console.Write("{0}", val);
        }
        return;
      }

      printlevel(level);
      WriteLine("Type name: {0}: ", type.Name);
      Write("");
      printlevel(level);
      foreach (var val in type.GetProperties()) {
        if (val is null) {
          break;
        }
        properties.Add(val);
        Console.Write("{0}\t", val.Name);
      }
      Console.WriteLine("");
      if (properties.Count <= 0) {
        return;
      }
      foreach (var val in obj) {
        Write("");
        printlevel(level);
        foreach (var property in properties) {
          if (property is null) {
            break;
          }
          if (property.GetGetMethod() is null) {
            Console.Write("(Null)\t");
            continue;
          }
          PrintObj(property.GetValue(val), level + 1);
          Console.Write("\t");
        }
        Console.WriteLine();
      }
    }
  }

  public static class RandomGenerator {

    public static int GenRandom() {
      var csp = System.Security.Cryptography.RandomNumberGenerator.Create();
      var randomNums = new byte[10];
      csp.GetNonZeroBytes(randomNums);
      foreach (var val in randomNums) {
        if (val >= 0 && val <= 100) {
          return val;
        }
      }
      return 0;
    }
  }
}