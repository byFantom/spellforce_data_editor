﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpellforceDataEditor
{
    public partial class SpelllforceCFFEditor : Form
    {
        private SFCategoryManager manager;
        private category_forms.SFControl ElementDisplay;
        private int elementselect_next_index = 0;
        private int elementselect_last_index = 0;
        private int elementselect_refresh_size = 100;
        private int elementselect_refresh_rate = 50;


        protected List<int> current_indices;

        public SpelllforceCFFEditor()
        {
            InitializeComponent();
            manager = new SFCategoryManager();
            current_indices = new List<int>();
        }

        private void loadGameDatacffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(OpenGameData.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (CategorySelect.Enabled)
                    close_data();
                manager.load_cff(OpenGameData.FileName);
                CategorySelect.Enabled = true;
                for (int i = 0; i < manager.get_category_number(); i++)
                    CategorySelect.Items.Add(manager.get_category(i).get_name());
                GC.Collect();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CategorySelect.Enabled)
                return;
            if (SaveGameData.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                manager.save_cff(SaveGameData.FileName);
            }
        }

        private void CategorySelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            ElementSelect.Enabled = true;
            SFCategory ctg = manager.get_category(CategorySelect.SelectedIndex);

            ElementSelect_refresh(ctg);

            ElementDisplay = Assembly.GetExecutingAssembly().CreateInstance(
                "SpellforceDataEditor.category_forms.Control" + (CategorySelect.SelectedIndex+1).ToString())
                as category_forms.SFControl;
            ElementDisplay.set_category(ctg);
            SearchPanel.Controls.Clear();
            SearchPanel.Controls.Add(ElementDisplay);
            SearchColumnID.Items.Clear();
            SearchColumnID.SelectedIndex = -1;
            SearchColumnID.Text = "";
            Dictionary<string, int[]>.KeyCollection keys = ElementDisplay.get_column_descriptions();
            foreach (string s in keys)
                SearchColumnID.Items.Add(s);
            ElementDisplay.Visible = false;
            panelSearch.Visible = true;
            panelElemManipulate.Visible = true;
        }

        private void ElementSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ElementSelect.SelectedIndex == -1)
            {
                ElementDisplay.Visible = false;
                return;
            }
            ElementDisplay.Visible = true;
            ElementDisplay.set_element(current_indices[ElementSelect.SelectedIndex]);
            ElementDisplay.show_element();
        }

        public void ElementSelect_refresh(SFCategory ctg)
        {
            ElementSelect.Items.Clear();
            current_indices.Clear();
            ElementSelect_RefreshTimer.Enabled = true;
            elementselect_next_index = 0;
            elementselect_last_index = ctg.get_element_count();
            ElementSelect_RefreshTimer.Interval = 10;
            ElementSelect_RefreshTimer.Start();
        }

        public void ElementSelect_add_elements(SFCategory ctg, List<int> indices)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                int index = indices[i];
                if (!current_indices.Contains(index))
                {
                    ElementSelect.Items.Add(ctg.get_element_string(manager, indices[i]));
                    current_indices.Add(indices[i]);
                }
            }
        }

        private void findElementByValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            return;
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (!ElementSelect.Enabled)
                return;
            if (SearchQuery.Text == "")
                return;
            if (ElementSelect_RefreshTimer.Enabled)
            {
                ElementSelect_RefreshTimer.Stop();
                ElementSelect_RefreshTimer.Enabled = false;
            }
            int elem_found = 0;
            SFCategory ctg = manager.get_category(CategorySelect.SelectedIndex);
            int columns = ctg.get_element_format().Length;

            //determine columns to search
            List<int> columns_searched = new List<int>();

            if ((checkSearchByColumn.Checked) && (SearchColumnID.Text != ""))
            {
                int[] query_columns = ElementDisplay.get_column_index(SearchColumnID.Text);
                foreach(int i in query_columns)
                    columns_searched.Add(i);
            }
            else
            {
                for (int i = 0; i < columns; i++)
                    columns_searched.Add(i);
            }

            //if searched, looks for string in element description
            List<int> query_result = new List<int>();
            if (checkSearchDescription.Checked)
            {
                string val = SearchQuery.Text;
                for(int i = 0; i < ElementSelect.Items.Count; i++)
                {
                    string elem_str = (string)ElementSelect.Items[i];
                    if (elem_str.Contains(val))
                    {
                        elem_found += 1;
                        query_result.Add(current_indices[i]);
                    }
                }
            }
            ElementSelect.Items.Clear();
            current_indices.Clear();
            ElementSelect_add_elements(ctg, query_result);
            //search columns for value and append results to the list of results
            if (radioSearchNumeric.Checked)
            {
                int val = Utility.TryParseInt32(SearchQuery.Text);
                foreach(int col in columns_searched)
                {
                    query_result = manager.query_by_column_numeric(CategorySelect.SelectedIndex, col, val);
                    elem_found += query_result.Count;
                    ElementSelect_add_elements(ctg, query_result);
                }
            }
            else
            {
                string val = SearchQuery.Text;
                foreach (int col in columns_searched)
                {
                    if (ctg.get_element_format()[col] != 's')
                        continue;
                    query_result = manager.query_by_column_text(CategorySelect.SelectedIndex, col, val);
                    elem_found += query_result.Count;
                    ElementSelect_add_elements(ctg, query_result);
                }
            }
            ElementDisplay.Visible = false;
            panelElemManipulate.Visible = false;
        }

        private void ButtonElemInsert_Click(object sender, EventArgs e)
        {
            int current_elem = current_indices[ElementSelect.SelectedIndex];
            SFCategory ctg = manager.get_category(CategorySelect.SelectedIndex);
            SFCategoryElement elem = new SFCategoryElement();
            string format_elem = ctg.get_element_format();
            foreach(char c in format_elem)
            {
                elem.add_single_variant(ctg.empty_variant(c));
            }
            List<SFCategoryElement> elems = ctg.get_elements();
            elems.Insert(current_elem+1, elem);
            ElementSelect.Items.Insert(current_elem+1, ctg.get_element_string(manager, current_elem+1));
            current_indices.Insert(current_elem+1, current_elem+1);
        }

        private void ButtonElemRemove_Click(object sender, EventArgs e)
        {
            int current_elem = current_indices[ElementSelect.SelectedIndex];
            SFCategory ctg = manager.get_category(CategorySelect.SelectedIndex);
            List<SFCategoryElement> elems = ctg.get_elements();
            current_indices.RemoveAt(current_elem);
            ElementSelect.Items.RemoveAt(ElementSelect.SelectedIndex);
            elems.RemoveAt(current_elem);
        }

        private void checkSearchByColumn_CheckedChanged(object sender, EventArgs e)
        {
            SearchColumnID.Enabled = checkSearchByColumn.Checked;
        }

        private void ElementSelect_RefreshTimer_Tick(object sender, EventArgs e)
        {
            SFCategory ctg = manager.get_category(CategorySelect.SelectedIndex);
            //code for adding stuff
            int last = Math.Min(elementselect_next_index + elementselect_refresh_size, elementselect_last_index);
            for (int i = elementselect_next_index; i < last; i++)
            {
                ElementSelect.Items.Add(ctg.get_element_string(manager, i));
                current_indices.Add(i);
            }
            elementselect_next_index += elementselect_refresh_size;
            if(last != elementselect_last_index)
            {
                ElementSelect_RefreshTimer.Interval = elementselect_refresh_rate;
                ElementSelect_RefreshTimer.Start();
            }
            else
            {
                ElementSelect_RefreshTimer.Enabled = false;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CategorySelect.Enabled)
            {
                close_data();
            }
        }

        public void close_data()
        {
            if (ElementSelect_RefreshTimer.Enabled)
            {
                ElementSelect_RefreshTimer.Stop();
                ElementSelect_RefreshTimer.Enabled = false;
            }
            if(ElementDisplay != null)
                ElementDisplay.Visible = false;
            ElementSelect.Items.Clear();
            ElementSelect.Enabled = false;
            CategorySelect.Items.Clear();
            CategorySelect.Enabled = false;
            panelElemManipulate.Visible = false;
            panelSearch.Visible = false;
            manager.unload_all();
            GC.Collect();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CategorySelect.Enabled)
            {
                close_data();
            }
            Application.Exit();
        }
    }
}
