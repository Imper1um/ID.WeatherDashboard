using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ID.WeatherDashboard.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ID.WeatherDashboard.Controls
{
    public partial class TemperatureGraph : UserControl
    {
        public static readonly StyledProperty<string?> DebugTextProperty = AvaloniaProperty.Register<TemperatureGraph, string?>(nameof(DebugText), "Testing");

        public string? DebugText
        {
            get => GetValue(DebugTextProperty);
            set => SetValue(DebugTextProperty, value);
        }

        public TemperatureGraph()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (Design.IsDesignMode)
                this.LayoutUpdated += (s, e) => DebugText = TempDebugText;
        }

        private TemperatureGraphViewModel? ViewModel => DataContext as TemperatureGraphViewModel;

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (ViewModel == null)
                return;

            const int margin = 4;

            var sb = new StringBuilder();

            var topLine = new Pen(Brushes.White, 2);
            var bottomLine = new Pen(Brushes.Black, 1);

            var highTemperature = ViewModel.MaxTemperature;
            var lowTemperature = ViewModel.MinTemperature;
            //sb.AppendLine($"highTemperature: {highTemperature}");
            //sb.AppendLine($"lowTemperature: {lowTemperature}");


            float maxTemperature = ViewModel.Temperatures.Any(t => t.Temperature != null) ? ViewModel.Temperatures.Max(t => (float)t.Temperature!) : highTemperature;
            float minTemperature = ViewModel.Temperatures.Any(t => t.Temperature != null) ? ViewModel.Temperatures.Min(t => (float)t.Temperature!) : lowTemperature;
            //sb.AppendLine($"maxTemperature: {maxTemperature}");
            //sb.AppendLine($"minTemperature: {minTemperature}");

            var floorTemperature = Math.Min(minTemperature, lowTemperature);
            var ceilingTemperature = Math.Max(maxTemperature, highTemperature);

            var boundsHeight = Bounds.Height - (margin * 2);
            //sb.AppendLine($"boundsHeight: {boundsHeight}");

            var highTemperatureHeight = GetHeight(floorTemperature, ceilingTemperature, highTemperature, boundsHeight);
            var lowTemperatureHeight = GetHeight(floorTemperature, ceilingTemperature, lowTemperature, boundsHeight);
            //sb.AppendLine($"highTemperatureHeight: {highTemperatureHeight}");
            //sb.AppendLine($"lowTemperatureHeight: {lowTemperatureHeight}");
            context.DrawLine(topLine, new Point(0, highTemperatureHeight + margin), new Point(Bounds.Width, highTemperatureHeight + margin));
            context.DrawLine(bottomLine, new Point(0, highTemperatureHeight + (margin*2)), new Point(Bounds.Width, highTemperatureHeight + (margin*2)));
            context.DrawLine(topLine, new Point(0, lowTemperatureHeight + margin), new Point(Bounds.Width, lowTemperatureHeight + margin));
            context.DrawLine(bottomLine, new Point(0, lowTemperatureHeight - 2 + margin), new Point(Bounds.Width, lowTemperatureHeight - 2 + margin));

            var divider = (Bounds.Width - (margin*2)) / 24;

            double? lastX = null;
            double? lastY = null;
            float? lastTemperature = null;
            var circlePoints = new List<Point>();
            for (int hour = 1; hour < 24; hour++)
            {
                var temp = ViewModel.Temperatures.FirstOrDefault(t => t.Hour == hour);
                if (temp?.Temperature == null)
                    continue;

                var curX = divider * (hour - 1);
                var curY = GetHeight(floorTemperature, ceilingTemperature, temp.Temperature.Value, boundsHeight);
                var currentTemperature = temp.Temperature;

                if (lastX != null && lastY != null && lastTemperature != null)
                {
                    var tempBetween = (currentTemperature + lastTemperature) / 2;
                    var currentPercentage = GetPercentageBetween(ceilingTemperature, floorTemperature, currentTemperature.Value);
                    var lastPercentage = GetPercentageBetween(ceilingTemperature, floorTemperature, lastTemperature.Value);
                    var currentColor = GetTemperatureBrush(currentPercentage);
                    var lastColor = GetTemperatureBrush(lastPercentage);
                    var gradientColor = new LinearGradientBrush
                    {
                        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                        EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                        GradientStops = new GradientStops
                        {
                            new GradientStop(lastColor.Color, 0),
                            new GradientStop(currentColor.Color, 1)
                        }
                    };

                    var percentageBetween = GetPercentageBetween(ceilingTemperature, floorTemperature, tempBetween.Value);
                    var colorBetween = GetTemperatureBrush(percentageBetween);
                    sb.AppendLine($"[{hour}] f{floorTemperature}c{ceilingTemperature} {tempBetween} = {percentageBetween} = {GetHexFromBrush(colorBetween)}");
                    context.DrawLine(new Pen(gradientColor, 2), new Point(lastX.Value + margin, lastY.Value + margin), new Point(curX + margin, curY + margin));
                }
                circlePoints.Add(new Point(curX + margin, curY + margin));
                lastX = curX;
                lastY = curY;
                lastTemperature = currentTemperature;
            }
            var currentTempX = Bounds.Width - 4;
            var percentageTimeToCurrent = 1 - ((double)DateTime.Now.Minute / 60);
            currentTempX -= divider * percentageTimeToCurrent;
            var currentTempY = GetHeight(floorTemperature, ceilingTemperature, ViewModel.CurrentTemperature, boundsHeight);
            if (lastX != null && lastY != null && lastTemperature != null)
            {
                var tempBetween = (ViewModel.CurrentTemperature + lastTemperature) / 2;
                var currentPercentage = GetPercentageBetween(ceilingTemperature, floorTemperature, ViewModel.CurrentTemperature);
                var lastPercentage = GetPercentageBetween(ceilingTemperature, floorTemperature, lastTemperature.Value);
                var currentColor = GetTemperatureBrush(currentPercentage);
                var lastColor = GetTemperatureBrush(lastPercentage);
                var gradientColor = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                        {
                            new GradientStop(lastColor.Color, 0),
                            new GradientStop(currentColor.Color, 1)
                        }
                };

                var percentageBetween = GetPercentageBetween(ceilingTemperature, floorTemperature, tempBetween.Value);
                var colorBetween = GetTemperatureBrush(percentageBetween);
                sb.AppendLine($"[CUR] f{floorTemperature}c{ceilingTemperature} {tempBetween} = {percentageBetween} = {GetHexFromBrush(colorBetween)}");
                context.DrawLine(new Pen(gradientColor, 2), new Point(lastX.Value + margin, lastY.Value + margin), new Point(currentTempX + margin, currentTempY + margin));
            }
            circlePoints.Add(new Point(currentTempX + margin, currentTempY + margin));

            foreach (var point in circlePoints)
            {
                context.DrawEllipse(Brushes.White, new Pen(Brushes.Black, 1), point, 3, 3);
            }

            TempDebugText = sb.ToString();
        }

        private string? TempDebugText { get; set; }

        public static string GetHexFromBrush(SolidColorBrush brush)
        {
            var color = brush.Color;
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static SolidColorBrush GetTemperatureBrush(double percent)
        {
            return new SolidColorBrush(GetColorBetween(Colors.Blue, Colors.Red, percent));
        }

        private static Color GetColorBetween(Color start, Color end, double percent)
        {
            percent = Math.Clamp(percent, 0.0, 1.0);

            var r = GetColorByte(start.R, end.R, percent);
            var g = GetColorByte(start.G, end.G, percent);
            var b = GetColorByte(start.B, end.G, percent);

            return Color.FromRgb(r, g, b);
        }

        private static byte GetColorByte(byte start, byte end, double percent)
        {
            return (byte)(start + (end - start) * percent);
        }

        private double GetHeight(float floorTemperature, float ceilingTemperature, float checkTemperature, double boundsHeight)
        {
            var percentage = 1 - GetPercentageBetween(ceilingTemperature, floorTemperature, checkTemperature);

            return boundsHeight * percentage;
        }

        private double GetPercentageBetween(float top, float bottom, float check)
        {
            if (top <= bottom) return 0;
            if (check > top) return 1;
            if (check < bottom) return 0;

            return (check - bottom) / (top - bottom);
        }
    }
}
