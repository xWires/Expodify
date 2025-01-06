using System.Collections.Specialized;
using Avalonia.Controls;
using Expodify.GUI.ViewModels;

namespace Expodify.GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel(this);
        DataContext = viewModel;

        viewModel.Logs.CollectionChanged += Logs_CollectionChanged;
    }

    private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            logBox.ScrollIntoView(e.NewItems[0]!);
        }
    }
}