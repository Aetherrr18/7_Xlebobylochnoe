using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _7_CheckSnils
{
    /// <summary>
    /// Логика взаимодействия для ValidDate.xaml
    /// </summary>
    public partial class ValidDate : Window
    {
        private string Snils = "";

        public ValidDate()
        {
            InitializeComponent();
        }

        private async void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            // Пробуем получить данные от API
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Ссылка на API
                    string url = "http://localhost:4444/TransferSimulator/snils";
                    string jsonAnswer = await client.GetStringAsync(url);
                    jsonAnswer = jsonAnswer.Replace("{", "");
                    jsonAnswer = jsonAnswer.Replace("}", "");
                    jsonAnswer = jsonAnswer.Replace("\"", "");
                    jsonAnswer = jsonAnswer.Replace("value :", "");
                    // Убираем пробелы по краям
                    Snils = jsonAnswer.Trim();
                    txtBoxFullNameText.Text = Snils;
                    txtBoxResultText.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка API", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем есть ли вообще СНИЛС
            if (string.IsNullOrWhiteSpace(Snils))
            {
                txtBoxResultText.Text = "Сначала получите данные";
                return;
            }

            // Строка с запрещенными символами
            string forbiddenSymbols = "!@#$%^&*():;_+=[]{}<>?/|\\&";
            // Если количество совпадений больше 0 - значит запрещенные символы найдены
            if (Snils.Intersect(forbiddenSymbols).Count() > 0)
            {
                txtBoxResultText.Text = "СНИЛС содержит запрещенные символы";
                return;
            }
            // Если все проверки пройдены
            txtBoxResultText.Text = "СНИЛС корректен";
        }
    }
}
