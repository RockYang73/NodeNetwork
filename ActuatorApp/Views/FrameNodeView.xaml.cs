using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ActuatorApp.ViewModels;
using ActuatorApp.ViewModels.Nodes;
using ReactiveUI;

namespace ActuatorApp.Views
{
    /// <summary>
    /// FrameNodeView - 視覺分組容器的視圖
    /// </summary>
    public partial class FrameNodeView : UserControl, IViewFor<FrameNodeViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                typeof(FrameNodeViewModel), typeof(FrameNodeView),
                new PropertyMetadata(null));

        public FrameNodeViewModel ViewModel
        {
            get => (FrameNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FrameNodeViewModel)value;
        }
        #endregion

        private bool _isDragging;
        private Point _dragStartPoint;

        public FrameNodeView()
        {
            InitializeComponent();

            // 監聽 ViewModel 變化
            this.WhenAnyValue(v => v.ViewModel)
                .Where(vm => vm != null)
                .Subscribe(vm =>
                {
                    System.Diagnostics.Debug.WriteLine($"[FrameNodeView] ViewModel set: {vm.Name}");
                    
                    // 立即設置尺寸
                    this.Width = vm.Size.Width;
                    this.Height = vm.Size.Height;
                    
                    // 立即設置顏色
                    BorderBrush.Color = vm.BorderColor;
                    HeaderBrush.Color = vm.BorderColor;
                    BackgroundBrush.Color = Color.FromArgb(0x30, vm.BorderColor.R, vm.BorderColor.G, vm.BorderColor.B);
                    
                    // 綁定標題
                    TitleLabel.Text = vm.Name;
                });

            this.WhenActivated(d =>
            {
                if (ViewModel == null) return;

                System.Diagnostics.Debug.WriteLine($"[FrameNodeView] WhenActivated - Frame: {ViewModel.Name}");

                // 綁定尺寸
                this.WhenAnyValue(v => v.ViewModel.Size)
                    .Subscribe(size =>
                    {
                        this.Width = size.Width;
                        this.Height = size.Height;
                    })
                    .DisposeWith(d);

                // 綁定邊框顏色
                this.WhenAnyValue(v => v.ViewModel.BorderColor)
                    .Subscribe(color =>
                    {
                        BorderBrush.Color = color;
                        HeaderBrush.Color = color;
                        BackgroundBrush.Color = Color.FromArgb(0x30, color.R, color.G, color.B);
                    })
                    .DisposeWith(d);

                // 綁定選取狀態
                this.WhenAnyValue(v => v.ViewModel.IsSelected)
                    .Subscribe(isSelected =>
                    {
                        SelectionIndicator.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
                    })
                    .DisposeWith(d);

                // 綁定標題
                this.WhenAnyValue(v => v.ViewModel.Name)
                    .Subscribe(name =>
                    {
                        TitleLabel.Text = name;
                    })
                    .DisposeWith(d);
            });

            // 設置 Header 拖曳事件 (使用 Preview 事件以在 DragCanvas 之前捕獲)
            HeaderBorder.PreviewMouseLeftButtonDown += OnHeaderMouseDown;
            HeaderBorder.PreviewMouseMove += OnHeaderMouseMove;
            HeaderBorder.PreviewMouseLeftButtonUp += OnHeaderMouseUp;
            HeaderBorder.LostMouseCapture += OnHeaderLostCapture;
        }

        #region Header 拖曳事件處理

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            System.Diagnostics.Debug.WriteLine($"[FrameNodeView] OnHeaderMouseDown - Frame: {ViewModel.Name}");

            _isDragging = true;
            // 使用螢幕座標而不是父元素座標
            _dragStartPoint = e.GetPosition(Application.Current.MainWindow);
            HeaderBorder.CaptureMouse();

            // 清除其他 Frame 的選取狀態
            if (Application.Current.MainWindow?.DataContext is MainViewModel mainVm)
            {
                mainVm.ClearFrameSelection();
            }
            
            // 選取此 Frame
            ViewModel.IsSelected = true;

            e.Handled = true;
        }

        private void OnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || ViewModel == null) return;

            // 檢查滑鼠左鍵是否仍按下
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                // 左鍵已放開，停止拖曳
                System.Diagnostics.Debug.WriteLine($"[FrameNodeView] Left button released during move - stopping drag");
                _isDragging = false;
                HeaderBorder.ReleaseMouseCapture();
                return;
            }

            // 使用螢幕座標
            var currentPoint = e.GetPosition(Application.Current.MainWindow);
            var delta = new Vector(
                currentPoint.X - _dragStartPoint.X,
                currentPoint.Y - _dragStartPoint.Y
            );

            if (delta.Length > 1) // 避免微小移動
            {
                System.Diagnostics.Debug.WriteLine($"[FrameNodeView] Moving Frame by delta: ({delta.X}, {delta.Y})");
                ViewModel.Move(delta);
                _dragStartPoint = currentPoint;
            }
            
            e.Handled = true;
        }

        private void OnHeaderMouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[FrameNodeView] OnHeaderMouseUp - _isDragging={_isDragging}");
            
            // 無論 _isDragging 狀態如何，都釋放滑鼠捕獲
            _isDragging = false;
            if (HeaderBorder.IsMouseCaptured)
            {
                HeaderBorder.ReleaseMouseCapture();
            }
            e.Handled = true;
        }

        private void OnHeaderLostCapture(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[FrameNodeView] OnHeaderLostCapture");
            _isDragging = false;
        }

        #endregion
    }
}
