using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Geometry
{
    /// <summary>
    /// Представляє 2D точку.
    /// </summary>
    public struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X:F2}, {Y:F2})";
    }

    /// <summary>
    /// Абстрактний полігон з методом обчислення площі (Shoelace).
    /// </summary>
    public abstract class Polygon
    {
        protected const double Epsilon = 1e-9;
        private readonly List<Point> _points = new List<Point>();
        private ReadOnlyCollection<Point> _readOnlyPoints;

        /// <summary>
        /// Точки доступні тільки для читання.
        /// </summary>
        public IReadOnlyList<Point> Points
        {
            get
            {
                if (_readOnlyPoints == null)
                {
                    _readOnlyPoints = _points.AsReadOnly();
                }
                return _readOnlyPoints;
            }
        }

        /// <summary>
        /// Встановлення точок полігону з валідацією.
        /// </summary>
        protected void SetPoints(IEnumerable<Point> points)
        {
            _points.Clear();
            _readOnlyPoints = null;

            foreach (var p in points)
                _points.Add(p);

            ValidatePoints();
        }

        /// <summary>
        /// Перевірка коректності масиву точок.
        /// Увага: цей метод сортує вершини за кутом відносно центру
        /// перед виконанням будь-яких додаткових перевірок у похідних класах.
        /// </summary>
        protected virtual void ValidatePoints()
        {
            if (_points.Count < 3)
                throw new ArgumentException("Полігон повинен мати мінімум 3 точки.");

            CheckDuplicates();
            OrderVertices();
        }

        /// <summary>
        /// Перевірка на дублікати точок.
        /// </summary>
        private void CheckDuplicates()
        {
            for (int i = 0; i < _points.Count; i++)
            {
                for (int j = i + 1; j < _points.Count; j++)
                {
                    if (NearlyZero(_points[i].X - _points[j].X) &&
                        NearlyZero(_points[i].Y - _points[j].Y))
                    {
                        throw new ArgumentException("Масив містить однакові точки.");
                    }
                }
            }
        }

        /// <summary>
        /// Впорядкування вершин полігону за кутом відносно центру.
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

        /// <summary>
        /// Алгоритм "Shoelace" для обчислення площі.
        /// </summary>
        public double GetArea()
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

        /// <summary>
        /// Порівняння з нулем з похибкою.
        /// </summary>
        protected bool NearlyZero(double value) => Math.Abs(value) < Epsilon;

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

            if (NearlyZero(area2))
                throw new ArgumentException("Точки трикутника лежать на одній прямій (колінеарні).");
        }

        public override string ToString() => $"Трикутник: вершини {Points[0]}, {Points[1]}, {Points[2]}, площа = {GetArea():F4}";
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

        protected override void ValidatePoints()
        {
            base.ValidatePoints();
            CheckConvexityAndNoIntersections();
        }

        private void CheckConvexityAndNoIntersections()
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

                if (NearlyZero(cross))
                    throw new ArgumentException("Чотирикутник містить колінеарні точки.");

                bool current = cross > 0;

                if (sign == null)
                    sign = current;
                else if (sign != current)
                    throw new ArgumentException("Фігура не є опуклим чотирикутником.");
            }

            // Додаткова перевірка на самоперетин (перетин несуміжних сторін)
            CheckSelfIntersection();
        }

        private void CheckSelfIntersection()
        {
            var pts = Points;
            
            // Перевіряємо перетин діагоналей (діагоналі в опуклому чотирикутнику мають перетинатися)
            if (!DoSegmentsIntersect(pts[0], pts[2], pts[1], pts[3]))
                throw new ArgumentException("Чотирикутник має самоперетин або не є опуклим.");
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
            
            // Додатково перевіряємо вироджені випадки
            if (NearlyZero(d1) && IsPointOnSegment(x3, y3, x4, y4, x1, y1)) return true;
            if (NearlyZero(d2) && IsPointOnSegment(x3, y3, x4, y4, x2, y2)) return true;
            if (NearlyZero(d3) && IsPointOnSegment(x1, y1, x2, y2, x3, y3)) return true;
            if (NearlyZero(d4) && IsPointOnSegment(x1, y1, x2, y2, x4, y4)) return true;

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
            return NearlyZero(cross);
        }

        public override string ToString() => $"Опуклий чотирикутник: вершини {Points[0]}, {Points[1]}, {Points[2]}, {Points[3]}, площа = {GetArea():F4}";
    }

    /// <summary>
    /// Клас для запуску програми.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Геометричні фігури");
            Console.WriteLine("==================");
            
            try
            {
                // Створення трикутника
                var triangle = new Triangle(
                    new Point(0, 0),
                    new Point(4, 0),
                    new Point(0, 3)
                );
                
                Console.WriteLine(triangle);
                Console.WriteLine($"Площа трикутника: {triangle.GetArea():F4}");
                Console.WriteLine();
                
                // Створення опуклого чотирикутника
                var quadrilateral = new ConvexQuadrilateral(
                    new Point(0, 0),
                    new Point(4, 0),
                    new Point(3, 3),
                    new Point(0, 4)
                );
                
                Console.WriteLine(quadrilateral);
                Console.WriteLine($"Площа чотирикутника: {quadrilateral.GetArea():F4}");
                Console.WriteLine();
                
                // Додатковий приклад
                Console.WriteLine("Додатковий приклад:");
                var quad2 = new ConvexQuadrilateral(
                    new Point(1, 1),
                    new Point(5, 1),
                    new Point(6, 4),
                    new Point(2, 5)
                );
                Console.WriteLine(quad2);
                Console.WriteLine($"Площа: {quad2.GetArea():F4}");
                
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неочікувана помилка: {ex.Message}");
            }
            
            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
