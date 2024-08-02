using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Blanketmen.Hypnos
{
    public readonly unsafe struct NI18nTextArray
    {
        public readonly int length;
        private readonly uint* pointer;

        public string this[int index]
        {
            get
            {
                return pointer == null || index < 0 || index >= length
                    ? throw new AccessViolationException()
                    : I18nTextManager.Instance.GetText(*(pointer + index));
            }
        }

        public NI18nTextArray(uint* ptr, int len)
        {
            pointer = ptr;
            length = len;
        }
    }

    public sealed unsafe class I18nTextManager
    {
        #region Singleton
        public static I18nTextManager Instance { get; } = new I18nTextManager();

        private I18nTextManager() { }

        ~I18nTextManager()
        {
            Dispose();
        }
        #endregion

        private const string FileExt = ".dat";
        private const string I18nFilePrefix = "I18nText_";
        private const int TextCountSize = 4;
        private const int KeySize = 4;
        private const int InfoTableEntrySize = 8;

        private static readonly System.Text.Encoding StringEncoder = System.Text.Encoding.UTF8; // For efficiency.

        private string i18nTextDirPath;

        private MemoryMappedFile i18nTextMmapFile;
        private MemoryMappedViewAccessor i18nTextMmapAccessor;
        private byte* i18nMmapPtr;

        public string CurrentLanguage { get; private set; }

        public void Dispose()
        {
            if (i18nTextMmapFile != null)
            {
                i18nTextMmapAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                i18nTextMmapFile.Dispose();
                i18nTextMmapFile = null;
            }
            GC.SuppressFinalize(this);
        }

        public void SetI18nTextDirectoryPath(string i18nTextDirPath)
        {
            this.i18nTextDirPath = i18nTextDirPath;
        }

        public void SwitchLanguage(string langName)
        {
            if (CurrentLanguage == langName)
            {
                return;
            }

            try
            {
                if (i18nTextMmapFile != null)
                {
                    i18nTextMmapAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                    i18nTextMmapFile.Dispose();
                }

                string filePath = Path.Combine(i18nTextDirPath, $"{I18nFilePrefix}{langName}{FileExt}");
                i18nTextMmapFile = MemoryMappedFile.CreateFromFile(filePath);
                i18nTextMmapAccessor = i18nTextMmapFile.CreateViewAccessor();
                i18nTextMmapAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref i18nMmapPtr);
                CurrentLanguage = langName;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }
        }

        public string GetText(uint id)
        {
            if (i18nMmapPtr == null)
            {
                return id.ToString();
            }

            int min = 0;
            int max = *(int*)i18nMmapPtr - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                int offset = TextCountSize + mid * InfoTableEntrySize;
                uint key = *(uint*)(i18nMmapPtr + offset);
                if (id == key)
                {
                    byte* textPtr = i18nMmapPtr + *(int*)(i18nMmapPtr + offset + KeySize);
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