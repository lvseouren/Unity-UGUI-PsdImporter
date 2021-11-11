using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    enum SliceType
    {
        Horizontal_3slice,
        Vertical_3slice,
        NineSlice,
    }
    
    public struct RectInfo
    {
        public int x, y, z, w;
        public RectInfo(int l,int r,int b,int t)
        {
            x = l;
            y = r;
            z = b;
            w = t;
        }

        public static implicit operator Vector4(RectInfo rect)
        {
            return new Vector4(rect.x, rect.y, rect.z, rect.w);
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
        int texWidth, texHeight;
        RectInfo rectInfo;//x:left,y:right,z:bottom,w:top
        
        int threshold_slice = 30;
        SliceType sliceType;
        //horizontal 3 slice
        int slice_left, slice_right;
        //vertical 3 slice
        int slice_top, slice_bottom;
        void InitRectInfo()
        {
            rectInfo.x = -1;
        }
        public RectInfo Calculate9SliceInfo(Texture2D tex)
        {
            texture = tex;
            textureData = texture.GetRawTextureData<byte>();
            texWidth = texture.width;
            texHeight = texture.height;

            InitRectInfo();
            //求出最大矩形
            TryHorizontal3Slice();
            TryVertical3Slice();
            int horizontal_length = slice_right - slice_left;
            int vertical_length = slice_top - slice_bottom;
            //try 9 slice
            if (vertical_length >= threshold_slice && horizontal_length >= threshold_slice)
            {
                if (!Try9SliceTexture())
                {
                    SetRectInfoBy3Slice(horizontal_length * texHeight > vertical_length * texWidth);
                }
                else
                    sliceType = SliceType.NineSlice;
            }
            else
            {
                if (horizontal_length >= threshold_slice)
                {
                    SetRectInfoBy3Slice(true);
                }
                else if (vertical_length >= threshold_slice)
                {
                    SetRectInfoBy3Slice(false);
                }
            }
            //try horizontal 3 slice
            //try vertical 3 slice
            return rectInfo;
        }

        void SetRectInfoBy3Slice(bool isHorizontalSlice)
        {
            if (isHorizontalSlice)
            {
                sliceType = SliceType.Horizontal_3slice;
                rectInfo.x = slice_left;
                rectInfo.y = slice_right;
                rectInfo.z = 0;
                rectInfo.w = texHeight - 1;
            }
            else
            {
                sliceType = SliceType.Vertical_3slice;
                rectInfo.x = 0;
                rectInfo.y = texWidth - 1;
                rectInfo.z = slice_bottom;
                rectInfo.w = slice_top;
            }
        }

        bool Try9SliceTexture()
        {
            var anchorColor = GetTexturePixelDataByRawAndCol(slice_bottom, slice_left);
            for(int row = slice_bottom;row<=slice_top;++row)
                if(!GetTexturePixelDataByRawAndCol(row,slice_left).Equals(anchorColor))
                    return false;

            rectInfo.x = slice_left;
            rectInfo.y = slice_right;
            rectInfo.z = slice_bottom;
            rectInfo.w = slice_top;
            if (Validate9Slice(rectInfo))
                return true;
            else
            {
                rectInfo.x = -1;//置为无效
                return false;
            }
        }

        /*
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
                rectInfo.height = Math.Min(rectInfo.height, height - 2 * rectInfo.bottom);
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
        */

        //O(n)时间复杂度
        bool Validate9Slice(RectInfo rect)
        {
            int left = rect.x;
            int right = rect.y;
            int bottom = rect.z;
            int top = rect.w;

            Color compareColor;
            //top
            for (int row = top + 1; row < texture.height; ++row)
            {
                compareColor = GetTexturePixelDataByRawAndCol(row, left);
                for (int col = left; col <= right; ++col)
                    if (!GetTexturePixelDataByRawAndCol(row, col).Equals(compareColor))
                        return false;
            }
            //bottom
            for (int row = 0; row < bottom; ++row)
            {
                compareColor = GetTexturePixelDataByRawAndCol(row, left);
                for (int col = left; col <= right; ++col)
                {
                    var color = GetTexturePixelDataByRawAndCol(row, col);
                    if (!color.Equals(compareColor))
                        return false;
                }
            }
            
            //left
            for (int col = 0; col < left; ++col)
            {
                compareColor = GetTexturePixelDataByRawAndCol(bottom, col);
                for (int row = bottom; row <= top; ++row)
                    if (!GetTexturePixelDataByRawAndCol(row, col).Equals(compareColor))
                        return false;
            }
            //right
            for (int col = right+1; col < texture.width; ++col)
            {
                compareColor = GetTexturePixelDataByRawAndCol(bottom, col);
                for (int row = bottom; row <= top; ++row)
                    if (!GetTexturePixelDataByRawAndCol(row, col).Equals(compareColor))
                        return false;
            }
            return true;
        }

        void TryHorizontal3Slice()
        {
            List<int> leftArr = new List<int>(texHeight);
            List<int> rightArr = new List<int>(texHeight);
            for(int row = 0;row<texHeight;row++)
            {
                int left = texWidth / 2, right = texWidth / 2;
                var anchordColor = GetTexturePixelDataByRawAndCol(row, left);
                while(left>0)
                {
                    var color = GetTexturePixelDataByRawAndCol(row, left);
                    if (color.Equals(anchordColor))
                        left--;
                    else
                        break;
                }
                while (right < texWidth-1)
                {
                    if (GetTexturePixelDataByRawAndCol(row, right).Equals(anchordColor))
                        right++;
                    else
                        break;
                }
                leftArr.Add(left+1);
                rightArr.Add(right-1);
            }
            slice_left = leftArr.Max();
            slice_right = rightArr.Min();
        }

        void TryVertical3Slice()
        {
            List<int> bottomArr = new List<int>(texWidth);
            List<int> topArr = new List<int>(texWidth);
            for (int col = 0; col < texWidth; col++)
            {
                int bottom = texHeight / 2, top = texHeight / 2;
                var anchordColor = GetTexturePixelDataByRawAndCol(bottom, col);
                while (bottom > 0)
                {
                    if (GetTexturePixelDataByRawAndCol(bottom, col).Equals(anchordColor))
                        bottom--;
                    else
                        break;
                }
                while (top < texHeight - 1)
                {
                    if (GetTexturePixelDataByRawAndCol(top, col).Equals(anchordColor))
                        top++;
                    else
                        break;
                }
                bottomArr.Add(bottom+1);
                topArr.Add(top-1);
            }
            slice_bottom = bottomArr.Max();
            slice_top = topArr.Min();
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

        internal Texture2D Get9SliceTexture(Texture2D tex, ref RectInfo rect)
        {
            Debug.Log("left="+rectInfo.x+",right="+rectInfo.y+ "bottom=" + rectInfo.z + ",top=" + rectInfo.w);

            if(rect.x >= 0)
            {
                if (rect.x == 0)
                    rect.x += 2;
                if (rect.y == tex.width - 1)
                    rect.y -= 2;
                switch (sliceType)
                {
                    case SliceType.NineSlice:
                        return GetSliceTexture_NineSlice(tex, ref rect);
                    case SliceType.Horizontal_3slice:
                        return Get3SliceTexture_Horizontal(tex, ref rect);
                    case SliceType.Vertical_3slice:
                        return GetSliceTexture_Vertical(tex, ref rect);
                }
            }
            return tex;
        }

        Texture2D Get3SliceTexture_Horizontal(Texture2D tex, ref RectInfo rect)
        {
            var rawData = texture.GetPixels32();
            int left = rect.x;
            int right = rect.y;

            int width = right - left + 1;
            width = tex.width - width + 2;
            int height = tex.height;

            Texture2D ret = new Texture2D(width, height);
            Color32[] pixels = new Color32[width * height];
            for(int row = 0;row<tex.height;++row)
                for(int col = 0;col<tex.width;++col)
                {
                    int convertCol = col;
                    if (col <= left+1)
                    {
                        pixels[row * width + convertCol] = rawData[row * tex.width + col];
                    }
                    else if (col <= right)
                    { }
                    else 
                    {
                        convertCol = col - (right - left)+1;
                        pixels[row * width + convertCol] = rawData[row * tex.width + col];
                    }
                }
            ret.SetPixels32(pixels);
            ret.Apply();

            //update rectinfo as border
            rect.x = left;
            rect.y = 0;
            rect.z = tex.width - right;
            rect.w = 0;

            return ret;
        }

        Texture2D GetSliceTexture_Vertical(Texture2D tex, ref RectInfo rect)
        {
            var rawData = texture.GetPixels32();
            int bottom = rect.z;
            int top = rect.w;

            int width = tex.width;
            int height = top - bottom + 1;
            height = tex.height - height + 2;

            Texture2D ret = new Texture2D(width, height);
            Color32[] pixels = new Color32[width * height];
            for (int col = 0; col < tex.width; ++col)
                for (int row = 0; row < tex.height; ++row)
                {
                    int convertCol = col;
                    if (row <= bottom + 1)
                    {
                        pixels[row * width + convertCol] = rawData[row * tex.width + col];
                    }
                    else if (col <= top)
                    { }
                    else
                    {
                        convertCol = col - (top-bottom) + 1;
                        pixels[row * width + convertCol] = rawData[row * tex.width + col];
                    }
                }
            ret.SetPixels32(pixels);
            ret.Apply();

            //update rectinfo as border
            rect.x = 0;
            rect.y = bottom;
            rect.z = 0;
            rect.w = tex.height-top;

            return ret;
        }

        Texture2D GetSliceTexture_NineSlice(Texture2D tex, ref RectInfo rect)
        {
            var rawData = texture.GetPixels32();
            int left = rect.x;
            int right = rect.y;
            int bottom = rect.z;
            int top = rect.w;

            int width = right - left + 1;
            width = tex.width - width + 2;
            int height = top - bottom + 1;
            height = tex.height - height + 2;

            Texture2D ret = new Texture2D(width, height);
            Color32[] pixels = new Color32[width * height];
            for (int row = 0; row < tex.height; ++row)
                for (int col = 0; col < tex.width; ++col)
                {
                    int convertRow = row;
                    if (col <= left + 1)
                    {
                        int convertCol = col;
                        if (row <= bottom + 1)
                        {
                            var color = rawData[row * tex.width + col];
                            pixels[convertRow * width + convertCol] = color;
                        }
                        else if (row > top)
                        {
                            convertRow = row - (top - bottom) + 1;
                            var color = rawData[row * tex.width + col];
                            pixels[convertRow * width + convertCol] = color;
                        }
                    }
                    else if (col > right)
                    {
                        int convertCol = col - (right - left) + 1;
                        if (row <= bottom + 1)
                        {
                            var color = rawData[row * tex.width + col];
                            pixels[convertRow * width + convertCol] = color;
                        }
                        else if (row > top)
                        {
                            convertRow = row - (top - bottom) + 1;
                            var color = rawData[row * tex.width + col];
                            pixels[convertRow * width + convertCol] = color;
                        }
                    }
                }

            ret.SetPixels32(pixels);
            ret.Apply();

            //update rectinfo as border
            rect.x = left;
            int rightBorder = tex.width - right;
            rect.z = rightBorder;
            rect.y = bottom;
            int topBorder = tex.height - top;
            rect.w = topBorder;
            return ret;
        }

        Color GetTexturePixelDataByRawAndCol(int row, int col)
        {
            return texture.GetPixel(col, row);
        }

        public Vector4 GetSpriteBorder(Texture2D tex, RectInfo rectInfo)
        {
            Vector4 border = Vector4.zero;
            border.x = rectInfo.x;
            border.y = rectInfo.y;
            border.z = rectInfo.z;
            border.w = rectInfo.w;
            return border;
        }
    }
}
