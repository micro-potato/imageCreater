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

namespace imageCreater
{
    public partial class Form1 : Form
    {
        private int xCount, yCount, imageWidth, imageHeight,spawnCount,imageCount,paraDegree;
        private List<string> imageNames = new List<string>();
        Stopwatch stopWatch;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Extract images to joint big image
        /// </summary>
        /// <returns></returns>
        private List<string> GetImageNamesFormFolder()
        {
            string folderPath = Application.StartupPath + "\\gallery";
            List<string> galleryList = new List<string>();
            //var galleryList = Directory.GetFiles(folderPath, "*.JPEG").ToList<string>();
            DirectoryInfo folder = new DirectoryInfo(folderPath);

            foreach (FileInfo file in folder.GetFiles("*.JPEG"))
            {
                galleryList.Add(file.Name.Split('.')[0]);
            }
            List<string> imageTextList = new List<string>();
            Random random = new Random();
            for (int i = 0; i < imageCount; i++)
            {
                //判断如果列表还有可以取出的项,以防下标越界
                if (galleryList.Count > 0)
                {
                    //在列表中产生一个随机索引
                    int arrIndex = random.Next(0, galleryList.Count);
                    //将此随机索引的对应的列表元素值复制出来
                    imageTextList.Add(galleryList[arrIndex]);
                    //然后删掉此索引的列表项
                    galleryList.RemoveAt(arrIndex);
                }
                else
                {
                    //列表项取完后,退出循环,比如列表本来只有10项,但要求取出20项.
                    break;
                }
            }
            return imageTextList;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
            paraDegree = int.Parse(txtDegree.Text);

            xCount = int.Parse(txtXCount.Text);
            yCount = int.Parse(txtYcount.Text);
            imageWidth = int.Parse(txtWidth.Text);
            imageHeight = int.Parse(txtHeight.Text);
            spawnCount = int.Parse(textBox1.Text);
            imageCount = xCount * yCount;

            //TPL
            List<Task> imageTasks = new List<Task>(paraDegree);
            int dealCount = spawnCount / paraDegree;
            for (int degreeIndex=1;degreeIndex<=paraDegree;degreeIndex++)
            {
                int startIndex = (degreeIndex-1)* dealCount + 1;
                int endIndex = startIndex + dealCount - 1;
                Task task = new Task(() => SpawnImages(startIndex, endIndex));
                imageTasks.Add(task);
                task.Start();
            }
            Task.WhenAll(imageTasks).ContinueWith(ShowElepseTime);

            //.net4.0
            //Task.WaitAll(imageTasks.ToArray());
            //stopWatch.Stop();
            //txtTime.Text = (stopWatch.ElapsedMilliseconds/1000).ToString();
        }

        private void ShowElepseTime(Task obj)
        {
            stopWatch.Stop();
            txtTime.Text = (stopWatch.ElapsedMilliseconds / 1000).ToString();
        }

        private void SpawnImages(int startIndex, int endIndex)
        {
            for (int spawnIndex = startIndex; spawnIndex <= endIndex; spawnIndex++)//everytime spawn a image
            {
                imageNames = GetImageNamesFormFolder();
                //Init Graphics,image
                Graphics resultGraphics;
                Bitmap resultImg;
                resultImg = new Bitmap(xCount * imageWidth, yCount * imageHeight);
                resultGraphics = Graphics.FromImage(resultImg);

                //image info
                StringBuilder imageInfoBuilder = new StringBuilder();
                StringBuilder imageLocationBuilder = new StringBuilder();

                for (int imageIndex = 1; imageIndex <= imageCount; imageIndex++)
                {
                    var yIndex = Math.Ceiling((double)imageIndex / xCount);
                    var xIndex = imageIndex % xCount == 0 ? xCount : imageIndex % xCount;
                    var imageX = (xIndex - 1) * imageWidth;
                    var imageY = (int)(yIndex - 1) * imageHeight;
                    string imagePath = string.Format("{0}\\gallery\\{1}.JPEG", Application.StartupPath, imageNames[imageIndex - 1]);

                    //image info
                    string imageName = imageNames[imageIndex - 1].Split('.')[0];
                    imageInfoBuilder.Append(string.Format("{0},", imageName));

                    DrawImage(resultGraphics, imagePath, imageX, imageY);
                }

                //Save image
                resultImg.Save(string.Format("{0}\\image\\{1}.jpg", Application.StartupPath, spawnIndex.ToString()));
                resultGraphics.Dispose();

                //Save imageInfo
                string imageInfo = imageInfoBuilder.ToString().TrimEnd(',');
                string infoPath = string.Format("{0}\\image\\{1}.txt", Application.StartupPath, spawnIndex.ToString());
                File.WriteAllText(infoPath, imageInfo);
            }
        }

        private void DrawImage(Graphics graphics, string imagePath, int imageX, int imageY)
        {
            Point leftupConner = new Point(imageX, imageY);
            Point rightupConner = new Point(imageWidth+imageX,imageY);
            Point leftfownConner=new Point(imageX, imageY+imageHeight);
            Point[] locationPoints = new Point[] { leftupConner, rightupConner, leftfownConner };
            graphics.DrawImage(Image.FromFile(imagePath), locationPoints);
        }
    }
}
