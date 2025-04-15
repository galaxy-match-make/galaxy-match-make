using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GalaxyMatchGUI.ViewModels;
using System.Collections.Specialized;
using System.Linq;
using System;

namespace GalaxyMatchGUI.Views;

public partial class MessageRoomView : UserControl
{
    private ScrollViewer _scrollViewer;

    public MessageRoomView()
    {
        InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _scrollViewer = this.FindControl<ScrollViewer>("MessagesScrollViewer");
        
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MessageRoomViewModel vm && vm.Messages is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += Messages_CollectionChanged;
        }
    }
    
    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            ScrollToBottom();
        }
    }

    private void ScrollToBottom()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToEnd();
            }
        });
    }
}