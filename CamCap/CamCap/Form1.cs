using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamCap
{
    public partial class Form1 : Form
    {
        int WIDTH = 640;
        int HEIGHT = 480;
        Mat _frame;
        VideoCapture _capture;
        VideoWriter _videoWriter;
        Bitmap _bmp;
        Graphics _graphic;
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
            this._timer.Tick += _timer_Tick;
            //カメラ画像取得用のVideoCapture作成
            _capture = new VideoCapture(0);
            if (!_capture.IsOpened())
            {
                MessageBox.Show("camera was not found!");
                this.Close();
            }
            _capture.FrameWidth = WIDTH;
            _capture.FrameHeight = HEIGHT;

            //取得先のMat作成
            _frame = new Mat(HEIGHT, WIDTH, MatType.CV_8UC3);

            //表示用のBitmap作成
            _bmp = new Bitmap(_frame.Cols, _frame.Rows, (int)_frame.Step(), System.Drawing.Imaging.PixelFormat.Format24bppRgb, _frame.Data);

            //PictureBoxを出力サイズに合わせる
            pictureBox1.Width = _frame.Cols;
            pictureBox1.Height = _frame.Rows;

            //描画用のGraphics作成
            _graphic = pictureBox1.CreateGraphics();

            //画像取得スレッド開始
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //描画
            _graphic.DrawImage(_bmp, 0, 0, _frame.Cols, _frame.Rows);

            _videoWriter?.Write(_frame);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            while (!backgroundWorker1.CancellationPending)
            {
                //画像取得
                _capture.Grab();
                NativeMethods.videoio_VideoCapture_operatorRightShift_Mat(_capture.CvPtr, _frame.CvPtr);

                bw.ReportProgress(0);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //スレッドの終了を待機
            backgroundWorker1.CancelAsync();
            while (backgroundWorker1.IsBusy)
                Application.DoEvents();
        }

        Timer _timer = new Timer();
        Stopwatch _sw = new Stopwatch();


        private void _timer_Tick(object sender, EventArgs e)
        {
            var path = Path.Combine(textBox1.Text, DateTime.Now.ToString("yyyyMMddhhmmssfff")) + ".jpg";
            _frame.SaveImage(path);
            if (_sw.Elapsed.TotalSeconds > 15)
            {
                _sw.Stop();
                _timer.Enabled = false;
                button1.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _timer.Interval = 50;
            _timer.Enabled = true;
            _sw.Restart();
            button1.Enabled = false;
        }

        private void buttonRecord_Click(object sender, EventArgs e)
        {
            if (_videoWriter == null)
            {
                _videoWriter = new VideoWriter();
                // https://www.atmarkit.co.jp/ait/articles/1610/18/news143_2.html
                if (!_videoWriter.Open(@"C:\Users\skeiya\caps\video.mp4", FourCC.MP4V, 20.0, new OpenCvSharp.Size(WIDTH, HEIGHT)))
                {
                    MessageBox.Show("hoge");
                    return;
                }
                buttonRecord.Text = "停止";
            }
            else
            {
                _videoWriter.Release();
                _videoWriter.Dispose();
                _videoWriter = null;
                buttonRecord.Text = "録画";
            }
        }
    }
}