using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks; // Добавьте в начале файла

namespace MemoryGame
{
    public class MainMenuForm : Form
    {
        // По умолчанию
        private string selectedTheme = "Животные";
        private Panel mainPanel;
        private bool isInitialized = false; // Флаг, указывающий на завершение инициализации формы

        private bool isFirstActivation = true; // Флаг первого запуска

        public MainMenuForm()
        {
            // Двойная буферизация - предотвращает мерцание
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |  //для снижения мерцания
                         ControlStyles.UserPaint |  //отображение элемента управления выполняет сам элемент, а не операционная система
                         ControlStyles.DoubleBuffer, true); //сначла рисует в буфере памяти,  затем за раз выводится все на экран
            this.DoubleBuffered = true; // Дополнительная двойная буферизация

            // Устанавливаем черный фон для предотвращения мигания рабочего стола при переключении
            this.BackColor = Color.Black;

            // Инициализируем компоненты формы (создаем интерфейс)
            InitializeCustomComponents();

            //когда форма загрузится. вызовет сама форма, пусто
            this.Load += (s, e) =>
            {
                isInitialized = true; // Устанавливаем флаг, что инициализация завершена
                // Теперь форма готова к показу
            };

            // когда онко станет активным
            this.Activated += async (s, e) =>
            {
                // Плавно появляемся при возвращении из других форм
                // НО ТОЛЬКО ЕСЛИ ЭТО НЕ ПЕРВЫЙ ЗАПУСК
                if (!isFirstActivation && this.Opacity < 1.0)
                {
                    await FadeIn(this, 300); // Уменьшили с 1000 до 300 мс
                }
                isFirstActivation = false; // После первого срабатывания - флаг сбрасываем
            };

            this.Shown += (s, e) =>
            {
                // Когда форма первый раз показывается - сбрасываем флаг
                isFirstActivation = false; // После первого показа
            };
        }

        // Переопределяем свойство CreateParams для настройки параметров создания окна
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams; // Получаем базовые параметры
                // Добавляем флаг двойной буферизации на уровне Windows API
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED - полная двойная буферизация
                return cp;
            }
        }

        private void InitializeCustomComponents()
        {
            // ОТКЛЮЧАЕМ перерисовку на время создания элементов - предотвращает мерцание
            this.SuspendLayout();

            // Настраиваем базовые свойства формы
            this.FormBorderStyle = FormBorderStyle.None;    // Убираем стандартную рамку окна
            this.WindowState = FormWindowState.Maximized;   // Разворачиваем окно на весь экран
            this.BackColor = Color.FromArgb(30, 40, 30);    // Устанавливаем темно-зеленый фон (запасной вариант)
            this.Font = new Font("Times New Roman", 14, FontStyle.Regular); // Устанавливаем шрифт

            // Создаем главную панель для размещения всех элементов интерфейса
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;                // Растягиваем панель на всё окно
            mainPanel.BackColor = Color.Transparent;        // Делаем панель прозрачной

            // Включаем двойную буферизацию для панели через рефлексию (скрытое свойство)
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(mainPanel, true, null);

            // Пытаемся загрузить фоновое изображение
            try
            {
                // Формируем путь к файлу фона
                string bgPath = System.IO.Path.Combine(Application.StartupPath, "img", "ui", "background.jpg");
                if (System.IO.File.Exists(bgPath))  // Проверяем существует ли файл
                {
                    // Загружаем изображение и устанавливаем как фон
                    mainPanel.BackgroundImage = Image.FromFile(bgPath);
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch; // Растягиваем на всю панель
                }
            }
            catch (Exception ex)
            {
                // Если не удалось загрузить фон, остаемся с цветным фоном
                // Можно добавить логирование ошибки: MessageBox.Show(ex.Message);
            }

            // Создаем интерфейс меню (все кнопки, метки и т.д.)
            CreateMenuInterface();

            // Добавляем главную панель на форму
            this.Controls.Add(mainPanel);

            // Подписываемся на событие закрытия формы
            this.FormClosing += MainMenuForm_FormClosing;

            // ВКЛЮЧАЕМ перерисовку после создания всех элементов
            this.ResumeLayout(true);    // Возобновляем перерисовку с обновлением разметки
            this.PerformLayout();       // Применяем вычисленные размеры и позиции
        }

        // Переопределяем метод, который вызывается когда форма впервые показывается
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e); // Вызываем базовый метод

            // Если форма еще не инициализирована, ждем завершения
            if (!isInitialized)
            {
                // Ожидаем установки флага isInitialized = true
                while (!isInitialized)
                {
                    Application.DoEvents(); // Обрабатываем ожидающие сообщения Windows
                    System.Threading.Thread.Sleep(10); // Небольшая пауза
                }
            }
        }

        private void CreateMenuInterface()
        {
            // Временное отключение перерисовки панели - предотвращает мерцание
            mainPanel.SuspendLayout();

            // Очищаем панель от старых элементов (если они были)
            mainPanel.Controls.Clear();

            // Создаем основную таблицу для размещения элементов меню
            TableLayoutPanel mainTable = new TableLayoutPanel();

            // Включаем двойную буферизацию для таблицы
            typeof(TableLayoutPanel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(mainTable, true, null);

            // Настраиваем таблицу
            mainTable.Dock = DockStyle.Fill;               // Растягиваем на всю панель
            mainTable.RowCount = 4;                        // 4 строки
            mainTable.ColumnCount = 1;                     // 1 столбец
            mainTable.BackColor = Color.Transparent;       // Прозрачный фон

            // Устанавливаем размеры строк (в процентах от высоты таблицы)
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f)); // Заголовок - 15%
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 25f)); // Темы - 25%
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 40f)); // Уровни - 40%
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f)); // Нижние кнопки - 20%

            // === 1. СОЗДАЕМ ЗАГОЛОВОК ===
            Label titleLabel = new Label();
            titleLabel.Text = "MEMORY GAME";
            titleLabel.Font = new Font("Times New Roman", 52, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(255, 255, 255); // Белый текст
            titleLabel.TextAlign = ContentAlignment.MiddleCenter; // Выравнивание по центру
            titleLabel.Dock = DockStyle.Fill;                     // Заполняет всю ячейку
            titleLabel.BackColor = Color.Transparent;             // Прозрачный фон
            titleLabel.Margin = new Padding(10);                  // Отступы вокруг
            mainTable.Controls.Add(titleLabel, 0, 0);             // Добавляем в таблицу (столбец 0, строка 0)

            // === 2. СОЗДАЕМ ПАНЕЛЬ ВЫБОРА ТЕМЫ ===
            Panel themePanel = new Panel();
            themePanel.Dock = DockStyle.Fill;
            themePanel.BackColor = Color.Transparent;
            themePanel.Margin = new Padding(20);   // Внешние отступы
            themePanel.Padding = new Padding(10);  // Внутренние отступы

            Label themeLabel = new Label();
            themeLabel.Text = "ВЫБЕРИТЕ ТЕМУ КАРТОЧЕК:";
            themeLabel.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            themeLabel.ForeColor = Color.FromArgb(255, 255, 255);
            themeLabel.TextAlign = ContentAlignment.MiddleCenter;
            themeLabel.Dock = DockStyle.Top;      // Прикрепляем к верху панели
            themeLabel.Height = 60;               // Фиксированная высота
            themeLabel.BackColor = Color.Transparent;

            // Создаем панель с потоковым расположением для радиокнопок
            FlowLayoutPanel themeFlow = new FlowLayoutPanel();
            themeFlow.Dock = DockStyle.Fill;
            themeFlow.FlowDirection = FlowDirection.LeftToRight; // Элементы слева направо
            themeFlow.WrapContents = false;                      // Не переносить на новую строку
            themeFlow.AutoSize = false;                          // Фиксированный размер
            themeFlow.BackColor = Color.Transparent;
            themeFlow.Margin = new Padding(50, 10, 50, 10);      // Отступы со всех сторон

            // Настраиваем привязки для центрирования
            themeFlow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            themeFlow.AutoScroll = false; // Отключаем скроллинг

            // Создаем три радиокнопки для выбора темы
            RadioButton rbAnimals = CreateThemeRadioButton("🐾 ЖИВОТНЫЕ", "Животные", true, Color.FromArgb(255, 200, 80)); // Оранжевый
            RadioButton rbGeometry = CreateThemeRadioButton("🔷 ГЕОМЕТРИЯ", "Геометрические фигуры", false, Color.FromArgb(100, 150, 255)); // Синий
            RadioButton rbPlants = CreateThemeRadioButton("🌿 РАСТЕНИЯ", "Растения", false, Color.FromArgb(150, 255, 100)); // Зелёный

            // Добавляем радиокнопки на панель
            themeFlow.Controls.Add(rbAnimals);
            themeFlow.Controls.Add(rbGeometry);
            themeFlow.Controls.Add(rbPlants);

            // Добавляем элементы на панель темы
            themePanel.Controls.Add(themeFlow);  // Сначала добавляем панель с кнопками
            themePanel.Controls.Add(themeLabel); // Затем заголовок (будет сверху)

            // Добавляем панель темы в таблицу (столбец 0, строка 1)
            mainTable.Controls.Add(themePanel, 0, 1);

            // === 3. СОЗДАЕМ ПАНЕЛЬ ВЫБОРА УРОВНЯ ===
            Panel levelPanel = new Panel();
            levelPanel.Dock = DockStyle.Fill;
            levelPanel.BackColor = Color.Transparent;
            levelPanel.Margin = new Padding(20);
            levelPanel.Padding = new Padding(10);

            Label levelLabel = new Label();
            levelLabel.Text = "ВЫБЕРИТЕ УРОВЕНЬ СЛОЖНОСТИ:";
            levelLabel.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            levelLabel.ForeColor = Color.FromArgb(255, 255, 255);
            levelLabel.TextAlign = ContentAlignment.MiddleCenter;
            levelLabel.Dock = DockStyle.Top;
            levelLabel.Height = 60;
            levelLabel.BackColor = Color.Transparent;

            // Создаем таблицу для кнопок уровней
            TableLayoutPanel levelTable = new TableLayoutPanel();
            levelTable.Dock = DockStyle.Fill;
            levelTable.RowCount = 5;        // 5 строк для 5 уровней
            levelTable.ColumnCount = 1;     // 1 столбец
            levelTable.BackColor = Color.Transparent;
            levelTable.Padding = new Padding(100, 10, 100, 10); // Большие отступы по бокам

            // Настраиваем высоту строк (равномерно по 20%)
            for (int i = 0; i < 5; i++)
            {
                levelTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
            }

            // Создаем кнопки уровней с разными цветами
            Button btnEasy = CreateLevelButton("⭐ ЛЕГКИЙ (2×2)", "Легкий", Color.FromArgb(255, 200, 80), Color.DarkBlue);
            Button btnMedium = CreateLevelButton("⭐⭐ СРЕДНИЙ (3×3)", "Средний", Color.FromArgb(255, 170, 50), Color.DarkBlue);
            Button btnHard = CreateLevelButton("⭐⭐⭐ СЛОЖНЫЙ (4×4)", "Сложный", Color.FromArgb(255, 140, 30), Color.White);
            Button btnExpert = CreateLevelButton("⭐⭐⭐⭐ ЭКСПЕРТ (5×5)", "Эксперт", Color.FromArgb(255, 110, 10), Color.White);
            Button btnCustom = CreateLevelButton("🎮 ПОЛЬЗОВАТЕЛЬСКИЙ", "Пользовательский", Color.FromArgb(200, 150, 255), Color.DarkBlue);

            // Добавляем кнопки в таблицу уровней
            levelTable.Controls.Add(btnEasy, 0, 0);     // Легкий - строка 0
            levelTable.Controls.Add(btnMedium, 0, 1);   // Средний - строка 1
            levelTable.Controls.Add(btnHard, 0, 2);     // Сложный - строка 2
            levelTable.Controls.Add(btnExpert, 0, 3);   // Эксперт - строка 3
            levelTable.Controls.Add(btnCustom, 0, 4);   // Пользовательский - строка 4

            // Добавляем элементы на панель уровня
            levelPanel.Controls.Add(levelTable);
            levelPanel.Controls.Add(levelLabel);

            // Добавляем панель уровня в основную таблицу (столбец 0, строка 2)
            mainTable.Controls.Add(levelPanel, 0, 2);

            // === 4. СОЗДАЕМ НИЖНЮЮ ПАНЕЛЬ С КНОПКАМИ ===
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.BackColor = Color.Transparent;
            bottomPanel.Margin = new Padding(20);
            bottomPanel.Padding = new Padding(20);

            // Таблица для размещения кнопок (Правила | Пусто | Выход)
            TableLayoutPanel buttonTable = new TableLayoutPanel();
            buttonTable.Dock = DockStyle.Fill;
            buttonTable.RowCount = 1;        // 1 строка
            buttonTable.ColumnCount = 3;     // 3 столбца
            buttonTable.BackColor = Color.Transparent;

            // Настраиваем ширину столбцов
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // Правила - 30%
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f)); // Пустое место - 40%
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // Выход - 30%

            // Создаем кнопку "Правила игры"
            Button btnRules = new Button();
            btnRules.Text = "📖 ПРАВИЛА ИГРЫ";
            btnRules.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            btnRules.BackColor = Color.LightSteelBlue;
            btnRules.ForeColor = Color.DarkBlue;
            btnRules.FlatStyle = FlatStyle.Flat;                     // Плоский стиль
            btnRules.FlatAppearance.BorderSize = 3;                  // Толщина рамки
            btnRules.FlatAppearance.BorderColor = Color.DarkBlue;    // Цвет рамки
            btnRules.Dock = DockStyle.Fill;                         // Заполняет ячейку
            btnRules.Margin = new Padding(20, 10, 10, 10);          // Отступы
            btnRules.Cursor = Cursors.Hand;                         // Курсор-рука при наведении
            btnRules.Click += (s, e) => ShowRules();               // Обработчик клика

            // Создаем пустую панель-разделитель
            Panel spacer = new Panel();
            spacer.Dock = DockStyle.Fill;
            spacer.BackColor = Color.Transparent;

            // Создаем кнопку "Выход"
            Button btnExit = new Button();
            btnExit.Text = "❌ ВЫЙТИ ИЗ ИГРЫ";
            btnExit.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            btnExit.BackColor = Color.IndianRed;
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 3;
            btnExit.FlatAppearance.BorderColor = Color.DarkRed;
            btnExit.Dock = DockStyle.Fill;
            btnExit.Margin = new Padding(10, 10, 20, 10);
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += ExitButton_Click;

            // Добавляем элементы в таблицу кнопок
            buttonTable.Controls.Add(btnRules, 0, 0);   // Правила - столбец 0
            buttonTable.Controls.Add(spacer, 1, 0);     // Разделитель - столбец 1
            buttonTable.Controls.Add(btnExit, 2, 0);    // Выход - столбец 2

            // Добавляем таблицу кнопок на нижнюю панель
            bottomPanel.Controls.Add(buttonTable);

            // Добавляем нижнюю панель в основную таблицу (столбец 0, строка 3)
            mainTable.Controls.Add(bottomPanel, 0, 3);

            // Добавляем основную таблицу на главную панель
            mainPanel.Controls.Add(mainTable);

            // Включаем перерисовку панели после добавления всех элементов
            mainPanel.ResumeLayout(true);
        }

        // Метод создания радиокнопки для выбора темы
        private RadioButton CreateThemeRadioButton(string text, string value, bool isChecked, Color bgColor)
        {
            RadioButton rb = new RadioButton();
            rb.Text = text;
            rb.Font = new Font("Times New Roman", 20, FontStyle.Bold);
            rb.Appearance = Appearance.Normal; // Обычный вид с кружочком
            rb.Size = new Size(250, 70);       // Фиксированный размер
            rb.Margin = new Padding(15);       // Отступы вокруг
            rb.TextAlign = ContentAlignment.MiddleCenter; // Текст по центру
            rb.Checked = isChecked;            // Устанавливаем выбран ли элемент
            rb.Tag = value;                    // Сохраняем значение темы в Tag

            // Подбираем цвет текста в зависимости от яркости фона
            rb.ForeColor = IsDarkColor(bgColor) ? Color.White : Color.DarkBlue;
            rb.BackColor = bgColor; // Устанавливаем цвет фона

            // Обработчик изменения состояния (выбрана/не выбрана)
            rb.CheckedChanged += (s, e) =>
            {
                if (rb.Checked) // Если кнопка выбрана
                {
                    selectedTheme = value;  // Сохраняем выбранную тему
                }
            };

            return rb;
        }

        // Вспомогательная функция: определяет, тёмный ли цвет
        private bool IsDarkColor(Color c)
        {
            // Вычисление яркости цвета по формуле восприятия человеком
            double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255;
            // Если яркость меньше 50%, считаем цвет темным
            return luminance < 0.5;
        }

        // Метод создания кнопки для выбора уровня
        private Button CreateLevelButton(string text, string levelTag, Color bgColor, Color textColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            btn.BackColor = bgColor;
            btn.ForeColor = textColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 3;
            btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.Dock = DockStyle.Fill; // Заполняет всю ячейку таблицы
            btn.Margin = new Padding(10, 5, 10, 5); // Отступы сверху/снизу меньше чем по бокам
            btn.Tag = levelTag; // Сохраняем название уровня в Tag
            btn.Cursor = Cursors.Hand; // Курсор-рука
            btn.TextAlign = ContentAlignment.MiddleCenter; // Текст по центру

            // Подписываемся на событие клика
            btn.Click += LevelButton_Click;
            return btn;
        }

        // Обработчик клика по кнопке уровня
        private void LevelButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;  // Получаем кнопку, по которой кликнули
            string level = btn?.Tag?.ToString();    // Получаем текст уровня из Tag

            if (level == "Пользовательский")   // Если выбран пользовательский уровень
            {
                // Создаем форму для настройки пользовательского уровня
                using (CustomLevelForm customForm = new CustomLevelForm())
                {
                    // Показываем форму как диалоговое окно
                    if (customForm.ShowDialog() == DialogResult.OK)
                    {
                        // Если пользователь нажал "OK", запускаем игру с выбранными параметрами
                        StartGame(selectedTheme, level, customForm.SelectedRows, customForm.SelectedColumns);
                    }
                }
            }
            // Иначе запускаем игру с выбранным стандартным уровнем
            else
            {
                StartGame(selectedTheme, level);
            }
        }

        // Метод запуска игры (упрощенный)
        private async void StartGame(string theme, string level, int rows = 0, int cols = 0)
        {
            // Плавно скрываем меню
            await FadeOut(this, 300);

            // Скрываем меню
            this.Hide();

            // Создаем игру
            GameForm gameForm = (rows > 0 && cols > 0)
                ? new GameForm(theme, level, rows, cols)
                : new GameForm(theme, level);

            // Когда игра закроется
            gameForm.FormClosed += (s, e) =>
            {
                // Просто показываем меню
                this.Show();
                this.BringToFront();
            };

            // Показываем игру
            gameForm.Show();
        }

        // Вспомогательный метод для прямого показа игры (если оверлей был закрыт)
        private void ShowGameDirectly(string theme, string level, int rows, int cols)
        {
            GameForm gameForm = (rows > 0 && cols > 0)
                ? new GameForm(theme, level, rows, cols)
                : new GameForm(theme, level);

            gameForm.FormClosed += (s, e) =>
            {
                this.Show();
                this.BringToFront();
            };

            gameForm.Show();
            gameForm.BringToFront(); // ← Игра поверх меню
        }

        // Метод показа формы с правилами
        private async void ShowRules()
        {
            // Создаем черную форму-оверлей для плавного перехода без видимости рабочего стола
            Form blackOverlay = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                WindowState = FormWindowState.Maximized,
                BackColor = Color.Black,
                Opacity = 0,
                TopMost = true,
                ShowInTaskbar = false,
                ControlBox = false
            };

            // Показываем черный оверлей
            blackOverlay.Show();
            blackOverlay.BringToFront();

            // Плавно увеличиваем прозрачность черного оверлея
            for (double opacity = 0; opacity <= 1.0; opacity += 0.1)
            {
                if (blackOverlay.IsDisposed) break;
                blackOverlay.Opacity = opacity;
                await Task.Delay(15);
                Application.DoEvents();
            }

            // Проверяем, что форма еще не закрыта
            if (blackOverlay.IsDisposed)
            {
                // Если форма уже закрыта, просто показываем правила
                ShowRulesDirectly();
                return;
            }

            // Скрываем главное меню
            this.Hide();

            // Создаем форму правил
            RulesForm rulesForm = new RulesForm();
            rulesForm.Opacity = 0;

            // Локальная переменная для отслеживания состояния оверлея
            Form localOverlay = blackOverlay;

            // Показываем форму правил
            rulesForm.Show();
            rulesForm.BringToFront();

            // Плавно уменьшаем прозрачность черного оверлея, одновременно увеличивая прозрачность правил
            for (double opacity = 1.0; opacity >= 0; opacity -= 0.1)
            {
                if (localOverlay.IsDisposed) break;
                localOverlay.Opacity = opacity;
                rulesForm.Opacity = 1.0 - opacity;
                await Task.Delay(15);
                Application.DoEvents();
            }

            // Закрываем черный оверлей
            if (!localOverlay.IsDisposed)
                localOverlay.Close();
        }

        // Вспомогательный метод для прямого показа правил
        private void ShowRulesDirectly()
        {
            RulesForm rulesForm = new RulesForm();

            rulesForm.FormClosed += (s, e) =>
            {
                this.Show();
                this.BringToFront();
            };

            this.Hide();
            rulesForm.Show();
        }

        // Метод плавного исчезновения формы
        private async Task FadeOut(Form form, int duration)
        {
            // 10 шагов анимации (от 100% до 0% прозрачности)
            for (double opacity = 1.0; opacity > 0; opacity -= 0.1)
            {
                if (form.IsDisposed) return; // Проверка на случай закрытия формы

                form.Opacity = opacity;
                await Task.Delay(duration / 10);
                Application.DoEvents(); // Обрабатываем сообщения Windows
            }
        }

        // Метод плавного появления формы
        // async - модификатор, что внутри await
        private async Task FadeIn(Form form, int duration)
        {
            // 10 шагов анимации (от 0% до 100% прозрачности)
            for (double opacity = 0; opacity <= 1.0; opacity += 0.1)
            {
                if (form.IsDisposed) return; // Проверка на случай закрытия формы

                form.Opacity = opacity;
                await Task.Delay(duration / 10);
                Application.DoEvents(); // Обрабатываем сообщения Windows
            }
        }

        // Обработчик клика по кнопке "Выход"
        private void ExitButton_Click(object sender, EventArgs e)
        {
            ExitApplication(); // Вызываем метод выхода
        }

        // Метод выхода из приложения
        private void ExitApplication()
        {
            if (MessageBox.Show(
                "Вы действительно хотите выйти из игры?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit(); // Закрываем приложение
            }
        }

        // Обработчик события закрытия формы
        private void MainMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Если причина закрытия формы = пользовательский выбор (крестик)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
                ExitApplication(); // Вызываем метод выхода с подтверждением
            }
        }
    }
}