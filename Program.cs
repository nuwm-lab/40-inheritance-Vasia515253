using System;
using System.Collections.Generic;

namespace LabWork
{
    // ------------------------------------------------------------
    // Структура точки
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
    // Інтерфейс фігури
    // ------------------------------------------------------------
    public interface IShape
    {
        double Area();
        void PrintPoints();
    }

    // ------------------------------------------------------------
    // Абстрактний багатокутник
    // ------------------------------------------------------------
    public abstract class Polygon : IShape
    {
        private Point[] _points;
        private const double Epsilon = 1e-9;

        public IReadOnlyList<Point> Points => Array.AsReadOnly(_points);

        protected Polygon(params Point[] points)
        {
            SetPoints(points);
        }

        public virtual void SetPoints(params Point[] points)
        {
            if (points == null || points.Length < 3)
                throw new ArgumentException("Фігура повинна мати мінімум 3 вершини.");

            // Робимо копію щоб уникнути сторонньої модифікації
            _points = (Point[])points.Clone();
        }

        /// <summary>Площа методом Shoelace</summary>
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
                Console.WriteLine($"P{i + 1} = {_points[i]}");
        }

        protected bool NearlyZero(double value)
        {
            return Math.Abs(value) < Epsilon;
        }

        protected double Cross(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) -
                   (b.Y - a.Y) * (c.X - a.X);
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
            if (NearlyZero(Area()))
                throw new ArgumentException("Трикутник вироджений: точки лежать на одній прямій.");
        }

        private bool NearlyZero(double v) => Math.Abs(v) < 1e-9;
    }

    // ------------------------------------------------------------
    // Опуклий чотирикутник
    // ------------------------------------------------------------
    public class ConvexQuadrilateral : Polygon
    {
        public ConvexQuadrilateral(Point p1, Point p2, Point p3, Point p4)
            : base(p1, p2, p3, p4)
        {
            OrderVertices();
            ValidateConvexity();
        }

        /// <summary>
        /// Упорядковує вершини за кутом навколо центру мас.
        /// Гарантує правильний обхід для перевірки опуклості.
        /// </summary>
        private void OrderVertices()
        {
            var pts = new List<Point>(Points);

            // Центр мас
            double cx = 0, cy = 0;
            foreach (var p in pts)
            {
                cx += p.X;
                cy += p.Y;
            }
            cx /= pts.Count;
            cy /= pts.Count;

            pts.Sort((a, b) =>
            {
                double angA = Math.Atan2(a.Y - cy, a.X - cx);
                double angB = Math.Atan2(b.Y - cy, b.X - cx);
                return angA.CompareTo(angB);
            });

            SetPoints(pts.ToArray());
        }

        private void ValidateConvexity()
        {
            int n = Points.Count;
            bool? positive = null;

            for (int i = 0; i < n; i++)
            {
                Point a = Points[i];
                Point b = Points[(i + 1) % n];
                Point c = Points[(i + 2) % n];

                double cross = Cross(a, b, c);

                if (Math.Abs(cross) < 1e-9)
                    throw new ArgumentException("Вироджений чотирикутник: три точки колінеарні.");

                bool currentPositive = cross > 0;

                if (positive == null)
                    positive = currentPositive;
                else if (positive != currentPositive)
                    throw new ArgumentException("Чотирикутник не є опуклим.");
            }
        }
    }

    // ------------------------------------------------------------
    // Головна програма
    // ------------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            Triangle t = new Triangle(
                new Point(0, 0),
                new Point(4, 0),
                new Point(0, 3)
            );

            t.PrintPoints();
            Console.WriteLine($"Площа трикутника = {t.Area()}\n");

            ConvexQuadrilateral q = new ConvexQuadrilateral(
                new Point(0, 0),
                new Point(4, 0),
                new Point(5, 3),
                new Point(0, 3)
            );

            q.PrintPoints();
            Console.WriteLine($"Площа чотирикутника = {q.Area()}");
        }
    }
}
