using ContentAwareResize.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ContentAwareResize.Algorithms
{
    public static class SeamCarving
    {
        public static bool[,] leaveIt;
        public static List<List<MyColor>> ImageList = new List<List<MyColor>>();
        public static bool Visualisation = false;
        public static int Height = 0, Width = 0;
        public static MyColor[,] TempMatrix;
        public static List<int>[] AddedCounter;

        public static void Transpose()
        {
            List<List<MyColor>> TransImageList = new List<List<MyColor>>();
            for (int i = 0; i < Width; i++)
            {
                List<MyColor> currList = new List<MyColor>();
                for (int j = 0; j < Height; j++)
                    currList.Add(ImageList[j][i]);
                TransImageList.Add(currList);
            }
            ImageList = TransImageList;
            int t = Height;
            Height = Width;
            Width = t;
        }

        public static MyColor[,] Resize(MyColor[,] ImageMatrix, int newHeight, int newWidth, BackgroundWorker w)
        {
            ImageList.Clear();
            Height = ImageMatrix.GetLength(0);
            Width = ImageMatrix.GetLength(1);
            int HeightDifference = Height - newHeight;
            int WidthDifference = Width - newWidth;
            double total = Height - newHeight + Width - newWidth;
            double done = 0;

            for (int i = 0; i < Height; i++)
            {
                List<MyColor> currList = new List<MyColor>();
                for (int j = 0; j < Width; j++)
                    currList.Add(ImageMatrix[i, j]);
                ImageList.Add(currList);
            }



            if (WidthDifference > 0)
            {
                AddedCounter = new List<int>[Height];
                for (int i = 0; i < Height; i++)
                    AddedCounter[i] = new List<int>();
                while (WidthDifference > 0)
                {
                    DeleteColumn();
                    done++;
                    w.ReportProgress((int)((done / total) * 100));
                    WidthDifference--;
                }
            }

            if (HeightDifference > 0)
            {
                AddedCounter = new List<int>[Width];
                for (int i = 0; i < Width; i++)
                    AddedCounter[i] = new List<int>();
                Transpose();
                while (HeightDifference > 0)
                {
                    DeleteRow();
                    done++;
                    w.ReportProgress((int)((done / total) * 100));
                    HeightDifference--;
                }
                Transpose();
            }


            MyColor[,] newImageMatrix = new MyColor[newHeight, newWidth];
            int deltaw = ImageMatrix.GetLength(1) - newWidth;
            int deltah = ImageMatrix.GetLength(0) - newHeight;
            Console.WriteLine(deltaw);
            // O(newWidth * new Height)
            for (int i = 0; i < newHeight; i++)
                for (int j = 0; j < newWidth; j++)
                {
                    newImageMatrix[i, j] = ImageList[i][j]; // O(1)

                    if (leaveIt[i, j])  // O(1);
                        newImageMatrix[i - deltah / 2, j - deltaw / 2] = ImageMatrix[i, j]; // O(1);
                }

            return newImageMatrix;
        }

        public static void DeleteColumn()
        {

            List<int> dp = new List<int>();
            int[,] memo = new int[Height, Width];
            for (int i = 0; i < Height; i++)
                memo[i, 0] = 0;

            int LeastStart = (int)1e9, currI = Height - 1, currJ = -1;
            int h = Height, w = Width;
            for (int i = 1; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    int maxxGedn = 0;
                   
                    //maxxGedn = (leaveIt[i, j] || leaveIt[i - 1, j]) ? (int)1e9 : 0;

                    int c1 = memo[i - 1, j] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j]) + maxxGedn;
                    int c2 = (int)1e9;

                    if (j - 1 >= 0)
                    {
                        //maxxGedn = (leaveIt[i, j] || leaveIt[i - 1, j - 1]) ? (int)1e9 : 0;
                        c2 = memo[i - 1, j - 1] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j - 1]) + maxxGedn;
                    }
                    int c3 = (int)1e9;
                    if (j + 1 < Width)
                    {
                        //maxxGedn = (leaveIt[i, j] || leaveIt[i - 1, j + 1]) ? (int)1e9 : 0;
                        c3 = memo[i - 1, j + 1] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j + 1]) + maxxGedn;
                    }
                    memo[i, j] = Math.Min(c1, Math.Min(c2, c3));
                    dp.Add(memo[i, j]);
                    if (i == currI)
                    {
                        if (memo[i, j] < LeastStart)
                        {
                            currJ = j;       
                            LeastStart = memo[i, j];
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine(dp.Count);
            while (true)
            {
                ImageList[currI].RemoveAt(currJ);

                if (Visualisation)
                {
                    int tempi = currI;
                    int tempj = currJ + AddedCounter[tempi].Count(x => x < currJ);
                    TempMatrix[tempi, tempj].red = 1;
                    TempMatrix[tempi, tempj].blue = 0;
                    TempMatrix[tempi, tempj].green = 0;
                    AddedCounter[tempi].Add(tempj);
                }
                if (currI == 0)
                    break;

                int c1 = memo[currI - 1, currJ];
                int c2 = (int)1e9, c3 = (int)1e9;
                if (currJ - 1 >= 0)
                    c2 = memo[currI - 1, currJ - 1];
                if (currJ + 1 < Width)
                    c3 = memo[currI - 1, currJ + 1];

                if (c1 <= c2 && c1 <= c3)
                { currI--; }
                else if (c2 <= c1 && c2 <= c3)
                { currI--; currJ--; }
                else if (c3 <= c1 && c3 <= c2)
                { currI--; currJ++; }
            }
            Width--;
        }

        public static void DeleteRow()
        {
            int[,] memo = new int[Height, Width];
            for (int i = 0; i < Height; i++)
                memo[i, 0] = 0;
            int LeastStart = (int)1e9, currI = Height - 1, currJ = -1;
                
            for (int i = 1; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    int c1 = memo[i - 1, j] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j]);
                    int c2 = (int)1e9;
                    if (j - 1 >= 0)
                        c2 = memo[i - 1, j - 1] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j - 1]);
                    int c3 = (int)1e9;
                    if (j + 1 < Width)
                        c3 = memo[i - 1, j + 1] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j + 1]);
                    memo[i, j] = Math.Min(c1, Math.Min(c2, c3));
                    if (i == currI)
                    {
                        if (memo[i, j] < LeastStart)
                        {
                            currJ = j;
                            LeastStart = memo[i, j];
                        }
                    }
                }
            }
            while (true)
            {
                ImageList[currI].RemoveAt(currJ);
                if (Visualisation)
                {
                    int tempi = currI;
                    int tempj = currJ + AddedCounter[tempi].Count(x => x < currJ);
                    TempMatrix[tempj, tempi].red = 1;
                    TempMatrix[tempj, tempi].blue = 0;
                    TempMatrix[tempj, tempi].green = 0;
                    AddedCounter[tempi].Add(tempj);
                }




                if (currI == 0)
                    break;
                int c1 = memo[currI - 1, currJ];
                int c2 = (int)1e9, c3 = (int)1e9;
                if (currJ - 1 >= 0)
                    c2 = memo[currI - 1, currJ - 1];
                if (currJ + 1 < Width)
                    c3 = memo[currI - 1, currJ + 1];
                if (c1 <= c2 && c1 <= c3)
                { currI--; }
                else if (c2 <= c1 && c2 <= c3)
                { currI--; currJ--; }
                else if (c3 <= c1 && c3 <= c2)
                { currI--; currJ++; }
            }
            Width--;
        }
    }
}
