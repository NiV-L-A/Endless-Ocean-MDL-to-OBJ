using System;
using System.IO;
using System.Text;

namespace EndlessOceanMDLToOBJExporter
{
    class Program
    {
        //ops...put these in this class :flushed:
        public static void PrintTMP(long TMP) //barely used
        {
            Console.WriteLine("#TMP: 0x" + TMP.ToString("X8"));
        }
        public static float ToBigEndianFloat(byte[] val) //Input: (4) byte array | Operation: reverse it | Output: float.
        {
            Array.Reverse(val);
            float aok = /*Convert.ToSingle(*/BitConverter.ToSingle(val)/*)*/;
            return aok;
        }
        public static UInt16 ReverseUInt16(UInt16 value) //Big Endian Unsigned Short 2 Bytes
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
        public static UInt32 ReverseUInt32(UInt32 value) //Big Endian Unsigned Int 4 bytes
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
        public static void PrintInfo()
        {
            string info = "Endless Ocean 1 & 2 .mdl to .obj Exporter\n";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (info.Length / 2)) + "}", info));
            string aut = "Authors: NiV & MDB\n";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (aut.Length / 2)) + "}", aut));
            string aut2 = "Special thanks to Hiroshi\n";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (aut2.Length / 2)) + "}", aut2));
            string ver = "Version 0.6\n";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (ver.Length / 2)) + "}", ver));
            string discorda = "If you have any issues, join this discord server and contact NiV-L-A:\n";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (discorda.Length / 2)) + "}", discorda));
            string discordb = "https://discord.gg/4hmcsmPMDG\n";
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (discordb.Length / 2)) + "}", discordb));
        }
        static void Main(string[] args)
        {
            Console.Title = "Endless Ocean 1 & 2 .mdl to .obj Exporter";
            if (args.Length == 0) //if no args are passed to the .exe
            {
                PrintInfo();
                Console.WriteLine("To use this tool, drag and drop a .mdl file onto the .exe!");
                Console.Read();
                return;
            }
            else
            {
                foreach (var arg in args) //damn boi, at least one arg, let's see what the script can do
                {
                    FileStream fs;
                    BinaryReader br;
                    string pathReal = Path.GetDirectoryName(arg) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(arg) + Path.GetExtension(arg);
                    try
                    {
                        fs = new FileStream(pathReal, FileMode.Open);
                        br = new BinaryReader(fs);
                    }
                    catch (IOException ex)
                    {
                        PrintInfo();
                        Console.WriteLine("Error: " + ex.Message);
                        fs = null;
                        br = null;
                        Console.WriteLine("Press any key to close the window");
                        Console.Read();
                        Environment.Exit(0);
                    }
                    catch (UnauthorizedAccessException uax)
                    {
                        PrintInfo();
                        Console.WriteLine("Error: " + uax.Message);
                        fs = null;
                        br = null;
                        Console.WriteLine("Consider moving the .mdl file in another folder!");
                        Console.WriteLine("Press any key to close the window");
                        Console.Read();
                        Environment.Exit(0);
                    }
                    //Some declaration stuff, nothin' personal, pardner
                    int ended;
                    int h = 0x00000000;
                    int printinfo = 1;
                    int printvtx = 1;
                    int printnorm = 1;
                    int printuv = 1;
                    int printind = 1;
                    int printTMP = 1;
                    uint indOff = 0x00;
                    ushort vtxCount = 0;
                    ushort normCount = 0;
                    ushort lightCount = 0;
                    ushort uvCount = 0;
                    ushort unk01Count = 0;
                    ushort unk02Count = 0;
                    byte indexCount = 0;
                    byte unkIsOneOrTwo = 0;
                    int floatcountloop = 0;
                    long cntCountAll = 0;
                    long aCountAll = 0;
                    bool vtxCountShort = false;
                    bool normCountShort = false;
                    bool lightCountShort = false;
                    bool uvCountShort = false;
                    bool unk01CountShort = false;
                    var pos = 0;
                    var pos2 = 0;
                    var pos3 = 0;
                    var pos4 = 0;
                    var norm = 0;
                    var norm2 = 0;
                    var norm3 = 0;
                    var norm4 = 0;
                    var light = 0;
                    var light2 = 0;
                    var light3 = 0;
                    var light4 = 0;
                    var uv = 0;
                    var uv2 = 0;
                    var uv3 = 0;
                    var uv4 = 0;
                    var unk011 = 0;
                    var unk012 = 0;
                    var unk013 = 0;
                    var unk014 = 0;
                    uint vtxOff = 0;
                    uint RFPMOff = 0;
                    bool RF2MD2 = false;
                    bool RF2MD3 = false;
                    bool RFPMD2 = false;

                    //***************************************************
                    //******* Magic detection and basic data read *******
                    //***************************************************

                    fs.Seek(0x00, SeekOrigin.Begin); //The magic is composed of a total of 6 bytes.
                    int Magic = br.ReadInt32(); //First 4, can be "RF2M" or "RFPM", RFPM is a special type used rarely.
                    short Magic2 = br.ReadInt16(); //Last 2, can be "D2" or "D3" - prob indcating the version
                    if (Magic != 0x4D324652 && Magic != 0x4D504652) //RF2M or RFPM
                    {
                        PrintInfo();
                        fs = null;
                        br = null;
                        Console.WriteLine("Error: RF2M Magic is missing! Are you loading the correct file?");
                        Console.WriteLine("Press any key to close the window");
                        Console.Read();
                        Environment.Exit(0);
                    }
                    //No need to assign the other bools to false. They will be false anyway.
                    if (Magic == 0x4D504652) //RFPM
                    {
                        RFPMD2 = true;
                    }
                    if (Magic2 == 0x3244) //D2
                    {
                        RF2MD2 = true;
                    }
                    else if (Magic2 == 0x3344) //D3
                    {
                        RF2MD3 = true;
                    }
                    else //No type of Magic2 recognized, most likely wrong file as input
                    {
                        PrintInfo();
                        fs = null;
                        br = null;
                        Console.WriteLine("Error: RF2M Magic is missing! Are you loading the correct file?");
                        Console.WriteLine("Press any key to close the window");
                        Console.WriteLine("Magic :" + Magic + Magic2);
                        Console.Read();
                        Environment.Exit(0);
                    }
                    fs.Seek(0x08, SeekOrigin.Begin); //Go to 0x08
                    ushort INFO_SIZE = br.ReadUInt16(); //should be three bytes, but let's get short anyway
                    fs.Seek(0x02, SeekOrigin.Current); //Skip DUMMY
                    uint HEAD_SIZE = br.ReadUInt32();
                    if (RFPMD2) //EO Special Format
                    {
                        fs.Seek(0x10, SeekOrigin.Current);
                        RFPMOff = br.ReadUInt32();
                    }
                    else //EO1 & EO2
                    {
                        fs.Seek(0x18, SeekOrigin.Current);
                    }
                    uint VDLOff = br.ReadUInt32(); //SIZE of the entire .vdl
                    fs.Seek(INFO_SIZE, SeekOrigin.Begin); //goto INFO_SIZE
                    if (RF2MD2) //EO1
                    {
                        fs.Seek(0x0A, SeekOrigin.Current);
                    }
                    else if (RF2MD3) //EO2
                    {
                        fs.Seek(0x0E, SeekOrigin.Current);
                    }
                    ushort MeshCount = br.ReadUInt16(); //Mesh count at +0x0E or both +0x0E and +0x10
                    if (MeshCount == 0) //Sometimes mesh count is not at 0x0E, so check if it's 00, if so, read the next 2 bytes
                    {
                        MeshCount = br.ReadUInt16(); //Mesh count at +0x10
                        if (RF2MD2)
                        {
                            fs.Seek(0x06, SeekOrigin.Current);
                        }
                        else if (RF2MD3)
                        {
                            fs.Seek(0x0E, SeekOrigin.Current);
                        }
                    }
                    else //could optimize this better
                    {
                        if (RF2MD2)
                        {
                            fs.Seek(0x08, SeekOrigin.Current);
                        }
                        else if (RF2MD3)
                        {
                            fs.Seek(0x10, SeekOrigin.Current);
                        }
                    }
                    Console.WriteLine("INFO_SIZE: 0x" + INFO_SIZE.ToString("X8") + "\nHEAD_SIZE: 0x" + HEAD_SIZE.ToString("X8") + "\n.VDL Start Off: 0x" + VDLOff.ToString("X8") + "\nMeshCount: 0x" + MeshCount.ToString("X8") + " (" + MeshCount + ")");

                    //**********************************************
                    //************* GET MESH INFO DATA *************
                    //**********************************************

                    for (int m = 0; m < MeshCount; m++) //Get INFO mesh data. It will go here everytime it finishes parsing the indices.
                    {
                        h += 0x00000001;
                        int MeshINFOSIZE; //useless variable
                        int contatore = 0;
                        int contatore2 = 0;
                        int contatoreoptimization = 0;
                        int contatoreoptimization2 = 0;
                        string NewPath = Path.GetDirectoryName(arg) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(arg) + Path.GetExtension(arg) + "_" + h + ".obj";
                        using FileStream fsNew = File.OpenWrite(NewPath);
                        ended = 0; //this variable is used to force the the indices data parsing process to move on to the next mesh
                        uint MeshStartOff = br.ReadUInt32(); //Start of the INFO for that particular mesh
                        long MeshTMP = fs.Seek(0x00, SeekOrigin.Current); //we will go back to this offset everytime we finish with the indices data
                        fs.Seek(MeshStartOff, SeekOrigin.Begin);
                        fs.Seek(0x04, SeekOrigin.Current); //skip 4 bytes
                        ushort unkTwoBytes = ReverseUInt16(br.ReadUInt16()); //get those 2 bytes, don't know what they are, but we will use them soon
                        //fs.Seek(0x06, SeekOrigin.Current);
                        ushort MeshNSection = ReverseUInt16(br.ReadUInt16()); //How many sections the indices data is composed of. 1 = 1 section, 2 = 2 sections. Always greater than 0.
                        fs.Seek(0x30, SeekOrigin.Current); //ASSUMING ALWAYS 0x30 - it contains some floats, like coordinate positions (xzy) - pos is given by the vertices tho
                        uint MeshHDRStartOff = ReverseUInt32(br.ReadUInt32()); //Offset that points to the beginning of the third (main) header of a mesh
                        uint MeshSize = ReverseUInt32(br.ReadUInt32()); //Size of the mesh
                        if (RFPMD2)
                        {
                            MeshHDRStartOff += RFPMOff; //EO Special format
                        }
                        else
                        {
                            MeshHDRStartOff += VDLOff; //Add the offset to the .vdl offset. This will give us the actual offset in the .mdl file.
                        }
                        long MeshUntilOff = MeshHDRStartOff + MeshSize;
                        uint[] MeshArray = new uint[MeshNSection - 1]; //-1 because we don't need to know the first offset, since it's the beginning of the indices data, we will get the offset later in the code.
                        byte[] optimization = new byte[MeshNSection];
                        /* Optimization in .mdl is either triangles or tris (triangle strips/tristrip).
                         * There's a specific byte in the INFO for each *index section*, if it's 0x03, it's triangles (rare), if it's 0x04, it's tristrip (very common).
                         * We declared the MeshArray array of length MeshNSection (how many index sections that mesh is composed of) -1,
                         * -1 because the first offset is the beginning of the index section,
                         * which we skip because is also present in the (third) Main header.
                         * optimization array has length MeshNSection, because we need to know what optimization the mesh uses for the first index section too!
                         */
                        if (MeshNSection == 1) //If 1 section, just read and out.
                        {
                            MeshINFOSIZE = 0x4C; //kinda untrue most of the time, but whatever
                            fs.Seek(0x03, SeekOrigin.Current);
                            optimization[contatoreoptimization] = br.ReadByte(); //0x04 = Tris | 0x03 = Triangles
                            if (optimization[contatoreoptimization] != 0x03 && optimization[contatoreoptimization] != 0x04)
                            { //A lot of INFO don't actually have the optimization byte in the same location, if they don't, then it will be at +0x07, so let's adjust it and re-read the byte, *without incrementing the counter!*
                                fs.Seek(0x07, SeekOrigin.Current);
                                optimization[contatoreoptimization] = br.ReadByte();
                            }
                        }
                        else //If that mesh has 2 index sections or more
                        {
                            MeshINFOSIZE = (MeshNSection * 0x0C) + 0x40; //+0x40 = -0x0C + 0x4C, not always true tho.
                            fs.Seek(0x02, SeekOrigin.Current);
                            ushort CheckOptimization = ReverseUInt16(br.ReadUInt16());
                            if (CheckOptimization == unkTwoBytes/*|| optimization[contatoreoptimization] != 0x03 && optimization[contatoreoptimization] != 0x04*/)
                            {
                                fs.Seek(0x07, SeekOrigin.Current);
                                optimization[contatoreoptimization] = br.ReadByte();
                            }
                            else
                            {
                                fs.Seek(fs.Position - 0x01, SeekOrigin.Begin); //Go back 0x01
                                optimization[contatoreoptimization] = br.ReadByte();
                            }
                            //optimization[contatoreoptimization] = br.ReadByte(); //0x04 = Tris | 0x03 = Triangles
                            //if (optimization[contatoreoptimization] != 0x03 && optimization[contatoreoptimization] != 0x04)
                            //{ //exceptions, adjust
                            //    fs.Seek(0x07, SeekOrigin.Current);
                            //    optimization[contatoreoptimization] = br.ReadByte();
                            //}
                            contatoreoptimization += 1;
                            fs.Seek(0x0B, SeekOrigin.Current);

                            for (int MeshOffTest = 1; MeshOffTest < MeshNSection; MeshOffTest++) //get other indices INFO
                            {
                                optimization[contatoreoptimization] = br.ReadByte(); //0x04 = Tris | 0x03 = Triangles
                                contatoreoptimization += 1;
                                fs.Seek(0x04, SeekOrigin.Current);
                                MeshArray[contatore] = ReverseUInt32(br.ReadUInt32());
                                MeshArray[contatore] += MeshHDRStartOff;
                                fs.Seek(0x03, SeekOrigin.Current);
                                contatore += 1;
                            }
                        }

                        //*********************************************
                        //************ (THIRD) MAIN HEADER ************
                        //*********************************************

                        fs.Seek(MeshHDRStartOff, SeekOrigin.Begin); //go to main (third) Header of .vdl
                        try
                        {
                            vtxOff = ReverseUInt32(br.ReadUInt32()); //get the vertices Offset (always 0x20 or 0x40)
                        }
                        catch (EndOfStreamException eose) //we'll get'em next time
                        {
                            PrintInfo();
                            Console.WriteLine("Error: " + eose.Message);
                            Console.WriteLine("Wrong Mesh Start Offset.");
                            PrintTMP(fs.Seek(0x00, SeekOrigin.Current));
                            Console.WriteLine("Press any key to close the window.");
                            fs = null;
                            br = null;
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        uint normOff = ReverseUInt32(br.ReadUInt32()); //get normals offset
                        uint lightOff = ReverseUInt32(br.ReadUInt32()); //get light offset
                        uint uvOff = ReverseUInt32(br.ReadUInt32()); //get uv offset

                        if (vtxOff == 0x20) //Endless Ocean 1 Format
                        {
                            indOff = ReverseUInt32(br.ReadUInt32());
                            vtxCount = ReverseUInt16(br.ReadUInt16());
                            normCount = ReverseUInt16(br.ReadUInt16());
                            lightCount = ReverseUInt16(br.ReadUInt16());
                            uvCount = ReverseUInt16(br.ReadUInt16());
                            ushort fakeIndexCount = ReverseUInt16(br.ReadUInt16());
                            ushort unkFlag = ReverseUInt16(br.ReadUInt16());
                            //fakeIndexCount = 6;
                            indOff = indOff + 0x20 + MeshHDRStartOff;
                            if (printinfo == 1)
                            {
                                Console.WriteLine("Endless Ocean 1 Format Detected");
                                Console.WriteLine("#MeshCount: " + MeshCount);
                                Console.WriteLine("#vtxOff: " + vtxOff + " [0x" + vtxOff.ToString("X8") + "]");
                                Console.WriteLine("#normOff: " + normOff + " [0x" + normOff.ToString("X8") + "]");
                                Console.WriteLine("#lightOff: " + lightOff + " [0x" + lightOff.ToString("X8") + "]");
                                Console.WriteLine("#uvOff: " + uvOff + " [0x" + uvOff.ToString("X8") + "]");
                                Console.WriteLine("#indOff: " + indOff + " [0x" + indOff.ToString("X8") + "]\n");
                                Console.WriteLine("#vtxCount: " + vtxCount + " [0x" + vtxCount.ToString("X4") + "]");
                                Console.WriteLine("#normCount: " + normCount + " [0x" + normCount.ToString("X4") + "]");
                                Console.WriteLine("#lightCount: " + lightCount + " [0x" + lightCount.ToString("X4") + "]");
                                Console.WriteLine("#uvCount: " + uvCount + " [0x" + uvCount.ToString("X4") + "]");
                                Console.WriteLine("#fakeIndexCount: 0x" + fakeIndexCount.ToString("X4"));
                                Console.WriteLine("#unkFlag: 0x" + unkFlag.ToString("X4"));
                                Console.WriteLine("#IndexCount: " + indexCount);
                            }
                        }
                        else if (vtxOff == 0x40) //Endless Ocean 2 Format
                        {
                            uint unk01Off = ReverseUInt32(br.ReadUInt32());
                            uint unk02Off = ReverseUInt32(br.ReadUInt32());
                            uint unk03Off = ReverseUInt32(br.ReadUInt32());
                            indOff = ReverseUInt32(br.ReadUInt32());
                            vtxCount = ReverseUInt16(br.ReadUInt16());
                            normCount = ReverseUInt16(br.ReadUInt16());
                            lightCount = ReverseUInt16(br.ReadUInt16());
                            uvCount = ReverseUInt16(br.ReadUInt16());
                            unk01Count = ReverseUInt16(br.ReadUInt16());
                            unk02Count = ReverseUInt16(br.ReadUInt16());
                            ushort unk03Count = ReverseUInt16(br.ReadUInt16());
                            ushort fakeIndexCount = ReverseUInt16(br.ReadUInt16());
                            fs.Seek(0x09, SeekOrigin.Current);
                            indexCount = br.ReadByte(); //Possible values: 0,3,4,5,6,7,8,9
                            byte isNormOff4C = br.ReadByte();
                            unkIsOneOrTwo = br.ReadByte();
                            indOff = indOff + 0x40 + MeshHDRStartOff;
                            vtxOff += MeshHDRStartOff;
                            if (printinfo == 1)
                            {
                                Console.WriteLine("#Endless Ocean 2 format detected");
                                Console.WriteLine("#MeshCount: " + MeshCount);
                                Console.WriteLine("#vtxOff: " + vtxOff + " [0x" + vtxOff.ToString("X8") + "]");
                                Console.WriteLine("#normOff: " + normOff + " [0x" + normOff.ToString("X8") + "]");
                                Console.WriteLine("#lightOff: " + lightOff + " [0x" + lightOff.ToString("X8") + "]");
                                Console.WriteLine("#uvOff: " + uvOff + " [0x" + uvOff.ToString("X8") + "]");
                                Console.WriteLine("#indOff: " + indOff + " [0x" + indOff.ToString("X8") + "]\n");
                                Console.WriteLine("#vtxCount: " + vtxCount + " [0x" + vtxCount.ToString("X4") + "]");
                                Console.WriteLine("#normCount: " + normCount + " [0x" + normCount.ToString("X4") + "]");
                                Console.WriteLine("#lightCount: " + lightCount + " [0x" + lightCount.ToString("X4") + "]");
                                Console.WriteLine("#uvCount: " + uvCount + " [0x" + uvCount.ToString("X4") + "]");
                                Console.WriteLine("#fakeIndexCount: " + fakeIndexCount + " [0x" + fakeIndexCount.ToString("X4") + "]");
                                Console.WriteLine("#IndexCount: " + indexCount);
                            }
                            fs.Seek(vtxOff, SeekOrigin.Begin);
                            var vtxcountdata = $"#vtxCount: {vtxCount} [0x{vtxCount:X4}]\n";
                            byte[] vtxcountdatabytes = Encoding.UTF8.GetBytes(vtxcountdata);
                            fsNew.Write(vtxcountdatabytes, 0, vtxcountdatabytes.Length);
                        }

                        /* A general and global rule with .mdls is that,
                         * if the count of a specific section is greater than or equal to 0xFF,
                         * it means that the index section will be composed of a 2 bytes version of the index.
                         * There're a few exceptions here and there tho!
                         */
                        if (vtxCount >= 0xFF)
                        {
                            vtxCountShort = true;
                        }
                        else
                        {
                            vtxCountShort = false;
                        }
                        if (normCount >= 0xFF)
                        {
                            normCountShort = true;
                        }
                        else
                        {
                            normCountShort = false;
                        }
                        if (lightCount >= 0xFF || RF2MD2 == true && lightOff > 0x00) //If RF2MD2 and there's light data, light will always be 2 bytes for the indices.
                        {
                            lightCountShort = true;
                        }
                        else
                        {
                            lightCountShort = false;
                        }
                        if (uvCount >= 0xFF)
                        {
                            uvCountShort = true;
                        }
                        else
                        {
                            uvCountShort = false;
                        }
                        if (unk01Count >= 0xFF) //unknown data, it's indexed tho, so let's count it.
                        {
                            unk01CountShort = true;
                        }
                        else
                        {
                            unk01CountShort = false;
                        }

                        //*********************************************
                        //*************** VERTICES DATA ***************
                        //*********************************************

                        floatcountloop = 0;
                        for (int i = 0; i < vtxCount; i++) //Read and log vertices
                        {
                            floatcountloop += 1;
                            long TMPVert = fs.Seek(0x00, SeekOrigin.Current);
                            byte[] q = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                            byte[] w = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                            byte[] e = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                            float x = ToBigEndianFloat(q);
                            float y = ToBigEndianFloat(w);
                            float z = ToBigEndianFloat(e);
                            if (printvtx == 1)
                            {
                                // Check if it's Not a Number, if so, assign that coordinate the value of 0.
                                if (float.IsNaN(x))
                                {
                                    x = 0;
                                }
                                if (float.IsNaN(y))
                                {
                                    y = 0;
                                }
                                if (float.IsNaN(z))
                                {
                                    z = 0;
                                }
                                var vtxdata = $"v {x} {y} {z} #0x{TMPVert:X8} N: {floatcountloop} \n";
                                byte[] vtxdatabytes = Encoding.UTF8.GetBytes(vtxdata);
                                fsNew.Write(vtxdatabytes, 0, vtxdatabytes.Length);
                                Console.WriteLine($"v {x} {y} {z} #0x{TMPVert:X8} N: {floatcountloop}");
                            }

                            /*Sometimes, the vertices and the normals alternate between each other.
                            If this happens, then just skip 0x0C bytes (the 3 norm floats) */
                            if (normOff == 0x2C || normOff == 0x4C) //EO1 + EO2
                            {
                                fs.Seek(0x0C, SeekOrigin.Current);
                            }
                        }

                        if (printTMP == 1)
                        {
                            PrintTMP(fs.Seek(0x00, SeekOrigin.Current));
                        }

                        //********************************************
                        //*************** NORMALS DATA ***************
                        //********************************************

                        floatcountloop = 0;
                        normOff += MeshHDRStartOff;
                        fs.Seek(normOff, SeekOrigin.Begin); //go to normals offset
                        normOff -= MeshHDRStartOff; //subtract the value added, we're going to check it later
                        var normcountdata = $"#normCount: {normCount} [0x{normCount:X4}]\n";
                        byte[] normcountdatabytes = Encoding.UTF8.GetBytes(normcountdata);
                        fsNew.Write(normcountdatabytes, 0, normcountdatabytes.Length);
                        for (int i = 0; i < normCount; i++) //normals
                        {
                            floatcountloop += 1;
                            long TMPNorm = fs.Seek(0x00, SeekOrigin.Current);
                            byte[] q = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                            byte[] w = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                            byte[] e = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                            float x = ToBigEndianFloat(q);
                            float y = ToBigEndianFloat(w);
                            float z = ToBigEndianFloat(e);
                            if (printnorm == 1)
                            {
                                if (float.IsNaN(x))
                                {
                                    x = 0;
                                }
                                if (float.IsNaN(y))
                                {
                                    y = 0;
                                }
                                if (float.IsNaN(z))
                                {
                                    z = 0;
                                }
                                var normdata = $"vn {x} {y} {z} #0x{TMPNorm:X8} N: {floatcountloop} \n";
                                byte[] normdatabytes = Encoding.UTF8.GetBytes(normdata);
                                fsNew.Write(normdatabytes, 0, normdatabytes.Length);
                                Console.WriteLine($"vn {x} {y} {z} #0x{TMPNorm:X8} N: {floatcountloop}");
                            }
                            if (normOff == 0x2C || normOff == 0x4C)
                            {
                                fs.Seek(0x0C, SeekOrigin.Current);
                            }
                        }
                        if (printTMP == 1)
                        {
                            PrintTMP(fs.Seek(0x00, SeekOrigin.Current));
                        }

                        //********************************************
                        //***************** UVS DATA *****************
                        //********************************************

                        floatcountloop = 0;
                        uvOff += MeshHDRStartOff;
                        fs.Seek(uvOff, SeekOrigin.Begin);
                        var uvcountdata = $"#uvCount: {uvCount} [0x{uvCount:X4}]\n";
                        byte[] uvcountdatabytes = Encoding.UTF8.GetBytes(uvcountdata);
                        fsNew.Write(uvcountdatabytes, 0, uvcountdatabytes.Length);
                        for (int i = 0; i < uvCount; i++) //UVs
                        {
                            floatcountloop += 1;
                            long TMPUV = fs.Seek(0x00, SeekOrigin.Current);
                            byte[] q = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                            byte[] w = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                            float u = ToBigEndianFloat(q);
                            float v = ToBigEndianFloat(w);
                            if (printuv == 1)
                            {
                                if (float.IsNaN(u))
                                {
                                    u = 0;
                                }
                                if (float.IsNaN(v))
                                {
                                    v = 0;
                                }
                                var uvdata = $"vt {u} {v} #0x{TMPUV:X8} N: {floatcountloop} \n";
                                byte[] uvdatabytes = Encoding.UTF8.GetBytes(uvdata);
                                fsNew.Write(uvdatabytes, 0, uvdatabytes.Length);
                                Console.WriteLine($"vt {u} {v} #0x{TMPUV:X8} N: {floatcountloop}");
                            }
                        }
                        if (printTMP == 1)
                        {
                            PrintTMP(fs.Seek(0x00, SeekOrigin.Current));
                        }

                        //********************************************
                        //*************** INDICES DATA ***************
                        //********************************************

                        cntCountAll = 0;
                        aCountAll = 0;
                        /* a = unknown 2 bytes
                         * cnt = Sequence Count - It indicates each "fragment".
                         * EO2 only: You have to multiply it by the 0x39 byte in the Main Header to know the size of the current draw.
                         * The Indices Section, usually, is divided in multiple draw calls.
                         * Though there could even be 1 single draw call (very rare, tho it will always happens if optimization is triangles).
                         * Index Sections are usually divided by 00s, these are here just for 0x10 alignment.
                         */
                        if (printind == 1) //The Epic Indices
                        {
                            try
                            {
                                fs.Seek(indOff, SeekOrigin.Begin);
                                if (vtxOff > 0x00) //not sure why I added this useless if, vtxOff is always > 0x00
                                {
                                    if (printinfo == 1)
                                    {
                                        long TMP3 = fs.Seek(0x00, SeekOrigin.Current);
                                        var info = "#indexCount: " + indexCount + " - 3\n#TMP: 0x" + TMP3.ToString("X8") + "\n";
                                        byte[] infobytes = Encoding.UTF8.GetBytes(info);
                                        fsNew.Write(infobytes, 0, infobytes.Length);
                                        Console.WriteLine("#indexCount: " + indexCount + " - 3");
                                    }
                                    for (int j = 0; j < 1000000; j++)
                                    {
                                        long TMP3 = fs.Seek(0x00, SeekOrigin.Current);
                                        var info = "#---TMP: 0x" + TMP3.ToString("X8") + "---\n";
                                        byte[] infobytes = Encoding.UTF8.GetBytes(info);
                                        fsNew.Write(infobytes, 0, infobytes.Length);
                                        Console.WriteLine(info);
                                        ushort a = ReverseUInt16(br.ReadUInt16());
                                        ushort cnt = ReverseUInt16(br.ReadUInt16());
                                        cntCountAll += cnt;
                                        aCountAll += a;
                                        TMP3 = fs.Seek(0x00, SeekOrigin.Current);

                                        if (cnt == 0x01 || cnt == 0x02)
                                        /*Sometimes, and don't even ask me why, the count is 0x01 or 0x02.
                                        * Which doesn't make sense, since you must need at least 3 indices to form a face.            */
                                        {
                                            for (int cntEqualsOneOrTwo = 0; cntEqualsOneOrTwo < 100000; cntEqualsOneOrTwo++)
                                            {
                                                // Let's assign indexcount, since we're going to use it to skip those indices
                                                if (RF2MD2 && lightOff > 0x00) //2 bytes(POS/NORM/LIGHT/UV) = 8 bytes
                                                {
                                                    indexCount = 8;
                                                }
                                                else if (RF2MD2 && lightOff == 0x00 || RF2MD3 && indexCount == 0 && lightOff == 0x00) //2 bytes(POS/NORM/UV)
                                                {
                                                    indexCount = 6;
                                                }
                                                if (cnt == 0x01)
                                                {
                                                    fs.Seek(indexCount, SeekOrigin.Current); //skip amount
                                                }
                                                else if (cnt == 0x02)
                                                {
                                                    fs.Seek(indexCount * 2, SeekOrigin.Current); //skip amount*2
                                                }
                                                if (RF2MD2 && lightOff > 0x00 || RF2MD2 && lightOff == 0x00 || RF2MD3 && lightOff == 0x00)
                                                {
                                                    //assign back to indexCount the value 0, we're going to use it
                                                    indexCount = 0;
                                                }
                                                // Let's see if next cnt equals to 0x01 or 0x02, if so, repeat the process.
                                                a = ReverseUInt16(br.ReadUInt16());
                                                cnt = ReverseUInt16(br.ReadUInt16());
                                                cntCountAll += cnt;
                                                aCountAll += a;
                                                TMP3 = fs.Seek(0x00, SeekOrigin.Current);
                                                if (cnt != 0x01 && cnt != 0x02) //else, just get out of the loop and move on
                                                {
                                                    cntEqualsOneOrTwo = 500000;
                                                    info = "#---TMP: 0x" + TMP3.ToString("X8") + "---\n";
                                                    infobytes = Encoding.UTF8.GetBytes(info);
                                                    fsNew.Write(infobytes, 0, infobytes.Length);
                                                }
                                            }
                                        }

                                        if (TMP3 >= MeshUntilOff) //When a mesh has ended
                                        {
                                            Console.WriteLine("#Mesh Ended");
                                            j = 5000000;
                                            ended = 1;
                                            fs.Seek(MeshTMP, SeekOrigin.Begin);
                                        }

                                        if (MeshNSection >= 2)
                                        {
                                            //For the love of my life, I don't remember why I added this if...
                                            if (contatore2 == contatore)
                                            {

                                            }
                                            else if (a == 0 && cnt == 0 && contatore2 != contatore || TMP3 >= MeshArray[contatore2])
                                            //if a == 0 and cnt == 0, then that's the alignment.
                                            {
                                                fs.Seek(MeshArray[contatore2], SeekOrigin.Begin);
                                                contatoreoptimization2 = contatore2 + 1;
                                                contatore2 += 1;
                                                a = ReverseUInt16(br.ReadUInt16());
                                                cnt = ReverseUInt16(br.ReadUInt16());
                                                cntCountAll += cnt;
                                                aCountAll += a;
                                                TMP3 = fs.Seek(0x00, SeekOrigin.Current);
                                            }
                                        }
                                        else if (a == 0 && cnt == 0 && contatore2 == contatore)
                                        {
                                            //If there's only 1 index section, then go here
                                            Console.WriteLine("#Mesh Ended");
                                            j = 5000000;
                                            ended = 1;
                                            fs.Seek(MeshTMP, SeekOrigin.Begin);
                                        }

                                        if (ended == 0)
                                        {
                                            int id = 3;
                                            int id2 = 0;
                                            var infocnt = "#---cnt: 0x" + cnt.ToString("X8") + "---\n";
                                            byte[] infocntbytes = Encoding.UTF8.GetBytes(infocnt);
                                            fsNew.Write(infocntbytes, 0, infocntbytes.Length);
                                            for (int q = 0; q < cnt; q++)
                                            {
                                                if (vtxCountShort || indexCount == 0) //read vtx short
                                                {
                                                    pos = ReverseUInt16(br.ReadUInt16());
                                                    pos = (ushort)(pos + 1);
                                                }
                                                else
                                                {
                                                    pos = br.ReadByte();//read vtx byte
                                                    pos = (byte)(pos + 1);
                                                }
                                                if (normCountShort || indexCount == 0)
                                                {
                                                    norm = ReverseUInt16(br.ReadUInt16()); //read norm short
                                                    norm = (ushort)(norm + 1);
                                                }
                                                else
                                                {
                                                    norm = br.ReadByte(); //read norm byte
                                                    norm = (byte)(norm + 1);
                                                }
                                                if (lightCountShort || RFPMD2 == true && lightOff > 0x00 || indexCount == 0 && lightOff != 0) //RFPMD2 special type
                                                {
                                                    light = ReverseUInt16(br.ReadUInt16());
                                                    light = (ushort)(light + 1);
                                                }
                                                else if (!lightCountShort && lightCount >= 0x01)
                                                {
                                                    light = br.ReadByte();
                                                    light = (byte)(light + 1);
                                                }
                                                if (uvCountShort || indexCount == 0)
                                                {
                                                    uv = ReverseUInt16(br.ReadUInt16());
                                                    uv = (ushort)(uv + 1);
                                                }
                                                else
                                                {
                                                    uv = br.ReadByte();
                                                    uv = (byte)(uv + 1);
                                                }
                                                if (unk01CountShort && indexCount != 0)
                                                {
                                                    unk011 = ReverseUInt16(br.ReadUInt16());
                                                    unk011 = (ushort)(unk011 + 1);
                                                }
                                                else if (!unk01CountShort && unk01Count >= 0x01 && indexCount != 0)
                                                {
                                                    unk011 = br.ReadByte();
                                                    unk011 = (byte)(unk011 + 1);
                                                }
                                                if (vtxCountShort || indexCount == 0)
                                                {
                                                    pos2 = ReverseUInt16(br.ReadUInt16());
                                                    pos2 = (ushort)(pos2 + 1);
                                                }
                                                else
                                                {
                                                    pos2 = br.ReadByte();
                                                    pos2 = (byte)(pos2 + 1);
                                                }
                                                if (normCountShort || indexCount == 0)
                                                {
                                                    norm2 = ReverseUInt16(br.ReadUInt16());
                                                    norm2 = (ushort)(norm2 + 1);
                                                }
                                                else
                                                {
                                                    norm2 = br.ReadByte();
                                                    norm2 = (byte)(norm2 + 1);
                                                }
                                                if (lightCountShort || RFPMD2 == true && lightOff > 0x00 || indexCount == 0 && lightOff != 0)
                                                {
                                                    light2 = ReverseUInt16(br.ReadUInt16());
                                                    light2 = (ushort)(light2 + 1);
                                                }
                                                else if (!lightCountShort && lightCount >= 0x01)
                                                {
                                                    light2 = br.ReadByte();
                                                    light2 = (byte)(light2 + 1);
                                                }
                                                if (uvCountShort || indexCount == 0)
                                                {
                                                    uv2 = ReverseUInt16(br.ReadUInt16());
                                                    uv2 = (ushort)(uv2 + 1);
                                                }
                                                else
                                                {
                                                    uv2 = br.ReadByte();
                                                    uv2 = (byte)(uv2 + 1);
                                                }
                                                if (unk01CountShort && indexCount != 0)
                                                {
                                                    unk012 = ReverseUInt16(br.ReadUInt16());
                                                    unk012 = (ushort)(unk012 + 1);
                                                }
                                                else if (!unk01CountShort && unk01Count >= 0x01 && indexCount != 0)
                                                {
                                                    unk012 = br.ReadByte();
                                                    unk012 = (byte)(unk012 + 1);
                                                }
                                                long TMP2 = fs.Seek(0x00, SeekOrigin.Current); //savepos to go back here
                                                if (vtxCountShort || indexCount == 0)
                                                {
                                                    pos3 = ReverseUInt16(br.ReadUInt16());
                                                    pos3 = (ushort)(pos3 + 1);
                                                }
                                                else
                                                {
                                                    pos3 = br.ReadByte();
                                                    pos3 = (byte)(pos3 + 1);
                                                }
                                                if (normCountShort || indexCount == 0)
                                                {
                                                    norm3 = ReverseUInt16(br.ReadUInt16());
                                                    norm3 = (ushort)(norm3 + 1);
                                                }
                                                else
                                                {
                                                    norm3 = br.ReadByte();
                                                    norm3 = (byte)(norm3 + 1);
                                                }
                                                if (lightCountShort || RFPMD2 == true && lightOff > 0x00 || indexCount == 0 && lightOff != 0)
                                                {
                                                    light3 = ReverseUInt16(br.ReadUInt16());
                                                    light3 = (ushort)(light3 + 1);
                                                }
                                                else if (!lightCountShort && lightCount >= 0x01)
                                                {
                                                    light3 = br.ReadByte();
                                                    light3 = (byte)(light3 + 1);
                                                }
                                                if (uvCountShort || indexCount == 0)
                                                {
                                                    uv3 = ReverseUInt16(br.ReadUInt16());
                                                    uv3 = (ushort)(uv3 + 1);
                                                }
                                                else
                                                {
                                                    uv3 = br.ReadByte();
                                                    uv3 = (byte)(uv3 + 1);
                                                }
                                                if (unk01CountShort && indexCount != 0)
                                                {
                                                    unk013 = ReverseUInt16(br.ReadUInt16());
                                                    unk013 = (ushort)(unk013 + 1);
                                                }
                                                else if (!unk01CountShort && unk01Count >= 0x01 && indexCount != 0)
                                                {
                                                    unk013 = br.ReadByte();
                                                    unk013 = (byte)(unk013 + 1);
                                                }
                                                //print "f %pos%/%uv%/%norm% %pos2%/%uv2%/%norm2% %pos3%/%uv3%/%norm3%"
                                                var data = "f " + pos + "/" + uv + "/" + norm + " " + pos2 + "/" + uv2 + "/" + norm2 + " " + pos3 + "/" + uv3 + "/" + norm3 + "\n";
                                                byte[] databytes = Encoding.UTF8.GetBytes(data);
                                                fsNew.Write(databytes, 0, databytes.Length);
                                                Console.WriteLine("f " + pos + "/" + uv + "/" + norm + " " + pos2 + "/" + uv2 + "/" + norm2 + " " + pos3 + "/" + uv3 + "/" + norm3);
                                                id += 1;

                                                if (optimization[contatoreoptimization2] == 0x04) //if it's tristrip
                                                {
                                                    if (id >= cnt) //draw call ended, out
                                                    {
                                                        q = 10000000;
                                                    }
                                                    if (cnt == 4) //if cnt equals to 4, do one more, and then out.
                                                    {
                                                        id += id2;
                                                        id2 = 1;
                                                        if (vtxCountShort || indexCount == 0)
                                                        {
                                                            pos4 = ReverseUInt16(br.ReadUInt16());
                                                            pos4 = (ushort)(pos4 + 1);
                                                        }
                                                        else
                                                        {
                                                            pos4 = br.ReadByte();
                                                            pos4 = (byte)(pos4 + 1);
                                                        }
                                                        if (normCountShort || indexCount == 0)
                                                        {
                                                            norm4 = ReverseUInt16(br.ReadUInt16());
                                                            norm4 = (ushort)(norm4 + 1);
                                                        }
                                                        else
                                                        {
                                                            norm4 = br.ReadByte();
                                                            norm4 = (byte)(norm4 + 1);
                                                        }
                                                        if (lightCountShort || RFPMD2 == true && lightOff > 0x00 || indexCount == 0 && lightOff != 0)
                                                        {
                                                            light4 = ReverseUInt16(br.ReadUInt16());
                                                            light4 = (ushort)(light4 + 1);
                                                        }
                                                        else if (!lightCountShort && lightCount >= 0x01)
                                                        {
                                                            light4 = br.ReadByte();
                                                            light4 = (byte)(light4 + 1);
                                                        }
                                                        if (uvCountShort || indexCount == 0)
                                                        {
                                                            uv4 = ReverseUInt16(br.ReadUInt16());
                                                            uv4 = (ushort)(uv4 + 1);
                                                        }
                                                        else
                                                        {
                                                            uv4 = br.ReadByte();
                                                            uv4 = (byte)(uv4 + 1);
                                                        }
                                                        if (unk01CountShort && indexCount != 0)
                                                        {
                                                            unk014 = ReverseUInt16(br.ReadUInt16());
                                                            unk014 = (ushort)(unk014 + 1);
                                                        }
                                                        else if (!unk01CountShort && unk01Count >= 0x01 && indexCount != 0)
                                                        {
                                                            unk014 = br.ReadByte();
                                                            unk014 = (byte)(unk014 + 1);
                                                        }
                                                        //print "f %pos4%/%uv4%/%norm4% %pos3%/%uv3%/%norm3% %pos2%/%uv2%/%norm2%"
                                                        var data2 = "f " + pos4 + "/" + uv4 + "/" + norm4 + " " + pos3 + "/" + uv3 + "/" + norm3 + " " + pos2 + "/" + uv2 + "/" + norm2 + "\n";
                                                        byte[] databytes2 = Encoding.UTF8.GetBytes(data2);
                                                        fsNew.Write(databytes2, 0, databytes2.Length);
                                                        Console.WriteLine("f " + pos4 + "/" + uv4 + "/" + norm4 + " " + pos3 + "/" + uv3 + "/" + norm3 + " " + pos2 + "/" + uv2 + "/" + norm2);
                                                        q = 10000000;
                                                    }
                                                    else if (id < cnt) //most hit if
                                                    {
                                                        id += id2;
                                                        id2 = 1;
                                                        if (vtxCountShort || indexCount == 0)
                                                        {
                                                            pos4 = ReverseUInt16(br.ReadUInt16());
                                                            pos4 = (ushort)(pos4 + 1);
                                                        }
                                                        else
                                                        {
                                                            pos4 = br.ReadByte();
                                                            pos4 = (byte)(pos4 + 1);
                                                        }
                                                        if (normCountShort || indexCount == 0)
                                                        {
                                                            norm4 = ReverseUInt16(br.ReadUInt16());
                                                            norm4 = (ushort)(norm4 + 1);
                                                        }
                                                        else
                                                        {
                                                            norm4 = br.ReadByte();
                                                            norm4 = (byte)(norm4 + 1);
                                                        }
                                                        if (lightCountShort || RFPMD2 == true && lightOff > 0x00 || indexCount == 0 && lightOff != 0)
                                                        {
                                                            light4 = ReverseUInt16(br.ReadUInt16());
                                                            light4 = (ushort)(light4 + 1);
                                                        }
                                                        else if (!lightCountShort && lightCount >= 0x01)
                                                        {
                                                            light4 = br.ReadByte();
                                                            light4 = (byte)(light4 + 1);
                                                        }
                                                        if (uvCountShort || indexCount == 0)
                                                        {
                                                            uv4 = ReverseUInt16(br.ReadUInt16());
                                                            uv4 = (ushort)(uv4 + 1);
                                                        }
                                                        else
                                                        {
                                                            uv4 = br.ReadByte();
                                                            uv4 = (byte)(uv4 + 1);
                                                        }
                                                        if (unk01CountShort && indexCount != 0)
                                                        {
                                                            unk014 = ReverseUInt16(br.ReadUInt16());
                                                            unk014 = (ushort)(unk014 + 1);
                                                        }
                                                        else if (!unk01CountShort && unk01Count >= 0x01 && indexCount != 0)
                                                        {
                                                            unk014 = br.ReadByte();
                                                            unk014 = (byte)(unk014 + 1);
                                                        }
                                                        var data3 = "f " + pos4 + "/" + uv4 + "/" + norm4 + " " + pos3 + "/" + uv3 + "/" + norm3 + " " + pos2 + "/" + uv2 + "/" + norm2 + "\n";
                                                        byte[] databytes3 = Encoding.UTF8.GetBytes(data3);
                                                        fsNew.Write(databytes3, 0, databytes3.Length);
                                                        Console.WriteLine("f " + pos4 + "/" + uv4 + "/" + norm4 + " " + pos3 + "/" + uv3 + "/" + norm3 + " " + pos2 + "/" + uv2 + "/" + norm2);
                                                        if (id == cnt) //now id equals to cnt, out
                                                        {
                                                            q = 10000000;
                                                        }
                                                        else //we haven't finished with this draw call yet
                                                        {
                                                            fs.Seek(TMP2, SeekOrigin.Begin);
                                                        }
                                                    }
                                                }
                                                else if (optimization[contatoreoptimization2] == 0x03) //if triangles
                                                {
                                                    if (id >= cnt)
                                                    {
                                                        q = 10000000; //out
                                                    }
                                                    id += 2; //else, just repeat the process
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (EndOfStreamException)
                            {
                                Console.WriteLine("#File Ended");
                            }
                        }
                        fs.Seek(MeshTMP, SeekOrigin.Begin); //actually useless, but don't tell anyone
                    }
                }
            }
            Console.WriteLine("Please, press any key to exit");
            Console.ReadKey();
        }
    }
}