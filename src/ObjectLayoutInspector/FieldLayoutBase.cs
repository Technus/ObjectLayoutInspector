using System;
using System.Reflection;

namespace ObjectLayoutInspector
{
    /// <summary>
    /// Base type that represents different field layouts.
    /// </summary>
    public abstract class FieldLayoutBase
    {
        /// <summary>
        /// Size of a field.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// An offset of a field from the beginning of a struct.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Type which declared this field
        /// </summary>
        public abstract Type DeclaringType { get; }

        /// <nodoc />
        protected FieldLayoutBase(int offset, int size)
        {
            Offset = offset;
            Size = size;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            int fieldSize = Size;
            string byteOrBytes = fieldSize == 1 ? "byte" : "bytes";

            string offsetStr = $"{Offset}-{Offset - 1 + fieldSize}";
            if (fieldSize == 1)
            {
                offsetStr = Offset.ToString();
            }

            return $"{offsetStr,5}: {NameOrDescription} ({fieldSize} {byteOrBytes})";
        }

        /// <nodoc />
        protected abstract string NameOrDescription { get; }
    }

    /// <summary>
    /// Represents an actual layout of a field.
    /// </summary>
    public sealed class FieldLayout : FieldLayoutBase
    {
        /// <nodoc />
        public FieldLayout(int offset, FieldInfo fieldInfo, int size)
            : base(offset, size)
        {
            FieldInfo = fieldInfo;
        }

        /// <nodoc />
        public FieldInfo FieldInfo { get; }

        /// <nodoc />
        public override Type DeclaringType => FieldInfo.DeclaringType;

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is FieldLayout fieldLayout
            && Offset == fieldLayout.Offset
            && Size == fieldLayout.Size
            && FieldInfo == fieldLayout.FieldInfo;

        /// <inheritdoc />
        public override int GetHashCode() => (Offset, Size, FieldInfo).GetHashCode();

        /// <inheritdoc />
        protected override string NameOrDescription =>
            $"{FieldInfo.FieldType.Name} {FieldInfo.Name} for {DeclaringType.Name}";
    }

    /// <summary>
    /// Represents a padding between fields.
    /// </summary>
    public sealed class Padding : FieldLayoutBase
    {
        /// <nodoc />
        public Padding(int offset, int size, Type declaringType) : base(offset, size)
        {
            DeclaringType = declaringType;
        }

        /// <nodoc />
        public override Type DeclaringType { get; }

        /// <inheritdoc />
        protected override string NameOrDescription => 
            $"padding for {DeclaringType.Name}";

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is Padding padding
            && Offset == padding.Offset
            && Size == padding.Size
            && DeclaringType == padding.DeclaringType;

        /// <inheritdoc />
        public override int GetHashCode() => (Offset, Size, DeclaringType).GetHashCode();
    }
}