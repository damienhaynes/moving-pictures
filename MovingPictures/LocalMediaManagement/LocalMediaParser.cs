using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DirectShowLib;
using DirectShowLib.Dvd;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement {
  public class MovieSignature {
    public string Title   = null; 
    public int?   Year    = null;
    public string ImdbId  = null; // ex. "tt0168122"
    public string Edition = null; // ex. unrated, director's cut etc.. Should maybe be an enum?
    public string Source  = null; // Original file/foldername
    public string DiscId = null; //  WMP/MC Disc Id (DirectShowLib)
    public string Path    = null; // Original path to the source of the media

    public override string ToString() {
      return String.Format("Path= {0} || Source= {1} || Title= \"{2}\" || Year= {3} || Edition= {4} || DiscId= {5} || ImdbId= {6} || ",
        this.Path, this.Source, this.Title, this.Year.ToString(), this.Edition, this.DiscId, this.ImdbId);
    }

  }

  public static class LocalMediaParser
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    public static MovieSignature parseMediaMatch(MovieMatch mm)
    {
      string sourceStr;
      MovieSignature signature = new MovieSignature();
      bool preferFolder = (bool)MovingPicturesCore.SettingsManager["importer_prefer_foldername"].Value;
      bool scanNFO = (bool)MovingPicturesCore.SettingsManager["importer_nfoscan"].Value;
      bool getDiscId = (bool)MovingPicturesCore.SettingsManager["importer_discid"].Value;
      bool useFolder = mm.FolderHint;
      bool isDvdFS = (mm.LocalMedia[0].File.Extension.ToLower() == ".ifo");
      bool isMultiPart = (mm.LocalMedia.Count > 1);
      Regex rxMultiPartFolder = new Regex(@"CD\d", RegexOptions.IgnoreCase);
      bool isMultiPartFolder = (!useFolder && isMultiPart && rxMultiPartFolder.Match(mm.LocalMedia[0].File.Directory.Name).Success);
    
      if (mm.LocalMedia == null || mm.LocalMedia.Count == 0) {
        // nothing to see here move along
        return signature;
      }
      else {        
        FileInfo file = mm.LocalMedia[0].File;
        DirectoryInfo folder = file.Directory;
       
        signature.Path = file.FullName;
        
        // If this is a DVD then calculate the discid and add it to the signature
        // @todo Only real DVD's? How to handle ISO's?
        if (isDvdFS && getDiscId) 
            signature.DiscId = GetDiscID(folder.FullName);

        if (useFolder || isDvdFS || isMultiPartFolder) {
          // If we have a folder hint or if we are looking at a DVD filesystem
          // we -can- use the foldername to create the searchstring
            if (isMultiPart || preferFolder || isDvdFS) {
            // If we meet one of the criteria for foldername parsing
            // let's do it but if we are in a DVD VIDEO_TS directory make 
            // sure we get the parent directory and alter the folder variable.
            // The same applies to the multi-part folder structure
                if ((isDvdFS && folder.Name.ToLower() == "video_ts") || isMultiPartFolder)
                  folder = folder.Parent;

            // prepare the string from the foldername
            sourceStr = parseFolderName(folder);
          }
          else {
            // If preferFolder is false we always use the filename for single part media
            sourceStr = parseFileName(file);
          }

          // Scan for NFO File if enabled
          // Adds IMDB ID to the MovieSignature if found
          if (scanNFO)
            signature.ImdbId = nfoScanner(folder);
        }
        // ## We can't use the foldername because it contains different movies 
        else {
          // if it's multi-part media use filename with stack marker cleaning
          if (isMultiPart)
            sourceStr = parseFileName(file, true);
          else
            sourceStr = parseFileName(file);

          // Scan for NFO File if enabled
          // Adds IMDB ID to the MovieSignature if found
          if (scanNFO)
            signature.ImdbId = nfoScanner(folder, sourceStr);
        }
      }

      // Complete and return the MovieSignature using the source string
      return parseSignature(sourceStr, signature);
    }

    public static MovieSignature parseSignature(string inputStr)
    {
      MovieSignature signature = new MovieSignature();
      return parseSignature(inputStr, signature);
    }

    // cleans a string up for movie name matching and returns/adds to a MovieSignature
    private static MovieSignature parseSignature(string inputStr, MovieSignature signature)
    {
      string sig = inputStr;
      int year;

      // Add uncleaned source string
      signature.Source = inputStr;

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
    public static string parseFileName(FileInfo file) {
      return parseFileName(file, false);
    }

    // Cleans up a filename for movie name matching. 
    // Removes extension and cleans stack markers.
    private static string parseFileName(FileInfo file, bool filterStackMatch) {
      // Remove the file extension from the filename
      string str = MovieImporter.RemoveFileExtension(file);

      // If specified also filter the stack markers from the remainder
      if (filterStackMatch) {
        // Remove stack markers using a regular expression
        Regex rxParser = new Regex(MovieImporter.rxMultiPartClean, RegexOptions.IgnoreCase);
        Match match = rxParser.Match(str);

        // if we have a match on this regexp we can just replace the matches.
        // otherwise we should just remove one character
        if (match.Success)
          str = rxParser.Replace(str, "");
        else
          str = str.Substring(0, (str.Length - 1));
      }

      // Return the cleaned filename to our caller
      return str;
    }

    // Cleans up a directory name for movie name matching. 
    public static string parseFolderName(DirectoryInfo dir) {
      return dir.Name;
    }

    // Filters "noise" from the input string
    public static string filterNoise(string input) {
      string rxPattern = MovingPicturesCore.SettingsManager["importer_filter"].Value.ToString();
      Regex rxParser = new Regex(rxPattern, RegexOptions.IgnoreCase);
      string rtn = rxParser.Replace(input, "");
      rtn = MovieImporter.rxReplaceDoubleSpace.Replace(rtn, " ");
      return rtn;
    }

    // Separates the year from the title string (if applicable)
    public static string filterYearFromTitle(string input, out int year) {
      string rtn = input;
      year = 0;

      // if there is a four digit number that looks like a year, parse it out
      Regex rxParser = new Regex(MovieImporter.rxYearScan);
      Match match = rxParser.Match(rtn);
      if (match.Success) {
        // set the year
        year = int.Parse(match.Groups[2].Value);
        // check if it's really a year value
        if (year > 1900 && year < DateTime.Now.Year + 2) {
          // clean the possible left overs from the title
          rtn = match.Groups[1].Value;
          rtn = rtn.TrimEnd('(', '[');
        }
        else {
          // year check failed so reset it to 0
          year = 0;
        }
      }

      // trim and return the title
      return rtn.Trim();
    }

    // NFO filescanner
    // Returns the full path of the first NFO file or empty
    public static string nfoScanner(DirectoryInfo dir) {
      return nfoScanner(dir, "*");
    }

    public static string nfoScanner(DirectoryInfo dir, string filename) {
      // TODO: optimize    
      string nfoExt = MovingPicturesCore.SettingsManager["importer_nfoext"].Value.ToString();
      Char[] splitters = new Char[] { ',', ';' };
      string[] extensions = nfoExt.Split(splitters);
      string[] mask = new string[extensions.Length];

      // combine the filename/mask
      // with the extension list to create
      // a list of files to look for
      for (int i = 0; i < extensions.Length; i++) {
        string ext = extensions[i].Trim();
        if (ext.Length > 1)
          mask[i] = filename + "." + ext;
      }

      // iterate through each pattern and get the corresponding files
      foreach (string pattern in mask) {
        // if pattern is null or empty continue to next pattern
        if (string.IsNullOrEmpty(pattern))
          continue;

        // Get all the files specfied by the current pattern from the directory
        FileInfo[] nfoList = dir.GetFiles(pattern.Trim());
        // If none continue to the next pattern
        if (nfoList.Length == 0)
          continue;

        // iterate through the list of files and scan them
        foreach (FileInfo file in nfoList) {
          // scan file and retrieve result
          string imdbid = parseNFO(file.FullName);
          // if a match is found return the imdb id
          if (imdbid != string.Empty)
            return imdbid;
        }

      }

      // we found nothing so return empty
      return string.Empty;
    }

    // Imdb ID scanner
    // Parses a NFO file and returns the IMDB ID (if any)
    public static string parseNFO(string file) {
      logger.Info("Scanning NFO file: {0}", file);

      // Read the nfo file content into a string
      string s = File.ReadAllText(file);
      // Check for the existance of a imdb id 
      Regex rxIMDB = new Regex(@"tt\d{7}", RegexOptions.IgnoreCase);
      Match match = rxIMDB.Match(s);

      // If success return the id, on failure return empty. 
      if (match.Success)
        s = match.Value;
      else
        s = string.Empty;

      // return the string
      return s;
    }

    // Get DiscID from DVD
    // api: http://movie.metaservices.microsoft.com/pas_movie_B/template/GetMDRDVDByCRC.xml?CRC="
    public static string GetDiscID(string path) {
        string rtn = null;
        try {
            IDvdInfo2 dvdInfo = (IDvdInfo2)new DVDNavigator();
            long id = 0;
            dvdInfo.GetDiscID(path, out id);
            rtn = Convert.ToString(id, 16); // HEX
        }
        catch (Exception e) {
            logger.Error("Error while retrieving disc id for: " + path, e);
        }

        return rtn;
    }

  }
}
