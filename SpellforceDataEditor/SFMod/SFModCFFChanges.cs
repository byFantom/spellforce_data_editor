﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;

namespace SpellforceDataEditor.SFMod
{
    public enum SFModCFFChangeType
    {
        REMOVE = 0, INSERT, REPLACE
    }

    public struct SFModCFFChangeElement
    {
        public SFModCFFChangeType type;
        public byte category_index;
        public ushort element_index;
        public SFCategoryElement element;

        static public SFModCFFChangeElement Load(BinaryReader br)
        {
            SFModCFFChangeElement celem = new SFModCFFChangeElement();
            celem.type = (SFModCFFChangeType)br.ReadInt16();
            celem.category_index = br.ReadByte();
            celem.element_index = br.ReadUInt16();
            celem.element = null;
            if (celem.type == SFModCFFChangeType.REMOVE)
                return celem;
            celem.element = new SFCategoryElement();
            try
            {
                celem.element.set(SFCategoryManager.get_category(celem.category_index).get_element(br));
            }
            catch(Exception)
            {
                throw new InvalidDataException("SFModCFFChangeElement.Load: Can't load element!");
            }
            return celem;
        }

        public int Save(BinaryWriter bw)
        {
            bw.Write((Int16)type);
            bw.Write(category_index);
            bw.Write(element_index);
            if (type == SFModCFFChangeType.REMOVE)
                return 0;
            if (element == null)
                return -1;
            SFCategoryManager.get_category(category_index).put_element(bw, element.get());
            return 0;
        }
    }

    public class SFModCFFChanges
    {
        public List<SFModCFFChangeElement> changes { get; private set; } = new List<SFModCFFChangeElement>();

        public int Load(BinaryReader br)
        {
            changes.Clear();
            br.ReadInt64();
            int change_count = br.ReadInt32();
            for(int i = 0; i < change_count; i++)
            {
                try
                {
                    changes.Add(SFModCFFChangeElement.Load(br));
                }
                catch(InvalidDataException)
                {
                    return -1;
                }
            }
            return 0;
        }

        public void Unload()
        {
            changes.Clear();
        }

        public int Save(BinaryWriter bw)
        {
            long init_pos = bw.BaseStream.Position;
            bw.Write((long)0);

            bw.Write(changes.Count);
            for(int i = 0; i < changes.Count; i++)
            {
                if (changes[i].Save(bw) != 0)
                    return -1;
            }

            long new_pos = bw.BaseStream.Position;
            bw.BaseStream.Position = init_pos;
            bw.Write(new_pos - init_pos);
            bw.BaseStream.Position = new_pos;
            return 0;
        }

        public void SortChanges()
        {

        }

        // called from ModCreator upon selecting a file
        public int GenerateDiff(string orig_fname, string new_fname)
        {
            changes.Clear();

            FileStream fs_orig = File.Open(orig_fname, FileMode.Open, FileAccess.Read);
            FileStream fs_new  = File.Open(new_fname,  FileMode.Open, FileAccess.Read);
            BinaryReader br_orig = new BinaryReader(fs_orig);
            BinaryReader br_new  = new BinaryReader(fs_new) ;
            SFCategory orig_data = null;
            SFCategory new_data  = null;
            byte[] mainHeader_orig = br_orig.ReadBytes(20);
            byte[] mainHeader_new  = br_new.ReadBytes(20) ;
            for(int i = 1; i <= SFCategoryManager.get_category_number(); i++)
            {
                orig_data = Assembly.GetExecutingAssembly().CreateInstance("SpellforceDataEditor.SFCategory" + i.ToString()) as SFCategory;
                new_data  = Assembly.GetExecutingAssembly().CreateInstance("SpellforceDataEditor.SFCategory" + i.ToString()) as SFCategory;
                if(orig_data.read(br_orig) != 0)
                {
                    changes.Clear();
                    br_new.Close();
                    br_orig.Close();
                    return -1;
                    // error
                }
                if(new_data.read(br_new)   != 0)
                {
                    changes.Clear();
                    br_new.Close();
                    br_orig.Close();
                    return -1;
                    // error
                }
                int orig_i = 0;
                int new_i = 0;
                int orig_id, new_id;
                bool orig_end = false;
                bool new_end = false;

                while (true)
                {
                    bool is_change = false;
                    SFModCFFChangeType change_type = SFModCFFChangeType.REPLACE;

                    if (new_i == new_data.get_element_count())
                        new_end = true;
                    if (orig_i == orig_data.get_element_count())
                        orig_end = true;

                    if (orig_end && new_end)
                        break;

                    orig_id = orig_data.get_element_id(orig_i);
                    new_id = new_data.get_element_id(new_i);

                    if (orig_end)
                    {
                        is_change = true;
                        change_type = SFModCFFChangeType.INSERT;
                    }
                    else if (new_end)
                    {
                        is_change = true;
                        change_type = SFModCFFChangeType.REMOVE;
                    }
                    else
                    {
                        if (orig_id == new_id)
                        {
                            if (orig_data.get_element(orig_i).same_as(new_data.get_element(new_i)))
                            {
                                // no change, addition, or deletion
                            }
                            else
                            {
                                is_change = true;
                                change_type = SFModCFFChangeType.REPLACE;
                                // change!
                                // this is ok
                            }
                        }
                        else if (orig_id > new_id)
                        {
                            is_change = true;
                            change_type = SFModCFFChangeType.INSERT;
                            // addition!

                            orig_i -= 1;
                        }
                        else if (orig_id < new_id)
                        {
                            is_change = true;
                            change_type = SFModCFFChangeType.REMOVE;
                            // deletion!

                            new_i -= 1;
                        }
                    }

                    if(is_change)
                    {
                        SFModCFFChangeElement change_elem = new SFModCFFChangeElement();
                        change_elem.type = change_type;
                        change_elem.category_index = (Byte)(i - 1);
                        if (change_type != SFModCFFChangeType.REMOVE)
                        {
                            change_elem.element = new_data.get_element(new_i).get_copy();
                            change_elem.element_index = (UInt16)(new_id);
                        }
                        else
                            change_elem.element_index = (UInt16)(orig_id);
                        changes.Add(change_elem);
                        //System.Diagnostics.Debug.WriteLine("NEW CHANGE! TYPE = "+change_type.ToString()
                        //                                 + ", ORIG_ID = " + orig_id.ToString() + ", NEW_ID = " + new_id.ToString()
                        //                                 + ", ORIG_I = " + orig_i.ToString() + ", NEW_I = " + new_i.ToString());
                    }

                    if (orig_i < orig_data.get_element_count())
                        orig_i += 1;
                    if (new_i < new_data.get_element_count())
                        new_i += 1;
                }
                orig_data.unload();
                new_data.unload();
                GC.Collect();
                //System.Diagnostics.Debug.WriteLine("CATEGORY "+i.ToString()+" READ SUCCESSFULLY");
            }
            br_orig.Close();
            br_new.Close();
            return 0;
        }

        public int Apply(SFGameData gd)
        {
            for(int i = 0; i < changes.Count; i++)
            {
                SFModCFFChangeElement change = changes[i];
                SFCategory cat = gd.categories[change.category_index];
                List<SFCategoryElement> elems = cat.get_elements();
                int index;
                switch (change.type)
                {
                    case SFModCFFChangeType.REPLACE:
                        index = cat.get_element_index(change.element_index);
                        if (index == -1)
                            elems.Insert(index, change.element);
                        else
                            elems[index] = change.element;
                        break;
                    case SFModCFFChangeType.REMOVE:
                        index = cat.get_element_index(change.element_index);
                        if (index == -1)
                            break;
                        elems.RemoveAt(index);
                        break;
                    case SFModCFFChangeType.INSERT:
                        // find suitable position for an element
                        index = cat.get_new_element_index(change.element_index);
                        if (index == -1)
                        {
                            index = cat.get_element_index(change.element_index);
                            elems[index] = change.element;
                        }
                        else
                            elems.Insert(index, change.element);
                        break;
                    default:
                        continue;
                }
            }
            return 0;
        }

        public override string ToString()
        {
            string ret = "";
            int[] stat = new int[3];
            foreach(SFModCFFChangeElement e in changes)
                stat[(int)e.type]++;
            ret += "Deletions: " + stat[0].ToString() + ", additions: " + stat[1].ToString() + ", modifications: " + stat[2].ToString();
            return ret;
        }
    }
}