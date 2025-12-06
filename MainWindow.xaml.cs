using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace WarehouseApp
{
    public partial class MainWindow : Window
    {
        SQLiteConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            ConnectDB();
            ShowProducts_Click(null, null);
        }

        void ConnectDB()
        {
            try
            {
                connection = new SQLiteConnection("Data Source=warehouse.db");
                connection.Open();

                // создание таблиц
                ExecuteSQL("CREATE TABLE IF NOT EXISTS Categories (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT)");
                ExecuteSQL("CREATE TABLE IF NOT EXISTS Suppliers (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Phone TEXT)");
                ExecuteSQL("CREATE TABLE IF NOT EXISTS Products (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, CategoryId INTEGER, Quantity INTEGER DEFAULT 0)");
                ExecuteSQL("CREATE TABLE IF NOT EXISTS Income (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductId INTEGER, SupplierId INTEGER, Quantity INTEGER, Date TEXT DEFAULT CURRENT_TIMESTAMP)");
                ExecuteSQL("CREATE TABLE IF NOT EXISTS Outcome (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductId INTEGER, Quantity INTEGER, Date TEXT DEFAULT CURRENT_TIMESTAMP)");

                // тестовые данные
                if (IsTableEmpty("Categories"))
                {
                    ExecuteSQL("INSERT INTO Categories (Name) VALUES ('Электроника'), ('Одежда'), ('Еда')");
                    ExecuteSQL("INSERT INTO Suppliers (Name, Phone) VALUES ('Иван', '123'), ('Петр', '456')");
                    ExecuteSQL("INSERT INTO Products (Name, CategoryId, Quantity) VALUES ('Ноутбук', 1, 10), ('Футболка', 2, 20), ('Хлеб', 3, 30)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}");
            }
        }

        bool IsTableEmpty(string tableName)
        {
            try
            {
                using (var cmd = new SQLiteCommand($"SELECT COUNT(*) FROM {tableName}", connection))
                {
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result) == 0;
                }
            }
            catch
            {
                return true;
            }
        }

        void ExecuteSQL(string sql)
        {
            try
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка SQL: {ex.Message}\nЗапрос: {sql}");
            }
        }

        // показ товаров
        private void ShowProducts_Click(object sender, RoutedEventArgs e)
        {
            string sql = "SELECT p.Id, p.Name, c.Name as Категория, p.Quantity FROM Products p LEFT JOIN Categories c ON p.CategoryId = c.Id";
            ShowData(sql);
        }

        // показ категорий
        private void ShowCategories_Click(object sender, RoutedEventArgs e)
        {
            ShowData("SELECT * FROM Categories");
        }

        // показ поставщиков
        private void ShowSuppliers_Click(object sender, RoutedEventArgs e)
        {
            ShowData("SELECT * FROM Suppliers");
        }

        void ShowData(string sql)
        {
            try
            {
                using (var adapter = new SQLiteDataAdapter(sql, connection))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    DataGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        // приход товара
        private void Income_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecuteSQL("INSERT INTO Income (ProductId, SupplierId, Quantity, Date) VALUES (1, 1, 10, datetime('now'))");
                ExecuteSQL("UPDATE Products SET Quantity = Quantity + 10 WHERE Id = 1");
                MessageBox.Show("Приход добавлен!");
                ShowProducts_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка прихода: {ex.Message}");
            }
        }

        // расход товара
        private void Outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecuteSQL("INSERT INTO Outcome (ProductId, Quantity, Date) VALUES (1, 5, datetime('now'))");
                ExecuteSQL("UPDATE Products SET Quantity = Quantity - 5 WHERE Id = 1");
                MessageBox.Show("Расход добавлен!");
                ShowProducts_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расхода: {ex.Message}");
            }
        }

        // обновление данных
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ShowProducts_Click(null, null);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}