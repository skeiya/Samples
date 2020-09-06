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
        Mat frame;
        VideoCapture capture;
        Bitmap bmp;
        Graphics graphic;
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
            this._timer.Tick += _timer_Tick;
            //カメラ画像取得用のVideoCapture作成
            capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                MessageBox.Show("camera was not found!");
                this.Close();
            }
            capture.FrameWidth = WIDTH;
            capture.FrameHeight = HEIGHT;

            //取得先のMat作成
            frame = new Mat(HEIGHT, WIDTH, MatType.CV_8UC3);

            //表示用のBitmap作成
            bmp = new Bitmap(frame.Cols, frame.Rows, (int)frame.Step(), System.Drawing.Imaging.PixelFormat.Format24bppRgb, frame.Data);

            //PictureBoxを出力サイズに合わせる
            pictureBox1.Width = frame.Cols;
            pictureBox1.Height = frame.Rows;

            //描画用のGraphics作成
            graphic = pictureBox1.CreateGraphics();

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
            graphic.DrawImage(bmp, 0, 0, frame.Cols, frame.Rows);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            while (!backgroundWorker1.CancellationPending)
            {
                //画像取得
                capture.Grab();
                NativeMethods.videoio_VideoCapture_operatorRightShift_Mat(capture.CvPtr, frame.CvPtr);

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
            frame.SaveImage(path);
            //using (Mat cap = new Mat(@"C:\cs_source\img\cap.png"))
            //{
            //    //保存されたキャプチャ画像の出力
            //    Cv2.ImShow("test1", frame);
            //}

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
    }
}