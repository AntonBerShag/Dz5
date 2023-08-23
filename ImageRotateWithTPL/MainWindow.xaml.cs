using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ImageRotateWithTPL
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string pictureDirectory;
        private string outputDirectory;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            // Отмена обработки (пока не реализовано)
        }

        private async void cmdProcess_Click(object sender, EventArgs e)
        {
            pictureDirectory = Path.Combine(Directory.GetCurrentDirectory(), "original_images");
            outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "modify_images");

            // Удаляем каталог результирующих файлов и создаем заново
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }
            Directory.CreateDirectory(outputDirectory);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            await ProcessFilesAsync();

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;

            WriteReport(elapsedTime);

            this.Title = "Работа с файлами завершена";
        }

        private async Task ProcessFilesAsync()
        {
            string[] files = Directory.GetFiles(pictureDirectory, "*.jpg", SearchOption.AllDirectories);

            foreach (string item in files)
            {
                await Task.Run(() => ProcessImage(item));
            }
        }

        private void ProcessImage(string item)
        {
            string filename = Path.GetFileName(item);
            string report = $"Обрабатываю {item} в потоке {Thread.CurrentThread.ManagedThreadId}\n";

            // Используйте Dispatcher для обновления интерфейса из другого потока
            Dispatcher.Invoke(() => myTextBox.Text += report);

            using (Bitmap bitmap = new Bitmap(item))
            {
                // 90 гр. по часовой стрелке
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                string modFile = Path.Combine(outputDirectory, filename);

                try
                {
                    bitmap.Save(modFile);
                    Dispatcher.Invoke(() => myTextBox.Text += $"{modFile} сконвертирован\n");
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() => myTextBox.Text += $"{modFile} не сконвертирован\n");
                }
            }
        }

        private void WriteReport(TimeSpan elapsedTime)
        {
            string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Время_работы.txt");

            using (StreamWriter writer = new StreamWriter(reportPath))
            {
                writer.WriteLine($"Время выполнения задачи: {elapsedTime}");
            }
        }
    }
}
