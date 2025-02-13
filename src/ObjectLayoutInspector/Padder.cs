using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectLayoutInspector
{
    internal static class Padder
    {
        public static void AddPaddings(bool includePaddings, int size, FieldLayout[] fieldsOffsets, List<FieldLayoutBase> layouts)
        {
            if (includePaddings)
            {
                var dict = new Dictionary<Type, (int start, int end, List<FieldLayout> fields)>();

                foreach (var fieldOffset in fieldsOffsets)
                {
                    if (dict.TryGetValue(fieldOffset.DeclaringType, out var range))
                    {
                        range.fields.Add(fieldOffset);
                        range = (Math.Min(range.start, fieldOffset.Offset), Math.Max(range.end, fieldOffset.Offset + fieldOffset.Size), range.fields);
                    }
                    else
                    {
                        range = (fieldOffset.Offset, fieldOffset.Offset + fieldOffset.Size, new List<FieldLayout>() { fieldOffset });
                    }

                    dict[fieldOffset.DeclaringType] = range;
                }

                foreach (var item in dict)
                {
                    if (item.Value.start != 0)
                    {
                        layouts.Add(new Padding(0, item.Value.start, item.Key));
                    }

                    var field = item.Value.fields[0];
                    layouts.Add(field);

                    for (int index = 1; index < item.Value.fields.Count; index++)
                    {
                        var fieldNext = item.Value.fields[index];
                        if(field.Offset+field.Size != fieldNext.Offset)
                        {
                            layouts.Add(new Padding(field.Offset + field.Size, fieldNext.Offset - (field.Offset + field.Size), item.Key));
                        }
                        layouts.Add(fieldNext);
                        field = fieldNext;
                    }

                    if (item.Value.end != size)
                    {
                        layouts.Add(new Padding(item.Value.end, size - item.Value.end, item.Key));
                    }
                }
            }
            else
            {
                layouts.AddRange(fieldsOffsets);
            }
        }
    }
}