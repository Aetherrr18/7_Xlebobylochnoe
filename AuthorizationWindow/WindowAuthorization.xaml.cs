using _7_Xlebobylochnoe.ApplicationDateBase;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace _7_Xlebobylochnoe.AuthorizationWindow
{
    /// <summary>
    /// Логика взаимодействия для WindowAuthorization.xaml
    /// </summary>
    public partial class WindowAuthorization : Window
    {
        private int _captchaFailedAttempts = 0;
        private const int MaxAttempts = 3;
        private string _tempLogin;
        private string _tempPassword;
        private users _currentUser;

        private Dictionary<int, ImageSource> _puzzlePieces;
        private Dictionary<Border, int> _currentPlacement;
        private Border _draggedFromBorder;

        public WindowAuthorization()
        {
            InitializeComponent();
            AppConnect.modelBD = DBXlebobylochnoeEntities.GetContext();
        }

        private void AuthorizationButton_Click(object sender, RoutedEventArgs e)
        {
            string loginUser = loginBox.Text.Trim();
            string passwordUser = passwordBox.Password;

            if (string.IsNullOrEmpty(loginUser) || string.IsNullOrEmpty(passwordUser))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentUser = AppConnect.modelBD.users
                .FirstOrDefault(u => u.login == loginUser && u.password == passwordUser);

            if (_currentUser == null)
            {
                var userToBlock = AppConnect.modelBD.users
                    .FirstOrDefault(u => u.login == loginUser);

                if (userToBlock != null)
                {
                    userToBlock.failed_attempts = (userToBlock.failed_attempts ?? 0) + 1;

                    if (userToBlock.failed_attempts >= MaxAttempts)
                    {
                        userToBlock.is_blocked = true;
                    }
                    AppConnect.modelBD.SaveChanges();

                    if (userToBlock.is_blocked == true)
                    {
                        MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetForm();
                        return;
                    }
                }

                int attemptsLeft = MaxAttempts - (userToBlock?.failed_attempts ?? 0);
                MessageBox.Show($"Вы ввели неверный логин или пароль. Осталось попыток: {attemptsLeft}",
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);

                passwordBox.Clear();
                loginBox.Focus();
                return;
            }

            if (_currentUser.is_blocked == true)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetForm();
                return;
            }

            _currentUser.failed_attempts = 0;
            AppConnect.modelBD.SaveChanges();

            _tempLogin = loginUser;
            _tempPassword = passwordUser;

            authPanel.Visibility = Visibility.Collapsed;
            captchaPanel.Visibility = Visibility.Visible;

            _captchaFailedAttempts = 0;
            GeneratePuzzleCaptcha();
        }

        private void GeneratePuzzleCaptcha()
        {
            _puzzlePieces = new Dictionary<int, ImageSource>();
            _currentPlacement = new Dictionary<Border, int>();
            _draggedFromBorder = null;

            try
            {
                for (int i = 0; i < 4; i++)
                {
                    string packUri = $"pack://application:,,,/7_Xlebobylochnoe;component/Resource/{i + 1}.png";
                    _puzzlePieces[i] = LoadImageFromPackUri(packUri);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображений: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var random = new Random();
            var shuffledPieces = _puzzlePieces.Keys.OrderBy(x => random.Next()).ToList();

            puzzlePiecesPanel.Children.Clear();
            ClearTargetCells();

            foreach (int pieceNumber in shuffledPieces)
            {
                Border pieceBorder = CreatePieceBorder(_puzzlePieces[pieceNumber], pieceNumber);
                puzzlePiecesPanel.Children.Add(pieceBorder);
            }

            attemptsText.Text = $"Осталось попыток: {MaxAttempts - _captchaFailedAttempts}";
        }

        private ImageSource LoadImageFromPackUri(string uriString)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uriString, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private Border CreatePieceBorder(ImageSource source, int pieceNumber)
        {
            Image pieceImage = new Image
            {
                Source = source,
                Width = 100,
                Height = 100,
                Margin = new Thickness(5),
                Cursor = Cursors.Hand,
                Tag = pieceNumber,
                Stretch = Stretch.UniformToFill
            };

            Border border = new Border
            {
                Child = pieceImage,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                Background = Brushes.White,
                Tag = pieceNumber
            };

            // Добавляем обработчики для drag
            border.MouseLeftButtonDown += PieceBorder_MouseLeftButtonDown;
            border.AllowDrop = true;
            border.DragOver += PieceBorder_DragOver;
            border.Drop += PieceBorder_Drop;

            return border;
        }

        private void ClearTargetCells()
        {
            targetCell0.Child = null;
            targetCell1.Child = null;
            targetCell2.Child = null;
            targetCell3.Child = null;

            // Добавляем обработчики для target ячеек
            targetCell0.AllowDrop = true;
            targetCell1.AllowDrop = true;
            targetCell2.AllowDrop = true;
            targetCell3.AllowDrop = true;

            targetCell0.DragOver += TargetCell_DragOver;
            targetCell1.DragOver += TargetCell_DragOver;
            targetCell2.DragOver += TargetCell_DragOver;
            targetCell3.DragOver += TargetCell_DragOver;

            targetCell0.Drop += TargetCell_Drop;
            targetCell1.Drop += TargetCell_Drop;
            targetCell2.Drop += TargetCell_Drop;
            targetCell3.Drop += TargetCell_Drop;

            _currentPlacement.Clear();
        }

        // Обработчики для кусочков в панели
        private void PieceBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Child is Image image)
            {
                _draggedFromBorder = border;
                DragDrop.DoDragDrop(border, border, DragDropEffects.Move);
            }
        }

        private void PieceBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void PieceBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Border)) is Border draggedBorder &&
                sender is Border targetBorder && draggedBorder != targetBorder)
            {
                // Меняем местами кусочки
                var parentPanel = draggedBorder.Parent as Panel;
                var targetParentPanel = targetBorder.Parent as Panel;

                if (parentPanel != null && targetParentPanel != null)
                {
                    int draggedIndex = parentPanel.Children.IndexOf(draggedBorder);
                    int targetIndex = targetParentPanel.Children.IndexOf(targetBorder);

                    parentPanel.Children.Remove(draggedBorder);
                    targetParentPanel.Children.Remove(targetBorder);

                    parentPanel.Children.Insert(targetIndex, draggedBorder);
                    targetParentPanel.Children.Insert(draggedIndex, targetBorder);
                }
            }
            e.Handled = true;
        }

        // Обработчики для целевых ячеек
        private void TargetCell_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void TargetCell_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Border)) is Border draggedBorder && sender is Border targetCell)
            {
                // Если в ячейке уже есть кусочек, возвращаем его в панель
                if (targetCell.Child != null)
                {
                    var existingBorder = CreatePieceBorder(((Image)targetCell.Child).Source,
                        _currentPlacement[targetCell]);
                    puzzlePiecesPanel.Children.Add(existingBorder);
                }

                // Перемещаем кусочек в ячейку
                int pieceNumber = (int)draggedBorder.Tag;

                Image newImage = new Image
                {
                    Source = ((Image)draggedBorder.Child).Source,
                    Width = 120,
                    Height = 120,
                    Stretch = Stretch.UniformToFill
                };

                targetCell.Child = newImage;
                _currentPlacement[targetCell] = pieceNumber;

                // Удаляем из исходной панели
                if (draggedBorder.Parent is Panel parentPanel)
                {
                    parentPanel.Children.Remove(draggedBorder);
                }
            }
            e.Handled = true;
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlacement.Count != 4)
            {
                MessageBox.Show("Соберите все 4 части пазла!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isCorrect = true;

            if (_currentPlacement.ContainsKey(targetCell0) && _currentPlacement[targetCell0] != 0)
                isCorrect = false;
            if (_currentPlacement.ContainsKey(targetCell1) && _currentPlacement[targetCell1] != 1)
                isCorrect = false;
            if (_currentPlacement.ContainsKey(targetCell2) && _currentPlacement[targetCell2] != 2)
                isCorrect = false;
            if (_currentPlacement.ContainsKey(targetCell3) && _currentPlacement[targetCell3] != 3)
                isCorrect = false;

            if (!isCorrect)
            {
                _captchaFailedAttempts++;

                if (_captchaFailedAttempts >= MaxAttempts)
                {
                    _currentUser.is_blocked = true;
                    _currentUser.failed_attempts = MaxAttempts;
                    AppConnect.modelBD.SaveChanges();

                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
                        "Блокировка", MessageBoxButton.OK, MessageBoxImage.Error);

                    ReturnToAuthForm();
                    return;
                }

                MessageBox.Show($"Неверно собран пазл! Осталось попыток: {MaxAttempts - _captchaFailedAttempts}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

                attemptsText.Text = $"Осталось попыток: {MaxAttempts - _captchaFailedAttempts}";
                GeneratePuzzleCaptcha();
                return;
            }

            CompleteAuthorization();
        }

        private void CompleteAuthorization()
        {
            MessageBox.Show("Вы успешно авторизовались",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            var context = AppConnect.modelBD;
            var role = context.role.FirstOrDefault(r => r.role_id == _currentUser.role_id);

            if (role != null)
            {
                string roleName = role.role_name.ToLower();
                Window nextWindow;

                if (roleName.Contains("админ") || roleName.Contains("admin"))
                {
                    nextWindow = new AdminWindow.WindowAdmin();
                }
                else
                {
                    nextWindow = new UserWindow.WindowUser();
                }

                nextWindow.Show();
            }
            this.Close();
        }

        private void ReturnToAuthForm()
        {
            loginBox.Clear();
            passwordBox.Clear();
            authPanel.Visibility = Visibility.Visible;
            captchaPanel.Visibility = Visibility.Collapsed;
            loginBox.Focus();
        }

        private void ResetForm()
        {
            loginBox.Clear();
            passwordBox.Clear();
            authPanel.Visibility = Visibility.Visible;
            captchaPanel.Visibility = Visibility.Collapsed;
            _captchaFailedAttempts = 0;
            loginBox.Focus();
        }
    }
}

