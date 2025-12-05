using System;

namespace Geometry
{
    /// <summary>
    /// Представляє 2D точку.
    /// </summary>
    public readonly struct Point : IEquatable<Point>
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Point other)
        {
            return Math.Abs(X - other.X) < 1e-9 && Math.Abs(Y - other.Y) < 1e-9;
        }

        public override bool Equals(object obj) => obj is Point other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Point left, Point right) => left.Equals(right);
        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        public override string ToString() => $"({X:F2}, {Y:F2})";
    }

    /// <summary>
    /// Абстрактний полігон з методом обчислення площі (Shoelace).
    /// Обмеження: сортування вершин за кутом працює для опуклих полігонів.
    /// Для увігнутих полігонів потрібна інша реалізація.
    /// </summary>
    public abstract class Polygon
    {
        protected const double Epsilon = 1e-9;
        private readonly System.Collections.Generic.List<Point> _points = new System.Collections.Generic.List<Point>();

        public System.Collections.Generic.IReadOnlyList<Point> Points => _points.AsReadOnly();

        /// <summary>
        /// Встановлює вершини полігону з валідацією.
        /// </summary>
        protected void SetPoints(System.Collections.Generic.IEnumerable<Point> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            _points.Clear();
            _points.AddRange(points);
            ValidatePoints();
        }

        /// <summary>
        /// Встановлює вершини полігону.
        /// </summary>
        public void SetVertices(System.Collections.Generic.IEnumerable<Point> vertices)
        {
            SetPoints(vertices);
        }

        /// <summary>
        /// Перевірка коректності вершин.
        /// Увага: сортує вершини за кутом, що працює для опуклих полігонів.
        /// </summary>
        protected virtual void ValidatePoints()
        {
            if (_points.Count < 3)
                throw new ArgumentException("Полігон повинен мати мінімум 3 точки.", nameof(_points));

            CheckDuplicates();
            OrderVertices();
        }

        private void CheckDuplicates()
        {
            for (int i = 0; i < _points.Count; i++)
            {
                for (int j = i + 1; j < _points.Count; j++)
                {
                    if (_points[i].Equals(_points[j]))
                    {
                        throw new ArgumentException("Масив містить однакові точки.", nameof(_points));
                    }
                }
            }
        }

        /// <summary>
        /// Сортує вершини за кутом відносно центру.
        /// Обмеження: працює для опуклих полігонів.
        /// </summary>
        protected void OrderVertices()
        {
            double cx = 0, cy = 0;

            foreach (var p in _points)
            {
                cx += p.X;
                cy += p.Y;
            }

            cx /= _points.Count;
            cy /= _points.Count;

            _points.Sort((a, b) =>
            {
                double angleA = Math.Atan2(a.Y - cy, a.X - cx);
                double angleB = Math.Atan2(b.Y - cy, b.X - cx);
                return angleA.CompareTo(angleB);
            });
        }

        public virtual double GetArea()
        {
            double sum = 0;
            int n = _points.Count;

            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                sum += _points[i].X * _points[j].Y - _points[j].X * _points[i].Y;
            }

            return Math.Abs(sum) / 2.0;
        }

        protected bool IsNearlyZero(double value) => Math.Abs(value) < Epsilon;

        public override string ToString() => $"Полігон: {Points.Count} вершин, площа = {GetArea():F4}";
    }

    /// <summary>
    /// Клас трикутника.
    /// </summary>
    public class Triangle : Polygon
    {
        public Triangle(Point a, Point b, Point c)
        {
            SetPoints(new[] { a, b, c });
        }

        public static Triangle FromPoints(params Point[] points)
        {
            if (points.Length != 3)
                throw new ArgumentException("Трикутник повинен мати рівно 3 точки.", nameof(points));
            
            return new Triangle(points[0], points[1], points[2]);
        }

        public static Triangle FromCoordinates(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            return new Triangle(new Point(x1, y1), new Point(x2, y2), new Point(x3, y3));
        }

        protected override void ValidatePoints()
        {
            base.ValidatePoints();
            CheckColinearity();
        }

        private void CheckColinearity()
        {
            var p = Points;

            double area2 =
                p[0].X * (p[1].Y - p[2].Y) +
                p[1].X * (p[2].Y - p[0].Y) +
                p[2].X * (p[0].Y - p[1].Y);

            if (IsNearlyZero(area2))
                throw new ArgumentException("Точки трикутника лежать на одній прямій.", nameof(_points));
        }

        public override string ToString() => $"Трикутник: {Points[0]}, {Points[1]}, {Points[2]}, площа = {GetArea():F4}";
    }

    /// <summary>
    /// Опуклий чотирикутник.
    /// </summary>
    public class ConvexQuadrilateral : Polygon
    {
        public ConvexQuadrilateral(Point a, Point b, Point c, Point d)
        {
            SetPoints(new[] { a, b, c, d });
        }

        public static ConvexQuadrilateral FromPoints(params Point[] points)
        {
            if (points.Length != 4)
                throw new ArgumentException("Чотирикутник повинен мати рівно 4 точки.", nameof(points));
            
            return new ConvexQuadrilateral(points[0], points[1], points[2], points[3]);
        }

        public static ConvexQuadrilateral FromCoordinates(double x1, double y1, double x2, double y2, 
                                                          double x3, double y3, double x4, double y4)
        {
            return new ConvexQuadrilateral(
                new Point(x1, y1), new Point(x2, y2), 
                new Point(x3, y3), new Point(x4, y4));
        }

        protected override void ValidatePoints()
        {
            base.ValidatePoints();
            CheckConvexity();
        }

        private void CheckConvexity()
        {
            var pts = Points;
            int n = pts.Count;

            bool? sign = null;

            for (int i = 0; i < n; i++)
            {
                var p0 = pts[i];
                var p1 = pts[(i + 1) % n];
                var p2 = pts[(i + 2) % n];

                double cross =
                    (p1.X - p0.X) * (p2.Y - p0.Y) -
                    (p1.Y - p0.Y) * (p2.X - p0.X);

                if (IsNearlyZero(cross))
                    throw new ArgumentException("Чотирикутник містить колінеарні точки.", nameof(_points));

                bool current = cross > 0;

                if (sign == null)
                    sign = current;
                else if (sign != current)
                    throw new ArgumentException("Фігура не є опуклим чотирикутником.", nameof(_points));
            }

            CheckSelfIntersection();
        }

        private void CheckSelfIntersection()
        {
            var pts = Points;
            
            if (!DoSegmentsIntersect(pts[0], pts[2], pts[1], pts[3]))
                throw new ArgumentException("Чотирикутник має самоперетин.", nameof(_points));
        }

        private bool DoSegmentsIntersect(Point a, Point b, Point c, Point d)
        {
            double Cross(double x1, double y1, double x2, double y2) => x1 * y2 - y1 * x2;

            double x1 = a.X, y1 = a.Y;
            double x2 = b.X, y2 = b.Y;
            double x3 = c.X, y3 = c.Y;
            double x4 = d.X, y4 = d.Y;

            double d1 = Cross(x4 - x3, y4 - y3, x1 - x3, y1 - y3);
            double d2 = Cross(x4 - x3, y4 - y3, x2 - x3, y2 - y3);
            double d3 = Cross(x2 - x1, y2 - y1, x3 - x1, y3 - y1);
            double d4 = Cross(x2 - x1, y2 - y1, x4 - x1, y4 - y1);

            bool hasIntersection = d1 * d2 < 0 && d3 * d4 < 0;
            
            if (IsNearlyZero(d1) && IsPointOnSegment(x3, y3, x4, y4, x1, y1)) return true;
            if (IsNearlyZero(d2) && IsPointOnSegment(x3, y3, x4, y4, x2, y2)) return true;
            if (IsNearlyZero(d3) && IsPointOnSegment(x1, y1, x2, y2, x3, y3)) return true;
            if (IsNearlyZero(d4) && IsPointOnSegment(x1, y1, x2, y2, x4, y4)) return true;

            return hasIntersection;
        }

        private bool IsPointOnSegment(double sx1, double sy1, double sx2, double sy2, double px, double py)
        {
            double minX = Math.Min(sx1, sx2);
            double maxX = Math.Max(sx1, sx2);
            double minY = Math.Min(sy1, sy2);
            double maxY = Math.Max(sy1, sy2);

            if (px < minX || px > maxX || py < minY || py > maxY)
                return false;

            double cross = (px - sx1) * (sy2 - sy1) - (py - sy1) * (sx2 - sx1);
            return IsNearlyZero(cross);
        }

        public override string ToString() => $"Опуклий чотирикутник: {Points[0]}, {Points[1]}, {Points[2]}, {Points[3]}, площа = {GetArea():F4}";
    }

    /// <summary>
    /// Допоміжний клас для тестування.
    /// </summary>
    public static class PolygonTests
    {
        public static void RunAllTests()
        {
            TestValidTriangle();
            TestColinearPoints();
            TestDuplicatePoints();
            TestInvalidQuadrilateral();
            TestValidQuadrilateral();
            
            Console.WriteLine("Всі тести пройдено!");
        }

        private static void TestValidTriangle()
        {
            var triangle = Triangle.FromCoordinates(0, 0, 3, 0, 0, 4);
            double area = triangle.GetArea();
            
            if (Math.Abs(area - 6.0) > 1e-9)
                throw new Exception($"Невірна площа трикутника: {area}");
            
            Console.WriteLine("✓ Валідний трикутник");
        }

        private static void TestColinearPoints()
        {
            try
            {
                Triangle.FromCoordinates(0, 0, 2, 2, 4, 4);
                throw new Exception("Очікувалась помилка колінеарності");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("на одній прямій"))
            {
                Console.WriteLine("✓ Коректно виявлено колінеарні точки");
            }
        }

        private static void TestDuplicatePoints()
        {
            try
            {
                Triangle.FromCoordinates(0, 0, 0, 0, 3, 4);
                throw new Exception("Очікувалась помилка дублікатів");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("однакові точки"))
            {
                Console.WriteLine("✓ Коректно виявлено дублікати точок");
            }
        }

        private static void TestValidQuadrilateral()
        {
            var quad = ConvexQuadrilateral.FromCoordinates(0, 0, 4, 0, 4, 3, 0, 3);
            double area = quad.GetArea();
            
            if (Math.Abs(area - 12.0) > 1e-9)
                throw new Exception($"Невірна площа чотирикутника: {area}");
            
            Console.WriteLine("✓ Валідний опуклий чотирикутник");
        }

        private static void TestInvalidQuadrilateral()
        {
            try
            {
                ConvexQuadrilateral.FromCoordinates(0, 0, 3, 3, 0, 3, 3, 0);
                throw new Exception("Очікувалась помилка неопуклого чотирикутника");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("не є опуклим") || ex.Message.Contains("самоперетин"))
            {
                Console.WriteLine("✓ Коректно виявлено неопуклий чотирикутник");
            }
        }
    }

    /// <summary>
    /// Головний клас програми.
    /// </summary>
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Геометричні фігури");
            Console.WriteLine("==================\n");

            try
            {
                RunExamples();
                
                if (args.Length > 0 && args[0] == "--test")
                {
                    Console.WriteLine("\nЗапуск тестів...");
                    PolygonTests.RunAllTests();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
                return;
            }

            if (System.Environment.UserInteractive && args.Length == 0)
            {
                Console.WriteLine("\nНатисніть будь-яку клавішу...");
                Console.ReadKey();
            }
        }

        private static void RunExamples()
        {
            // Трикутник
            var triangle1 = new Triangle(
                new Point(0, 0),
                new Point(4, 0),
                new Point(0, 3));
            
            Console.WriteLine($"1. {triangle1}");

            // Трикутник через фабричний метод
            var triangle2 = Triangle.FromCoordinates(1, 1, 5, 1, 3, 4);
            Console.WriteLine($"2. {triangle2}");

            // Зміна вершин
            triangle2.SetVertices(new[] { new Point(0, 0), new Point(3, 0), new Point(1.5, 2) });
            Console.WriteLine($"3. Після зміни: {triangle2}");

            // Чотирикутник
            var quadrilateral = new ConvexQuadrilateral(
                new Point(0, 0),
                new Point(4, 0),
                new Point(3, 3),
                new Point(0, 4));
            
            Console.WriteLine($"4. {quadrilateral}");

            // Чотирикутник через фабричний метод
            var quadrilateral2 = ConvexQuadrilateral.FromCoordinates(1, 1, 5, 1, 6, 4, 2, 5);
            Console.WriteLine($"5. {quadrilateral2}");
        }
    }
}
