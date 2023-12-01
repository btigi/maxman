/*
maxman convert graphics_file palette_file output_directory

maxman unpack res_file output_directory
maxman unpack wrl_file output_directory

maxman pack res_file output_directory
maxman pack wrl_file output_directory
*/

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

//foreach (var f in Directory.GetFiles(@"D:\_\MAX\out"))
//{
//    try
//    {
//        ConvertFile(f, @"D:\_\MAX\SNOW_6\pallete.dat", Path.Combine(@"D:\_\MAX\gfx", Path.GetFileName(f) + ".bmp"));
//    }
//    catch { };
//}

var rootCommand = new RootCommand();


var unpackFileOption = new Option<string>(
           name: "--file",
           description: "The RES/WRL file to unpack.");
unpackFileOption.IsRequired = true;

var unpackDirectoryOption = new Option<string>(
           name: "--dir",
           description: "The directory to unpack to.");
unpackDirectoryOption.IsRequired = true;

var unpackCommand = new Command("unpack", "Unpack a RES/WRL file.")
    {
        unpackFileOption,
        unpackDirectoryOption
    };
rootCommand.AddCommand(unpackCommand);

unpackCommand.SetHandler(async (file, directory) =>
{
    await Unpack(file, directory);
}, unpackFileOption, unpackDirectoryOption);


var convertFileOption = new Option<string>(
           name: "--infile",
           description: "The file to convert.");
convertFileOption.IsRequired = true;

var convertPaletteFileOption = new Option<string>(
           name: "--pal",
           description: "The palette to use.");
convertPaletteFileOption.IsRequired = true;

var convertOutputFileOption = new Option<string>(
           name: "--outfile",
           description: "The file to output.");
convertOutputFileOption.IsRequired = true;

var convertCommand = new Command("convert", "Convert a graphics file to a bitmap file.")
    {
        convertFileOption,
        convertPaletteFileOption,
        convertOutputFileOption
    };
rootCommand.AddCommand(convertCommand);

convertCommand.SetHandler(ConvertFile, convertFileOption, convertPaletteFileOption, convertOutputFileOption);


return await rootCommand.InvokeAsync(args);


async Task<int> Unpack(string filename, string outputDirectory)
{
    Console.WriteLine($"Unpack {filename} to {outputDirectory}");

    if (!File.Exists(filename))
    {
        Console.WriteLine("File does not exist");
        return await Task.FromResult(0);
    }

    var fileType = Path.GetExtension(filename);
    switch (fileType?.ToUpper())
    {
        case ".RES":
            UnpackRes(filename, outputDirectory);
            break;
        case ".WRL":
            UnpackWrl(filename, outputDirectory);
            break;
        default:
            Console.WriteLine("Unknown file type");
            break;
    }

    return await Task.FromResult(0);
}

async Task<int> ConvertFile(string inputFile, string palette, string outputFile)
{
    Console.WriteLine($"Convert {inputFile}");

    if (!File.Exists(inputFile))
    {
        Console.WriteLine("Graphic file does not exist");
        return await Task.FromResult(0);
    }

    if (!File.Exists(palette))
    {
        Console.WriteLine("Palette file does not exist");
        return await Task.FromResult(0);
    }

    ConvertGFX(inputFile, palette, outputFile);

    return await Task.FromResult(0);
}


//------------------------------------------------------------
// RES
//------------------------------------------------------------
void UnpackRes(string resFilename, string outputDirectory)
{
    const int FilenameLength = 8;

    try
    {
        using var fs = new FileStream(resFilename, FileMode.Open, FileAccess.Read);
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
            File.WriteAllBytes(Path.Combine(outputDirectory, fileInfo.Name), bytes);
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
void UnpackWrl(string filename, string outputDirectory)
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
        File.WriteAllBytes(Path.Combine(outputDirectory, "palette.pal"), paletteBytes);

        var terrainInfo = br.ReadBytes(tileCount);
        //  0 - land
        //  1 - water
        //  2 - shoreline
        //  3 - mountain         
    }
    catch
    {
        Console.WriteLine("An unexpected error occured");
    }
}


//------------------------------------------------------------
// GRAPHIC
//------------------------------------------------------------
void ConvertGFX(string inputFile, string palette, string outputFile)
{
    const int PaletteLength = 768;

    try
    {
        using var paletteFs = new FileStream(palette, FileMode.Open, FileAccess.Read);
        using var paletteBr = new BinaryReader(paletteFs);
        var paletteBytes = paletteBr.ReadBytes(PaletteLength);


        using var fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        // Greyscale WIP
        //for (int i = 0; i < 256; i = i + 3)
        //{
        //    paletteBytes[i] = Convert.ToByte(i / 3);
        //    paletteBytes[i + 1] = Convert.ToByte(i / 3);
        //    paletteBytes[i + 2] = Convert.ToByte(i / 3);
        //}

        using FileStream fs2 = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
        using BinaryReader br2 = new BinaryReader(fs2);
        var width = br2.ReadInt16();
        var height = br2.ReadInt16();
        _ = br2.ReadInt16(); // hotspotX
        _ = br2.ReadInt16(); // hotspotY
        const int HeaderSize = 8;
        var content = br2.ReadBytes((int)br2.BaseStream.Length - HeaderSize);

        var allBytes = new List<byte>(width * height * 4);
        foreach (var c in content)
        {
            allBytes.Add(paletteBytes[c + 2]); // b
            allBytes.Add(paletteBytes[c + 1]); // g
            allBytes.Add(paletteBytes[c]);     // r
            allBytes.Add(255);
        }

        if (content.Length != width * height)
        {
            Console.WriteLine("Skipped file (not a graphic file)");
            return;
        }

        var img = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(allBytes.ToArray(), 0));
        img.Save(outputFile);
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