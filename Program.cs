using System;

namespace LabWork
{
    class Triangle
    {
        protected (double x, double y)[] vertices = new (double x, double y)[3];

        public virtual void SetVertices((double x, double y) a,
                                        (double x, double y) b,
                                        (double x, double y) c)
        {
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public virtual void PrintVertices()
        {
            Console.WriteLine("Вершини трикутника:");
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"V{i + 1}: ({vertices[i].x}, {vertices[i].y})");
            }
        }

        public virtual double GetArea()
        {
            // Формула площі трикутника через координати
            double x1 = vertices[0].x, y1 = vertices[0].y;
            double x2 = vertices[1].x, y2 = vertices[1].y;
            double x3 = vertices[2].x, y3 = vertices[2].y;

            return Math.Abs(
                x1 * (y2 - y3) +
                x2 * (y3 - y1) +
                x3 * (y1 - y2)
            ) / 2.0;
        }
    }

    class ConvexQuadrilateral : Triangle
    {
        public ConvexQuadrilateral()
        {
            vertices = new (double x, double y)[4];
        }

        public void SetVertices((double x, double y) a,
                                (double x, double y) b,
                                (double x, double y) c,
                                (double x, double y) d)
        {
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
            vertices[3] = d;
        }

        public override void PrintVertices()
        {
            Console.WriteLine("Вершини опуклого чотирикутника:");
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine($"V{i + 1}: ({vertices[i].x}, {vertices[i].y})");
            }
        }

        public override double GetArea()
        {
            // Площа опуклого чотирикутника: розбиваємо на два трикутники
            var tri1 = new Triangle();
            tri1.SetVertices(vertices[0], vertices[1], vertices[2]);

            var tri2 = new Triangle();
            tri2.SetVertices(vertices[0], vertices[2], vertices[3]);

            return tri1.GetArea() + tri2.GetArea();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // --- Трикутник ---
            Triangle triangle = new Triangle();
            triangle.SetVertices((0, 0), (4, 0), (0, 3));

            triangle.PrintVertices();
            Console.WriteLine($"Площа трикутника: {triangle.GetArea()}\n");

            // --- Опуклий чотирикутник ---
            ConvexQuadrilateral quad = new ConvexQuadrilateral();
            quad.SetVertices((0, 0), (4, 0), (5, 3), (0, 4));

            quad.PrintVertices();
            Console.WriteLine($"Площа чотирикутника: {quad.GetArea()}\n");
        }
    }
}
