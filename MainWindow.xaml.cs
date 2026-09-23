using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Tracciamento_lavoro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string logFilePath = "log.csv";
        public string Code { get; set; }
        private DateTimeOffset startTime;
        private DispatcherTimer timer;
        private TimeSpan duration = TimeSpan.Zero;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            timer = new DispatcherTimer();
            loadLog();
        }

        private void updateDuration()
        {
            if (startTime != default)
            {
                duration = DateTimeOffset.Now - startTime;
            }
        }

        private void updateTxbStart()
        {
            txbStart.Text = startTime.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void updateTxbDuration()
        {
            if (duration.Days > 0)
            {
                txbDuration.Text = duration.ToString(@"d\.hh\:mm\:ss");
            }
            else
            {
                txbDuration.Text = duration.ToString(@"hh\:mm\:ss");
            }
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Code))
            {
                MessageBox.Show("Please scan a code before starting the timer.");
                return;
            }

            updateDtgLog(Code);
            startTimer();
        }

        private void startTimer()
        {
            if (!timer.IsEnabled)
            {
                if (startTime == default)
                {
                    startTime = DateTimeOffset.Now;
                    startWorkLog();
                }
                else
                {
                    updateDuration();
                }
                
                updateTxbStart();
                updateTxbDuration();

                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                endWorkLog();
                startTime = default;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            updateDuration();
            updateTxbDuration();
        }

        private void loadLog()
        {
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath);
                return;
            }

            String? lastLine = File.ReadLines(logFilePath).LastOrDefault();
            if (lastLine != null)
            {
                if (!lastLine.Contains(","))
                {
                    MessageBox.Show("Log file is corrupted. Please check the log file.");
                    return;
                }

                String[] parts = lastLine.Split(',');
                if (parts.Length == 2)
                {
                    Code = parts[0];
                    startTime = DateTimeOffset.Parse(parts[1]);
                    updateTxbStart();
                    updateDuration();
                    updateTxbDuration();
                    btnScan_Click(null, null);
                }
            }
            return;
        }

        private void updateDtgLog(String code)
        {
            List<WorkLogEntry> logs = new List<WorkLogEntry>();

            foreach (string line in File.ReadAllLines(logFilePath))
            {
                string[] parts = line.Split(',');

                if (parts[0] == code && parts.Length == 4)
                {
                    logs.Add(new WorkLogEntry
                    {
                        Code = parts[0],
                        Start = DateTimeOffset.ParseExact(parts[1], "O", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm:ss"),
                        End = DateTimeOffset.ParseExact(parts[2], "O", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm:ss"),
                        Elapsed = parts[3]
                    });
                }
            }

            dtgLog.ItemsSource = logs;
        }

        private void startWorkLog()
        {
            File.AppendAllText(logFilePath, $"{Code},{startTime:O}{Environment.NewLine}");
        }

        private void endWorkLog()
        {
            String text = File.ReadAllText(logFilePath);
            text = text.Replace($"{Code},{startTime:O}", $"{Code},{startTime:O},{DateTimeOffset.Now:O},{duration:d\\.hh\\:mm\\:ss}");
            File.WriteAllText(logFilePath, text);
        }
    }
}