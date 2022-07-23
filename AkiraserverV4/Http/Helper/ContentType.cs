using System.Collections.Generic;

namespace AkiraserverV4.Http.Helper
{
    public static class Mime
    {
        private static readonly Dictionary<ContentType, string> MimeString = new Dictionary<ContentType, string>()
        {
            { ContentType.Invalid ,string.Empty },
            { ContentType.AAC ,"audio/aac" },
            { ContentType.AbiWord ,"application/x-abiword" },
            { ContentType.AVI ,"video/x-msvideo" },
            { ContentType.AmazonEbook ,"application/vnd.amazon.ebook" },
            { ContentType.Binary ,"application/octet-stream" },
            { ContentType.BZip ,"application/x-bzip" },
            { ContentType.BZip2 ,"application/x-bzip2" },
            { ContentType.CShell ,"application/x-csh" },
            { ContentType.CSS ,"text/css" },
            { ContentType.CSV ,"text/csv" },
            { ContentType.MicrosoftWord ,"application/msword" },
            { ContentType.EPUB ,"application/epub+zip" },
            { ContentType.GIF ,"image/gif" },
            { ContentType.HTML ,"text/html" },
            { ContentType.Icon ,"image/x-icon" },
            { ContentType.iCalendar ,"text/calendar" },
            { ContentType.JAR ,"application/java-archive" },
            { ContentType.JPEG ,"image/jpeg" },
            { ContentType.JavaScript ,"application/javascript" },
            { ContentType.JSON ,"application/json" },
            { ContentType.MIDI ,"audio/midi" },
            { ContentType.MPEG ,"video/mpeg" },
            { ContentType.ApplePackage ,"application/vnd.apple.installer+xml" },
            { ContentType.OpenDocumentPresentation ,"application/vnd.oasis.opendocument.presentation" },
            { ContentType.OpenDocumentSpreadsheet ,"application/vnd.oasis.opendocument.spreadsheet" },
            { ContentType.OpenDocumentText ,"application/vnd.oasis.opendocument.text" },
            { ContentType.OGGAudio ,"audio/ogg" },
            { ContentType.OGGVideo ,"video/ogg" },
            { ContentType.OGG ,"application/ogg" },
            { ContentType.PortableDocumentFormat ,"application/pdf" },
            { ContentType.MicrosoftPowerPoint ,"application/vnd.ms-powerpoint" },
            { ContentType.RAR ,"application/x-rar-compressed" },
            { ContentType.RTF ,"application/rtf" },
            { ContentType.ShellScript ,"application/x-sh" },
            { ContentType.SVG ,"image/svg+xml" },
            { ContentType.SWF ,"application/x-shockwave-flash" },
            { ContentType.TAR ,"application/x-tar" },
            { ContentType.TIFF ,"image/tiff" },
            { ContentType.TrueTypeFont ,"font/ttf" },
            { ContentType.MicrosftVisio ,"application/vnd.visio" },
            { ContentType.WAV ,"audio/x-wav" },
            { ContentType.WEBMAudio ,"audio/webm" },
            { ContentType.WEBMVideo ,"video/webm" },
            { ContentType.WEBP ,"image/webp" },
            { ContentType.OpenFontFormat ,"font/woff" },
            { ContentType.OpenFontFormat2 ,"font/woff2" },
            { ContentType.XHTML ,"application/xhtml+xml" },
            { ContentType.MicrosoftExcel ,"application/vnd.ms-excel" },
            { ContentType.XML ,"application/xml" },
            { ContentType.XUL ,"application/vnd.mozilla.xul+xml" },
            { ContentType.ZIP ,"application/zip" },
            { ContentType.Container3GPPVideo ,"video/3gpp" },
            { ContentType.Container3GPPAudio ,"audio/3gpp" },
            { ContentType.Container3GPP2Video ,"video/3gpp2" },
            { ContentType.Container3GPP2Audio ,"audio/3gpp2" },
            { ContentType.Compressed7Zip ,"application/x-7z-compressed" },
            { ContentType.FormUrlEncoded ,"application/x-www-form-urlencoded" },
            { ContentType.FormMultipart ,"multipart/form-data" },
            { ContentType.PlainText ,"text/plain" },
        };

        public static string ToString(ContentType value)
        {
            if (MimeString.ContainsKey(value))
            {
                return MimeString[value];
            }
            return null;
        }

        public static ContentType FromString(string value)
        {
            foreach (var keyValue in MimeString)
            {
                if (value.Contains(keyValue.Value))
                {
                    return keyValue.Key;
                }
            }
            return ContentType.Invalid;
        }
    }
    public enum ContentType
    {
        Invalid,
        AAC,
        AbiWord,
        AVI,
        AmazonEbook,
        Binary,
        BZip,
        BZip2,
        CShell,
        CSS,
        CSV,
        MicrosoftWord,
        EPUB,
        GIF,
        HTML,
        Icon,
        iCalendar,
        JAR,
        JPEG,
        JavaScript,
        JSON,
        MIDI,
        MPEG,
        ApplePackage,
        OpenDocumentPresentation,
        OpenDocumentSpreadsheet,
        OpenDocumentText,
        OGGAudio,
        OGGVideo,
        OGG,
        PortableDocumentFormat,
        MicrosoftPowerPoint,
        RAR,
        RTF,
        ShellScript,
        SVG,
        SWF,
        TAR,
        TIFF,
        TrueTypeFont,
        MicrosftVisio,
        WAV,
        WEBMAudio,
        WEBMVideo,
        WEBP,
        OpenFontFormat,
        OpenFontFormat2,
        XHTML,
        MicrosoftExcel,
        XML,
        XUL,
        ZIP,
        Container3GPPAudio,
        Container3GPPVideo,
        Container3GPP2Audio,
        Container3GPP2Video,
        Compressed7Zip,
        FormUrlEncoded,
        FormMultipart,
        PlainText
    }
}