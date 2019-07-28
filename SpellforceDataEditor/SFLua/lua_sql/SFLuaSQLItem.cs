﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SpellforceDataEditor.SFLua.lua_sql
{
    public class SFLuaSQLItemData: ILuaParsable
    {
        public string MeshMaleCold;
        public string MeshFemaleCold;
        public string MeshMaleWarm;
        public string MeshFemaleWarm;
        public double ShadowRNG;
        public double SelectionSize;
        public string AnimSet;
        public int Race;
        public int Category;
        public int SubCategory;

        public void ParseLoad(LuaParser.LuaTable table)
        {
            MeshMaleCold = (string)table["meshmalecold"];
            MeshFemaleCold = (string)table["meshfemalecold"];
            MeshMaleWarm = (string)table["meshmalewarm"];
            MeshFemaleWarm = (string)table["meshfemalewarm"];
            ShadowRNG = (double)table["shadowrng"];
            SelectionSize = (double)table["selectionsize"];
            AnimSet = (string)table["animset"];
            Race = (int)(double)table["race"];
            Category = (int)(double)table["cat"];
            SubCategory = (int)(double)table["subcat"];
        }

        public string ParseToString()
        {
            string ret = "";
            ret += "meshmalecold = \"" + MeshMaleCold.ToString() + "\",";
            ret += "\r\nmeshfemalecold = \"" + MeshFemaleCold.ToString() + "\",";
            ret += "\r\nmeshmalewarm = \"" + MeshMaleWarm.ToString() + "\",";
            ret += "\r\nmeshfemalewarm = \"" + MeshFemaleWarm.ToString() + "\",";
            ret += "\r\nshadowrng = " + ShadowRNG.ToString(Utility.ci) + ",";
            ret += "\r\nselectionsize = " + SelectionSize.ToString(Utility.ci) + ",";
            ret += "\r\nanimset = \"" + AnimSet.ToString() + "\",";
            ret += "\r\nrace = " + Race.ToString() + ",";
            ret += "\r\ncat = " + Category.ToString() + ",";
            ret += "\r\nsubcat = " + SubCategory.ToString() + ",";
            return ret;
        }
    }

    public class SFLuaSQLItem: ILuaSQL
    {
        public Dictionary<int, SFLuaSQLItemData> items { get; private set; } = null;

        public SFLuaSQLItemData this[int index]
        {
            get
            {
                if (items.ContainsKey(index))
                    return items[index];
                return null;
            }
            set
            {
                if (items.ContainsKey(index))
                    items[index] = value;
                else
                    items.Add(index, value);
            }
        }

        public int Load()
        {
            LogUtils.Log.Info(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Load() called");

            // check if file exists
            string filename = SFUnPak.SFUnPak.game_directory_name;
            if (filename == "")
            {
                LogUtils.Log.Error(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Load(): Game directory not found!");
                return -1;
            }

            LogUtils.Log.Info(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Load(): Executing script script\\sql_item.lua");

            object[] ret = SFLuaEnvironment.ExecuteGameScript("script\\sql_item.lua");
            if (ret == null)
            {
                LogUtils.Log.Error(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Load(): Could not execute item script!");
                return -6;
            }

            if (items == null)
                items = new Dictionary<int, SFLuaSQLItemData>();

            int log_current_item = 0;
            try
            {
                items.Clear();

                LuaParser.LuaTable table = (LuaParser.LuaTable)ret[0];
                List<double> indices = new List<double>();
                foreach (var key in table.entries.Keys)
                    indices.Add((double)key);

                indices.Sort();
                // iterate over the rts coop spawn table
                foreach (double i in indices)
                {
                    int _i = (int)i;
                    log_current_item = _i;
                    LuaParser.LuaTable i_table = (LuaParser.LuaTable)table[i];
                    SFLuaSQLItemData data = new SFLuaSQLItemData();
                    data.ParseLoad(i_table);
                    items.Add(_i, data);
                }
            }
            catch (Exception e)
            {
                Unload();
                LogUtils.Log.Error(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Load(): Error reading item file! Item ID = " + log_current_item.ToString());
                return -3;
            }

            return 0;
        }

        public int Save()
        {
            LogUtils.Log.Info(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Save(): called");

            if (items == null)
            {
                LogUtils.Log.Error(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Save(): Items are not loaded in!");
                return -4;
            }

            // check if file exists
            string filename = SFUnPak.SFUnPak.game_directory_name;
            if (filename == "")
            {
                LogUtils.Log.Error(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Save(): Game directory not found!");
                return -1;
            }
            filename += "\\script\\sql_item.lua";

            try
            {
                File.WriteAllText(filename, "return\r\n" + SFLuaEnvironment.ParseDictToString(items));
            }
            catch (Exception)
            {
                LogUtils.Log.Error(LogUtils.LogSource.SFLua, "SFLuaSQLItem.Save(): Error writing item data to file (filename = " + filename + ")");
                return -3;
            }

            return 0;
        }

        public void Unload()
        {
            if (items != null)
            {
                items.Clear();
                items = null;
            }
        }
    }
}
