using System;

namespace LabWork
{
    class Practicant
    {
        public string Surname { get; private set; }
        public string Name { get; private set; }
        public string University { get; private set; }

        // java 

        public void SetData(string surname, string name, string university)
        {
            Surname = surname;
            Name = name;
            University = university;
        }

        // Метод перевірки чи прізвище симетричне: наприклад "Анна", "Оно"
        public bool IsSurnameSymmetric()
        {
            if (string.IsNullOrWhiteSpace(Surname))
                return false;

            string lower = Surname.ToLower();
            char[] arr = lower.ToCharArray();
            Array.Reverse(arr);
            string reversed = new string(arr);

            return lower == reversed;
        }
    }

    class FirmEmployee : Practicant
    {
        public DateTime HireDate { get; private set; }
        public string FinishedEducation { get; private set; }
        public string Position { get; private set; }

        public void SetData(
            string surname,
            string name,
            string university,
            DateTime hireDate,
            string finishedEducation,
            string position)
        {
            base.SetData(surname, name, university);

            HireDate = hireDate;
            FinishedEducation = finishedEducation;
            Position = position;
        }

        public int GetWorkExperienceYears()
        {
            DateTime now = DateTime.Now;
            int years = now.Year - HireDate.Year;

            if (HireDate > now.AddYears(-years))
            {
                years--;
            }

            return years;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Створюємо практиканта
            Practicant practicant = new Practicant();
            practicant.SetData("Анна", "Ігорівна", "НУВГП");

            Console.WriteLine("Практикант:");
            Console.WriteLine($"Прізвище симетричне? {practicant.IsSurnameSymmetric()}");

            // Створюємо працівника фірми
            FirmEmployee employee = new FirmEmployee();
            employee.SetData(
                "Шевченко",
                "Олександр",
                "КПІ",
                new DateTime(2020, 5, 10),
                "КПІ",
                "Інженер");

            Console.WriteLine("\nПрацівник фірми:");
            Console.WriteLine($"Стаж роботи: {employee.GetWorkExperienceYears()} років");
        }
    }
}
