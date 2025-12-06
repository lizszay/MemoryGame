using System;
using System.Windows.Forms; // ← обязательно, чтобы использовать Windows Forms Timer

namespace MemoryGame.GameLogic
{
    public class GameTimer
    {
        private System.Windows.Forms.Timer timer; // ← явное указание!
        private int seconds;

        public int ElapsedSeconds => seconds;

        public event Action<int> OnTick;

        public GameTimer()
        {
            timer = new System.Windows.Forms.Timer(); // ← явное указание!
            timer.Interval = 1000; // 1 секунда
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            seconds++;
            OnTick?.Invoke(seconds);
        }

        public void Start()
        {
            seconds = 0;
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public void Reset()
        {
            seconds = 0;
            timer.Stop();
        }

        public string GetFormattedTime()
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString(@"mm\:ss");
        }

        // ВАЖНО: освобождение ресурсов, если нужно (например, при закрытии игры)
        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
        }
    }
}