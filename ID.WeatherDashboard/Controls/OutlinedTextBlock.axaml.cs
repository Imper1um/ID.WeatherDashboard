using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.WeatherDashboard.Controls
{
    public partial class OutlinedTextBlock : UserControl
    {
        public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<OutlinedTextBlock, string?>(nameof(Text), "Testing");
        public static readonly StyledProperty<IBrush> StrokeBrushProperty = AvaloniaProperty.Register<OutlinedTextBlock, IBrush>(nameof(StrokeBrush), Brushes.Black);
        public static readonly StyledProperty<int> StrokeThicknessProperty = AvaloniaProperty.Register<OutlinedTextBlock, int>(nameof(StrokeThickness), 1);
        public static readonly StyledProperty<Thickness> TopLeftOffsetProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, Thickness>(nameof(TopLeftOffset), new Thickness(-1, -1, 0, 0));

        public static readonly StyledProperty<Thickness> TopOffsetProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, Thickness>(nameof(TopOffset), new Thickness(0, -1, 0, 0));

        public static readonly StyledProperty<Thickness> TopRightOffsetProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, Thickness>(nameof(TopRightOffset), new Thickness(1, -1, 0, 0));

        public static readonly StyledProperty<Thickness> LeftOffsetProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, Thickness>(nameof(LeftOffset), new Thickness(-1, 0, 0, 0));

        public static readonly StyledProperty<Thickness> RightOffsetProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, Thickness>(nameof(RightOffset), new Thickness(1, 0, 0, 0));

        public static readonly StyledProperty<Thickness> BottomLeftOffsetProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, Thickness>(nameof(BottomLeftOffset), new Thickness(-1, 1, 0, 0));

        public static readonly StyledProperty<Thickness> BottomOffsetProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, Thickness>(nameof(BottomOffset), new Thickness(0, 1, 0, 0));

        public static readonly StyledProperty<Thickness> BottomRightOffsetProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, Thickness>(nameof(BottomRightOffset), new Thickness(1, 1, 0, 0));
        public static readonly StyledProperty<double> LetterSpacingProperty =
            AvaloniaProperty.Register<OutlinedTextBlock, double>(nameof(LetterSpacing), 1);

        public OutlinedTextBlock()
        {
            InitializeComponent();
            
            this.GetObservable(StrokeThicknessProperty).Subscribe(_ => UpdateOffsets());
        }

        private void UpdateOffsets()
        {
            var s = StrokeThickness;
            TopLeftOffset = new Thickness(-s, -s, 0, 0);
            TopOffset = new Thickness(0, -s, 0, 0);
            TopRightOffset = new Thickness(s, -s, 0, 0);
            LeftOffset = new Thickness(-s, 0, 0, 0);
            RightOffset = new Thickness(s, 0, 0, 0);
            BottomLeftOffset = new Thickness(-s, s, 0, 0);
            BottomOffset = new Thickness(0, s, 0, 0);
            BottomRightOffset = new Thickness(s, s, 0, 0);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public IBrush StrokeBrush
        {
            get => GetValue(StrokeBrushProperty);
            set => SetValue(StrokeBrushProperty, value);
        }

        public int StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public Thickness TopLeftOffset
        {
            get => GetValue(TopLeftOffsetProperty);
            set => SetValue(TopLeftOffsetProperty, value);
        }

        public Thickness TopOffset
        {
            get => GetValue(TopOffsetProperty);
            set => SetValue(TopOffsetProperty, value);
        }

        public Thickness TopRightOffset
        {
            get => GetValue(TopRightOffsetProperty);
            set => SetValue(TopRightOffsetProperty, value);
        }

        public Thickness LeftOffset
        {
            get => GetValue(LeftOffsetProperty);
            set => SetValue(LeftOffsetProperty, value);
        }

        public Thickness RightOffset
        {
            get => GetValue(RightOffsetProperty);
            set => SetValue(RightOffsetProperty, value);
        }

        public Thickness BottomLeftOffset
        {
            get => GetValue(BottomLeftOffsetProperty);
            set => SetValue(BottomLeftOffsetProperty, value);
        }

        public Thickness BottomOffset
        {
            get => GetValue(BottomOffsetProperty);
            set => SetValue(BottomOffsetProperty, value);
        }

        public Thickness BottomRightOffset
        {
            get => GetValue(BottomRightOffsetProperty);
            set => SetValue(BottomRightOffsetProperty, value);
        }

        public double LetterSpacing
        {
            get => GetValue(LetterSpacingProperty);
            set => SetValue(LetterSpacingProperty, value);
        }

    }
}
