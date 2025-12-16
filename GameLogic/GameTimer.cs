using System;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MemoryGame.GameLogic
{
    public class GameTimer
    {
        private Timer timer;
        private int elapsedSeconds; //прошедшие секунды
        private bool isRunning; //флаг работы таймера

        //событие вызывается каждую секунду, список вызовов, которые нужно выполнить
        public event Action<int> OnTick;

        // Добавьте это публичное свойство только для чтения
        public bool IsRunning
        {
            get { return isRunning; }   
            private set { isRunning = value; } 
        }

        public int ElapsedSeconds => elapsedSeconds;

        public GameTimer()
        {
            timer = new Timer();
            timer.Interval = 1000; // 1 секунда
            timer.Tick += Timer_Tick;
            elapsedSeconds = 0; //начальное
            isRunning = false;  //запущено
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedSeconds++;
            //Метод, который выполняет все функции, сохраненные в делегате(ссылка на совместимый метод например label)
            OnTick?.Invoke(elapsedSeconds);

        }

        public void Start()
        {
            if (!isRunning)
            {
                timer.Start();
                isRunning = true;
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
            }
        }

        public void Pause()
        {
            Stop();
        }

        //продолжение таймера
        public void Resume()
        {
            Start();
        }

        //сброс 
        public void Reset()
        {
            Stop();
            elapsedSeconds = 0;
        }

        public string GetFormattedTime()
        {
            int minutes = elapsedSeconds / 60;
            int seconds = elapsedSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

       // public int GetElapsedSeconds() => elapsedSeconds;
    }
}