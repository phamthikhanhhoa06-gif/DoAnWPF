using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ql_ks.Views
{
    public class PieChartControl : FrameworkElement
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource",
                typeof(IEnumerable<PieSliceData>), typeof(PieChartControl),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public IEnumerable<PieSliceData> ItemsSource
        {
            get => (IEnumerable<PieSliceData>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (ItemsSource == null) return;

            double centerX = ActualWidth / 2;
            double centerY = ActualHeight / 2;
            double radius = Math.Min(centerX, centerY) - 10;

            if (radius <= 0) return;

            Point center = new Point(centerX, centerY);
            double startAngle = 0;

            var slices = new List<PieSliceData>(ItemsSource);

            if (slices.Count == 0)
            {
                dc.DrawEllipse(Brushes.LightGray,
                    new Pen(Brushes.White, 2), center, radius, radius);
                return;
            }

            foreach (var slice in slices)
            {
                if (slice.Angle <= 0) continue;

                double endAngle = startAngle + slice.Angle;

                var figure = new PathFigure
                {
                    StartPoint = center,
                    IsClosed = true
                };

                Point startPoint = GetPointOnCircle(center, radius, startAngle);
                figure.Segments.Add(new LineSegment(startPoint, true));

                Point endPoint = GetPointOnCircle(center, radius, endAngle);
                bool isLargeArc = slice.Angle > 180;

                if (slice.Angle >= 359.99)
                {
                    Point midPoint = GetPointOnCircle(center, radius, startAngle + 180);
                    figure.Segments.Add(new ArcSegment(midPoint,
                        new Size(radius, radius), 0, true,
                        SweepDirection.Clockwise, true));
                    figure.Segments.Add(new ArcSegment(endPoint,
                        new Size(radius, radius), 0, true,
                        SweepDirection.Clockwise, true));
                }
                else
                {
                    figure.Segments.Add(new ArcSegment(endPoint,
                        new Size(radius, radius), 0, isLargeArc,
                        SweepDirection.Clockwise, true));
                }

                var geometry = new PathGeometry(new[] { figure });
                dc.DrawGeometry(slice.Fill, new Pen(Brushes.White, 2), geometry);

                // Vẽ % nếu tỷ lệ >= 5%
                if (slice.Percentage >= 0.05)
                {
                    double midAngle = startAngle + slice.Angle / 2;
                    double labelRadius = radius * 0.65;
                    Point labelPoint = GetPointOnCircle(center, labelRadius, midAngle);

                    string labelText = $"{slice.Percentage:P0}";

                    var formattedText = new FormattedText(
                        labelText,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        12,
                        Brushes.White,
                        1.0); // pixelsPerDip

                    dc.DrawText(formattedText,
                        new Point(
                            labelPoint.X - formattedText.Width / 2,
                            labelPoint.Y - formattedText.Height / 2));
                }

                startAngle = endAngle;
            }
        }

        private Point GetPointOnCircle(Point center, double radius, double angleDegrees)
        {
            double angleRadians = (angleDegrees - 90) * Math.PI / 180;
            return new Point(
                center.X + radius * Math.Cos(angleRadians),
                center.Y + radius * Math.Sin(angleRadians));
        }
    }

    public class PieSliceData
    {
        public string Label { get; set; }
        public double Angle { get; set; }
        public double Percentage { get; set; }
        public Brush Fill { get; set; }
        public decimal Value { get; set; }
    }
}