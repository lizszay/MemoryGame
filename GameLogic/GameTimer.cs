using System;
using System.Windows.Forms; // ← обязательно, чтобы использовать Windows Forms Timer
using Timer = System.Windows.Forms.Timer; // чтобы каждый раз не прописывть

namespace MemoryGame.GameLogic
{
    public class GameTimer
    {
        private Timer timer;
        private int elapsedSeconds;
        private bool isRunning;

        public event Action<int> OnTick;

        // Добавьте это публичное свойство только для чтения
        public bool IsRunning
        {
            get { return isRunning; }
            private set { isRunning = value; }
        }

        public GameTimer()
        {
            timer = new Timer();
            timer.Interval = 1000; // 1 секунда
            timer.Tick += Timer_Tick;
            elapsedSeconds = 0;
            isRunning = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedSeconds++;
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

        public void Resume()
        {
            Start();
        }

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

        public int GetElapsedSeconds() => elapsedSeconds;
    }
}