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
    }

    /// <summary>
    /// Абстрактний полігон з методом обчислення площі (Shoelace).
    /// </summary>
    public abstract class Polygon
    {
        protected const double EPS = 1e-9;

        private readonly List<Point> _points = new List<Point>();

        /// <summary>
        /// Точки доступні тільки для читання.
        /// </summary>
        public IReadOnlyList<Point> Points => _points.AsReadOnly();

        /// <summary>
        /// Встановлення точок полігону з валідацією.
        /// </summary>
        protected void SetPoints(IEnumerable<Point> points)
        {
            _points.Clear();

            foreach (var p in points)
                _points.Add(p);

            ValidatePoints();
        }

        /// <summary>
        /// Перевірка коректності масиву точок.
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
        protected bool NearlyZero(double value) => Math.Abs(value) < EPS;

        public override string ToString() => $"Polygon: {Points.Count} вершин, площа = {GetArea():F4}";
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

                if (NearlyZero(cross))
                    throw new ArgumentException("Чотирикутник містить колінеарні точки.");

                bool current = cross > 0;

                if (sign == null)
                    sign = current;
                else if (sign != current)
                    throw new ArgumentException("Фігура не є опуклим чотирикутником.");
            }
        }
    }
}
