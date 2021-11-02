using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Object = UnityEngine.Object;
#endif

namespace PantheonGames.TexturePacker
{
    [ExecuteAlways]
    [CreateAssetMenu(fileName = nameof(SpriteAtlas), menuName = "Texture Packer/" + nameof(SpriteAtlas))]
    public sealed class SpriteAtlas : ScriptableObject
    {
        [SerializeField]
        private string m_Tag;
        [SerializeField]
        private Sprite[] m_Sprites;

        private Dictionary<string, Sprite> m_SpriteDic;

        public string tag
        {
            get { return m_Tag; }
        }

        public int spriteCount { get { return m_Sprites.Length; } }

        public Sprite this[int index]
        {
            get
            {
                if (m_Sprites.Length > 0 && index >= 0 && index < m_Sprites.Length)
                    return m_Sprites[index];
                return null;
            }
        }

        void OnEnable()
        {
            if (m_Sprites == null)
                return;

            if (m_SpriteDic == null)
                m_SpriteDic = new Dictionary<string, Sprite>(m_Sprites.Length);
            m_SpriteDic.Clear();
            foreach (var sprite in m_Sprites)
            {
                if (sprite != null)
                    m_SpriteDic.Add(sprite.name, sprite);
            }
        }

        public bool CanBindTo(Sprite sprite)
        {
            if (m_SpriteDic != null && m_SpriteDic.TryGetValue(sprite.name, out Sprite cached) && sprite == cached)
                return true;
            return false;
        }

        public Sprite GetSprite(string name)
        {
            if (m_SpriteDic != null && m_SpriteDic.TryGetValue(name, out Sprite sprite))
                return sprite;
            return null;
        }

        public Sprite[] GetSprites()
        {
            Sprite[] sprites = new Sprite[m_Sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
                sprites[i] = m_Sprites[i];
            return sprites;
        }

#if UNITY_EDITOR
        public static readonly int[] validMaxSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        [SerializeField]
        private Vector2Int m_MaxSize = new Vector2Int(2048, 2048);
        [SerializeField]
        private Vector2Int m_FixedSize = Vector2Int.zero;
        [SerializeField]
        private SizeConstraints m_SizeConstraints = SizeConstraints.AnySize;
        [SerializeField]
        private bool m_ForceSquared;
        
        [SerializeField]
        private Algorithm m_Algorithm = Algorithm.MaxRects;
        [SerializeField]
        private BasicSorting m_Sorting = BasicSorting.Best;
        [SerializeField]
        private BasicSortingOrder m_SortingOrder = BasicSortingOrder.Ascending;
        [SerializeField]
        private HeuristicsMode m_HeuristicsMode = HeuristicsMode.Best;
        [SerializeField]
        private PackMode m_PackMode = PackMode.Best;
        [SerializeField, Range(1, 32)]
        private int m_AlignToGrid = 1;
        
        [SerializeField]
        private TrimMode m_TrimMode = TrimMode.None;
        [SerializeField, Range(0, 512)]
        private int m_TrimMargin = 1;
        [SerializeField, Range(1, 255)]
        private int m_AlphaThreshold = 1;
        [SerializeField, Range(10, 1000)]
        private int m_TracerTolerance = 200;
        
        [SerializeField]
        private int m_Extrude = 1;
        [SerializeField, Range(0, 512)]
        private int m_BorderPadding;
        [SerializeField, Range(0, 512)]
        private int m_ShapePadding;
        
        [SerializeField]
        private int m_FormatMaxSize = 2048;
        [SerializeField]
        private Object[] m_Objects;

        public Vector2Int MaxSize
        {
            get => m_MaxSize;
            set => m_MaxSize = value;
        }

        public Vector2Int FixedSize
        {
            get => m_FixedSize;
            set => m_FixedSize = value;
        }

        public SizeConstraints SizeConstraints
        {
            get => m_SizeConstraints;
            set => m_SizeConstraints = value;
        }

        public bool ForceSquared
        {
            get => m_ForceSquared;
            set => m_ForceSquared = value;
        }

        public Algorithm Algorithm
        {
            get => m_Algorithm;
            set => m_Algorithm = value;
        }

        public BasicSorting Sorting
        {
            get => m_Sorting;
            set => m_Sorting = value;
        }

        public BasicSortingOrder SortingOrder
        {
            get => m_SortingOrder;
            set => m_SortingOrder = value;
        }

        public HeuristicsMode HeuristicsMode
        {
            get => m_HeuristicsMode;
            set => m_HeuristicsMode = value;
        }

        public PackMode PackMode
        {
            get => m_PackMode;
            set => m_PackMode = value;
        }

        public int AlignToGrid
        {
            get => m_AlignToGrid;
            set => m_AlignToGrid = value;
        }

        public TrimMode TrimMode
        {
            get => m_TrimMode;
            set => m_TrimMode = value;
        }

        public int TrimMargin
        {
            get => m_TrimMargin;
            set => m_TrimMargin = value;
        }

        public int AlphaThreshold
        {
            get => m_AlphaThreshold;
            set => m_AlphaThreshold = value;
        }

        public int TracerTolerance
        {
            get => m_TracerTolerance;
            set => m_TracerTolerance = value;
        }

        public int Extrude
        {
            get => m_Extrude;
            set => m_Extrude = value;
        }

        public int BorderPadding
        {
            get => m_BorderPadding;
            set => m_BorderPadding = value;
        }

        public int ShapePadding
        {
            get => m_ShapePadding;
            set => m_ShapePadding = value;
        }

        public int FormatMaxSize
        {
            get => m_FormatMaxSize;
            set => m_FormatMaxSize = value;
        }

        public Object[] Objects
        {
            get => m_Objects;
            set => m_Objects = value;
        }

        private void OnValidate()
        {
            if (m_MaxSize.x < 1)
                m_MaxSize.x = 1;
            if (m_MaxSize.y < 1)
                m_MaxSize.y = 1;
            if (m_FixedSize.x < 0)
                m_FixedSize.x = 0;
            if (m_FixedSize.y < 0)
                m_FixedSize.y = 0;
        }
#endif
    }
}
