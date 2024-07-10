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

namespace UI {

  /// <summary>
  /// PageQuestions.xaml 的交互逻辑
  /// </summary>
  public partial class PageQuestions : Page {
    private int quest = 0;
    private List<LotteryCore.QuestionEntry> quiz;

    public PageQuestions() {
      InitializeComponent();
      quiz = MainWindow.Quiz.QuestionDB.Values.Where(x => x.Difficulty == PageForegroundStaticInstance.Instance.Difficulty).ToList();
      if (quiz.Count <= 0) {
        return;
      }
      Questions.Text = quiz[quest].Contents.ToString();
      var val = quiz[quest].GetChoices;
      foreach (var choice in val) {
        switch (choice.Key) {
          case 'a':
            a.Content = choice.Value; break;
          case 'b':
            b.Content = choice.Value; break;
          case 'c':
            c.Content = choice.Value; break;
          case 'd':
            d.Content = choice.Value; break;
        }
      }
    }

    private void a_Click(object sender, RoutedEventArgs e) {
      if (quiz.Count <= 0) {
        return;
      }
      if (quiz[quest].CheckAnswer("a")) {
        for (var i = 1; i < MainWindow.CorrectAnswers.Length; i++) {
          if (MainWindow.CorrectAnswers[i] is null) {
            MainWindow.CorrectAnswers[i] = quiz[quest];
            break;
          }
          if (2 == i) {
            MainWindow.CorrectAnswers[0] = quiz[quest];
            MainWindow.CorrectAnswers[1] = null; MainWindow.CorrectAnswers[2] = null;
          }
        }
        Questions.Text = "Correct Answwer";
      } else {
        Questions.Text = "Answwer Incorrect";
      }
    }

    private void b_Click(object sender, RoutedEventArgs e) {
      if (quiz.Count <= 0) {
        return;
      }
      if (quiz[quest].CheckAnswer("b")) {
        for (var i = 1; i < MainWindow.CorrectAnswers.Length; i++) {
          if (MainWindow.CorrectAnswers[i] is null) {
            MainWindow.CorrectAnswers[i] = quiz[quest];
            break;
          }
          if (2 == i) {
            MainWindow.CorrectAnswers[0] = quiz[quest];
            MainWindow.CorrectAnswers[1] = null; MainWindow.CorrectAnswers[2] = null;
          }
        }
        Questions.Text = "Correct Answwer";
      } else {
        Questions.Text = "Answwer Incorrect";
      }
    }

    private void c_Click(object sender, RoutedEventArgs e) {
      if (quiz.Count <= 0) {
        return;
      }
      if (quiz[quest].CheckAnswer("c")) {
        for (var i = 1; i < MainWindow.CorrectAnswers.Length; i++) {
          if (MainWindow.CorrectAnswers[i] is null) {
            MainWindow.CorrectAnswers[i] = quiz[quest];
            break;
          }
          if (2 == i) {
            MainWindow.CorrectAnswers[0] = quiz[quest];
            MainWindow.CorrectAnswers[1] = null; MainWindow.CorrectAnswers[2] = null;
          }
        }
        Questions.Text = "Correct Answwer";
      } else {
        Questions.Text = "Answwer Incorrect";
      }
    }

    private void d_Click(object sender, RoutedEventArgs e) {
      if (quiz.Count <= 0) {
        return;
      }
      if (quiz[quest].CheckAnswer("d")) {
        for (var i = 1; i < MainWindow.CorrectAnswers.Length; i++) {
          if (MainWindow.CorrectAnswers[i] is null) {
            MainWindow.CorrectAnswers[i] = quiz[quest];
            break;
          }
          if (2 == i) {
            MainWindow.CorrectAnswers[0] = quiz[quest];
            MainWindow.CorrectAnswers[1] = null; MainWindow.CorrectAnswers[2] = null;
          }
        }
        Questions.Text = "Correct Answwer";
      } else {
        Questions.Text = "Answwer Incorrect";
      }
    }

    private void Next_Click(object sender, RoutedEventArgs e) {
      if (quiz.Count <= 0) {
        return;
      }
      Questions.Text = quiz[quest = ++quest >= quiz.Count ? 0 : quest].Contents.ToString();
      var val = quiz[quest].GetChoices;
      foreach (var choice in val) {
        switch (choice.Key) {
          case 'a':
            a.Content = choice.Value; break;
          case 'b':
            b.Content = choice.Value; break;
          case 'c':
            c.Content = choice.Value; break;
          case 'd':
            d.Content = choice.Value; break;
        }
      }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) {
      var parent = Utils.GetAncestor<PageForeground>(this);
      if (parent != null) {
        parent.frmDisplay.Navigate("");
      }
    }

    private void Previous_Click(object sender, RoutedEventArgs e) {
      if (quiz.Count <= 0) {
        return;
      }
      Questions.Text = quiz[quest = --quest < 0 ? quiz.Count - 1 : quest].Contents.ToString();
      var val = quiz[quest].GetChoices;
      foreach (var choice in val) {
        switch (choice.Key) {
          case 'a':
            a.Content = choice.Value; break;
          case 'b':
            b.Content = choice.Value; break;
          case 'c':
            c.Content = choice.Value; break;
          case 'd':
            d.Content = choice.Value; break;
        }
      }
    }
  }
}