﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpellforceDataEditor.category_forms
{
    public partial class Control16 : SpellforceDataEditor.category_forms.SFControl
    {
        public Control16()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            category.set_element_variant(current_element, 0, Utility.TryParseUInt8(textBox1.Text));
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Byte[] data_array = Utility.TryParseByteArray(textBox3.Text, 6);
            category.set_element_variant(current_element, 1, data_array[0]);
            category.set_element_variant(current_element, 2, data_array[1]);
            category.set_element_variant(current_element, 3, data_array[2]);
            category.set_element_variant(current_element, 4, data_array[3]);
            category.set_element_variant(current_element, 5, data_array[4]);
            category.set_element_variant(current_element, 6, data_array[5]);
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            category.set_element_variant(current_element, 7, Utility.TryParseUInt16(textBox9.Text));
        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            Byte[] data_array = Utility.TryParseByteArray(textBox15.Text, 7);
            category.set_element_variant(current_element, 8, data_array[0]);
            category.set_element_variant(current_element, 9, data_array[1]);
            category.set_element_variant(current_element, 10, data_array[2]);
            category.set_element_variant(current_element, 11, data_array[3]);
            category.set_element_variant(current_element, 12, data_array[4]);
            category.set_element_variant(current_element, 13, data_array[5]);
            category.set_element_variant(current_element, 14, data_array[6]);
        }

        private void textBox19_TextChanged(object sender, EventArgs e)
        {
            category.set_element_variant(current_element, 15, Utility.TryParseUInt8(textBox19.Text));
        }

        private void textBox18_TextChanged(object sender, EventArgs e)
        {
            category.set_element_variant(current_element, 16, Utility.TryParseUInt8(textBox18.Text));
        }

        private void textBox17_TextChanged(object sender, EventArgs e)
        {
            category.set_element_variant(current_element, 17, Utility.TryParseUInt8(textBox17.Text));
        }

        private void textBox26_TextChanged(object sender, EventArgs e)
        {
            Byte[] data_array = Utility.TryParseByteArray(textBox26.Text, 8);
            category.set_element_variant(current_element, 18, data_array[0]);
            category.set_element_variant(current_element, 19, data_array[1]);
            category.set_element_variant(current_element, 20, data_array[2]);
            category.set_element_variant(current_element, 21, data_array[3]);
            category.set_element_variant(current_element, 22, data_array[4]);
            category.set_element_variant(current_element, 23, data_array[5]);
            category.set_element_variant(current_element, 24, data_array[6]);
            category.set_element_variant(current_element, 25, data_array[7]);
        }
    }
}