using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Morpheus.GameData
{
    public unsafe struct Ni18NTextArray
    {
        public readonly int Length;
        private readonly uint* pointer;

        public string this[int index]
        {
            get
            {
                return pointer == null || index < 0 || index >= Length
                    ? throw new AccessViolationException()
                    : I18NTextManager.Instance.GetText(*(pointer + index));
            }
        }

        public Ni18NTextArray(uint* ptr, int len)
        {
            pointer = ptr;
            Length = len;
        }
    }

    public sealed unsafe class I18NTextManager
    {
        #region Singleton
        public static I18NTextManager Instance { get; private set; }

        public static void CreateInstance()
        {
            Instance ??= new I18NTextManager();
        }

        public static void ReleaseInstance()
        {
            Instance = null;
        }
        #endregion

        private const string FileExt = ".dat";
        private const string I18NFilePrefix = "I18nText_";
        private const int TextCountSize = 4;
        private const int KeySize = 4;
        private const int InfoTableEntrySize = 8;

        private static readonly Encoding StringEncoder = Encoding.UTF8; // For efficiency.

        private string i18NTextDirPath;

        private MemoryMappedFile i18NTextMmapFile;
        private MemoryMappedViewAccessor i18NTextMmapAccessor;
        private byte* i18NMmapPtr;

        public string CurrentLanguage { get; private set; }

        private I18NTextManager() { }

        ~I18NTextManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (i18NTextMmapFile != null)
            {
                i18NTextMmapAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                i18NTextMmapFile.Dispose();
                i18NTextMmapFile = null;
            }
            GC.SuppressFinalize(this);
        }

        public void SetI18NTextDirectoryPath(string i18NTextDirPath)
        {
            this.i18NTextDirPath = i18NTextDirPath;
        }

        public void SwitchLanguage(string langName)
        {
            if (CurrentLanguage == langName)
            {
                return;
            }

            try
            {
                if (i18NTextMmapFile != null)
                {
                    i18NTextMmapAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                    i18NTextMmapFile.Dispose();
                }

                string filePath = Path.Combine(i18NTextDirPath, $"{I18NFilePrefix}{langName}{FileExt}");
                i18NTextMmapFile = MemoryMappedFile.CreateFromFile(filePath);
                i18NTextMmapAccessor = i18NTextMmapFile.CreateViewAccessor();
                i18NTextMmapAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref i18NMmapPtr);
                CurrentLanguage = langName;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }
        }

        public string GetText(uint id)
        {
            if (i18NMmapPtr == null)
            {
                return id.ToString();
            }

            int min = 0;
            int max = *(int*)i18NMmapPtr - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                int offset = TextCountSize + mid * InfoTableEntrySize;
                uint key = *(uint*)(i18NMmapPtr + offset);
                if (id == key)
                {
                    byte* textPtr = i18NMmapPtr + *(int*)(i18NMmapPtr + offset + KeySize);
                    return StringEncoder.GetString(textPtr + TextCountSize, *(int*)textPtr);
                }

                if (id < key)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }
            return id.ToString();
        }
    }
}