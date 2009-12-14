using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using MediaPortal.GUI.Library;
using MediaPortal.Player;
using System.IO;
using NLog;

#region API

# endregion

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement
{
    /// <summary>
    /// This class is here to temporarily replace the MediaInfoWrapper included with MediaPortal
    /// Once MP has the new changes, this file can be deleted.
    /// </summary>
  public class MediaInfoWrapper
  {
    #region private vars

    private static Logger logger = LogManager.GetCurrentClassLogger();

    private MediaInfo _mI = null;

    private double _framerate = 0;
    private int _width = 0;
    private int _height = 0;
    private int _audioRate = 0;
    private int _audioChannels = 0;
    private int _numSubtitles = 0;
    private string _aspectRatio = "";
    private string _videoCodec = string.Empty;
    private string _audioCodec = string.Empty;
    private string _scanType = string.Empty;
    private bool _isDIVX = false; // mpeg4 DivX
    private bool _isXVID = false; // mpeg4 asp
    private bool _isH264 = false; // mpeg4 avc h264/x264
    private bool _isMP1V = false; // mpeg1 video (VCD)
    private bool _isMP2V = false; // mpeg2 video
    private bool _isMP4V = false; // mpeg4 generic
    private bool _isWMV = false;  // WMV 7-9
    private bool _is720P = false; // is 1280x720 video
    private bool _is1080P = false; // is 1980x1080 video, progressive
    private bool _is1080I = false; // is 1920x1080 video, interlaced
    private bool _isInterlaced = false; // is interlaced
    private bool _isHDTV = false; // is HDTV resolution
    private bool _isSDTV = false; // is SDTV resolution
    private bool _isAC3 = false;  // AC3
    private bool _isMP3 = false;  // MPEG-1 Audio layer 3
    private bool _isMP2A = false; // MPEG-1 Audio layer 2
    private bool _isDTS = false;  // DTS
    private bool _isOGG = false;  // OGG VORBIS
    private bool _isAAC = false;  // AAC
    private bool _isWMA = false;  // Windows Media Audio
    private bool _isPCM = false;  // RAW audio
    private int _duration = 0;

    private bool _hasSubtitles = false;
    private static List<string> _subTitleExtensions = new List<string>();

    #endregion

    #region ctor's

    public MediaInfoWrapper(string strFile)
    {

      bool isTV = Util.Utils.IsLiveTv(strFile);
      bool isRadio = Util.Utils.IsLiveRadio(strFile);
      bool isDVD = Util.Utils.IsDVD(strFile);
      bool isVideo = Util.Utils.IsVideo(strFile);
      bool isAVStream = Util.Utils.IsAVStream(strFile); //rtsp users for live TV and recordings.

      if (isTV || isRadio || isAVStream)
      {
        return;
      }

      //currently mediainfo is only used for video related material
      if (!isDVD && !isVideo)
      {
        return;
      }

      try
      {
        _mI = new MediaInfo();
        _mI.Open(strFile);

        NumberFormatInfo providerNumber = new NumberFormatInfo();
        providerNumber.NumberDecimalSeparator = ".";

        double.TryParse(_mI.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, providerNumber, out _framerate);
        _videoCodec = _mI.Get(StreamKind.Video, 0, "Codec").ToLower();
        _scanType = _mI.Get(StreamKind.Video, 0, "ScanType").ToLower();
        int.TryParse(_mI.Get(StreamKind.Video, 0, "Width"), out _width);
        int.TryParse(_mI.Get(StreamKind.Video, 0, "Height"), out _height);
        int.TryParse(_mI.Get(StreamKind.General, 0, "TextCount"), out _numSubtitles);
        int intValue;
        int iAudioStreams = _mI.Count_Get(StreamKind.Audio);
        for (int i = 0; i < iAudioStreams; i++)
        {
            if (int.TryParse(_mI.Get(StreamKind.Audio, i, "Channel(s)"), out intValue)
                && intValue > _audioChannels)
            {
                _audioChannels = intValue;
                int.TryParse(_mI.Get(StreamKind.Audio, i, "SamplingRate"), out _audioRate);
                _audioCodec = _mI.Get(StreamKind.Audio, i, "Codec/String").ToLower();
            }
        }

        string aspectStr = _mI.Get(StreamKind.Video, 0, "AspectRatio/String");
        if (aspectStr == "4/3" || aspectStr == "4:3")
            _aspectRatio = "fullscreen";
        else
            _aspectRatio = "widescreen";

        if (strFile.ToLower().EndsWith(".ifo") && !DeviceManager.IsOpticalDrive(strFile)) {
            // mediainfo is not able to obtain duration of IFO files
            // so we use this to loop through all corresponding VOBs and add up the duration
            // we do not do this for optical drives because there are issues with some discs
            // taking more than 2 minutes(copy protection?)
            _duration = 0;
            string filePrefix = Path.GetFileName(strFile);
            filePrefix = filePrefix.Substring(0, filePrefix.LastIndexOf('_'));
            MediaInfo mi = new MediaInfo();
            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(strFile), filePrefix + "*.VOB")) {
                mi.Open(file);
                int durationPart = 0;
                int.TryParse(_mI.Get(StreamKind.Video, 0, "PlayTime"), out durationPart);
                _duration += durationPart;
            }
        }
        else {
            int.TryParse(_mI.Get(StreamKind.Video, 0, "PlayTime"), out _duration);
        }

        _isInterlaced = (_scanType.IndexOf("interlaced") > -1);

        if (_height >= 720)
        {
          _isHDTV = true;
        }
        else
        {
          _isSDTV = true;
        }

        if ((_width == 1280 || _height == 720) && !_isInterlaced)
        {
          _is720P = true;
        }

        if ((_width == 1920 || _height == 1080) && !_isInterlaced)
        {
          _is1080P = true;
        }

        if ((_width == 1920 || _height == 1080) && _isInterlaced)
        {
          _is1080I = true;
        }

        _isDIVX = (_videoCodec.IndexOf("dx50") > -1) | (_videoCodec.IndexOf("div3") > -1); // DivX 5 and DivX 3
        _isXVID = (_videoCodec.IndexOf("xvid") > -1);
        _isH264 = (_videoCodec.IndexOf("avc") > -1 || _videoCodec.IndexOf("h264") > -1);
        _isMP1V = (_videoCodec.IndexOf("mpeg-1v") > -1);
        _isMP2V = (_videoCodec.IndexOf("mpeg-2v") > -1);
        _isMP4V = (_videoCodec.IndexOf("fmp4") > -1); // add more
        _isWMV = (_videoCodec.IndexOf("wmv") > -1); // wmv3 = WMV9
        // missing cvid etc
        _isAC3 = (System.Text.RegularExpressions.Regex.IsMatch(_audioCodec, "ac-?3"));
        _isMP3 = (_audioCodec.IndexOf("mpeg-1 audio layer 3") > -1) || (_audioCodec.IndexOf("mpeg-2 audio layer 3") > -1);
        _isMP2A = (_audioCodec.IndexOf("mpeg-1 audio layer 2") > -1);
        _isDTS = (_audioCodec.IndexOf("dts") > -1);
        _isOGG = (_audioCodec.IndexOf("ogg") > -1);
        _isAAC = (_audioCodec.IndexOf("aac") > -1);
        _isWMA = (_audioCodec.IndexOf("wma") > -1); // e.g. wma3
        _isPCM = (_audioCodec.IndexOf("pcm") > -1);

        if (checkHasExternalSubtitles(strFile))
        {
            _hasSubtitles = true;
        } 
        else if (_numSubtitles > 0)
        {
            _hasSubtitles = true;
        }
        else
        {
            _hasSubtitles = false;
        }

        logger.Debug("MediaInfoWrapper: inspecting media : {0}", strFile);
        logger.Debug("MediaInfoWrapper: FrameRate : {0}", _framerate);
        logger.Debug("MediaInfoWrapper: VideoCodec : {0}", _videoCodec);
        if (_isDIVX)
          logger.Debug("MediaInfoWrapper: IsDIVX: {0}", _isDIVX);
        if (_isXVID)
          logger.Debug("MediaInfoWrapper: IsXVID: {0}", _isXVID);
        if (_isH264)
          logger.Debug("MediaInfoWrapper: IsH264: {0}", _isH264);
        if (_isMP1V)
          logger.Debug("MediaInfoWrapper: IsMP1V: {0}", _isMP1V);
        if (_isMP2V)
          logger.Debug("MediaInfoWrapper: IsMP2V: {0}", _isMP2V);
        if (_isMP4V)
          logger.Debug("MediaInfoWrapper: IsMP4V: {0}", _isMP4V);
        if (_isWMV)
          logger.Debug("MediaInfoWrapper: IsWMV: {0}", _isWMV);

        logger.Debug("MediaInfoWrapper: HasSubtitles : {0}", _hasSubtitles);
        logger.Debug("MediaInfoWrapper: NumSubtitles : {0}", _numSubtitles);
        logger.Debug("MediaInfoWrapper: Scan type : {0}", _scanType);
        logger.Debug("MediaInfoWrapper: IsInterlaced: {0}", _isInterlaced);
        logger.Debug("MediaInfoWrapper: Width : {0}", _width);
        logger.Debug("MediaInfoWrapper: Height : {0}", _height);
        logger.Debug("MediaInfoWrapper: Audiochannels : {0}", _audioChannels);
        logger.Debug("MediaInfoWrapper: Audiorate : {0}", _audioRate);
        logger.Debug("MediaInfoWrapper: AspectRatio : {0}", _aspectRatio);
        logger.Debug("MediaInfoWrapper: AudioCodec : {0}", _audioCodec);
        if (_isAC3)
          logger.Debug("MediaInfoWrapper: IsAC3 : {0}", _isAC3);
        if (_isMP3)
          logger.Debug("MediaInfoWrapper: IsMP3 : {0}", _isMP3);
        if (_isMP2A)
          logger.Debug("MediaInfoWrapper: IsMP2A: {0}", _isMP2A);
        if (_isDTS)
          logger.Debug("MediaInfoWrapper: IsDTS : {0}", _isDTS);
        if (_isOGG)
          logger.Debug("MediaInfoWrapper: IsOGG : {0}", _isOGG);
        if (_isAAC)
          logger.Debug("MediaInfoWrapper: IsAAC : {0}", _isAAC);
        if (_isWMA)
          logger.Debug("MediaInfoWrapper: IsWMA: {0}", _isWMA);
        if (_isPCM)
          logger.Debug("MediaInfoWrapper: IsPCM: {0}", _isPCM);
      }
      catch (Exception ex)
      {
        logger.Error("MediaInfo processing failed ('MediaInfo.dll' may be missing): {0}", ex.Message);
      }
      finally
      {
        if (_mI != null)
        {
          _mI.Close();
        }
      }
    }

    #endregion

    #region private methods

    private bool checkHasExternalSubtitles(string strFile)
    {
      if (_subTitleExtensions.Count == 0)
      {
        // load them in first time
        _subTitleExtensions.Add(".aqt");
        _subTitleExtensions.Add(".asc");
        _subTitleExtensions.Add(".ass");
        _subTitleExtensions.Add(".dat");
        _subTitleExtensions.Add(".dks");
        _subTitleExtensions.Add(".js");
        _subTitleExtensions.Add(".jss");
        _subTitleExtensions.Add(".lrc");
        _subTitleExtensions.Add(".mpl");
        _subTitleExtensions.Add(".ovr");
        _subTitleExtensions.Add(".pan");
        _subTitleExtensions.Add(".pjs");
        _subTitleExtensions.Add(".psb");
        _subTitleExtensions.Add(".rt");
        _subTitleExtensions.Add(".rtf");
        _subTitleExtensions.Add(".s2k");
        _subTitleExtensions.Add(".sbt");
        _subTitleExtensions.Add(".scr");
        _subTitleExtensions.Add(".smi");
        _subTitleExtensions.Add(".son");
        _subTitleExtensions.Add(".srt");
        _subTitleExtensions.Add(".ssa");
        _subTitleExtensions.Add(".sst");
        _subTitleExtensions.Add(".ssts");
        _subTitleExtensions.Add(".stl");
        _subTitleExtensions.Add(".sub");
        _subTitleExtensions.Add(".txt");
        _subTitleExtensions.Add(".vkt");
        _subTitleExtensions.Add(".vsf");
        _subTitleExtensions.Add(".zeg");

      }
      string filenameNoExt = System.IO.Path.GetFileNameWithoutExtension(strFile);
      try
      {
        foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(strFile), filenameNoExt + "*"))
        {
          System.IO.FileInfo fi = new System.IO.FileInfo(file);
          if (_subTitleExtensions.Contains(fi.Extension.ToLower())) return true;
        }
      }
      catch (Exception)
      {
        // most likley path not available
      }

      return false;
    }

    #endregion

    #region public video related properties

    public string AspectRatio
    {
      get { return _aspectRatio; }
    }

    public string VideoCodec
    {
      get { 
          string tempCodec = String.Empty;
          if (_isDIVX)
              tempCodec = "DIVX";
          else if (_isXVID)
              tempCodec = "XVID";
          else if (_isH264)
              tempCodec = "H264";
          else if (_isMP1V)
              tempCodec = "MP1V";
          else if (_isMP2V)
              tempCodec = "MP2V";
          else if (_isWMV)
              tempCodec = "WMV";
          else
              tempCodec = _videoCodec;

          return tempCodec; 
      }
    }

    public string VideoResolution
    {
        get
        {
            if (Is1080P)
                return "1080p";
            else if (Is1080I)
                return "1080i";
            else if (Is720P)
                return "720p";
            else if (IsHDTV)
                return "HD";
            else
                return "SD";
        }
    }

    public double Framerate
    {
      get { return _framerate; }
    }

    public bool IsDIVX
    {
      get { return _isDIVX; }
    }

    public bool IsXVID
    {
      get { return _isXVID; }
    }

    public bool IsH264
    {
      get { return _isH264; }
    }

    public bool IsMP1V
    {
      get { return _isMP1V; }
    }

    public bool IsMP2V
    {
      get { return _isMP2V; }
    }

    public bool IsMP4V
    {
      get { return _isMP4V; }
    }

    public bool IsWMV
    {
      get { return _isWMV; }
    }

    public bool Is720P
    {
      get { return _is720P; }
    }

    public bool Is1080P
    {
      get { return _is1080P; }
    }

    public bool Is1080I
    {
      get { return _is1080I; }
    }

    public bool IsHDTV
    {
      get { return _isHDTV; }
    }

    public bool IsSDTV
    {
      get { return _isSDTV; }
    }

    public bool IsInterlaced
    {
      get { return _isInterlaced; }
    }

    public int Width
    {
      get { return _width; }
    }

    public int Height
    {
      get { return _height; }
    }

    #endregion

    #region public audio related properties

    public string AudioCodec
    {
      get {
          string tempCodec = String.Empty;
          if (_isAC3)
              tempCodec = "AC3";
          else if (_isMP3)
              tempCodec = "MP3";
          else if (_isMP2A)
              tempCodec = "MP2A";
          else if (_isDTS)
              tempCodec = "DTS";
          else if (_isOGG)
              tempCodec = "OGG";
          else if (_isAAC)
              tempCodec = "AAC";
          else if (_isWMA)
              tempCodec = "WMA";
          else if (_isPCM)
              tempCodec = "PCM";
          else
              tempCodec = _audioCodec;
          return tempCodec; 
      }
    }

    public int AudioRate
    {
      get { return _audioRate; }
    }

    public int AudioChannels
    {
      get { return _audioChannels; }
    }

    public string AudioChannelsFriendly
    {
        get
        {
            switch (AudioChannels)
            {
                case 8:
                    return "7.1";
                case 6:
                    return "5.1";
                case 2:
                    return "stereo";
                case 1:
                    return "mono";
                default:
                    return this.AudioChannels.ToString();
            }
        }
    }

    public bool IsAC3
    {
      get { return _isAC3; }
    }

    public bool IsMP3
    {
      get { return _isMP3; }
    }

    public bool IsMP2A
    {
      get { return _isMP2A; }
    }

    public bool IsWMA
    {
      get { return _isWMA; }
    }

    public bool IsPCM
    {
      get { return _isPCM; }
    }

    public bool IsDTS
    {
      get { return _isDTS; }
    }

    public bool IsOGG
    {
      get { return _isOGG; }
    }

    public bool IsAAC
    {
      get { return _isAAC; }
    }

    #endregion

    #region public misc properties

    public bool HasSubtitles
    {
      get { return _hasSubtitles; }
    }

    public int NumSubtitles
    {
        get { return _numSubtitles; }
    }

    public int Duration
    {
        get { return _duration; }
    }

    #endregion
  }
}
