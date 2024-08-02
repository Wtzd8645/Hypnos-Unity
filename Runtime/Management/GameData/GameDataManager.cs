using Blanketmen.Hypnos.Hash;
using System;
using System.IO.MemoryMappedFiles;

namespace Blanketmen.Hypnos
{
    public sealed unsafe class GameDataManager
    {
        #region Singleton
        public static GameDataManager Instance { get; } = new GameDataManager();

        private GameDataManager() { }

        ~GameDataManager()
        {
            Dispose();
        }
        #endregion

        private const int InvalidOffset = -1;

        private const int StrCountSize = 4;
        private const int TableCapacitySize = 4;
        private const int InfoTableEntrySize = 24;
        private const int KeyTableEntrySize = 12;

        private static readonly System.Text.Encoding StringEncoder = System.Text.Encoding.UTF8; // For efficiency.

        private MemoryMappedFile dataMmapFile;
        private MemoryMappedViewAccessor dataMmapAccessor;
        private byte* dataMmapPtr;

        public void Dispose()
        {
            if (dataMmapFile != null)
            {
                dataMmapAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                dataMmapFile.Dispose();
                dataMmapFile = null;
            }
            GC.SuppressFinalize(this);
        }

        public void CreateMmap(string dataFilePath)
        {
            if (dataMmapFile != null)
            {
                dataMmapAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                dataMmapFile.Dispose();
            }

            dataMmapFile = MemoryMappedFile.CreateFromFile(dataFilePath);
            dataMmapAccessor = dataMmapFile.CreateViewAccessor();
            dataMmapAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref dataMmapPtr);
        }

        public int GetKeyTableOffset(string tableName)
        {
            const int NameOffsetField = 4;
            const int TableOffsetField = 8;
            const int NextOffsetField = 20;

            uint hash = HashUtil.BkdrHash(StringEncoder.GetBytes(tableName));
            int capacity = *(int*)dataMmapPtr;
            int slot = (int)(hash % (uint)capacity);
            int offset = TableCapacitySize + slot * InfoTableEntrySize;
            while (offset > InvalidOffset)
            {
                if (hash == *(uint*)(dataMmapPtr + offset))
                {
                    int strOffset = *(int*)(dataMmapPtr + offset + NameOffsetField);
                    if (tableName == GetString(strOffset))
                    {
                        return *(int*)(dataMmapPtr + offset + TableOffsetField);
                    }
                }
                offset = *(int*)(dataMmapPtr + offset + NextOffsetField);
            }
            return InvalidOffset;
        }

        public byte* GetDataPointer(int keyTableOffset, uint key)
        {
            const int DataOffsetField = 4;
            const int NextOffsetField = 8;

            int capacity = *(int*)(dataMmapPtr + keyTableOffset);
            int slot = (int)(key % (uint)capacity);
            int offset = keyTableOffset + TableCapacitySize + slot * KeyTableEntrySize;
            while (offset > InvalidOffset)
            {
                if (key == *(uint*)(dataMmapPtr + offset))
                {
                    return dataMmapPtr + *(int*)(dataMmapPtr + offset + DataOffsetField);
                }
                offset = *(int*)(dataMmapPtr + offset + NextOffsetField);
            }
            return null;
        }

        public byte* GetDataPointer(int offset)
        {
            return offset > InvalidOffset ? dataMmapPtr + offset : null;
        }

        public string GetString(int offset)
        {
            if (offset <= InvalidOffset)
            {
                return "";
            }

            byte* strPtr = dataMmapPtr + offset;
            return StringEncoder.GetString(strPtr + StrCountSize, *(int*)strPtr);
        }
    }

    public unsafe interface IGameData
    {
        void SetPointer(byte* ptr);
    }

    public readonly unsafe struct NArray<T> where T : unmanaged
    {
        public readonly int length;
        private readonly T* pointer;

        public T this[int index] => pointer == null || index < 0 || index >= length
                                    ? throw new AccessViolationException()
                                    : *(pointer + index);

        public NArray(T* ptr, int len)
        {
            pointer = ptr;
            length = len;
        }
    }

    public readonly unsafe struct NStructArray<T> where T : struct, IGameData
    {
        public readonly int length;
        private readonly byte* pointer;
        private readonly int size;

        public T this[int index]
        {
            get
            {
                if (pointer == null || index < 0 || index >= length)
                {
                    throw new AccessViolationException();
                }

                T data = new T();
                data.SetPointer(pointer + index * size);
                return data;
            }
        }

        public NStructArray(byte* ptr, int size, int len)
        {
            pointer = ptr;
            this.size = size;
            length = len;
        }
    }

    public readonly unsafe struct NStringArray
    {
        public readonly int length;
        private readonly int* pointer;

        public string this[int index] => pointer == null || index < 0 || index >= length
                                         ? throw new AccessViolationException()
                                         : GameDataManager.Instance.GetString(*(pointer + index));

        public NStringArray(int* ptr, int len)
        {
            pointer = ptr;
            length = len;
        }
    }

    public readonly unsafe struct NReferenceArray<T> where T : struct, IGameData
    {
        public readonly int length;
        private readonly int* pointer;

        public T this[int index]
        {
            get
            {
                if (pointer == null || index < 0 || index >= length)
                {
                    throw new AccessViolationException();
                }

                T data = new T();
                data.SetPointer(GameDataManager.Instance.GetDataPointer(*(pointer + index)));
                return data;
            }
        }

        public NReferenceArray(int* ptr, int len)
        {
            pointer = ptr;
            length = len;
        }
    }
}