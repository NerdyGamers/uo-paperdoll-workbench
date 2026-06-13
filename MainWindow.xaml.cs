using System.Windows;
using System.Windows.Controls;
using UOGumpClassifier.Models;
using UOGumpClassifier.ViewModels;

namespace UOGumpClassifier;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;
    }

    // Auto-scroll the status log to the bottom when text changes
    private void StatusLog_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
            tb.ScrollToEnd();
    }

    // Update the current image preview when a queue item is selected
    private void QueueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox lb && lb.SelectedItem is GumpAssetItem item)
            _vm.SelectedItem = item;
    }
}
