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

﻿using System;
using ICSharpCode.TreeView;

namespace ProtoTreeView
{
    public abstract class ProtoNode : SharpTreeNode
    {
        public abstract string FullName { get; }

        public abstract string DataType { get; }

        public abstract string Name { get; }

        public override object Text => Name;

        public virtual long? FileSize => null;

        public virtual DateTime? FileModified => null;

        public override string ToString()
        {
            return FullName;
        }
    }
}
