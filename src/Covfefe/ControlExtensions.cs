﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Covfefe
{
    public static class ControlExtensions
    {
        public static void LoadFromEnum<TEnumType>(this ComboBox comboBox)
        {
            comboBox.Items.Clear();
            var enumType = typeof(TEnumType);
            
            // examine the enum, create a key-value pair of the enum description and value.  If there is no description attribute, just use the value for the key and the value.
            comboBox.DataSource = Enum.GetValues(enumType).Cast<TEnumType>().Select(value =>
                {
                    var descriptionAttribute = Attribute.GetCustomAttribute(enumType.GetField(value.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute;
                    return new KeyValuePair<TEnumType, string>(value, descriptionAttribute?.Description ?? value.ToString());
                }).ToArray();

            comboBox.DisplayMember = "Value";
            comboBox.ValueMember = "Key";
        }

        public static void UncheckAllItems(this ContextMenuStrip contextMenu)
        {
            foreach (object item in contextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                    menuItem.Checked = false;
            }
        }

    }
}
