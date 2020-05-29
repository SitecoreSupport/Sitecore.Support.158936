using Sitecore.Data;
using Sitecore.Data.ItemResolvers;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using Sitecore.Web;
using System;
using System.Globalization;
using System.Web;

namespace Sitecore.Support.Resources.Media
{
  public class MediaRequest : Sitecore.Resources.Media.MediaRequest
  {
    private HttpRequest innerRequest;

    /// <summary>
    /// Media URI.
    /// </summary>
    private MediaUri mediaUri;

    /// <summary>
    /// The Media Options.
    /// </summary>
    private MediaOptions options;

    /// <summary>
    /// The Media Query String.
    /// </summary>
    private MediaUrlOptions mediaQueryString;

    /// <summary>
    /// Keeps information whether RawUrl is safe and does not contain any hacks.
    /// </summary>
    private bool isRawUrlSafe;

    /// <summary>
    /// The is raw url safe field initialized
    /// </summary>
    private bool isRawUrlSafeInitialized;

    /// <summary>
    /// Item path resolver.
    /// </summary>
    private ItemPathResolver pathResolver;


    public override MediaUri MediaUri
    {
      get
      {
        if (this.mediaUri == null)
        {
          string mediaPath = this.GetMediaPath();
          if (string.IsNullOrEmpty(mediaPath))
          {
            return null;
          }
          Database database = this.GetDatabase();
          if (database == null)
          {
            return null;
          }
          this.mediaUri = new MediaUri(mediaPath, this.GetLanguage(), this.GetVersion(), database);
        }
        return this.mediaUri;
      }
    }
    public override Sitecore.Resources.Media.MediaRequest Clone()
    {
      Assert.IsTrue(base.GetType() == typeof(MediaRequest), "The Clone() method must be overridden to support prototyping.");
      return new Sitecore.Support.Resources.Media.MediaRequest
      {
        innerRequest = this.innerRequest,
        mediaUri = this.mediaUri,
        options = this.options,
        mediaQueryString = this.mediaQueryString
      };
    }
    protected override string GetMediaPath()
    {
      string text = Context.RawUrl;
      if (WebUtil.Is404Request(text))
      {
        text = WebUtil.GetRequestUri404(text);
      }
      string localPath = WebUtil.GetLocalPath(text);
      return this.GetMediaPath(localPath);
    }
    protected override string GetMediaPath(string localPath)
    {
      int num = -1;
      string text = string.Empty;
# region Sitecore.Support.158936
      //string text2 = MainUtil.EncodeName(localPath)
      string text2 = localPath;
      // foreach (string current in MediaManager.Provider.Config.MediaPrefixes.Select(new Func<string, string>(MainUtil.DecodeName))
      foreach (string current in MediaManager.Config.MediaPrefixes) 
      {
        num = text2.IndexOf(current, StringComparison.InvariantCultureIgnoreCase);
        if (num >= 0)
        {
          text = current;
          break;
        }
      }
#endregion
      if (num < 0)
      {
        return string.Empty;
      }
      if (string.Compare(text2, num, text, 0, text.Length, true, CultureInfo.InvariantCulture) != 0)
      {
        return string.Empty;
      }
      string text3 = StringUtil.Divide(StringUtil.Mid(text2, num + text.Length), '.', true)[0];
      if (text3.EndsWith("/", StringComparison.InvariantCulture))
      {
        return string.Empty;
      }
      if (ShortID.IsShortID(text3))
      {
        return ShortID.Decode(text3);
      }
      string text4 = "/sitecore/media library/" + text3.TrimStart(new char[]
      {
        '/'
      });
      Database database = this.GetDatabase();
      if (database.GetItem(text4) == null)
      {
        Item item = database.GetItem("/sitecore/media library");
        if (item != null)
        {
          text3 = StringUtil.Divide(StringUtil.Mid(localPath, num + text.Length), '.', true)[0];
          Item item2 = this.PathResolver.ResolveItem(text3, item);
          if (item2 != null)
          {
            text4 = item2.Paths.Path;
          }
        }
      }
      return text4;
    }

  }
}
