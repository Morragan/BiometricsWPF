using Biometria.Extensions;
using Biometria.Models;
using System;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Input;

namespace Biometria
{
    /// <summary>
    /// Interaction logic for KeystrokeDynamicsReadingWindow.xaml
    /// </summary>
    public partial class KeystrokeDynamicsReadingWindow : Window
    {
        readonly string pangram1 = "how quickly daft jumping zebras vex",
            pangram2 = "public junk dwarves hug my quartz fox";
        readonly (int count, long time)[] readings = new (int count, long time)[26];
        public Visibility NameTextBoxVisibility { get; set; } = Visibility.Visible;
        public KDReading Reading { get; set; } = new KDReading();
        public string UserName { get; set; }
        public KeystrokeDynamicsReadingWindow()
        {
            InitializeComponent();
            readings.Initialize();
            DataContext = this;
        }

        private void RestrictReadingDown1(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Back && e.Key != Key.Tab && e.Key != Key.Enter && (Reading1.Text.Length >= pangram1.Length || char.ToLower(e.Key.GetChar()) != pangram1[Reading1.Text.Length]))
            {
                SystemSounds.Beep.Play();
            }
        }
        private void RestrictReadingDown2(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Back && e.Key != Key.Tab && e.Key != Key.Enter && (Reading2.Text.Length >= pangram2.Length || char.ToLower(e.Key.GetChar()) != pangram2[Reading2.Text.Length]))
            {
                SystemSounds.Beep.Play();
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            if (Reading1.Text == pangram1 && Reading2.Text == pangram2)
            {
                Reading.Name = UserName;
                Reading.LetterMeasurements = readings.Select(reading => (int)reading.time / reading.count).ToArray();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Proszę wypełnić wszystkie pola!", "Nie tak prędko", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void StartRecording(object sender, KeyEventArgs e)
        {
            var charPressed = e.Key.GetChar();
            if (charPressed > 'z' || charPressed < 'a') return;
            readings[charPressed - 'a'].count++;
            readings[charPressed - 'a'].time -= DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        private void StopRecording(object sender, KeyEventArgs e)
        {
            var charPressed = e.Key.GetChar();
            if (charPressed > 'z' || charPressed < 'a') return;
            readings[charPressed - 'a'].time += DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
