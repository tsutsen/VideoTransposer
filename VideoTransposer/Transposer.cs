using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
//using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
	public class Transposer
	{
		public VideoCapture videofile;
		public int originalWidth, originalHeight, originalFramesCount, fps;
		public int resultWidth, resultHeight, resultFramesCount, resultLength;
		public int currentColumn;
		//readonly char mode;

		public Transposer(string video_name)
		{
			videofile = new VideoCapture(video_name);
			originalWidth = (int)videofile.GetCaptureProperty(CapProp.FrameWidth);
			originalHeight = (int)videofile.GetCaptureProperty(CapProp.FrameHeight);
			originalFramesCount = (int)videofile.GetCaptureProperty(CapProp.FrameCount);
			fps = (int)videofile.GetCaptureProperty(CapProp.Fps);
			currentColumn = 0;
		}

		public void TransposeVideo(string savePath, char mode)
		{
			if (mode == '1')
			{
				resultWidth = originalFramesCount;
				resultHeight = originalHeight;
				resultFramesCount = originalWidth;
			}
			if (mode == '2')
			{
				resultWidth = originalFramesCount;
				resultHeight = originalWidth;
				resultFramesCount = originalHeight;
			}

			Mat[] matArray = new Mat[resultFramesCount];
			for (int i = 0; i < resultFramesCount; ++i)
				matArray[i] = new Mat(resultHeight, resultWidth, DepthType.Cv8U, 3);


			//await Task.Run(() => {
			Application.Current.Dispatcher.Invoke(new Action(() => { ((MainWindow)Application.Current.MainWindow).progress_textbox.Text = "transposing frames"; }));

			//await Task.Run(() => { while (working) Application.Current.Dispatcher.Invoke(new Action(() => { ((MainWindow)Application.Current.MainWindow).progress_bar.Value = currentColumn + 1; })); });

			for (currentColumn = 0; currentColumn < resultWidth; ++currentColumn)
			{
				Mat frame = new Mat();
				videofile.Read(frame);

				for (int j = 0; j < resultFramesCount; ++j)
				{
					if (mode == '2')
					{
						Mat row = frame.Row(j);
						Mat rotatedRow = new Mat(originalWidth, 1, DepthType.Cv8U, 3);
						CvInvoke.Rotate(row, rotatedRow, (RotateFlags)2);
						rotatedRow.CopyTo(matArray[j].Col(currentColumn));
					}
					else
					{
						frame.Col(j).CopyTo(matArray[j].Col(currentColumn));
					}
					//await Task.Run(() => {Application.Current.Dispatcher.Invoke(new Action(() => { ((MainWindow)Application.Current.MainWindow).progress_bar.Value = currentColumn+1; }));});
				}
			}
			//});

			SaveVideo(savePath, ref matArray);
		}


		private void SaveVideo(string savePath, ref Mat[] transformedImages)
		{
			Application.Current.Dispatcher.Invoke(new Action(() => { ((MainWindow)Application.Current.MainWindow).progress_textbox.Text = "saving video"; }));
			//int codec = 861292868; //div3
			int codec = 1446269005; //mp4v
			VideoWriter outputVideo = new VideoWriter(savePath, codec, fps, new System.Drawing.Size(resultWidth, resultHeight), true);

			for (int i = 0; i < resultFramesCount; ++i)
			{
				outputVideo.Write(transformedImages[i]);
			}

			Application.Current.Dispatcher.Invoke(new Action(() => { ((MainWindow)Application.Current.MainWindow).progress_textbox.Text = "done"; }));

			//MessageBox.Show("done");

			//Application.Current.Dispatcher.Invoke(new Action(() => { progress_textbox }));
			//((MainWindow)Application.Current.MainWindow).progress_textbox.Text = "video saved";
		}
	}
}
