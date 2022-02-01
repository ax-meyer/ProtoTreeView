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
    public class MapNode : ProtoNode
    {
        public MapNode(string name, IDictionary map)
        {
            this.map = map;
            LazyLoading = true;
            Name = name;
            FullName = name;
            DataType = map == null ? "Map<null>" : "Repeated<" + this.map.GetType().ToString() + ">";
            if (map.Count == 0)
            {
                IsExpanded = true;
            }
        }

        private readonly IDictionary map;

        public override string Name { get; }

        public override string FullName { get; }

        public override string DataType { get; }

        protected override void LoadChildren()
        {
            if (map != null)
            {
                foreach (object key in map.Keys)
                {
                    try
                    {
                        Children.Add(new MessageNode((IMessage)map[key], name_override: key.ToString()));
                    }
                    catch
                    {
                        Children.Add(new FieldNode(key.ToString(), map[key].ToString(), map[key].GetType().ToString()));
                    }
                }
            }
        }
    }
}
