﻿using System;

namespace StartLauncher.Utilities
{
    public class ShutdownTimer : IDisposable
    {
        public bool AutoShutdownCancelled { get; private set; }

        private int timerSecondsRemaining;
        private bool disposedValue;
        private readonly System.Windows.Threading.DispatcherTimer _timer;
        private readonly Controls.ProgressBarWithText _progresBar;
        private readonly App _app;
        private readonly Action shutdownAction;

        public ShutdownTimer(int secondsToShutdown, Controls.ProgressBarWithText progressBar, App app, ShutdownTimerPicker.ShutdownTimerAction action, PersistentSettings.StartObjects.StartObjectsManager startObjects)
        {
            AutoShutdownCancelled = false;
            timerSecondsRemaining = secondsToShutdown;
            _progresBar = progressBar;
            _app = app;
            _progresBar.Bar.Maximum = secondsToShutdown;
            _progresBar.Bar.Value = secondsToShutdown;
            switch (action)
            {
                case ShutdownTimerPicker.ShutdownTimerAction.Quit:
                    shutdownAction = () => _app.Shutdown();
                    break;
                case ShutdownTimerPicker.ShutdownTimerAction.LaunchAndQuit:
                    shutdownAction = () => { startObjects.LaunchAllInCurrentProfile().Wait(); _app.Shutdown(); };
                    break;
                default:
                    break;
            }
            _timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = System.TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public void Cancel()
        {
            AutoShutdownCancelled = true;
            _timer.Stop();
        }

        public void SetRunState(bool run)
        {
            if (run && !_timer.IsEnabled)
            {
                _timer.Start();
            }
            if (!run && _timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            timerSecondsRemaining--;
            _progresBar.Bar.Value = timerSecondsRemaining;
            if (timerSecondsRemaining == 0 && !AutoShutdownCancelled)
            {
                shutdownAction();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Cancel();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
