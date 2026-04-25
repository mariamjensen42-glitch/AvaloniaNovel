using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaNovel.ViewModels;
using LiveMarkdown.Avalonia;

namespace AvaloniaNovel.Views;

public partial class CreateView : UserControl
{
    private MarkdownRenderer? _renderer;
    private CreateViewModel? _lastViewModel;

    public CreateView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _renderer = this.FindControl<MarkdownRenderer>("ContentRenderer");

        if (DataContext is CreateViewModel vm)
            BindRenderer(vm);
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        // 解绑旧 ViewModel
        if (_lastViewModel != null)
        {
            _lastViewModel.StreamingContentUpdated -= OnStreamingContentUpdated;
            _lastViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        // 绑定新 ViewModel
        if (DataContext is CreateViewModel newVm)
        {
            newVm.StreamingContentUpdated += OnStreamingContentUpdated;
            newVm.PropertyChanged += OnViewModelPropertyChanged;
            _lastViewModel = newVm;
            BindRenderer(newVm);
        }
        else
        {
            _lastViewModel = null;
        }
    }

    private void BindRenderer(CreateViewModel vm)
    {
        if (_renderer == null) return;

        if (vm.IsStreaming)
        {
            // 流式模式：绑定 ObservableStringBuilder
            _renderer.MarkdownBuilder = vm.StreamingBuilder;
        }
        else
        {
            // 非流式：用新的 builder 显示已保存内容
            var builder = new ObservableStringBuilder();
            if (!string.IsNullOrEmpty(vm.ChapterMarkdown))
                builder.Append(vm.ChapterMarkdown);
            _renderer.MarkdownBuilder = builder;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not CreateViewModel vm) return;

        switch (e.PropertyName)
        {
            // IsStreaming 从 true→false：流式结束，切换为显示完整内容
            case nameof(CreateViewModel.IsStreaming):
                if (!vm.IsStreaming)
                {
                    var builder = new ObservableStringBuilder();
                    if (!string.IsNullOrEmpty(vm.ChapterMarkdown))
                        builder.Append(vm.ChapterMarkdown);
                    _renderer?.SetValue(MarkdownRenderer.MarkdownBuilderProperty, builder);
                }
                else
                {
                    // 流式开始：绑定 StreamingBuilder
                    _renderer?.SetValue(MarkdownRenderer.MarkdownBuilderProperty, vm.StreamingBuilder);
                }
                break;

            // ChapterMarkdown 变化：非流式时更新渲染内容
            case nameof(CreateViewModel.ChapterMarkdown):
                if (!vm.IsStreaming)
                {
                    var builder = new ObservableStringBuilder();
                    if (!string.IsNullOrEmpty(vm.ChapterMarkdown))
                        builder.Append(vm.ChapterMarkdown);
                    _renderer?.SetValue(MarkdownRenderer.MarkdownBuilderProperty, builder);
                }
                break;
        }
    }

    private void OnStreamingContentUpdated()
    {
        // 流式输出时自动滚动到底部
        var scrollViewer = this.FindControl<ScrollViewer>("ContentScrollViewer");
        scrollViewer?.ScrollToEnd();
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_lastViewModel != null)
        {
            _lastViewModel.StreamingContentUpdated -= OnStreamingContentUpdated;
            _lastViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _lastViewModel = null;
        }
    }
}
