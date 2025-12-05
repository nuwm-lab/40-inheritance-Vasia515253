using System;

namespace LabWork
{
    // -------------------------------------------
    // Структура Point
    // -------------------------------------------
    public struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }

    // -------------------------------------------
    // Базовий клас Triangle
    // -------------------------------------------
    public class Triangle
    {
        protected Point A;
        protected Point B;
        protected Point C;

        public virtual void SetPoints(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;
        }

        public virtual void PrintPoints()
        {
            Console.WriteLine("Трикутник:");
            Console.WriteLine($"A = {A}");
            Console.WriteLine($"B = {B}");
            Console.WriteLine($"C = {C}");
        }

        public virtual double Area()
        {
            return Math.Abs(
                (A.X * (B.Y - C.Y) +
                 B.X * (C.Y - A.Y) +
                 C.X * (A.Y - B.Y)) / 2.0
            );
        }
    }

    // -------------------------------------------
    // Похідний клас ConvexQuadrilateral
    // -------------------------------------------
    public class ConvexQuadrilateral : Triangle
    {
        private Point D;

        public void SetPoints(Point a, Point b, Point c, Point d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public override void PrintPoints()
        {
            Console.WriteLine("Опуклий чотирикутник:");
            Console.WriteLine($"A = {A}");
            Console.WriteLine($"B = {B}");
            Console.WriteLine($"C = {C}");
            Console.WriteLine($"D = {D}");
        }

        // Площа чотирикутника = площа двох трикутників
        // ABC + ACD
        public override double Area()
        {
            double area1 = Math.Abs(
                (A.X * (B.Y - C.Y) +
                 B.X * (C.Y - A.Y) +
                 C.X * (A.Y - B.Y)) / 2.0
            );

            double area2 = Math.Abs(
                (A.X * (C.Y - D.Y) +
                 C.X * (D.Y - A.Y) +
                 D.X * (A.Y - C.Y)) / 2.0
            );

            return area1 + area2;
        }
    }

    // -------------------------------------------
    // Program
    // -------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            // --- Трикутник ---
            Triangle triangle = new Triangle();
            triangle.SetPoints(
                new Point(0, 0),
                new Point(4, 0),
                new Point(0, 3)
            );
            triangle.PrintPoints();
            Console.WriteLine($"Площа трикутника = {triangle.Area()}\n");

            // --- Опуклий чотирикутник ---
            ConvexQuadrilateral quad = new ConvexQuadrilateral();
            quad.SetPoints(
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
