using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ObjectLayoutInspector.Helpers;

namespace ObjectLayoutInspector
{
    internal enum NodeKind : byte
    {
        Primitive,
        Complex,
        Nullable,
        Root,
        Fixed,
        Reference
    }

    // could make usual object or boxed nodes, but eating own dog food for fun and performance(fun)
    [StructLayout(LayoutKind.Explicit)]
    internal struct FieldNode
    {
        public static (FieldNode[] fields, Type[] types) GetFieldNodes(Type type)
        {
            var (fields, types) = ReflectionHelper.GetInstanceFields(type);
            return (fields.Select(x =>
            {
                if (x.FieldType.IsClass)
                {
                    return new FieldNode { kind = NodeKind.Reference, referenceNode = new ReferenceNode(x) };
                }

                if (Detectors.IsPrimitive(x))
                {
                    return new FieldNode { kind = NodeKind.Primitive, primitiveNode = new PrimitiveNode { info = x } };
                }

                if (Detectors.IsNullable(x))
                {
                    return new FieldNode { kind = NodeKind.Nullable, nullableNode = new NullableNode { info = x } };
                }

                if (Detectors.IsFixed(x, out int length))
                {
                    return new FieldNode { kind = NodeKind.Fixed, fixedNode = new FixedNode { info = x, length = length } };
                }

                return new FieldNode { kind = NodeKind.Complex, complexNode = new ComplexNode { info = x } };
            }).ToArray(), types);
        }

        [FieldOffset(0)]
        public NodeKind kind;

        [FieldOffset(8)]
        public int size;

        [FieldOffset(12)]
        public int totalOffset;

        [FieldOffset(24)]
        public FieldInfo info;

        [FieldOffset(8)]
        public PrimitiveNode primitiveNode;

        [FieldOffset(8)]
        public ComplexNode complexNode;

        [FieldOffset(8)]
        public RootNode rootNode;

        [FieldOffset(8)]
        public NullableNode nullableNode;

        [FieldOffset(8)]
        public FixedNode fixedNode;

        [FieldOffset(8)]
        public ReferenceNode referenceNode;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ReferenceNode
    {
        public ReferenceNode(FieldInfo info)
        {
            size = IntPtr.Size;
            this.info = info;
            totalOffset = -1;
        }

        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct NullableNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        // Nullable can be only of Complex or Primitive, so may model FieldNode as remain part only of 2
        // validate of same size and field layout it test and Unsafe.As map to reinterpret cast
        /// <summary>
        /// Boolean.
        /// </summary>
        [FieldOffset(24)]
        public Ref<FieldNode> hasValue;

        [FieldOffset(32)]
        public Ref<FieldNode> value;

        public Type Type => info.FieldType;
    }

    internal class Ref<T> where T : struct
    {
        public T value;

        public Ref(T value)
        {
            this.value = value;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct FixedNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(8)]
        public int length;

        [FieldOffset(16)]
        public FieldInfo info;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct PrimitiveNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ComplexNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        [FieldOffset(24)]
        public FieldNode[] children;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct RootNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        [FieldOffset(24)]
        public FieldNode[] children;

        public bool IsPrimitive => children is null;
    }
}