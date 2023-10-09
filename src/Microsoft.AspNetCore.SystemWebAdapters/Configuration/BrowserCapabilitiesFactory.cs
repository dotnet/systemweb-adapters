// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Configuration;

/// <summary>
/// This implementation draws inspiration from <see href="https://referencesource.microsoft.com/#System.Web/Configuration/BrowserCapabilitiesFactory.cs"/>, but
/// fixes up some patterns that were inefficient:
/// 
/// - Regex instances were not cached and created with each process
/// - Matches were copied to a separate array instead of accessing directly (new Regex APIs help here)
/// - The default process method listed a ton of methods to process and called them individually - now we put it in a list and iterate through
/// - The useragent was accessed from the header list each time but was the only thing used
/// - This class was public and had user-overridable functions for before and after each process.
///   For instance, CrawlerProcess would do an initial check, then call CrawlerProcessGateways and CrawlerProcessBrowsers
///   to do custom stuff. These are not implemented in the framework and are potentially added by uses. For now, we don't
///   support that pattern.
/// </summary>
internal sealed class BrowserCapabilitiesFactory : IBrowserCapabilitiesFactory
{
    // Func will return true if we want to end the processing of further funcs
    private readonly Func<string, ParsedBrowserResult, bool>[] _processList;

    public BrowserCapabilitiesFactory()
    {
        _processList = new[]
        {
            DefaultProcess,
            DefaultProcessGateways,
            CrawlerProcess,
            PlatformProcess,
            WinProcess,
            BlackberryProcess,
            OperaProcess,
            GenericdownlevelProcess,
            MozillaProcess,
            UcbrowserProcess,
        };
    }

    IHttpBrowserCapabilityFeature IBrowserCapabilitiesFactory.Create(HttpRequestCore request)
    {
        var userAgent = request.Headers.UserAgent.ToString();

        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return EmptyBrowserFeatures.Instance;
        }
        else if (request.HttpContext.RequestServices.GetService<IMemoryCache>() is { } cache)
        {
            return cache.GetOrCreate(userAgent, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(2);

                return Parse(userAgent);
            }) ?? EmptyBrowserFeatures.Instance;
        }
        else
        {
            return Parse(userAgent);
        }
    }

    public IHttpBrowserCapabilityFeature Parse(string userAgent)
    {
        var dictionary = new ParsedBrowserResult();

        foreach (var process in _processList)
        {
            if (process(userAgent, dictionary))
            {
                break;
            }
        }

        return dictionary;
    }

    private bool DefaultProcess(string userAgent, ParsedBrowserResult result)
    {
        result["activexcontrols"] = "false";
        result["aol"] = "false";
        result["backgroundsounds"] = "false";
        result["beta"] = "false";
        result["browser"] = "Unknown";
        result["canCombineFormsInDeck"] = "true";
        result["canInitiateVoiceCall"] = "false";
        result["canRenderAfterInputOrSelectElement"] = "true";
        result["canRenderEmptySelects"] = "true";
        result["canRenderInputAndSelectElementsTogether"] = "true";
        result["canRenderMixedSelects"] = "true";
        result["canRenderOneventAndPrevElementsTogether"] = "true";
        result["canRenderPostBackCards"] = "true";
        result["canRenderSetvarZeroWithMultiSelectionList"] = "true";
        result["canSendMail"] = "true";
        result["cdf"] = "false";
        result["cookies"] = "true";
        result["crawler"] = "false";
        result["defaultSubmitButtonLimit"] = "1";
        result["ecmascriptversion"] = "0.0";
        result["frames"] = "false";
        result["gatewayMajorVersion"] = "0";
        result["gatewayMinorVersion"] = "0";
        result["gatewayVersion"] = "None";
        result["hasBackButton"] = "true";
        result["hidesRightAlignedMultiselectScrollbars"] = "false";
        result["inputType"] = "telephoneKeypad";
        result["isColor"] = "false";
        result["isMobileDevice"] = "false";
        result["javaapplets"] = "false";
        result["javascript"] = "false";
        result["jscriptversion"] = "0.0";
        result["majorversion"] = "0";
        result["maximumHrefLength"] = "10000";
        result["maximumRenderedPageSize"] = "2000";
        result["maximumSoftkeyLabelLength"] = "5";
        result["minorversion"] = "0";
        result["mobileDeviceManufacturer"] = "Unknown";
        result["mobileDeviceModel"] = "Unknown";
        result["msdomversion"] = "0.0";
        result["numberOfSoftkeys"] = "0";
        result["platform"] = "Unknown";
        result["preferredImageMime"] = "image/gif";
        result["preferredRenderingMime"] = "text/html";
        result["preferredRenderingType"] = "html32";
        result["rendersBreakBeforeWmlSelectAndInput"] = "false";
        result["rendersBreaksAfterHtmlLists"] = "true";
        result["rendersBreaksAfterWmlAnchor"] = "false";
        result["rendersBreaksAfterWmlInput"] = "false";
        result["rendersWmlDoAcceptsInline"] = "true";
        result["rendersWmlSelectsAsMenuCards"] = "false";
        result["requiredMetaTagNameValue"] = "";
        result["requiresAbsolutePostbackUrl"] = "false";
        result["requiresAdaptiveErrorReporting"] = "false";
        result["requiresAttributeColonSubstitution"] = "false";
        result["requiresContentTypeMetaTag"] = "false";
        result["requiresControlStateInSession"] = "false";
        result["requiresDBCSCharacter"] = "false";
        result["requiresFullyQualifiedRedirectUrl"] = "false";
        result["requiresLeadingPageBreak"] = "false";
        result["requiresNoBreakInFormatting"] = "false";
        result["requiresOutputOptimization"] = "false";
        result["requiresPhoneNumbersAsPlainText"] = "false";
        result["requiresPostRedirectionHandling"] = "false";
        result["requiresSpecialViewStateEncoding"] = "false";
        result["requiresUniqueFilePathSuffix"] = "false";
        result["requiresUniqueHtmlCheckboxNames"] = "false";
        result["requiresUniqueHtmlInputNames"] = "false";
        result["requiresUrlEncodedPostfieldValues"] = "false";
        result["requiresXhtmlCssSuppression"] = "false";
        result["screenBitDepth"] = "1";
        result["supportsAccesskeyAttribute"] = "false";
        result["supportsBodyColor"] = "true";
        result["supportsBold"] = "false";
        result["supportsCallback"] = "false";
        result["supportsCacheControlMetaTag"] = "true";
        result["supportsCss"] = "false";
        result["supportsDivAlign"] = "true";
        result["supportsDivNoWrap"] = "false";
        result["supportsEmptyStringInCookieValue"] = "true";
        result["supportsFileUpload"] = "false";
        result["supportsFontColor"] = "true";
        result["supportsFontName"] = "false";
        result["supportsFontSize"] = "false";
        result["supportsImageSubmit"] = "false";
        result["supportsIModeSymbols"] = "false";
        result["supportsInputIStyle"] = "false";
        result["supportsInputMode"] = "false";
        result["supportsItalic"] = "false";
        result["supportsJPhoneMultiMediaAttributes"] = "false";
        result["supportsJPhoneSymbols"] = "false";
        result["SupportsMaintainScrollPositionOnPostback"] = "false";
        result["supportsMultilineTextBoxDisplay"] = "false";
        result["supportsQueryStringInFormAction"] = "true";
        result["supportsRedirectWithCookie"] = "true";
        result["supportsSelectMultiple"] = "true";
        result["supportsUncheck"] = "true";
        result["supportsVCard"] = "false";
        result["tables"] = "false";
        result["tagwriter"] = "System.Web.UI.Html32TextWriter";
        result["type"] = "Unknown";
        result["vbscript"] = "false";
        result["version"] = "0.0";
        result["w3cdomversion"] = "0.0";
        result["win16"] = "false";
        result["win32"] = "false";

        result.AddBrowser("Default");

        return false;
    }

    private bool DefaultProcessGateways(string userAgent, ParsedBrowserResult dictionary)
    {
        return false;
    }

    private readonly RegexWorker _crawler = new("crawler|Crawler|Googlebot|bingbot");

    private bool CrawlerProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_crawler.Process(userAgent) is { HasMatches: true })
        {
            dictionary["crawler"] = "true";
        }

        return false;
    }

    private readonly RegexWorker _win = new("Windows NT|WinNT|Windows XP");

    private bool PlatformProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_win.Process(userAgent) is { HasMatches: true })
        {
            dictionary["platform"] = "WinNT";
        }

        return false;
    }

    private bool WinProcess(string userAgent, ParsedBrowserResult dictionary) => false;

    private readonly RegexWorker _blackberry = new(@"BlackBerry(?'deviceName'\w+)/(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)");

    private bool BlackberryProcess(string userAgent, ParsedBrowserResult result)
    {
        if (_blackberry.Process(userAgent) is { HasMatches: true } match)
        {
            result["layoutEngine"] = "BlackBerry";
            result["browser"] = "BlackBerry";
            result["majorversion"] = match["major"];
            result["minorversion"] = match["minor"];
            result["type"] = match["BlackBerry${major"];
            result["mobileDeviceModel"] = match["deviceName"];
            result["isMobileDevice"] = "true";
            result["version"] = match["version"];
            result["ecmascriptversion"] = "3.0";
            result["javascript"] = "true";
            result["javascriptversion"] = "1.3";
            result["w3cdomversion"] = "1.0";
            result["supportsAccesskeyAttribute"] = "true";
            result["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            result["cookies"] = "true";
            result["frames"] = "true";
            result["javaapplets"] = "true";
            result["supportsCallback"] = "true";
            result["supportsDivNoWrap"] = "false";
            result["supportsFileUpload"] = "true";
            result["supportsMultilineTextBoxDisplay"] = "true";
            result["supportsXmlHttp"] = "true";
            result["tables"] = "true";
            result["canInitiateVoiceCall"] = "true";

            result.AddBrowser("BlackBerry");
            return true;
        }

        return false;
    }

    private readonly RegexWorker _opera = new("Opera[ /](?'version'(?'major'\\d+)(\\.(?'minor'\\d+)?)(?'letters'\\w*))", "OPR/(?'version'(?'major'\\d+)(\\.(?'minor'\\d+)?)(?'letters'\\w*))");

    private bool OperaProcess(string userAgent, ParsedBrowserResult capabilities)
    {
        if (_opera.Process(userAgent) is { HasMatches: true } match)
        {
            capabilities.AddBrowser("Opera");
            capabilities["browser"] = "Opera";
            capabilities["majorversion"] = match["major"];
            capabilities["minorversion"] = match["minor"];
            capabilities["type"] = match["Opera${major"];
            capabilities["version"] = match["version"];
            capabilities["layoutEngine"] = "Presto";
            capabilities["layoutEngineVersion"] = match["layoutVersion"];
            capabilities["ecmascriptversion"] = "3.0";
            capabilities["javascript"] = "true";
            capabilities["javascriptversion"] = "1.5";
            capabilities["letters"] = match["letters"];
            capabilities["w3cdomversion"] = "1.0";
            capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            capabilities["cookies"] = "true";
            capabilities["frames"] = "true";
            capabilities["javaapplets"] = "true";
            capabilities["supportsAccesskeyAttribute"] = "true";
            capabilities["supportsCallback"] = "true";
            capabilities["supportsFileUpload"] = "true";
            capabilities["supportsMultilineTextBoxDisplay"] = "true";
            capabilities["supportsXmlHttp"] = "true";
            capabilities["tables"] = "true";
            capabilities["inputType"] = "keyboard";
            capabilities["isColor"] = "true";
            capabilities["isMobileDevice"] = "false";
            capabilities["maximumRenderedPageSize"] = "300000";
            capabilities["screenBitDepth"] = "8";
            capabilities["supportsBold"] = "true";
            capabilities["supportsCss"] = "true";
            capabilities["supportsDivNoWrap"] = "true";
            capabilities["supportsFontName"] = "true";
            capabilities["supportsFontSize"] = "true";
            capabilities["supportsImageSubmit"] = "true";
            capabilities["supportsItalic"] = "true";

            return true;
        }

        return false;
    }

    private bool GenericdownlevelProcess(string userAgent, ParsedBrowserResult dictionary) => false;

    private readonly RegexWorker _mozilla = new("Mozilla");

    private bool MozillaProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_mozilla.Process(userAgent) is { HasMatches: true })
        {
            dictionary.AddBrowser("Mozilla");
            dictionary["browser"] = "Mozilla";
            dictionary["cookies"] = "true";
            dictionary["ecmascriptversion"] = "3.0";
            dictionary["frames"] = "true";
            dictionary["inputType"] = "keyboard";
            dictionary["isColor"] = "true";
            dictionary["isMobileDevice"] = "false";
            dictionary["javascript"] = "true";
            dictionary["javascriptversion"] = "1.5";
            dictionary["maximumRenderedPageSize"] = "300000";
            dictionary["screenBitDepth"] = "8";
            dictionary["supportsBold"] = "true";
            dictionary["supportsCallback"] = "true";
            dictionary["supportsCss"] = "true";
            dictionary["supportsDivNoWrap"] = "true";
            dictionary["supportsFileUpload"] = "true";
            dictionary["supportsFontName"] = "true";
            dictionary["supportsFontSize"] = "true";
            dictionary["supportsImageSubmit"] = "true";
            dictionary["supportsItalic"] = "true";
            dictionary["supportsMaintainScrollPositionOnPostback"] = "true";
            dictionary["supportsMultilineTextBoxDisplay"] = "true";
            dictionary["supportsXmlHttp"] = "true";
            dictionary["tables"] = "true";
            dictionary["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            dictionary["type"] = "Mozilla";
            dictionary["w3cdomversion"] = "1.0";

            if (!IeProcess(userAgent, dictionary) &&
                !FirefoxProcess(userAgent, dictionary) &&
                !WebkitProcess(userAgent, dictionary) &&
                !IemobileProcess(userAgent, dictionary))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private readonly RegexWorker _ie = new(@"MSIE (?'version'(?'major'\d+)(\.(?'minor'\d+)?)(?'letters'\w*))(?'extra'[^)]*)", @"Trident/(?'layoutVersion'\d+)");
    private readonly RegexWorker _ieMobileBasic = new(@"IEMobile");

    private bool IeProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_ie.Process(userAgent) is { HasMatches: true } regexWorker && _ieMobileBasic.Process(userAgent) is { HasMatches: false })
        {
            dictionary["browser"] = "IE";
            dictionary["layoutEngine"] = "Trident";
            dictionary["layoutEngineVersion"] = regexWorker["layoutVersion"];
            dictionary["extra"] = regexWorker["extra"];
            dictionary["isColor"] = "true";
            dictionary["letters"] = regexWorker["letters"];
            dictionary["majorversion"] = regexWorker["major"];
            dictionary["minorversion"] = regexWorker["minor"];
            dictionary["screenBitDepth"] = "8";
            dictionary["type"] = regexWorker["IE${major"];
            dictionary["version"] = regexWorker["version"];
            dictionary.AddBrowser("IE");

            return true;
        }

        return false;
    }

    private readonly RegexWorker _firefox = new(@"Firefox/(?'version'(?'major'\d+)(\.(?'minor'\d+)))", @"Gecko/(?'layoutVersion'\d+)");

    private bool FirefoxProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_firefox.Process(userAgent) is { HasMatches: true } regexWorker)
        {
            dictionary["browser"] = "Firefox";
            dictionary["majorversion"] = regexWorker["major"];
            dictionary["minorversion"] = regexWorker["minor"];
            dictionary["version"] = regexWorker["version"];
            dictionary["type"] = regexWorker["Firefox${major"];
            dictionary["layoutEngine"] = "Gecko";
            dictionary["layoutEngineVersion"] = regexWorker["layoutVersion"];
            dictionary["supportsAccesskeyAttribute"] = "true";
            dictionary["javaapplets"] = "true";
            dictionary["supportsDivNoWrap"] = "false";

            dictionary.AddBrowser("Firefox");

            return true;
        }

        return false;
    }

    private readonly RegexWorker _ieMobile = new(@"IEMobile.(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)", @"MSIE (?'msieMajorVersion'\d+)");

    private bool IemobileProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_ieMobile.Process(userAgent) is { HasMatches: true } regexWorker)
        {
            dictionary["layoutEngine"] = "Trident";
            dictionary["browser"] = "IEMobile";
            dictionary["majorversion"] = regexWorker["major"];
            dictionary["minorversion"] = regexWorker["minor"];
            dictionary["type"] = regexWorker["IEMobile${msieMajorVersion"];
            dictionary["isMobileDevice"] = "true";
            dictionary["version"] = regexWorker["version"];
            dictionary["jscriptversion"] = "5.6";
            dictionary["msdomversion"] = regexWorker["majorversion}.${minorversion"];
            dictionary["supportsAccesskeyAttribute"] = "true";
            dictionary["javaapplets"] = "true";
            dictionary["supportsDivNoWrap"] = "false";
            dictionary["vbscript"] = "true";
            dictionary["inputType"] = "virtualKeyboard";
            dictionary["numberOfSoftkeys"] = "2";
            dictionary.AddBrowser("IEMobile");
            return true;
        }

        return false;
    }

    private readonly RegexWorker _webkit = new(@"AppleWebKit/(?'layoutVersion'\d+)");

    private bool WebkitProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_webkit.Process(userAgent) is { HasMatches: true } regexWorker)
        {
            dictionary["layoutEngine"] = "WebKit";
            dictionary["layoutEngineVersion"] = regexWorker["layoutVersion"];
            dictionary.AddBrowser("WebKit");

            WebkitmobileProcess(userAgent, dictionary);

            if (ChromeProcess(userAgent, dictionary))
            {
            }
            else if (SafariProcess(userAgent, dictionary))
            {
            }

            return true;
        }

        return false;
    }

    private readonly RegexWorker _webkitMobile = new(@"Mobile( Safari)?/(?'iOSVersion'[^ ]+)", @"Mozilla/5.0 \((?'deviceName'[^;]+)");

    private void WebkitmobileProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_webkitMobile.Process(userAgent) is { HasMatches: true } regexWorker)
        {
            dictionary["mobileDeviceModel"] = regexWorker["deviceName"];
            dictionary["isMobileDevice"] = "true";
            dictionary["ecmascriptversion"] = "3.0";
            dictionary["javascript"] = "true";
            dictionary["javascriptversion"] = "1.6";
            dictionary["w3cdomversion"] = "1.0";
            dictionary["supportsAccesskeyAttribute"] = "true";
            dictionary["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            dictionary["cookies"] = "true";
            dictionary["frames"] = "true";
            dictionary["supportsCallback"] = "true";
            dictionary["supportsDivNoWrap"] = "false";
            dictionary["supportsFileUpload"] = "true";
            dictionary["supportsMaintainScrollPositionOnPostback"] = "true";
            dictionary["supportsMultilineTextBoxDisplay"] = "true";
            dictionary["supportsXmlHttp"] = "true";
            dictionary["tables"] = "true";
        }
    }

    private readonly RegexWorker _chrome = new(@"Chrome/(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)");

    private bool ChromeProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_chrome.Process(userAgent) is { HasMatches: true } regexWorker)
        {
            dictionary["browser"] = "Chrome";
            dictionary["majorversion"] = regexWorker["major"];
            dictionary["minorversion"] = regexWorker["minor"];
            dictionary["type"] = regexWorker["Chrome${major"];
            dictionary["version"] = regexWorker["version"];
            dictionary["ecmascriptversion"] = "3.0";
            dictionary["javascript"] = "true";
            dictionary["javascriptversion"] = "1.7";
            dictionary["w3cdomversion"] = "1.0";
            dictionary["supportsAccesskeyAttribute"] = "true";
            dictionary["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            dictionary["cookies"] = "true";
            dictionary["frames"] = "true";
            dictionary["javaapplets"] = "true";
            dictionary["supportsCallback"] = "true";
            dictionary["supportsDivNoWrap"] = "false";
            dictionary["supportsFileUpload"] = "true";
            dictionary["supportsMaintainScrollPositionOnPostback"] = "true";
            dictionary["supportsMultilineTextBoxDisplay"] = "true";
            dictionary["supportsXmlHttp"] = "true";
            dictionary["tables"] = "true";
            dictionary.AddBrowser("Chrome");

            return true;
        }

        return false;
    }

    private readonly RegexWorker _safari = new("Safari");
    private readonly RegexWorker _notSafari = new("Chrome|Android");

    private bool SafariProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_safari.Process(userAgent) is { HasMatches: true } && _notSafari.Process(userAgent) is { HasMatches: false })
        {
            dictionary["browser"] = "Safari";
            dictionary["type"] = "Safari";
            dictionary.AddBrowser("Safari");

            IphoneProcess(userAgent, dictionary);
            IpadProcess(userAgent, dictionary);
            IpodProcess(userAgent, dictionary);
            Safari3plusProcess(userAgent, dictionary);

            return true;
        }

        return false;
    }

    private readonly RegexWorker _iphone = new("iPhone");
    private void IphoneProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_iphone.Process(userAgent) is { HasMatches: true })
        {
            dictionary["isMobileDevice"] = "true";
            dictionary["mobileDeviceManufacturer"] = "Apple";
            dictionary["mobileDeviceModel"] = "IPhone";
            dictionary["canInitiateVoiceCall"] = "true";
        }
    }

    private readonly RegexWorker _ipad = new("iPad");
    private void IpadProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_ipad.Process(userAgent) is { HasMatches: true })
        {
            dictionary["isMobileDevice"] = "true";
            dictionary["mobileDeviceManufacturer"] = "Apple";
            dictionary["mobileDeviceModel"] = "IPad";
        }
    }

    private readonly RegexWorker _ipod = new("iPod");
    private void IpodProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_ipod.Process(userAgent) is { HasMatches: true })
        {
            dictionary["isMobileDevice"] = "true";
            dictionary["mobileDeviceManufacturer"] = "Apple";
            dictionary["mobileDeviceModel"] = "IPod";
        }
    }

    private readonly RegexWorker _safari3plus = new(@"Version/(?'version'(?'major'[3-9]|\d{2,})(\.(?'minor'\d+)?)\w*)");

    private bool Safari3plusProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_safari3plus.Process(userAgent) is { HasMatches: true } regexWorker)
        {
            dictionary["version"] = regexWorker["version"];
            dictionary["majorversion"] = regexWorker["major"];
            dictionary["minorversion"] = regexWorker["minor"];
            dictionary["type"] = regexWorker["Safari${major"];
            dictionary["ecmascriptversion"] = "3.0";
            dictionary["javascript"] = "true";
            dictionary["javascriptversion"] = "1.6";
            dictionary["w3cdomversion"] = "1.0";
            dictionary["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            dictionary["cookies"] = "true";
            dictionary["frames"] = "true";
            dictionary["javaapplets"] = "true";
            dictionary["supportsAccesskeyAttribute"] = "true";
            dictionary["supportsCallback"] = "true";
            dictionary["supportsDivNoWrap"] = "false";
            dictionary["supportsFileUpload"] = "true";
            dictionary["supportsMaintainScrollPositionOnPostback"] = "true";
            dictionary["supportsMultilineTextBoxDisplay"] = "true";
            dictionary["supportsXmlHttp"] = "true";
            dictionary["tables"] = "true";
            dictionary.AddBrowser("Safari3Plus");
            return true;
        }

        return false;
    }

    private readonly RegexWorker _ucBrowser = new(@"(UC Browser |UCWEB)(?'version'(?'major'\d+)(\.(?'minor'[\d\.]+)?)\w*)");

    private bool UcbrowserProcess(string userAgent, ParsedBrowserResult dictionary)
    {
        if (_ucBrowser.Process(userAgent) is { HasMatches: true } regexWorker)
        {
            dictionary["browser"] = "UCBrowser";
            dictionary["majorversion"] = regexWorker["major"];
            dictionary["minorversion"] = regexWorker["minor"];
            dictionary["isMobileDevice"] = "true";
            dictionary["version"] = regexWorker["version"];
            dictionary["ecmascriptversion"] = "3.0";
            dictionary["javascript"] = "true";
            dictionary["javascriptversion"] = "1.5";
            dictionary["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            dictionary["cookies"] = "true";
            dictionary["frames"] = "true";
            dictionary["supportsCallback"] = "true";
            dictionary["supportsFileUpload"] = "true";
            dictionary["supportsMultilineTextBoxDisplay"] = "true";
            dictionary["supportsXmlHttp"] = "true";
            dictionary["tables"] = "true";
            dictionary.AddBrowser("UCBrowser");

            return true;
        }

        return false;
    }

    private sealed class ParsedBrowserResult : Dictionary<string, string?>, IHttpBrowserCapabilityFeature
    {
        public ParsedBrowserResult()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        [Conditional("NotNeededYet")]
        [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Retained in case we need the data later")]
        public void AddBrowser(string browser)
        {
        }

        string? IHttpBrowserCapabilityFeature.this[string key] => TryGetValue(key, out var value) ? value : null;
    }

    private readonly struct RegexResult
    {
        private readonly Match? _match;
        private readonly Match? _match2;

        public RegexResult(Match? match, Match? match2)
        {
            if (match is { Success: true })
            {
                _match = match;
                _match2 = match2;
            }
            else
            {
                _match = null;
                _match2 = null;
            }
        }

        public bool HasMatches => _match is not null;

        public string? this[string key] => Get(_match, key) ?? Get(_match2, key);

        private static string? Get(Match? match, string key)
        {
            if (match is not null && match.Groups.TryGetValue(key, out var group))
            {
                return group.Value;
            }

            return null;
        }
    }

    private sealed class RegexWorker
    {
        private readonly Regex _regex;
        private readonly Regex? _regex2;

        public RegexWorker(string pattern, string? pattern2 = null)
        {
            _regex = new Regex(pattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled);

            if (pattern2 is not null)
            {
                _regex2 = new Regex(pattern2, RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            }
        }

        public RegexResult Process(string userAgent)
            => new(_regex?.Match(userAgent), _regex2?.Match(userAgent));
    }

    private sealed class EmptyBrowserFeatures : IHttpBrowserCapabilityFeature
    {
        public static IHttpBrowserCapabilityFeature Instance { get; } = new EmptyBrowserFeatures();

        public string? this[string key] => null;
    }
}
