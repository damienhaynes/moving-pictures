using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement
{
  public struct MediaSignature
  {
    public string Title;
    public int Year;
    public string ImdbId;
    public string Edition;

    public MediaSignature(string title, int year, string imdb, string edition) 
    {
      this.Title = title;
      this.Year = year;
      this.ImdbId = imdb;
      this.Edition = edition;
    }

  }

  public static class LocalMediaParser
  {    
    public static MediaSignature parseMediaMatch (MediaMatch mm) {
      string sourceStr;
      MediaSignature signature = new MediaSignature();
      bool preferFolder = (bool)MovingPicturesCore.SettingsManager["importer_prefer_foldername"].Value;
      bool scanNFO = (bool)MovingPicturesCore.SettingsManager["importer_nfoscan"].Value;

      if (mm.LocalMedia == null || mm.LocalMedia.Count == 0)
        return signature;
      else 
        if (mm.FolderHint)
      { // ## If FolderHint is true we -can- use the foldername to create the searchstring
        if (mm.LocalMedia.Count > 1 || preferFolder)
        { // if it's multi-part media use the folder name
          // if the preferFolder value is true (one movie one folder) also use the folder name
          sourceStr = parseFolderName(mm.LocalMedia[0].File.Directory);
        }
        else
        {
          // If preferFolder is false we always use the filename for single part media
          sourceStr = parseFileName(mm.LocalMedia[0].File);
        }

        // Scan for NFO Files
        if (scanNFO)
        {
          string nfoPath = nfoScanner(mm.LocalMedia[0].File.Directory);
          if (nfoPath != string.Empty)
            signature.ImdbId = parseNFO(nfoPath);
        }
      }
      // ## We can't use the foldername because it contains different movies 
      else
      {
        if (mm.LocalMedia.Count > 1)
        { // if it's multi-part media  use filename with stack marker cleaning
          sourceStr = parseFileName(mm.LocalMedia[0].File, true);
        }
        else
        {
          // just use filename
          sourceStr = parseFileName(mm.LocalMedia[0].File);
        }
      }
      
      return parseSignature(sourceStr, signature);
    }
    
    public static MediaSignature parseSignature(string inputStr) {
      MediaSignature signature = new MediaSignature();
      return parseSignature(inputStr, signature);
    }

    // cleans a string up for movie name matching.
    private static MediaSignature parseSignature(string inputStr, MediaSignature signature)
    {
      string sig = inputStr;
      int year;

      // Phase #1: Spacing
      // if there are periods or underscores, assume the period is replacement for spaces.
      sig = sig.Replace('.', ' ');
      sig = sig.Replace('_', ' ');

      // Phase #2: Cleaning (remove noise)
      sig = filterNoise(sig);
      
      // Phase #3: Year detection
      signature.Title = filterYearFromTitle(sig, out year);
      signature.Year = year;
      return signature;
    }

    // Cleans up a filename (without stack markers)
    public static string parseFileName(FileInfo file)
    {
      return parseFileName(file, false);
    }

    // Cleans up a filename for movie name matching. 
    // Removes extension and cleans stack markers.
    private static string parseFileName(FileInfo file, bool filterStackMatch)
    {
      string str = MovieImporter.RemoveFileExtension(file);

      if (filterStackMatch)
      {
        // Remove stack markers
        Regex rxParser = new Regex(MovieImporter.rxMultiPartClean, RegexOptions.IgnoreCase);
        Match match = rxParser.Match(str);
        if (match.Success)
        {
          // if we have a match on this regexp we can just replace the matches.
          str = rxParser.Replace(str, "");
        }
        else
        {
          // if we don't have match we should remove just one character.
          str = str.Substring(0, (str.Length - 1));
        }
      }

      return str;
    }

    // Cleans up a directory name for movie name matching. 
    public static string parseFolderName(DirectoryInfo dir)
    {
      return dir.Name;
    }
    
    // Filters "noise" from the input string
    public static string filterNoise(string input)
    {
      string rxPattern = MovingPicturesCore.SettingsManager["importer_filter"].Value.ToString();
      Regex rxParser = new Regex(rxPattern, RegexOptions.IgnoreCase);
      return rxParser.Replace(input, "");
    }
    
    // Separates the year from the title string (if applicable)
    public static string filterYearFromTitle(string input, out int year)
    {
      string rtn = input;
      year = 0;

      // if there is a four digit number that looks like a year, parse it out
      Regex rxParser = new Regex(MovieImporter.rxYearScan);
      Match match = rxParser.Match(rtn);
      if (match.Success)
      {
        // set the year
        year = int.Parse(match.Groups[2].Value);
        // check if it's really a year value
        if (year > 1900 && year < DateTime.Now.Year + 2)
        {
          // clean the possible left overs from the title
          rtn = match.Groups[1].Value.TrimEnd('(', '[').Trim();
        } else {
          // year check failed so reset it to 0
          year = 0;
        }
      }

      // return the title
      return rtn;
    }

    // NFO filescanner
    // Returns the full path of the first NFO file or empty
    public static string nfoScanner(DirectoryInfo dir)
    {
      // Get all the nfo files from a directory
      FileInfo[] nfoList = dir.GetFiles("*.nfo");
      // If none found just return empty
      if (nfoList.Length == 0)
        return string.Empty;
      
      // if we found nfo files just return the path of 
      // the first one, this should be tuned in the future
      return nfoList[0].FullName;
    }

    // Imdb ID scanner
    // Parses a NFO file and returns the IMDB ID (if any)
    public static string parseNFO(string path)
    {
      // Read the nfo file content into a string
      string s = File.ReadAllText(path);
      // Check for the existance of a imdb id 
      Regex rxIMDB = new Regex("tt[0-9]{7}", RegexOptions.IgnoreCase);
      Match match = rxIMDB.Match(s);
      // If success return the id, on failure return empty. 
      if (match.Success)
      {
        s = match.Value;
        //logger.Info("Found imdb id ({0}) in file: {1}", s, path);
      }
      else
      {
        s = string.Empty;
      }

      return s;
    }

  }
}
