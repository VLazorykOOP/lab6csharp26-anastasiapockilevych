using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Lab6_Tasks
{
    public interface IAnimalInfo { void ShowInfo(); }
    public interface IMovable { void Move(); }
    public interface IAnimalBase : IAnimalInfo, IMovable, ICloneable { }

    public abstract class Animal : IAnimalBase
    {
        public string Name { get; set; }
        public double Weight { get; set; }

        public Animal(string name, double weight)
        {
            Name = name;
            Weight = weight;
        }

        public abstract void ShowInfo();
        public abstract void Move();
        public virtual void SpecialBehavior() => Console.WriteLine($"  -> {Name} проявляє базову поведінку.");
        public object Clone() => this.MemberwiseClone();
        ~Animal() => Console.WriteLine($"[Деструктор] {Name} знищено.");
    }

    public class Mammal : Animal
    {
        public Mammal(string name, double weight) : base(name, weight) { }
        public override void ShowInfo() => Console.WriteLine($"[Савець] {Name}, Вага: {Weight} кг");
        public override void Move() => Console.WriteLine($"  -> {Name} біжить.");
        public void Nurse() => Console.WriteLine($"  -> {Name} годує малят молоком.");
    }

    public class Bird : Animal
    {
        public Bird(string name, double weight) : base(name, weight) { }
        public override void ShowInfo() => Console.WriteLine($"[Птах] {Name}, Вага: {Weight} кг");
        public override void Move() => Console.WriteLine($"  -> {Name} летить.");
        public void Fly() => Console.WriteLine($"  -> {Name} махає крилами.");
    }

    public class Artiodactyl : Mammal
    {
        public Artiodactyl(string name, double weight) : base(name, weight) { }
        public override void ShowInfo() => Console.WriteLine($"[Парнокопитне] {Name}, Вага: {Weight} кг");
        public void ChewCud() => Console.WriteLine($"  -> {Name} жує жуйку.");
    }

    [Serializable]
    public class CustomTransException : ApplicationException
    {
        public CustomTransException() : base() { }
        public CustomTransException(string message) : base(message) { }
        public CustomTransException(string message, Exception inner) : base(message, inner) { }
        protected CustomTransException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public interface ITrans : IComparable
    {
        void ShowInfo();
        double GetPayload();
        string GetBrand();
        string GetNumber();
    }

    public abstract class Trans : ITrans
    {
        public string Brand { get; set; }
        public string Number { get; set; }
        public double Speed { get; set; }
        protected double BasePayload { get; set; }

        public Trans(string brand, string number, double speed, double payload)
        {
            if (speed < 0) throw new CustomTransException("Швидкість не може бути від'ємною!");
            Brand = brand; Number = number; Speed = speed; BasePayload = payload;
        }

        public abstract void ShowInfo();
        public abstract double GetPayload();
        public string GetBrand() => Brand;
        public string GetNumber() => Number;

        public int CompareTo(object? obj)
        {
            if (obj is ITrans other) return this.GetPayload().CompareTo(other.GetPayload());
            throw new ArgumentException("Об'єкт не реалізує ITrans");
        }

        public static bool operator ==(Trans? left, Trans? right) => left?.Brand == right?.Brand && left?.Number == right?.Number;
        public static bool operator !=(Trans? left, Trans? right) => !(left == right);
        public override bool Equals(object? obj) => obj is Trans t && this == t;
        public override int GetHashCode() => HashCode.Combine(Brand, Number);
        ~Trans() => Console.WriteLine($"[Деструктор] {Brand} ({Number}) знищено.");
    }

    public class PassengerCar : Trans
    {
        public PassengerCar(string b, string n, double s, double p) : base(b, n, s, p) { }
        public override double GetPayload() => BasePayload;
        public override void ShowInfo() => Console.WriteLine($"Легкова: {Brand} | {Number} | Швидкість: {Speed} км/год | Вантаж: {GetPayload()} кг");
    }

    public class Motorcycle : Trans
    {
        public bool HasSidecar { get; set; }
        public Motorcycle(string b, string n, double s, double p, bool sidecar) : base(b, n, s, p) { HasSidecar = sidecar; }
        public override double GetPayload() => HasSidecar ? BasePayload : 0;
        public override void ShowInfo() => Console.WriteLine($"Мотоцикл: {Brand} | {Number} | Коляска: {HasSidecar} | Вантаж: {GetPayload()} кг");
    }

    public class Truck : Trans
    {
        public bool HasTrailer { get; set; }
        public Truck(string b, string n, double s, double p, bool trailer) : base(b, n, s, p) { HasTrailer = trailer; }
        public override double GetPayload() => HasTrailer ? BasePayload * 2 : BasePayload;
        public override void ShowInfo() => Console.WriteLine($"Вантажівка: {Brand} | {Number} | Причіп: {HasTrailer} | Вантаж: {GetPayload()} кг");
    }

    public class Garage : IEnumerable, IEnumerator
    {
        private ITrans[] vehicles = new ITrans[10];
        private int count = 0;
        private int position = -1;

        public void AddTrans(ITrans t)
        {
            if (count < vehicles.Length) vehicles[count++] = t;
        }

        public IEnumerator GetEnumerator() => this;
        public bool MoveNext() { position++; return position < count; }
        public void Reset() => position = -1;
        public object Current => vehicles[position];

        public ITrans this[int index]
        {
            get => index >= 0 && index < count ? vehicles[index] : throw new IndexOutOfRangeException("Індекс виходить за межі гаража.");
            set => vehicles[index] = value;
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("================= ЛАБОРАТОРНА РОБОТА №6 =================");

            RunTask1();
            RunTask2();
            RunTask3();
            RunTask4();

            Console.WriteLine("\nНатисніть Enter для завершення...");
            Console.ReadLine();
        }

        static void RunTask1()
        {
            Console.WriteLine("\n>>> ЗАВДАННЯ 1.14: Інтерфейси та Патерни типів <<<");
            IAnimalInfo[] animals = new IAnimalInfo[]
            {
                new Mammal("Тигр", 200),
                new Bird("Орел", 5),
                new Artiodactyl("Олень", 150)
            };

            Console.WriteLine("--- Виклик інтерфейсних методів ---");
            foreach (var animal in animals) animal.ShowInfo();

            Console.WriteLine("\n--- Виклик особистих методів (Type Pattern) ---");
            foreach (var animal in animals)
            {
                if (animal is Mammal mammal) mammal.Nurse();
                else if (animal is Bird bird) bird.Fly();

                if (animal is Artiodactyl artio) artio.ChewCud();
            }
        }

        static void RunTask2()
        {
            Console.WriteLine("\n>>> ЗАВДАННЯ 2.4: Ієрархія з інтерфейсами .NET (IComparable) <<<");
            ITrans[] vehicles = {
                new PassengerCar("Toyota", "AA1111", 180, 450),
                new Motorcycle("Yamaha", "BB2222", 220, 150, false),
                new Motorcycle("Дніпро", "CC3333", 110, 200, true),
                new Truck("Volvo", "DD4444", 120, 5000, true),
                new Truck("MAN", "EE5555", 110, 6000, false)
            };

            Console.WriteLine("--- До сортування ---");
            foreach (var v in vehicles) v.ShowInfo();

            Array.Sort(vehicles);

            Console.WriteLine("\n--- Після сортування (за вантажопідйомністю) ---");
            foreach (var v in vehicles) v.ShowInfo();

            double searchPayload = 500;
            Console.WriteLine($"\n--- Пошук: вантажопідйомність >= {searchPayload} кг ---");
            foreach (var v in vehicles)
            {
                if (v.GetPayload() >= searchPayload) v.ShowInfo();
            }
        }

        static void RunTask3()
        {
            Console.WriteLine("\n>>> ЗАВДАННЯ 3.7: Обробка винятків <<<");
            try
            {
                Console.WriteLine("Спроба створити авто з від'ємною швидкістю...");
                _ = new PassengerCar("Lada", "XX0000", -50, 400);
            }
            catch (CustomTransException ex)
            {
                Console.WriteLine($"[ПЕРЕХОПЛЕНО ВЛАСНИЙ ВИНЯТОК]: {ex.Message}");
            }

            try
            {
                Console.WriteLine("\nСпроба згенерувати StackOverflowException...");
                throw new StackOverflowException("Штучне переповнення стеку для демонстрації!");
            }
            catch (StackOverflowException ex)
            {
                Console.WriteLine($"[ПЕРЕХОПЛЕНО СТАНДАРТНИЙ ВИНЯТОК]: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ІНША ПОМИЛКА]: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Блок finally відпрацював успішно.");
            }
        }

        static void RunTask4()
        {
            Console.WriteLine("\n>>> ЗАВДАННЯ 4: IEnumerable та IEnumerator (foreach) <<<");
            Garage myGarage = new Garage();
            myGarage.AddTrans(new PassengerCar("Honda", "HH123", 200, 400));
            myGarage.AddTrans(new Truck("Scania", "SS999", 110, 8000, false));

            Console.WriteLine("--- Перебір гаража через foreach ---");
            foreach (ITrans t in myGarage) t.ShowInfo();

            Console.WriteLine("\n--- Демонстрація індексатора ---");
            if (myGarage[0] is ITrans first) Console.WriteLine($"Перший об'єкт у гаражі: {first.GetBrand()}");
        }
    }
}
