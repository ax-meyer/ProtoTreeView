/*
---- Copyright Start ----

This file is part of the ProtoTreeView project.

Copyright (C) 2022 ax-meyer

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

---- Copyright End ----
*/

﻿
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace ProtoTreeView
{
    public class FieldNode : ProtoNode
    {
        public FieldNode(FieldDescriptor field, IMessage parentMessage)
        {
            Name = field.Name;
            FullName = field.FullName;
            if (field.FieldType == FieldType.Bytes)
            {
                ByteString bs = field.Accessor.GetValue(parentMessage) as ByteString;


                FieldValue = string.Join("; ", bs.ToByteArray());
            }
            else
            {
                FieldValue = field == null ? "" : field.Accessor.GetValue(parentMessage).ToString();
            }

            DataType = field == null ? "" : field.FieldType.ToString();
        }

        public FieldNode(string name, string value, string type, string fullname = "")
        {
            Name = name;
            FieldValue = value;
            DataType = type;
            FullName = fullname == "" ? name : fullname;
        }

        public string FieldValue { get; }

        public override string DataType { get; }

        public override string FullName { get; }

        public override string Name { get; }
    }
}
