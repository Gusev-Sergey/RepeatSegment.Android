using System.Net;
using SkiaSharp;

namespace RepeatSegment.Maui.Pages;

public partial class ImageSearchPage : ContentPage
{
    private readonly string _provider;
    private TaskCompletionSource<string?> _tcs = new();

    public Task<string?> Result => _tcs.Task;

    public ImageSearchPage(string query, string provider = "google")
    {
        InitializeComponent();
        _provider = provider == "yandex" ? "yandex" : "google";
        TxtQuery.Text = query;
        Loaded += OnLoaded;
    }

    private string YandexPopupKillerJs()
    {
        return @"
(function(){
var s=document.createElement('style');
s.textContent='*[class*=""cam-banner""],*[class*=""popup2""],*[class*=""promo-banner""],*[class*=""app-install""],*[class*=""distribution""],*[class*=""mini-suggest""]{display:none!important;visibility:hidden!important;width:0!important;height:0!important;overflow:hidden!important;opacity:0!important;pointer-events:none!important;position:absolute!important;z-index:-1!important;}';
document.head.appendChild(s);
new MutationObserver(function(ms){
  ms.forEach(function(m){
    m.addedNodes.forEach(function(n){
      if(n.nodeType===1){
        var c=(n.className||'').toString();
        if(c.indexOf('popup')>=0||c.indexOf('banner')>=0||c.indexOf('cam')>=0||c.indexOf('promo')>=0){n.remove();return;}
        var btns=n.querySelectorAll('[class*=""close""],[class*=""Close""],[class*=""cross""],[class*=""Cross""]');
        for(var i=0;i<btns.length;i++){try{btns[i].click();}catch(e){}}
      }
    });
  });
}).observe(document.body||document.documentElement,{childList:true,subtree:true});
setInterval(function(){
  var bad=document.querySelectorAll('*[class*=""popup2__overlay""],*[class*=""Overlay""],*[class*=""overlay""]');
  for(var i=0;i<bad.length;i++){
    var c=(bad[i].className||'').toString().toLowerCase();
    if(c.indexOf('popup')>=0||c.indexOf('banner')>=0||c.indexOf('cam')>=0){bad[i].remove();}
  }
  var crosses=document.querySelectorAll('*[aria-label*=""close""],*[aria-label*=""Close""],*[aria-label*=""\u0437\u0430\u043A\u0440\u044B\u0442\u044C""]');
  for(var j=0;j<crosses.length;j++){try{crosses[j].click();crosses[j].remove();}catch(e){}}
},600);
})();";
    }

    private string VisualFeedbackJs()
    {
        return @"
(function(){
window.__lastSelImg=null;
window.__markSelected=function(img){
  if(window.__lastSelImg&&window.__lastSelImg!==img){
    window.__lastSelImg.style.outline='';
    window.__lastSelImg.style.outlineOffset='';
  }
  if(!img)return;
  img.style.outline='4px solid #00B4A6';
  img.style.outlineOffset='2px';
  img.style.borderRadius='2px';
  window.__lastSelImg=img;
  var st=document.getElementById('__rsStatus');
  if(!st){
    st=document.createElement('div');
    st.id='__rsStatus';
    st.style.cssText='position:fixed;bottom:16px;left:50%;transform:translateX(-50%);background:#1E3A5F;color:#00B4A6;padding:10px 24px;border-radius:24px;font-size:15px;font-weight:bold;z-index:999999;pointer-events:none;box-shadow:0 4px 12px rgba(0,0,0,0.4);';
    document.body.appendChild(st);
  }
  st.textContent='\u2713 Image selected \u2014 tap \u2713 Use';
  st.style.display='block';
  clearTimeout(window.__statusTimer);
  window.__statusTimer=setTimeout(function(){if(st)st.style.display='none';},3000);
};
})();";
    }

    private void OnLoaded(object? s, EventArgs e)
    {
        string q = Uri.EscapeDataString(TxtQuery.Text?.Trim() ?? "");
        if (string.IsNullOrWhiteSpace(q)) q = "language+learning";

        string url = _provider == "google"
            ? "https://www.google.com/search?tbm=isch&q=" + q
            : "https://yandex.ru/images/search?text=" + q;

        ImageBrowser.Navigated += OnPageNavigated;
        ImageBrowser.Source = new UrlWebViewSource { Url = url };

#if ANDROID
        try
        {
            if (ImageBrowser.Handler?.PlatformView is Android.Webkit.WebView wv)
            {
                wv.Settings.UserAgentString =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";
                wv.Settings.JavaScriptEnabled = true;
                wv.Settings.DomStorageEnabled = true;
                if (_provider == "google")
                {
                    var cm = Android.Webkit.CookieManager.Instance;
                    cm?.SetAcceptCookie(true);
                    cm?.SetCookie(".google.com", "CONSENT=YES+cb");
                    cm?.SetCookie(".google.com", "NID=511");
                    cm?.Flush();
                }
            }
        }
        catch { }
#endif
    }

    private void OnPageNavigated(object? s, WebNavigatedEventArgs e)
    {
        InjectEverything();
    }

    private async void InjectEverything()
    {
        try
        {
            // Anti-bot
            _ = ImageBrowser.EvaluateJavaScriptAsync(
                "Object.defineProperty(navigator,'webdriver',{get:()=>false});window.chrome={runtime:{}};");

            // Yandex popup killer (always inject, harmless on Google)
            if (_provider == "yandex")
            {
                _ = ImageBrowser.EvaluateJavaScriptAsync(YandexPopupKillerJs());
            }

            // Visual feedback — always
            _ = ImageBrowser.EvaluateJavaScriptAsync(VisualFeedbackJs());

            // Click handler
            _ = ImageBrowser.EvaluateJavaScriptAsync(ClickHandlerJs());
        }
        catch { }
    }

    private string ClickHandlerJs()
    {
        string isG = _provider == "google" ? "true" : "false";
        return @"if(!window.___rsTrack){window.___rsTrack=1;var isG=" + isG + @";
if(isG){
  document.addEventListener('click',function(e){
    var el=e.target;
    while(el&&el!==document.body){
      if(el.tagName==='A'&&el.href&&el.href.indexOf('/imgres?')>=0){
        var m=el.href.match(/[?&]imgurl=([^&]+)/i);
        if(m){window.__img=decodeURIComponent(m[1]);var im=el.querySelector('img');if(im)window.__markSelected(im);return;}
      }
      if(el.hasAttribute&&el.hasAttribute('data-ou')){
        window.__img=el.getAttribute('data-ou');if(el.tagName==='IMG')window.__markSelected(el);return;
      }
      el=el.parentElement;
    }
  },true);
  window.__findBestImg=function(){
    var imgs=document.querySelectorAll('img[src^=""http""],img[src^=""/""],img[src^=""data""]');
    var best=null,bestW=0;
    for(var i=0;i<imgs.length;i++){
      var s=imgs[i].src||imgs[i].getAttribute('data-src')||'';
      if(!s||s.indexOf('data:')===0||s.indexOf('gstatic')>=0||s.indexOf('encrypted')>=0||s.indexOf('/favicon')>=0)continue;
      if(s.indexOf('/')===0)s=location.origin+s;
      var w=imgs[i].naturalWidth||imgs[i].width||imgs[i].clientWidth||0;
      if(w>bestW&&w>150){bestW=w;best=s;}
    }
    return best||window.__img||'';
  };
}else{
  document.addEventListener('click',function(e){
    function isPopup(el){var c=(el.className||'').toString().toLowerCase();return c.indexOf('close')>=0||c.indexOf('cross')>=0||c.indexOf('dismiss')>=0||c.indexOf('popup')>=0||c.indexOf('cam')>=0||c.indexOf('banner')>=0||c.indexOf('promo')>=0||c.indexOf('overlay')>=0||c.indexOf('smart')>=0||c.indexOf('install')>=0;}
    var s2=e.target;while(s2&&s2!==document.body){if(isPopup(s2))return;s2=s2.parentElement;}
    function ok(s){return s&&typeof s==='string'&&s.startsWith('http')&&s.indexOf('data:')===-1;}
    function f(el){if(!el)return null;var s=el.src||el.getAttribute('data-src')||el.getAttribute('src');if(ok(s))return s;var im=el.tagName==='PICTURE'?el.querySelector('img'):null;if(im){s=im.src||im.getAttribute('data-src')||im.getAttribute('src');if(ok(s))return s;}if(el.querySelector){var q=el.querySelector('img');if(q){s=q.src||q.getAttribute('data-src')||q.getAttribute('src');if(ok(s))return s;}}return null;}
    var els=document.elementsFromPoint(e.clientX,e.clientY);
    for(var j=0;j<els.length;j++){
      var s=f(els[j]);
      if(s){
        window.__img=s;e.preventDefault();e.stopPropagation();
        var t=els[j];if(t.tagName!=='IMG'){t=t.querySelector('img')||t;}window.__markSelected(t);return;
      }
      var p=els[j].parentElement;
      while(p){
        s=f(p);
        if(s){
          window.__img=s;e.preventDefault();e.stopPropagation();
          var t2=p;if(t2.tagName!=='IMG'){t2=t2.querySelector('img')||t2;}window.__markSelected(t2);return;
        }
        p=p.parentElement;
      }
    }
  },true);
}}";
    }

    private async void OnSearchClicked(object? s, EventArgs e)
    {
        string q = Uri.EscapeDataString(TxtQuery.Text?.Trim() ?? "");
        if (string.IsNullOrWhiteSpace(q)) return;

        string url = _provider == "google"
            ? $"https://www.google.com/search?tbm=isch&q={q}"
            : $"https://yandex.ru/images/search?text={q}";

        ImageBrowser.Source = new UrlWebViewSource { Url = url };
        LblStatus.Text = "Click an image, then ✓ Use";
    }

    private async void OnUseClicked(object? s, EventArgs e)
    {
        BtnUse.IsEnabled = false;
        LblStatus.Text = "Extracting image URL...";

        try
        {
            string script = _provider == "google"
                ? "(window.__findBestImg&&window.__findBestImg())||window.__img||''"
                : "window.__img || ''";

            string? result = await ImageBrowser.EvaluateJavaScriptAsync(script);
            string url = (result ?? "").Trim('"');

            if (string.IsNullOrWhiteSpace(url) || url == "null" || url.Length < 10)
            {
                LblStatus.Text = "Click on an image first, then ✓ Use";
                BtnUse.IsEnabled = true;
                return;
            }

            LblStatus.Text = "Downloading image...";

            byte[] data;
            try
            {
                string esc = url.Replace("\\", "\\\\").Replace("'", "\\'");
                string js = "(async()=>{var ctrl=new AbortController();var t=setTimeout(()=>ctrl.abort(),10000);try{var r=await fetch('" + esc + "',{mode:'cors',credentials:'include',signal:ctrl.signal});clearTimeout(t);if(!r.ok)return'ERR:'+r.status;var b=await r.blob();return await new Promise(rs=>{var fr=new FileReader();fr.onloadend=()=>rs(fr.result);fr.readAsDataURL(b);});}catch(e){clearTimeout(t);return'ERR:'+e.message;}})()";
                string r = await ImageBrowser.EvaluateJavaScriptAsync(js);
                r = r.Trim('"');
                if (r.StartsWith("data:"))
                {
                    int idx = r.IndexOf(',');
                    data = idx > 0 ? Convert.FromBase64String(r[(idx + 1)..]) : throw new Exception("bad data");
                }
                else throw new Exception(r.StartsWith("ERR:") ? r[4..] : "fetch failed");
            }
            catch
            {
                using var h = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All };
                using var c = new HttpClient(h) { Timeout = TimeSpan.FromSeconds(12) };
                c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/125.0.0.0 Safari/537.36");
                c.DefaultRequestHeaders.Referrer = new Uri(_provider == "google" ? "https://www.google.com/" : "https://yandex.ru/");
                using var tok = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                data = await c.GetByteArrayAsync(url, tok.Token);
            }

            using var bmp = SKBitmap.Decode(data);
            if (bmp != null)
            {
                int maxDim = 600, w = bmp.Width, h = bmp.Height;
                if (w > maxDim || h > maxDim)
                {
                    float scale = Math.Min((float)maxDim / w, (float)maxDim / h);
                    w = (int)(w * scale); h = (int)(h * scale);
                    using var rs = bmp.Resize(new SKImageInfo(w, h), SKSamplingOptions.Default);
                    using var img = SKImage.FromBitmap(rs);
                    using var d2 = img.Encode(SKEncodedImageFormat.Jpeg, 75);
                    data = d2.ToArray();
                }
                else if (w * h > 100_000)
                {
                    using var img = SKImage.FromBitmap(bmp);
                    using var d2 = img.Encode(SKEncodedImageFormat.Jpeg, 75);
                    data = d2.ToArray();
                }
            }

            string mediaDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RepeatSegment", "decks", "media");
            Directory.CreateDirectory(mediaDir);
            string savedPath = Path.Combine(mediaDir, $"img_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.jpg");
            await File.WriteAllBytesAsync(savedPath, data);

            LblStatus.Text = "Image saved ✓";
            _tcs.TrySetResult(savedPath);
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            LblStatus.Text = $"Image: {ex.Message}";
            BtnUse.IsEnabled = true;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        _tcs.TrySetResult(null);
        return base.OnBackButtonPressed();
    }
}
