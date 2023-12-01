Introduction
============
maxman is an tool to manipulate files from the 1996 RTS/TBS game Mechanized Assault & Exploration, developed by Interplay Studios.

maxman is currently in development with limited features and support, and only runs on Windows, however features include:
- Unpacking RES files
- Unpacking WLR files
- Converting graphics to bitmap

Download
========
Compiled downloads are not available during this early development phase.

Compiling
=========
To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/maxman

# Go into the repository
$ cd maxman

# Build  the app
$ dotnet build
```

Usage
=====
maxman is a command-line application and accepts a command line as below:

```maxman command options```

Where command is unpack 
```maxman unpack --file resfile --dir outputdirectory```

e.g. 
```maxman --file D:\data\max.res --dir D:\data\unpacked```

Where command is convert
```maxman convert --infile graphicfile --pal palettefile --outfile outputfile```

e.g. 
```maxman convert --infile D:\data\unpacked\imagefile --pal D:\data\unpacked\snow.pal --outfile D:\data\unpacked\imagefile.bmp```


Licencing
=========
maxman is licenced under CC BY-NC-ND 4.0 https://creativecommons.org/licenses/by-nc-nd/4.0/ Full licence details are available in licence.md

