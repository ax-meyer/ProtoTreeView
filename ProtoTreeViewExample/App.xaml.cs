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

﻿using ProtoTreeView;
using SampleMessage;
using System.Windows;

namespace ProtoTreeViewExample
{
 // All rights reserverd.
 // (c) 2021, Alexander Meyer for Fraunhofer ILT
    
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs args)
        {
            AddressBook addressBook = new AddressBook();
            for (int i = 0; i < 10; i++)
            {
                Person person = new Person
                {
                    Email = "test" + i.ToString() + "@test.com",
                    Id = i,
                    Name = "Person " + (i * 10).ToString()
                };
                for (int j = 0; j < 3; j++)
                {
                    Person.Types.PhoneNumber phone = new Person.Types.PhoneNumber
                    {
                        Number = (5 * i + 3 * j).ToString(),
                        Type = (Person.Types.PhoneType)j
                    };
                    person.Phones.Add(phone);
                }
                addressBook.People.Add(person);
            }
            ProtoTreeViewWindow treeViewWindow = new ProtoTreeViewWindow(addressBook);
            treeViewWindow.Show();
        }
    }
}
