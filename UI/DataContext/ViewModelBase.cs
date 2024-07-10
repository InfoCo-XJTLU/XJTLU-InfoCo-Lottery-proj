using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.DataContext {

  public class CommandBase : ICommand {

    public event EventHandler? CanExecuteChanged;

    public Action? ExecuteAction { get; set; }
    public Func<bool>? CanExecuteFunc { get; set; }
    public Action<object>? ExecuteParaAction { get; set; }
    public Func<object, bool>? CanExecuteParaFunc { get; set; }

    public CommandBase(Action executeAction, Func<bool> canExecuteFunc = null!) {
      this.ExecuteAction = executeAction;
      this.CanExecuteFunc = canExecuteFunc;
    }

    public CommandBase(Action<object> executeParaAction, Func<object, bool> canExecuteParaFunc = null!) {
      this.ExecuteParaAction = executeParaAction;
      this.CanExecuteParaFunc = canExecuteParaFunc;
    }

    public bool CanExecute(object? parameter = null) {
      if (null == parameter) {
        return null == this.CanExecuteFunc || CanExecuteFunc.Invoke();
      }
      return null == CanExecuteParaFunc || CanExecuteParaFunc.Invoke(parameter);
    }

    public void Execute(object? parameter = null) {
      if (null == parameter) {
        ExecuteAction?.Invoke();
      } else {
        ExecuteParaAction?.Invoke(parameter);
      }
    }
  }

  public partial class ViewModelBase : INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = "") {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public partial class MainViewModel : ViewModelBase {
    private FrameworkElement? mainContent = null;

    public FrameworkElement? MainContent {
      get => mainContent;
      set {
        mainContent = value;
        OnPropertyChanged();
      }
    }

    public ICommand? ChangePageCommand { get; set; }

    public MainViewModel() {
    }
  }

  public partial class PageForegroundViewModel : ViewModelBase {
    public ObservableCollection<Button> ButtonCollection { get; set; }

    public PageForegroundViewModel() {
      ButtonCollection = [new Button()];
    }
  }
}