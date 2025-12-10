using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
    public partial class MainMenuForm : BufferedForm
    {
        // Выбранная тема (по умолчанию "Животные")
        private string selectedTheme = "Животные";

        // Флаги состояния формы
        private bool isInitialized = false;
        private bool isFirstActivation = true;

        public MainMenuForm()
        {
            // Инициализация компонентов из Designer
            InitializeComponent();

            // Настройка событий формы
            ConfigureFormEvents();
        }

        // Переопределение параметров создания окна
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Добавление флага двойной буферизации Windows API
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        // Переопределение метода показа формы
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Ожидание завершения инициализации
            WaitForInitialization();
        }

        // Обработчик клика по кнопке выхода
        private void ExitButton_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        // Обработчик выбора уровня
        private void LevelButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            string level = clickedButton?.Tag?.ToString();

            // Обработка пользовательского уровня
            if (level == "Пользовательский")
            {
                ShowCustomLevelForm();
            }
            else
            {
                // Запуск стандартного уровня
                StartGame(selectedTheme, level);
            }
        }

        // Обработчик изменения выбора темы
        private void ThemeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                selectedTheme = rb.Tag.ToString();
            }
        }

        // Показ формы правил
        private void ShowRules()
        {
            using (RulesForm rules = new RulesForm())
            {
                rules.ShowDialog(this);
            }
        }

        // Запуск игры
        private async void StartGame(string theme, string level, int rows = 0, int cols = 0)
        {
            GameForm gameForm;

            // Создание формы игры с соответствующими параметрами
            if (rows > 0 && cols > 0)
            {
                gameForm = new GameForm(theme, level, rows, cols);
            }
            else
            {
                gameForm = new GameForm(theme, level);
            }

            // Переключение на форму игры
            await SwitchToForm(gameForm);

            // Настройка событий для показа правил из игры
            ConfigureGameFormEvents(gameForm);
        }

        // Показ формы настройки пользовательского уровня
        private void ShowCustomLevelForm()
        {
            using (CustomLevelForm customForm = new CustomLevelForm())
            {
                if (customForm.ShowDialog() == DialogResult.OK)
                {
                    StartGame(selectedTheme, "Пользовательский",
                        customForm.SelectedRows, customForm.SelectedColumns);
                }
            }
        }

        // Переключение между формами с анимацией
        private async System.Threading.Tasks.Task SwitchToForm(Form nextForm)
        {
            // Создание и показ формы-заглушки
            CoverForm cover = new CoverForm();
            cover.Show();
            cover.BringToFront();

            // Даем системе время отрисовать затычку
            await System.Threading.Tasks.Task.Yield();

            // Скрываем текущую форму
            this.Hide();

            // Настройка события закрытия новой формы
            ConfigureFormClosedEvent(nextForm, cover);

            // Показ новой формы
            nextForm.Show();
            nextForm.BringToFront();

            // Закрытие заглушки
            if (!cover.IsDisposed)
            {
                cover.Close();
            }
        }

        // Выход из приложения
        private void ExitApplication()
        {
            if (MessageBox.Show(
                "Вы действительно хотите выйти из игры?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        // Ожидание завершения инициализации
        private void WaitForInitialization()
        {
            if (!isInitialized)
            {
                while (!isInitialized)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        // Настройка событий формы
        private void ConfigureFormEvents()
        {
            // Установка флага инициализации при загрузке
            this.Load += (s, e) => isInitialized = true;

            // Сброс флага первого активации
            this.Activated += (s, e) => isFirstActivation = false;
            this.Shown += (s, e) => isFirstActivation = false;
        }

        // Настройка события закрытия формы
        private void ConfigureFormClosedEvent(Form nextForm, CoverForm cover)
        {
            nextForm.FormClosed += (sender, args) =>
            {
                // Возврат к главному меню
                if (!this.IsDisposed)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.Show();
                        this.BringToFront();
                    });
                }

                // Закрытие заглушки
                if (!cover.IsDisposed)
                {
                    cover.Invoke((MethodInvoker)delegate
                    {
                        cover.Close();
                    });
                }
            };
        }

        // Настройка событий формы игры
        private void ConfigureGameFormEvents(GameForm gameForm)
        {
            gameForm.ShowRulesRequested += async (s, e) =>
            {
                // Пауза игры при показе правил
                gameForm.PauseGameForRules();

                // Показ формы правил
                RulesForm rulesForm = new RulesForm();
                await SwitchToForm(rulesForm);

                // Возобновление игры после закрытия правил
                gameForm.ResumeGameAfterRules();
            };
        }
    }
}