using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileView2D
{
	public partial class ViewerForm : Form
	{
		public byte[] Data = new byte[0];
		public Image DistanceImage;

		public ViewerForm()
		{
			InitializeComponent();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Open();
		}

		private void Open()
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				Load(openFileDialog.FileName);
			}
		}

		private void Load(string fileName)
		{
			try
			{
				using (var stream = File.OpenRead(fileName))
				{
					Data = new byte[stream.Length];

					stream.Read(Data, 0, Data.Length);

					stream.Close();

					LinearPictureBox.Image = LinearBitmap(Data);
					DistanceImage = DistanceBitmap(Data);
					DistancePictureBox.Image = DistanceImage;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		private Image LinearBitmap(byte[] data)
		{
			var bitmap = new Bitmap(16, Math.Max(data.Length, 16) / 16);

			var index = 0;

			while (index < data.Length &&
				index / bitmap.Width < bitmap.Height)
			{
				bitmap.SetPixel(index % bitmap.Width, index / bitmap.Width, Color.FromArgb(Math.Min(data[index] * 1, 255), Math.Min(data[index] * 2, 255), Math.Min(data[index] * 4, 255)));
				index++;
			}

			return bitmap;
		}

		private Image DistanceBitmap(byte[] data, int start = 0, int length = -1, int brightness = 8)
		{
			var bitmap = new Bitmap(256, 256);

			using (var graphics = Graphics.FromImage(bitmap))
				graphics.Clear(Color.Black);

			var index = start;

			while (index < data.Length - 1 &&
				(length == -1 || index < start + length))
			{
				var x = data[index];
				var y = 255 - data[index + 1];

				var pixel = bitmap.GetPixel(x, y);

				pixel = Color.FromArgb(Math.Min(pixel.R + brightness, 255), Math.Min(pixel.G + (brightness * 2), 255), Math.Min(pixel.B + (brightness * 3), 255));

				bitmap.SetPixel(x, y, pixel);
				index++;
			}

			return bitmap;
		}

		private void ViewerForm_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
		}

		private void ViewerForm_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

				if (files != null)
					foreach (var file in files.Take(1))
						new Thread((f) => { Load((string)f); }).Start(file);
			}
		}

		private void LinearPictureBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button.HasFlag(MouseButtons.Left))
			{
				var start = Math.Max(0, Math.Min(Data.Length - 1024, (int)(Data.Length * (e.Y / (float)LinearPictureBox.Height))));

				DistancePictureBox.Image = DistanceBitmap(Data, start, 1024, 32);
			}
		}

		private void LinearPictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			DistancePictureBox.Image = DistanceImage;
		}
	}
}
