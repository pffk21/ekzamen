using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

class Program
{
    static List<User> users = new List<User>();
    static List<QuizResult> results = new List<QuizResult>();
    static User loggedInUser = null;
    static Dictionary<string, List<Voprosi>> quizVoprosi = new Dictionary<string, List<Voprosi>>();
   
    static void Main(string[] args)
    {
        Voprosiki();
        while (true)
            if (loggedInUser == null)
            {
                Console.WriteLine("Выберите действие: ");
                Console.WriteLine("1 - Войти ");
                Console.WriteLine("2 - Зарегистрироваться ");
                Console.WriteLine("3 - Выйти ");
                string userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        Registration();
                        break;
                    case "3":
                        Console.WriteLine("Выход");
                        break;
                }
            }
            else
            {
                UserMenu();
            }
    }
    static void Voprosiki()
    {
        foreach (var file in Directory.EnumerateFiles("voprosi", "*.txt"))
        {
            string category = Path.GetFileNameWithoutExtension(file);
            var questions = new List<Voprosi>();
            string[] lines = File.ReadAllLines(file); 
            for (int i = 0; i < lines.Length; i += 2) 
            {
                string text = lines[i]; 
                var correctAnswers = lines[i + 1].Split(',').Select(a => a.Trim()).ToList(); 
                questions.Add(new Voprosi(text, correctAnswers));
            }
            quizVoprosi[category] = questions;
        }
    }

    static void UserMenu()
    {
        if (loggedInUser == null) 
        {
            Console.WriteLine("Ошибка: вы не авторизованы.");
            return;
        }
        Console.WriteLine($"{loggedInUser.Login}, выберите действие:");
        Console.WriteLine("1 - Начать новую викторину");
        Console.WriteLine("2 - Результаты прошлых викторин");
        Console.WriteLine("3 - Топ 20 игроков");
        Console.WriteLine("4 - Изменить настройки");
        string choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                StartQuiz();
                break;
            case"2":
                Results();
                break;
            case "3":
                Top20();
                break;
            case "4":
                ChangeSettings();
                break;
        }
    }

    static void Registration()
        {
            Console.WriteLine("Регистрация нового пользователя");
            Console.WriteLine("Введите логин: ");
            string login = Console.ReadLine();
            
            if (users.Exists(user => user.Login == login))
            {
                Console.WriteLine("Логин уже занят");
                return;
            }
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();
            Console.Write("Введите дату рождения: ");
            string birthDate = Console.ReadLine();
            users.Add(new User(login, password, birthDate));
            Console.WriteLine("Регистрация закончена");
        }

        static void Login()
        {
            Console.WriteLine("Вход");
            Console.Write("Введите логин: ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();
            User user = users.Find(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                loggedInUser = user;
                Console.WriteLine($"Добро пожаловать, {user.Login}!");
                UserMenu();
                StartQuiz(); 
            }
            else
            {
                Console.WriteLine("Неверный логин или пароль");
            }
        }
        static void Results()
        {
            Console.WriteLine("Прошлые результаты: ");
            var userResult = results.Where(r => r.UserLogin == loggedInUser.Login);
            foreach (var result in userResult)
            {
                Console.WriteLine($"Викторина: {result.QuizName}, Результат: {result.Score}");

            }
        }
        static void Top20()
        {
            Console.WriteLine("Топ-20 игроков:");
            var topPlayers = results
                .OrderByDescending(r => r.Score)
                .Take(20);

            foreach (var result in topPlayers)
            {
                Console.WriteLine($"Пользователь: {result.UserLogin}, Результат: {result.Score} баллов");
            }
        }
        
        static void ChangeSettings()
        {
            Console.WriteLine("1 - Изменить пароль");
            Console.WriteLine("2 - Изменить дату рождения");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.Write("Введите новый пароль: ");
                    loggedInUser.Password = Console.ReadLine();
                    Console.WriteLine("Пароль изменен");
                    break;
                case "2":
                    Console.Write("Введите новую дату рождения: ");
                    loggedInUser.BirthDate = Console.ReadLine();
                    Console.WriteLine("Дата рождения  изменена");
                    break;
            }
        }


        static void StartQuiz()
        {
            Console.WriteLine("Выберите раздел викторины:");
            int index = 1;
            foreach (var category in quizVoprosi.Keys)
            {
                Console.WriteLine($"{index}. {category}");
                index++;
            }
            Console.WriteLine($"{index}. Смешанная викторина");

            Console.Write("Введите номер категории: ");
            if (!int.TryParse(Console.ReadLine(), out int selectedCategoryIndex) || 
                selectedCategoryIndex < 1 || selectedCategoryIndex > quizVoprosi.Keys.Count + 1)
            {
                Console.WriteLine("Неверный выбор категории");
                return;
            }

            string selectedCategory = selectedCategoryIndex == quizVoprosi.Keys.Count + 1 ? "Смешанная" : quizVoprosi.Keys.ElementAt(selectedCategoryIndex - 1);
            var selectedQuestions = selectedCategory == "Смешанная"
                ? quizVoprosi.Values.SelectMany(q => q).OrderBy(_ => Guid.NewGuid()).Take(20).ToList()
                : quizVoprosi.ContainsKey(selectedCategory) 
                    ? quizVoprosi[selectedCategory].OrderBy(_ => Guid.NewGuid()).Take(20).ToList() 
                    : null;

            if (selectedQuestions == null)
            {
                Console.WriteLine("Категория не найдена");
                return;
            }

            int score = 0;
            foreach (var question in selectedQuestions)
            {
                Console.WriteLine(question.Text);
                Console.WriteLine("Введите ваши ответы:");
                var userAnswers = Console.ReadLine()?.Split(',').Select(a => a.Trim()).ToArray();

                if (userAnswers != null && userAnswers.OrderBy(a => a).SequenceEqual(question.CorrectAnswers.OrderBy(a => a)))
                {
                    score++;
                }
            }
            Console.WriteLine($"Викторина завершена. Ваш результат: {score} из {selectedQuestions.Count}");
            results.Add(new QuizResult(loggedInUser.Login, score,selectedCategory));
        }
}

       
    class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string BirthDate { get; set; }

        public User(string login, string password, string birthDate)
        {
            Login = login;
            Password = password;
            BirthDate = birthDate;
        }
    }
    class QuizResult
    {
        public string UserLogin { get; }
        public int Score { get; }
        public string QuizName { get; }

        public QuizResult(string userLogin, int score,string quizName)
        {
            UserLogin = userLogin;
            Score = score;
            QuizName = quizName;
        }
    }
    
    class Voprosi
    {
        public string Text { get; }
        public List<string> CorrectAnswers { get; }

        public Voprosi(string text, List<string> correctAnswers)
        {
            Text = text;
            CorrectAnswers = correctAnswers;
        }
    }
