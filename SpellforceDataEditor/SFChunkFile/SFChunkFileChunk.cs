﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace SpellforceDataEditor.SFChunkFile
{
    public class SFChunkFileChunk
    {
        byte[] header;
        byte[] data;
        MemoryStream datastream = null;
        BinaryReader databr = null;

        public short get_id()
        {
            return BitConverter.ToInt16(header, 0);
        }

        public short get_occurence()
        {
            return BitConverter.ToInt16(header, 2);
        }

        public short get_is_packed()
        {
            return BitConverter.ToInt16(header, 4);
        }

        public int get_data_length()
        {
            return BitConverter.ToInt32(header, 6);
        }

        public short get_data_type()
        {
            return BitConverter.ToInt16(header, 10);
        }

        public int get_unpacked_data_length()
        {
            return BitConverter.ToInt32(header, 12);
        }

        public int get_original_data_length()
        {
            if (get_is_packed() == 0)
                return get_data_length();
            else
                return get_unpacked_data_length();
        }

        public byte[] get_raw_data()
        {
            return data;
        }

        public BinaryReader Open()
        {
            if (datastream != null)
            {
                databr.BaseStream.Position = 0;
                return databr;
            }
            datastream = new MemoryStream(data);
            databr = new BinaryReader(datastream);
            return databr;
        }

        public void Close()
        {
            if (datastream == null)
                return;
            databr.Close();
            datastream.Close();
            databr = null;
            datastream = null;
        }

        public int Read(BinaryReader br)
        {
            if (get_is_packed() == 0)
            {
                header = br.ReadBytes(12);
                try
                {
                    data = br.ReadBytes(get_data_length());
                }
                catch (EndOfStreamException)
                {
                    return -4;
                }
                return 0;
            }
            else
            {
                header = br.ReadBytes(16);
                br.ReadBytes(2);

                byte[] tmp_data;
                try
                {
                    tmp_data = br.ReadBytes(get_data_length() - 2);
                }
                catch (EndOfStreamException)
                {
                    return -4;
                }

                try
                {
                    using (MemoryStream ms_dest = new MemoryStream())
                    {
                        using (MemoryStream ms_src = new MemoryStream(tmp_data))
                        {
                            using (DeflateStream ds = new DeflateStream(ms_src, CompressionMode.Decompress))
                            {
                                ds.CopyTo(ms_dest);
                            }
                        }
                        data = ms_dest.ToArray();
                    }
                }
                catch (Exception)
                {
                    return -2;
                }

                if (data.Length != get_unpacked_data_length())
                    return -3;
                return 0;
            }
        }

        public int ReadHeader(BinaryReader br, int chunk_mode, bool proceed_stream = false)
        {
            try
            {
                switch (chunk_mode)
                {
                    case 2:
                        header = br.ReadBytes(12);
                        if (!proceed_stream)
                            br.BaseStream.Position -= 12;
                        return 0;
                    case 3:
                        header = br.ReadBytes(16);
                        if (!proceed_stream)
                            br.BaseStream.Position -= 16;
                        return 0;
                    default:
                        return -1;
                }
            }
            catch(EndOfStreamException)
            {
                return -2;
            }
        }

        public void SetHeader(byte[] h)
        {
            header = h;
        }
    }
}
