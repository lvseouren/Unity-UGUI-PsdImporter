using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    public struct TexRect
    {
        public int left;
        public int right;
        public int height;
        public int bottom;
        public Color color;

        public void Init(int bot, int hgt, Color clr)
        {
            bottom = bot;
            height = hgt;
            color = clr;
        }

        public int GetArea()
        {
            return (right - left) * height;
        }

        public bool IsSameColor(TexRect other)
        {
            return color.Equals(other.color);
        }

        public Vector4 GetRectInfo()
        {
            return new Vector4(left+1, right-1, bottom, bottom + height - 1);
        }
    }
    public class MyPsdImportUtility
    {
        static MyPsdImportUtility instance;
        public static MyPsdImportUtility Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new MyPsdImportUtility();
                }
                return instance;
            }
        }

        Texture2D texture;
        NativeArray<byte> textureData;
        int width, height;
        Vector4 rectInfo;//x:left,y:right,z:bottom,w:top
        int threshold = 500;
        void InitRectInfo()
        {
            if (rectInfo == null)
                rectInfo = Vector4.zero;
            rectInfo.x = -1;
        }
        public Vector4 Calculate9SliceInfo(Texture2D tex)
        {
            texture = tex;
            textureData = texture.GetRawTextureData<byte>();
            width = texture.width;
            height = texture.height;

            InitRectInfo();
            //求出最大矩形
            //try 9 slice
            Try9SliceTexture();
            //try horizontal 3 slice
            //try vertical 3 slice
            return rectInfo;
            //
        }

        void Try9SliceTexture()
        {
            //find the max valid internal rect
            int width = texture.width;
            int height = texture.height;
            if(width>threshold&&height>threshold)
            {
                if (!texture.GetPixel(width / 2, height / 2).Equals(texture.GetPixel(width / 2 + 10, height / 2 + 10)))
                    return;
            }
            for(int startRow = 0;startRow<height;++startRow)
            {
                List<TexRect> heights = new List<TexRect>(width);
                for(int col = 0;col<width;++col)
                {
                    for(int row = startRow;row<height;++row)
                    {
                        TexRect texRect;
                        if (row == startRow)
                        {
                            texRect = new TexRect();
                            texRect.Init(startRow, 1, GetTextureDataByRawAndCol(row, col));
                            heights.Add(texRect);
                        }else
                        {
                            texRect = heights[col];
                            var color = GetTextureDataByRawAndCol(row, col);
                            if (color.Equals(texRect.color))
                            {
                                texRect.height++;
                                heights[col] = texRect;
                            }
                            else
                                break;
                        }
                    }
                }
                var maxHeight = largestRectArea(heights);
                startRow += maxHeight;
            }
        }

        int largestRectArea(List<TexRect> heights)
        {
            int maxHeight = 0;
            int n = heights.Count;
            Stack<int> mono_stack = new Stack<int>();
            for (int i = 0; i < n; ++i)
            {
                while (mono_stack.Count > 0 && heights[mono_stack.Peek()].height >= heights[i].height && heights[mono_stack.Peek()].IsSameColor(heights[i]))
                    mono_stack.Pop();
                var rectInfo = heights[i];
                rectInfo.left = mono_stack.Count == 0 ? -1 : mono_stack.Peek();
                heights[i] = rectInfo;
                mono_stack.Push(i);
            }

            mono_stack.Clear();
            for (int i = n -1; i >= 0; --i)
            {
                while (mono_stack.Count > 0 && heights[mono_stack.Peek()].height >= heights[i].height && heights[mono_stack.Peek()].IsSameColor(heights[i]))
                    mono_stack.Pop();
                var rectInfo = heights[i];
                rectInfo.right = mono_stack.Count == 0 ? n : mono_stack.Peek();
                heights[i] = rectInfo;
                mono_stack.Push(i);
            }
            for(int i = 0;i<n;++i)
            {
                var rectInfo = heights[i];
                rectInfo.left = Math.Max(rectInfo.left, i);
                rectInfo.right = Math.Min(rectInfo.right, width - i);
                heights[i] = rectInfo;
            }
            int maxArea = 0;
            for(int i = 0;i<n;++i)
            {
                if (heights[i].GetArea() > maxArea && Validate9Slice(heights[i]))
                {
                    maxArea = heights[i].GetArea();
                    rectInfo = heights[i].GetRectInfo();
                    //maxHeight = Math.Max(maxHeight, heights[i].height - 1);
                    i = heights[i].right;
                }
            }
            return maxHeight;
        }

        //O(n)时间复杂度
        bool Validate9Slice(TexRect rectInfo)
        {
            if (rectInfo.height == 1||rectInfo.right<=rectInfo.left)
                return false;
            var rect = rectInfo.GetRectInfo();
            int left = (int)rect.x;
            int right = (int)rect.y;
            int bottom = (int)rect.z;
            int top = (int)rect.w;
            var compareColor = GetTextureDataByRawAndCol(bottom, left);
            for (int col = left; col <= right; ++col)
                for (int row = bottom; row <= top; ++row)
                    if (!GetTextureDataByRawAndCol(row, col).Equals(compareColor))
                        return false;

            //top
            for (int row = top + 1; row < texture.height; ++row)
            {
                compareColor = GetTextureDataByRawAndCol(row, left);
                for (int col = left; col <= right; ++col)
                    if (!GetTextureDataByRawAndCol(row, col).Equals(compareColor))
                        return false;
            }
            //bottom
            for (int row = 0; row < bottom; ++row)
            {
                compareColor = GetTextureDataByRawAndCol(row, left);
                for (int col = left; col <= right; ++col)
                    if (!GetTextureDataByRawAndCol(row, col).Equals(compareColor))
                        return false;
            }
            //left
            for (int col = 0; col < left; ++col)
            {
                compareColor = GetTextureDataByRawAndCol(bottom, col);
                for (int row = bottom; row <= top; ++row)
                    if (!GetTextureDataByRawAndCol(row, col).Equals(compareColor))
                        return false;
            }
            //right
            for (int col = right+1; col < texture.width; ++col)
            {
                compareColor = GetTextureDataByRawAndCol(bottom, col);
                for (int row = bottom; row <= top; ++row)
                    if (!GetTextureDataByRawAndCol(row, col).Equals(compareColor))
                        return false;
            }
            return true;
        }

        internal void TestTextureCoordinates(Texture2D texture)
        {
            //x代表列，y代表行
            for (int i = 0; i < texture.height; ++i)
                Debug.Log("texture travel y = "+i+",x=0:" + texture.GetPixel(texture.width/2, i));
            for (int i = 0; i < texture.width; ++i)
                Debug.Log("texture travel x = " + i + ",y=0:" + texture.GetPixel(i,texture.height / 2));
            //for (int i = 0; i < texture.width; ++i)
            //    Debug.Log("texture travel y,x=0:" + texture.GetPixel(0, i));
        }

        internal Texture2D Get9SliceTexture(Texture2D texture, Vector4 rectInfo)
        {
            Debug.Log("left="+rectInfo.x+",right="+rectInfo.y+ "bottom=" + rectInfo.z + ",top=" + rectInfo.w);
            int left = (int)rectInfo.x;
            if (left == 0)
                left += 2;

            var rawData = texture.GetPixels32();
            if(left >= 0)
            {
                int right = (int)rectInfo.y;
                if (right == texture.width - 1)
                    right -= 2;
                right = Math.Max(right, left);

                int bottom = (int)rectInfo.z;
                int top = (int)rectInfo.w;

                int width = right - left + 1;
                width = texture.width - width + 1;
                int height = top - bottom + 1;
                height = texture.height - height + 1;
                Texture2D ret = new Texture2D(width, height);
                Color32[] pixels = new Color32[width*height];
                for(int row = 0;row<texture.height;++row)
                    for(int col = 0;col<texture.width;++col)
                    {
                        int convertRow = row;
                        if(col<=left)
                        {
                            int convertCol = col;
                            if (row <= bottom)
                            {
                                var color = rawData[row * texture.width + col];
                                pixels[convertRow * width + convertCol] = color;
                            }
                            else if (row > top)
                            {
                                convertRow = row - (top - bottom);
                                var color = rawData[row * texture.width + col];
                                pixels[convertRow * width + convertCol] = color;
                            }
                        }else if(col>right)
                        {
                            int convertCol = col-(right-left);
                            if (row <= bottom)
                            {
                                var color = rawData[row * texture.width + col];
                                pixels[convertRow * width + convertCol] = color;
                            }
                            else if (row > top)
                            {
                                convertRow = row - (top - bottom);
                                var color = rawData[row * texture.width + col];
                                pixels[convertRow * width + convertCol] = color;
                            }
                        }
                    }

                ret.SetPixels32(pixels);
                ret.Apply();
                return ret;
            }
            return texture;
        }

        Color GetTextureDataByRawAndCol(int row, int col)
        {
            return texture.GetPixel(col, row);
        }
    }
}
