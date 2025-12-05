using System;
using System.Linq;

namespace LabWork
{
    // ------------------------------------------------------------
    // Структура для точки
    // ------------------------------------------------------------
    public struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X}, {Y})";
    }

    // ------------------------------------------------------------
    // Інтерфейс для геометричних фігур
    // ------------------------------------------------------------
    public interface IShape
    {
        double Area();
        void PrintPoints();
    }

    // ------------------------------------------------------------
    // Базовий клас багатокутника
    // ------------------------------------------------------------
    public abstract class Polygon : IShape
    {
        protected Point[] _points;

        public IReadOnlyList<Point> Points => _points;

        protected Polygon(params Point[] points)
        {
            SetPoints(points);
        }

        public virtual void SetPoints(params Point[] points)
        {
            if (points == null || points.Length < 3)
                throw new ArgumentException("Багатокутник повинен мати мінімум 3 вершини.");

            _points = points;
        }

        /// <summary>
        /// Формула «Shoelace» (Гаусса) для площі багатокутника
        /// </summary>
        public virtual double Area()
        {
            double sum = 0;

            for (int i = 0; i < _points.Length; i++)
            {
                int j = (i + 1) % _points.Length;
                sum += _points[i].X * _points[j].Y - _points[j].X * _points[i].Y;
            }

            return Math.Abs(sum) / 2.0;
        }

        public virtual void PrintPoints()
        {
            Console.WriteLine($"{GetType().Name}:");
            for (int i = 0; i < _points.Length; i++)
            {
                Console.WriteLine($"P{i + 1} = {_points[i]}");
            }
        }
    }

    // ------------------------------------------------------------
    // Трикутник
    // ------------------------------------------------------------
    public class Triangle : Polygon
    {
        public Triangle(Point p1, Point p2, Point p3)
            : base(p1, p2, p3)
        {
            Validate();
        }

        private void Validate()
        {
            double area = Area();
            if (area == 0)
                throw new ArgumentException("Точки трикутника лежать на одній прямій (площа = 0).");
        }
    }

    // ------------------------------------------------------------
    // Опуклий чотирикутник
    // ------------------------------------------------------------
    public class ConvexQuadrilateral : Polygon
    {
        public ConvexQuadrilateral(Point p1, Point p2, Point p3, Point p4)
            : base(p1, p2, p3, p4)
        {
            ValidateConvex();
        }

        private void ValidateConvex()
        {
            // Перевірка опуклості за знаком псевдоскалярного добутку
            int n = _points.Length;
            bool? isPositive = null;

            for (int i = 0; i < n; i++)
            {
                Point a = _points[i];
                Point b = _points[(i + 1) % n];
                Point c = _points[(i + 2) % n];

                double cross = CrossProduct(a, b, c);

                if (cross == 0)
                    throw new ArgumentException("Чотирикутник вироджений (три вершини лежать на одній лінії).");

                bool current = cross > 0;

                if (isPositive == null)
                    isPositive = current;
                else if (isPositive != current)
                    throw new ArgumentException("Чотирикутник не опуклий.");
            }
        }

        private static double CrossProduct(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) -
                   (b.Y - a.Y) * (c.X - a.X);
        }
    }

    // ------------------------------------------------------------
    // Програма
    // ------------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            // ---------------- Трикутник ----------------
            Triangle triangle = new Triangle(
                new Point(0, 0),
                new Point(4, 0),
                new Point(0, 3)
            );

            triangle.PrintPoints();
            Console.WriteLine($"Площа трикутника = {triangle.Area()}\n");

            // ---------------- Опуклий чотирикутник ----------------
            ConvexQuadrilateral quad = new ConvexQuadrilateral(
                new Point(0, 0),
                new Point(4, 0),
                new Point(5, 3),
                new Point(0, 3)
            );

            quad.PrintPoints();
            Console.WriteLine($"Площа чотирикутника = {quad.Area()}");
        }
    }
}
