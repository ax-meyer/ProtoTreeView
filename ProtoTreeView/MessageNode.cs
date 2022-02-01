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
using System.Collections;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace ProtoTreeView
{
    public class MessageNode : ProtoNode
    {
        public MessageNode(IMessage message, string name_suffix = "", string name_override = "")
        {
            this.message = message;
            LazyLoading = true;

            if (name_override != "")
            {
                Name = name_override;
            }
            else
            {
                Name = message == null ? "" : message.Descriptor.Name;
                Name += name_suffix;
            }
            FullName = message == null ? "" : message.Descriptor.FullName;
            DataType = message == null ? "null" : message.GetType().ToString();
            if (message == null)
            {
                IsExpanded = true;
            }
        }

        private readonly IMessage message;

        public override string Name { get; }

        public override string FullName { get; }

        public override string DataType { get; }

        protected override void LoadChildren()
        {
            if (message != null)
            {
                foreach (FieldDescriptor field in message.Descriptor.Fields.InFieldNumberOrder())
                {
                    if (field.FieldType == Google.Protobuf.Reflection.FieldType.Message)
                    {
                        if (field.IsMap) // A map is also a repeated field, so IsMap needs to be checked before IsRepeated!
                        {
                            IDictionary d = field.Accessor.GetValue(message) as IDictionary;
                            Children.Add(new MapNode(field.Name, d));
                        }
                        else if (field.IsRepeated)
                        {

                            IList d = field.Accessor.GetValue(message) as IList;

                            Children.Add(new RepeatedMessageNode(field.Name, d));
                        }
                        else
                        {
                            IMessage msg = (IMessage)field.Accessor.GetValue(message);
                            if (msg == null)
                            {
                                Children.Add(new MessageNode(msg, name_override: field.Name));
                            }
                            else
                            {
                                Children.Add(new MessageNode(msg));
                            }
                        }
                    }
                    else
                    {
                        if (field.IsRepeated)
                        {
                            Children.Add(new RepeatedFieldNode(field.Name, field.Accessor.GetValue(message) as IList));
                        }
                        else
                        {
                            Children.Add(new FieldNode(field, message));
                        }
                    }
                }
            }
        }
    }
}
