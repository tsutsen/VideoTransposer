using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;


namespace WpfApp1
{
	public interface IDialogService
	{
		void ShowMessage(string message);   // показ сообщения
		string FilePath { get; set; }   // путь к выбранному файлу
		bool OpenFileDialog();  // открытие файла
		bool SaveFileDialog();  // сохранение файла
	}


	public partial class MainWindow : Window
	{
		private readonly SynchronizationContext _syncContext;
		Transposer transposer = new Transposer("");
		string version = "v.1.0.5";

		public MainWindow()
		{
			InitializeComponent();
			progress_textbox.Text = version;
			_syncContext = SynchronizationContext.Current;
		}


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			progress_textbox.Text = version;
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "Video Files(*.mp4;*.avi;*.MOV)|*.mp4;*.avi;*.MOV|All files (*.*)|*.*"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				String openPath = openFileDialog.FileName;
				transposer = new Transposer(openPath);
				//progress_bar.Maximum = transposer.originalFramesCount;

				gridbutton_mode1.Content = String.Format("MODE 1: {0}x{1}, {2}s.",
					transposer.originalFramesCount, transposer.originalHeight, transposer.originalWidth / transposer.fps);
				gridbutton_mode2.Content = String.Format("MODE 2: {0}x{1}, {2}s.",
					transposer.originalFramesCount, transposer.originalWidth, transposer.originalHeight / transposer.fps);

				DoubleAnimation doubleAnimation = new DoubleAnimation(0, 100, new Duration(TimeSpan.FromMilliseconds(250)));
				mode_grid.Visibility = Visibility.Visible;
				mode_grid.BeginAnimation(MaxHeightProperty, doubleAnimation);
			}
		}

		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void Status_bar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		async private void Gridbutton_mode1_Click(object sender, RoutedEventArgs e)
		{
			//mode_grid.Visibility = Visibility.Collapsed;

			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "Video files (.mp4)|*.mp4",
				FileName = DateTime.Now.ToString("ddMMyy_HHmmss") + "_m1", // Default file name
				DefaultExt = ".mp4"
			};

			if (saveFileDialog.ShowDialog() == true)
			{
				mode_grid.Visibility = Visibility.Collapsed;
				string savePath = saveFileDialog.FileName;
				await Task.Run(() => { transposer.TransposeVideo(savePath, '1'); });
			}
			GC.Collect();
		}

		async private void Gridbutton_mode2_Click(object sender, RoutedEventArgs e)
		{
			

			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "Video files (.mp4)|*.mp4",
				FileName = DateTime.Now.ToString("ddMMyy_HHmmss") + "_m2", // Default file name
				DefaultExt = ".mp4"
			};

			if (saveFileDialog.ShowDialog() == true)
			{
				mode_grid.Visibility = Visibility.Collapsed;
				string savePath = saveFileDialog.FileName;
				await Task.Run(() => { transposer.TransposeVideo(savePath, '2'); });
			}
			GC.Collect();
		}
	}
}
