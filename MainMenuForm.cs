using System;
using System.Drawing;
using System.Windows.Forms;

namespace MemoryGame
{
	//partial - класс разделен на несколько файлов
	public partial class MainMenuForm : BufferedForm    
	{
		private string selectedTheme = "Животные";

		private bool isInitialized = false; //инициализировалась ли форма
		private bool isFirstActivation = true;  //была ли открыта форма

		public MainMenuForm()
		{
			InitializeComponent();  // Инициализация компонентов из Designer

			ConfigureFormEvents();// Настройка событий формы
		}

		// Переопределение метода показа формы
		//Было в Windows: "Окно появилось → ничего не делать"
		//в игре: "Окно появилось → начать загружать игру"
        protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);	//форма показана

			WaitForInitialization();// Ожидание завершения инициализации
		}

		// Обработчик клика по кнопке выхода
		private void ExitButton_Click(object sender, EventArgs e)
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

        // Обработчик выбора уровня
        //sender - объект, вызваший событие (кнопка)
        private void LevelButton_Click(object sender, EventArgs e)
		{
			Button clickedButton = sender as Button;    //приводит объект sender к типу Button
            string level = clickedButton?.Tag?.ToString();  // ?. - не null

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

            // ShowDialog БЛОКИРУЕТ меню автоматически!
            gameForm.ShowDialog(this); // ← this означает, что меню - владелец игры

            // После закрытия игры код продолжается здесь
            this.Activate(); // Активируем меню
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
    }
}