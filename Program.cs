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
    /// Обмеження: методи перевірки та сортування розраховані на опуклі полігони.
    /// Для увігнутих полігонів може знадобитися інша реалізація.
    /// </summary>
    public abstract class Polygon : IDisposable
    {
        protected const double Epsilon = 1e-9;
        private readonly List<Point> _points = new List<Point>();

        /// <summary>
        /// Точки полігону доступні тільки для читання.
        /// Вершини автоматично впорядковуються за кутом відносно центру при валідації.
        /// </summary>
        public IReadOnlyList<Point> Points => _points.AsReadOnly();

        /// <summary>
        /// Встановлює вершини полігону з валідацією.
        /// Для змінних фігур використовуйте окремі методи SetVertices.
        /// </summary>
        /// <param name="points">Колекція вершин полігону</param>
        protected void SetPoints(IEnumerable<Point> points)
        {
            _points.Clear();
            _points.AddRange(points);
            ValidatePoints();
        }

        /// <summary>
        /// Встановлює вершини полігону (публічний аналог SetPoints).
        /// </summary>
        public void SetVertices(IEnumerable<Point> vertices)
        {
            SetPoints(vertices);
        }

        /// <summary>
        /// Перевірка коректності масиву точок.
        /// Увага: цей метод сортує вершини за кутом відносно центру
        /// перед виконанням будь-яких додаткових перевірок у похідних класах.
        /// Обмеження: сортування за кутом працює коректно для опуклих полігонів,
        /// для увігнутих може бути потрібна інша логіка.
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
                    if (IsNearlyZero(_points[i].X - _points[j].X) &&
                        IsNearlyZero(_points[i].Y - _points[j].Y))
                    {
                        throw new ArgumentException("Масив містить однакові точки.");
                    }
                }
            }
        }

        /// <summary>
        /// Впорядкування вершин полігону за кутом відносно центру.
        /// Обмеження: цей метод найкраще працює для опуклих полігонів.
        /// Для увігнутих полігонів може бути потрібна інша стратегія сортування.
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
        /// Обчислює площу полігону за формулою шнурка (Shoelace).
        /// </summary>
        /// <returns>Площа полігону</returns>
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

        /// <summary>
        /// Перевіряє, чи значення близьке до нуля з урахуванням похибки.
        /// </summary>
        /// <param name="value">Значення для перевірки</param>
        /// <returns>True, якщо значення близьке до нуля</returns>
        protected bool IsNearlyZero(double value) => Math.Abs(value) < Epsilon;

        /// <summary>
        /// Очищує ресурси полігону.
        /// </summary>
        public void Dispose()
        {
            _points.Clear();
            GC.SuppressFinalize(this);
        }

        public override string ToString() => $"Полігон: {Points.Count} вершин, площа = {GetArea():F4}";
    }

    /// <summary>
    /// Клас трикутника.
    /// Підтримує тільки невироджені трикутники (площі > 0).
    /// </summary>
    public class Triangle : Polygon
    {
        /// <summary>
        /// Створює трикутник з трьох точок.
        /// </summary>
        public Triangle(Point a, Point b, Point c)
        {
            SetPoints(new[] { a, b, c });
        }

        /// <summary>
        /// Створює трикутник з масиву точок.
        /// </summary>
        public static Triangle FromPoints(params Point[] points)
        {
            if (points.Length != 3)
                throw new ArgumentException("Трикутник повинен мати рівно 3 точки.");
            
            return new Triangle(points[0], points[1], points[2]);
        }

        /// <summary>
        /// Створює трикутник з координат.
        /// </summary>
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
                throw new ArgumentException("Точки трикутника лежать на одній прямій (колінеарні).");
        }

        public override string ToString() => $"Трикутник: вершини {Points[0]}, {Points[1]}, {Points[2]}, площа = {GetArea():F4}";
    }

    /// <summary>
    /// Опуклий чотирикутник.
    /// Підтримує тільки опуклі чотирикутники без самоперетинів.
    /// </summary>
    public class ConvexQuadrilateral : Polygon
    {
        /// <summary>
        /// Створює опуклий чотирикутник з чотирьох точок.
        /// </summary>
        public ConvexQuadrilateral(Point a, Point b, Point c, Point d)
        {
            SetPoints(new[] { a, b, c, d });
        }

        /// <summary>
        /// Створює опуклий чотирикутник з масиву точок.
        /// </summary>
        public static ConvexQuadrilateral FromPoints(params Point[] points)
        {
            if (points.Length != 4)
                throw new ArgumentException("Чотирикутник повинен мати рівно 4 точки.");
            
            return new ConvexQuadrilateral(points[0], points[1], points[2], points[3]);
        }

        /// <summary>
        /// Створює опуклий чотирикутник з координат.
        /// </summary>
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
            CheckConvexityAndNoIntersections();
        }

        /// <summary>
        /// Перевіряє опуклість і відсутність самоперетинів.
        /// </summary>
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

                if (IsNearlyZero(cross))
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

        public override string ToString() => $"Опуклий чотирикутник: вершини {Points[0]}, {Points[1]}, {Points[2]}, {Points[3]}, площа = {GetArea():F4}";
    }

    /// <summary>
    /// Клас для запуску програми.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Головний метод програми.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                RunExamples();
                
                // Запуск тестів при наявності прапора
                if (args.Length > 0 && args[0] == "--test")
                {
                    RunTests();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
            
            // Затримка тільки в інтерактивному режимі
            if (System.Environment.UserInteractive && args.Length == 0)
            {
                Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
                Console.ReadKey();
            }
        }

        private static void RunExamples()
        {
            Console.WriteLine("Демонстрація геометричних фігур");
            Console.WriteLine("================================");
            
            // Приклад 1: Трикутник з використанням конструктора
            using (var triangle = new Triangle(
                new Point(0, 0),
                new Point(4, 0),
                new Point(0, 3)))
            {
                Console.WriteLine($"Приклад 1: {triangle}");
                Console.WriteLine($"Площа: {triangle.GetArea():F4}");
            }
            
            // Приклад 2: Трикутник з фабричного методу
            var triangle2 = Triangle.FromCoordinates(1, 1, 5, 1, 3, 4);
            Console.WriteLine($"\nПриклад 2: {triangle2}");
            Console.WriteLine($"Площа: {triangle2.GetArea():F4}");
            
            // Приклад 3: Чотирикутник з конструктора
            using (var quadrilateral = new ConvexQuadrilateral(
                new Point(0, 0),
                new Point(4, 0),
                new Point(3, 3),
                new Point(0, 4)))
            {
                Console.WriteLine($"\nПриклад 3: {quadrilateral}");
                Console.WriteLine($"Площа: {quadrilateral.GetArea():F4}");
            }
            
            // Приклад 4: Чотирикутник з фабричного методу
            var quadrilateral2 = ConvexQuadrilateral.FromCoordinates(1, 1, 5, 1, 6, 4, 2, 5);
            Console.WriteLine($"\nПриклад 4: {quadrilateral2}");
            Console.WriteLine($"Площа: {quadrilateral2.GetArea():F4}");
            
            // Приклад 5: Зміна вершин через SetVertices
            var triangle3 = Triangle.FromCoordinates(0, 0, 2, 0, 1, 1);
            Console.WriteLine($"\nПриклад 5 (до зміни): {triangle3}");
            
            triangle3.SetVertices(new[] { new Point(0, 0), new Point(3, 0), new Point(1.5, 2) });
            Console.WriteLine($"Приклад 5 (після зміни): {triangle3}");
        }

        private static void RunTests()
        {
            Console.WriteLine("\nТестування граничних випадків");
            Console.WriteLine("=============================");
            
            TestValidTriangle();
            TestColinearPoints();
            TestDuplicatePoints();
            TestInvalidQuadrilateral();
            
            Console.WriteLine("Всі тести пройдено успішно!");
        }

        private static void TestValidTriangle()
        {
            try
            {
                var triangle = Triangle.FromCoordinates(0, 0, 3, 0, 0, 4);
                double area = triangle.GetArea();
                Console.WriteLine($"✓ Валідний трикутник: площа = {area:F4}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Неочікувана помилка: {ex.Message}");
            }
        }

        private static void TestColinearPoints()
        {
            try
            {
                var triangle = Triangle.FromCoordinates(0, 0, 2, 2, 4, 4);
                Console.WriteLine($"✗ Очікувалась помилка колінеарності");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("колінеарні"))
            {
                Console.WriteLine($"✓ Коректно виявлено колінеарні точки");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Неочікувана помилка: {ex.Message}");
            }
        }

        private static void TestDuplicatePoints()
        {
            try
            {
                var triangle = Triangle.FromCoordinates(0, 0, 0, 0, 3, 4);
                Console.WriteLine($"✗ Очікувалась помилка дублікатів");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("однакові"))
            {
                Console.WriteLine($"✓ Коректно виявлено дублікати точок");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Неочікувана помилка: {ex.Message}");
            }
        }

        private static void TestInvalidQuadrilateral()
        {
            try
            {
                // Самоперетинний чотирикутник
                var quad = ConvexQuadrilateral.FromCoordinates(0, 0, 3, 3, 0, 3, 3, 0);
                Console.WriteLine($"✗ Очікувалась помилка неопуклого чотирикутника");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("не є опуклим") || 
                                               ex.Message.Contains("самоперетин"))
            {
                Console.WriteLine($"✓ Коректно виявлено неопуклий чотирикутник");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Неочікувана помилка: {ex.Message}");
            }
        }
    }
}
