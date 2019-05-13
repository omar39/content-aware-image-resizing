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
        public static bool[,] leaveIt;  // O(1)
        public static List<List<MyColor>> ImageList = new List<List<MyColor>>();    // O(1)
        public static bool Visualisation = false;   // O(1)
        public static int Height = 0, Width = 0;    // O(1)
        public static MyColor[,] TempMatrix;    // O(1)
        public static List<int>[] AddedCounter; // O(1)

        // O(Height * Width)
        public static void Transpose()
        {
            List<List<MyColor>> TransImageList = new List<List<MyColor>>(); // O(1)
            for (int i = 0; i < Width; i++) // O(Width)
            {
                List<MyColor> currList = new List<MyColor>();   // O(1)
                for (int j = 0; j < Height; j++)    // O(Height)
                    currList.Add(ImageList[j][i]);  // O(1)
                TransImageList.Add(currList);   // O(1)
            }
            ImageList = TransImageList; // O(1)
            int t = Height; // O(1)
            Height = Width; // O(1)
            Width = t;  // O(1)
        }

        public static MyColor[,] Resize(MyColor[,] ImageMatrix, int newHeight, int newWidth, BackgroundWorker w)
        {
            ImageList.Clear();  // O(N) : where N is the list count
            Height = ImageMatrix.GetLength(0);  // O(1)
            Width = ImageMatrix.GetLength(1);   // O(1)
            int HeightDifference = Height - newHeight;  // O(1)
            int WidthDifference = Width - newWidth; // O(1)
            double total = Height - newHeight + Width - newWidth;   // O(1)
            double done = 0;    // O(1)

            // O(Height * Width)
            for (int i = 0; i < Height; i++)    // O(Height)
            {
                List<MyColor> currList = new List<MyColor>();   // O(1)
                for (int j = 0; j < Width; j++) // O(Width)
                    currList.Add(ImageMatrix[i, j]);    // O(1)
                ImageList.Add(currList);    // O(1)
            }



            if (WidthDifference > 0)    // O(1)
            {
                AddedCounter = new List<int>[Height];   // O(1)
                for (int i = 0; i < Height; i++)    // O(Height)
                    AddedCounter[i] = new List<int>();  // O(1)
                while (WidthDifference > 0) // O()
                {
                    DeleteColumn(); // O(1)
                    done++; // O(1)
                    w.ReportProgress((int)((done / total) * 100));  // O(1)
                    WidthDifference--;  // O(1)
                }
            }

            if (HeightDifference > 0)   // O(1)
            {
                AddedCounter = new List<int>[Width];    // O(1)
                for (int i = 0; i < Width; i++) // O(Width)
                    AddedCounter[i] = new List<int>();  // O(1)
                Transpose();    // O(Height * Width)

                // O(HeightDifference * Height * Width)
                while (HeightDifference > 0)    // O(HeightDifference)
                {
                    DeleteRow();    // O(Height * Width)
                    done++; // O(1)
                    w.ReportProgress((int)((done / total) * 100));  // O(1)
                    HeightDifference--; // O(1)
                }
                Transpose();    // O(Height * Width)
            }

            MyColor[,] newImageMatrix = new MyColor[newHeight, newWidth];   // O(1)
            int deltaw = ImageMatrix.GetLength(1) - newWidth;   // O(1)
            int deltah = ImageMatrix.GetLength(0) - newHeight;  // O(1)

            // O(newWidth * new Height)
           
            for (int i = 0; i < newHeight; i++) // O(newHeight)
                for (int j = 0; j < newWidth; j++)  // O(newWidth)
                {
                    newImageMatrix[i, j] = ImageList[i][j]; // O(1)

                    if (leaveIt[i, j])  // O(1);
                        newImageMatrix[i - deltah / 2, j - deltaw / 2] = ImageMatrix[i, j]; // O(1);
                }

            return newImageMatrix;  // O(1)
        }

        //O(Height * Width)
        public static void DeleteColumn()
        {

            List<int> dp = new List<int>(); // O(1)
            int[,] memo = new int[Height, Width];   // O(1)

            //O(Height)
            for (int i = 0; i < Height; i++)
                memo[i, 0] = 0;

            int LeastStart = (int)1e9, currI = Height - 1, currJ = -1;  // O(1)
            int h = Height, w = Width;  // O(1)

            // O(Height * Width)
            for (int i = 1; i < h; i++) // O(Height)
            {
                for (int j = 0; j < w; j++) //O(Width)
                {

                    int c1 = memo[i - 1, j] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j]);  // O(1)
                    int c2 = (int)1e9;  // O(1)

                    if (j - 1 >= 0) // O(1)
                    {
                        c2 = memo[i - 1, j - 1] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j - 1]);  // O(1)
                    }

                    int c3 = (int)1e9;  // O(1)

                    if (j + 1 < Width)  // O(1)
                    {
                        c3 = memo[i - 1, j + 1] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j + 1]);  // O(1)
                    }
                    memo[i, j] = Math.Min(c1, Math.Min(c2, c3));    // O(1)
                    dp.Add(memo[i, j]); // O(1)
                    if (i == currI) // O(1)
                    {
                        if (memo[i, j] < LeastStart)    // O(1)
                        {
                            currJ = j;  // O(1)
                            LeastStart = memo[i, j];    // O(1)
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine(dp.Count);
            
            // O(N * currI) : where N is the "ImageList[currI]" count

            while (true)
            {
                ImageList[currI].RemoveAt(currJ);   // O(N) : where the N is the list count

                if (Visualisation)
                {
                    int tempi = currI;  // O(1)
                    int tempj = currJ + AddedCounter[tempi].Count(x => x < currJ);  // O(1)
                    TempMatrix[tempi, tempj].red = 1;   // O(1)
                    TempMatrix[tempi, tempj].blue = 0;  // O(1)
                    TempMatrix[tempi, tempj].green = 0; // O(1)
                    AddedCounter[tempi].Add(tempj); // O(1)
                }
                if (currI == 0) // O(1)
                    break;  // O(1)

                int c1 = memo[currI - 1, currJ]; // O(1)
                int c2 = (int)1e9, c3 = (int)1e9;   // O(1)
                if (currJ - 1 >= 0) // O(1)
                    c2 = memo[currI - 1, currJ - 1];    // O(1)
                if (currJ + 1 < Width)      // O(1)
                    c3 = memo[currI - 1, currJ + 1];    // O(1)

                if (c1 <= c2 && c1 <= c3)   // O(1)
                { currI--; }    // O(1)
                else if (c2 <= c1 && c2 <= c3)  // O(1)
                { currI--; currJ--; }   // O(1)
                else if (c3 <= c1 && c3 <= c2)  // O(1)
                { currI--; currJ++; }   // O(1)
            }
            Width--;    // O(1)
        }

        //O(Height * Width)
        public static void DeleteRow()
        {
            int[,] memo = new int[Height, Width];   // O(1)
            for (int i = 0; i < Height; i++)    // O(Height)
                memo[i, 0] = 0; // O(1)
            int LeastStart = (int)1e9, currI = Height - 1, currJ = -1;  // O(1)

            // O(Height * Width)
            for (int i = 1; i < Height; i++)    // O(Height)
            {
                for (int j = 0; j < Width; j++) // O(Width)
                {
                    int c1 = memo[i - 1, j] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j]);  // O(1)
                    int c2 = (int)1e9;  // O(1)
                    if (j - 1 >= 0) // O(1)
                        c2 = memo[i - 1, j - 1] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j - 1]);  // O(1)
                    int c3 = (int)1e9;  // O(1)
                    if (j + 1 < Width)  // O(1)
                        c3 = memo[i - 1, j + 1] + ImageOperations.CalculatePixelsEnergy(ImageList[i][j], ImageList[i - 1][j + 1]);  // O(1)
                    memo[i, j] = Math.Min(c1, Math.Min(c2, c3));    // O(1)

                    if (i == currI) // O(1)
                    {
                        if (memo[i, j] < LeastStart)    // O(1)
                        {
                            currJ = j;  // O(1)
                            LeastStart = memo[i, j];    // O(1)
                        }
                    }
                }
            }
            // O(N * currI) : where N is the "ImageList[currI]" count
            while (true)
            {
                ImageList[currI].RemoveAt(currJ);   // O(N) : where N is the list count
                if (Visualisation)  // O(1)
                {
                    int tempi = currI;  // O(1)
                    int tempj = currJ + AddedCounter[tempi].Count(x => x < currJ);  // O(1)
                    TempMatrix[tempj, tempi].red = 1;   // O(1)
                    TempMatrix[tempj, tempi].blue = 0;  // O(1)
                    TempMatrix[tempj, tempi].green = 0; // O(1)
                    AddedCounter[tempi].Add(tempj); // O(1)
                }




                if (currI == 0) // O(1)
                    break;  // O(1)
                int c1 = memo[currI - 1, currJ];    // O(1)
                int c2 = (int)1e9, c3 = (int)1e9;   // O(1)
                if (currJ - 1 >= 0) // O(1)
                    c2 = memo[currI - 1, currJ - 1];    // O(1)
                if (currJ + 1 < Width)  // O(1)
                    c3 = memo[currI - 1, currJ + 1];    // O(1)
                if (c1 <= c2 && c1 <= c3)   // O(1)
                { currI--; }    // O(1)
                else if (c2 <= c1 && c2 <= c3)  // O(1)
                { currI--; currJ--; }   // O(1)
                else if (c3 <= c1 && c3 <= c2) // O(1) 
                { currI--; currJ++; }   // O(1)
            }
            Width--;    // O(1)
        }
    }
}
