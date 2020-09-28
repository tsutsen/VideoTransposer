using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
		char mode = '0';
		private readonly SynchronizationContext _syncContext;
		Transposer transposer = new Transposer("C:\\Users\\brows\\source\\repos\\WpfApp1\\WpfApp1\\bin\\x64\\Release\\test.mp4", '1');

		public MainWindow()
		{
			InitializeComponent();
			_syncContext = SynchronizationContext.Current;
		}

		async private void UpdateStatus(Transposer transposer)
		{
			await Task.Run(() =>
			{
				Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "waiting"; }));
				while (transposer.working)
				{
					if (transposer.transposingFrames)
						Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "transposing frames"; }));
					if (transposer.savingVideo)
						Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "saving video"; }));
				}
				Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "done"; }));
			});
		}

		async private void UpdateProgressBar(Transposer transposer)
		{
			await Task.Run(() =>
			{
				while (transposer.working)
				{
					Application.Current.Dispatcher.Invoke(new Action(() =>
					{
						progress_bar.Value = transposer.currentColumn;
					}));
				}
			});
		}

		async private void Button_Click(object sender, RoutedEventArgs e)
		{
			progress_bar.Value = 0;
			progress_textbox.Text = "v.1.0.0";
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Video Files(*.mp4;*.avi;)|*.mp4;*.avi;|All files (*.*)|*.*";

			if (openFileDialog.ShowDialog() == true)
			{
				String openPath = openFileDialog.FileName;
				transposer = new Transposer(openPath, mode);
				/*
				BackgroundWorker worker = new BackgroundWorker();
				worker.RunWorkerCompleted += worker_RunWorkerCompleted;
				worker.WorkerReportsProgress = true;
				worker.DoWork += worker_DoWork;
				worker.ProgressChanged += worker_ProgressChanged;
				worker.RunWorkerAsync();
				*/

				mode = '0';
				progress_bar.Maximum = transposer.originalFramesCount;
				//UpdateStatus(transposer);

				gridbutton_mode1.Content = String.Format("MODE 1: {0}x{1}, {2}s.",
					transposer.originalFramesCount, transposer.originalHeight, transposer.originalWidth / transposer.fps);
				gridbutton_mode2.Content = String.Format("MODE 2: {0}x{1}, {2}s.",
					transposer.originalFramesCount, transposer.originalWidth, transposer.originalHeight / transposer.fps);

				//option_mode1.Visibility = Visibility.Visible;
				//option_mode2.Visibility = Visibility.Visible;

				//DoubleAnimation doubleAnimation = new DoubleAnimation();
				/*
				doubleAnimation.From = 0;
				doubleAnimation.To = 100;
				doubleAnimation.Duration = TimeSpan.FromMilliseconds(100);
				*/
				DoubleAnimation doubleAnimation = new DoubleAnimation(0, 100, new Duration(TimeSpan.FromMilliseconds(250)));
				mode_grid.Visibility = Visibility.Visible;
				mode_grid.BeginAnimation(MaxHeightProperty, doubleAnimation);
				//option_mode1.BeginAnimation(MaxHeightProperty, doubleAnimation);
				//option_mode2.BeginAnimation(MaxHeightProperty, doubleAnimation);

				await Task.Run(() =>
				{
					while (true)
					{
						if (mode != '0')
						{

							Application.Current.Dispatcher.Invoke((Action)(() =>
							{
								//mode_grid.BeginAnimation(MinHeightProperty, doubleAnimation);
								mode_grid.Visibility = Visibility.Collapsed;
								//option_mode1.Visibility = Visibility.Collapsed;
								//option_mode2.Visibility = Visibility.Collapsed;
							}));


							SaveFileDialog saveFileDialog = new SaveFileDialog();
							saveFileDialog.Filter = "Video files (.avi)|*.avi";
							saveFileDialog.FileName = DateTime.Now.ToString("ddMMyy_HHmmss"); // Default file name
							saveFileDialog.DefaultExt = ".avi";

							if (saveFileDialog.ShowDialog() == true)
							{
								string savePath = saveFileDialog.FileName;
								//Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "wait..."; }));


								//UpdateProgressBar(transposer);
								transposer.TransposeVideo(savePath, mode);

								mode = '0';

							}
						}
					}
				}
			);
			}
			//Console.WriteLine("Finished");

			progress_textbox.Text = "wait...";
		}

		private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(new Action(() => { progress_bar.Value = transposer.currentColumn; }));

			if (!transposer.transposingFrames && !transposer.savingVideo && !transposer.working)
				Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "waiting"; }));
			if (transposer.transposingFrames)
				Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "transposing frames"; }));
			if (transposer.savingVideo)
				Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "saving video"; }));
			Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox.Text = "done"; }));
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			var worker = sender as BackgroundWorker;
			worker.ReportProgress(0, String.Format("starting"));
			while (transposer.working)
				worker.ReportProgress(transposer.currentColumn / transposer.originalWidth, "transposing video");
			worker.ReportProgress(100, "done");
		}

		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			MessageBox.Show("done");
			progress_bar.Value = 0;
			progress_textbox.Text = "v.1.0.1";
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

		private void Gridbutton_mode1_Click(object sender, RoutedEventArgs e)
		{
			mode = '1';
		}

		private void Gridbutton_mode2_Click(object sender, RoutedEventArgs e)
		{
			mode = '2';
		}
	}
}
