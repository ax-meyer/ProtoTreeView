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

namespace ProtoTreeView
{
    public class RepeatedMessageNode : ProtoNode
    {
        public RepeatedMessageNode(string name, IList messages)
        {
            this.messages = messages;
            LazyLoading = true;
            Name = name;
            FullName = name;
            DataType = messages == null ? "Repeated<null>" : "Repeated<" + messages.GetType().ToString() + ">";
            if (messages.Count == 0)
            {
                IsExpanded = true;
            }
        }

        private readonly IList messages;

        public override string Name { get; }

        public override string FullName { get; }

        public override string DataType { get; }

        protected override void LoadChildren()
        {
            if (messages != null)
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    Children.Add(new MessageNode((IMessage)messages[i], " [" + i + "]"));
                }
            }
        }
    }
}
