/*
GOG Release

ext[.386]=3         -> DLL
ext[.BAT]=3         -> Batch
ext[.CAM]=20        -> Objectives (plain text)
ext[.conf]=5        -> GOG specific (networking configuration)
ext[.dat]=1         -> Uninstaller
ext[.DMO]=1         -> Save game (demo)
ext[.DTA]=1         -> Save game
ext[.EXE]=5         -> Executable
ext[.FLC]=71        -> Video
ext[.FON]=7         -> Font
ext[.gog]=1         -> GOG specific
ext[.hashdb]=1      -> GOG specific
ext[.ico]=4         -> Icon
ext[.info]=1        -> GOG specific
ext[.ini]=4         -> Configuration (max.ini read by MAX, other files related to GOG or the installer)
ext[.ins]=1         -> GOG specific installer
ext[.lnk]=1         -> Windows shortcut
ext[.MPS]=24        -> Save files / Objectives (plain text)
ext[.MSC]=13        -> Music (playable in VLC/Goldwave)
ext[.msg]=1         -> Uninstaller info
ext[.MVE]=3         -> Movie
ext[.OLD]=1         -> Old DOS extender
ext[.PAL]=1         -> Palette (unused?)
ext[.PDF]=1         -> Readme
ext[.RES]=1         -> Archive file
ext[.SCE]=48        -> Save files / Objectives (plain text)
ext[.script]=1      -> GOG specific installer
ext[.SPW]=272       -> Sound effects
ext[.TRA]=30        -> Objectives (plain text)
ext[.TXT]=2         -> Readme file
ext[.wri]=1         -> Readme file
ext[.WRL]=24        -> World (tileset graphics)
                        Crater, Desert, Green, Snow
*/

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;


// var filename = @"D:\_\MAX\max.res";
//var filename = @"D:\_\MAX\SNOW_6.WRL";
var filename = @"D:\_\MAX\GREEN_2.WRL";
//var filename = @"D:\_\MAX\out\CLN3LOGO";

if (!File.Exists(filename))
{
    Console.WriteLine("File does not exist");
    return;
}

var fileType = Path.GetExtension(filename);
switch (fileType?.ToUpper())
{
    case ".RES":
        ReadRes(filename);
        break;
    case ".WRL":
        ReadWrl(filename);
        break;
    case ".GFX":
    case "":
        ReadGFX(filename);
        break;
    default:
        Console.WriteLine("Unknown file type");
        break;
}


//------------------------------------------------------------
// RES
//------------------------------------------------------------
void ReadRes(string filename)
{
    const int FilenameLength = 8;

    try
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        var signature = br.ReadChars(4);
        if (String.Join("", signature) != "RES0")
        {
            Console.WriteLine("File does not appear a RES file");
            return;
        }

        var fileListingOffset = br.ReadInt32();
        var fileListingCount = br.ReadInt32() / 16;

        br.BaseStream.Seek(fileListingOffset, SeekOrigin.Begin);
        var fileInfos = new List<FileInfo>(fileListingCount);
        for (int i = 0; i < fileListingCount; i++)
        {
            var fileInfo = new FileInfo();
            fileInfo.Name = String.Join("", br.ReadChars(FilenameLength));
            fileInfo.Name = fileInfo.Name.Trim('\0');
            fileInfo.Offset = br.ReadInt32();
            fileInfo.Length = br.ReadInt32();
            fileInfos.Add(fileInfo);
        }


        foreach (var fileInfo in fileInfos)
        {
            br.BaseStream.Seek(fileInfo.Offset, SeekOrigin.Begin);
            var bytes = br.ReadBytes(fileInfo.Length);
            File.WriteAllBytes(Path.Combine(@"D:\_\MAX\out", fileInfo.Name), bytes);
        }
    }
    catch
    {
        Console.WriteLine("An unexpected error occured");
    }
}


//------------------------------------------------------------
// WRL
//------------------------------------------------------------
void ReadWrl(string filename)
{
    const int TileDimension = 64;
    const int PaletteLength = 768;

    try
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        var signature = br.ReadChars(3);
        if (String.Join("", signature) != "WRL")
        {
            Console.WriteLine("File does not appear a WRL file");
            return;
        }

        var version = br.ReadInt16();
        if (version != 1)
        {
            Console.WriteLine("File does not appear a supported WRL file version");
            return;
        }

        var x = br.ReadInt16();
        var y = br.ReadInt16();

        _ = br.ReadByte(); // 1 byte null marker to separate contents
        _ = br.ReadBytes(12544); // This seems a fixed size in all WRL files

        _ = br.ReadByte(); // 1 byte null marker to separate contents
        _ = br.ReadBytes(25085); // This seems a fixed size in all WRL files

        _ = br.ReadByte(); // 1 byte null marker to separate contents
        var tileCount = br.ReadInt16();

        var tileDataSize = tileCount * TileDimension * TileDimension;
        var tileDataBytes = br.ReadBytes(tileDataSize);

        var paletteBytes = br.ReadBytes(PaletteLength);
        File.WriteAllBytes(Path.Combine(@"D:\_\MAX\wrl", "palette.pal"), paletteBytes);

        var terrainInfo = br.ReadBytes(tileCount);
        /*
          0 - land
          1 - water
          2 - shoreline
          3 - mountain 
        */
    }
    catch
    {
        Console.WriteLine("An unexpected error occured");
    }
}


//------------------------------------------------------------
// GRAPHIC
//------------------------------------------------------------
void ReadGFX(string filename)
{
    try
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        var p = new byte[256 * 4];
        for (int i = 0; i < 256; i = i + 4)
        {
            p[i] = Convert.ToByte(i / 4);
            p[i + 1] = Convert.ToByte(i / 4);
            p[i + 2] = Convert.ToByte(i / 4);
            p[i + 3] = Convert.ToByte(i / 4);
        }

        using FileStream fs2 = new FileStream(Path.Combine(@"D:\_\MAX\out", filename), FileMode.Open, FileAccess.Read);
        using BinaryReader br2 = new BinaryReader(fs2);
        var width = br2.ReadInt16();
        var height = br2.ReadInt16();
        var hotspotX = br2.ReadInt16();
        var hotspotY = br2.ReadInt16();
        const int HeaderSize = 8;
        var content = br2.ReadBytes((int)br2.BaseStream.Length - HeaderSize);

        //TODO: We need a palette
        var allBytes = new List<byte>(width * height * 4);
        foreach (var c in content)
        {
            allBytes.Add(p[c]);
            allBytes.Add(p[c]);
            allBytes.Add(p[c]);
            allBytes.Add(p[c]);
        }

        var img = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(allBytes.ToArray(), 0));
        img.Save(@"D:\_\MAX\out\_out.bmp");
    }
    catch
    {
        Console.WriteLine("An unexpected error occured");
    }
}


[SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "Single file project")]
public struct FileInfo
{
    public string Name { get; set; }
    public int Offset { get; set; }
    public int Length { get; set; }
}