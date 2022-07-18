using AkiraserverV4.Http.Helper;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SampleServer
{
    [Controller("/[controller]")]
    public class ConvertContext : CustomBaseContext
    {
        [Post("/[method]")]
        public async Task<object> UrlEncoded()
        {
            return new JsonResult(Request.Params);
        }

        //[Post("/[method]")]
        //public object UrlEncoded()
        //{
        //    return new JsonResult(Request.ReadUrlEncodedPayload().Result);
        //}

        //[Post("/[method]")]
        //public async Task<FileResponse> MultipartEncoded()
        //{
        //    var data = await Request.ReadMultipartPayload().ConfigureAwait(false);

        //    if (data.FormFile.Count == 0)
        //    {
        //        return null;
        //    }

        //    const string savePath = @"M:\Code\C#\AkiraServerV4Other\tests";

        //    foreach (var file in data.FormFile)
        //    {
        //        using (var fileStream = File.Create(Path.Combine(savePath, file.Filename)))
        //        {
        //            await file.CopyToAsync(fileStream).ConfigureAwait(false);
        //            fileStream.Close();
        //        }
        //    }

        //    var selectedFile = data.FormFile[new Random().Next(0, data.FormFile.Count)];

        //    var buffer = new MemoryStream();
        //    await selectedFile.CopyToAsync(buffer).ConfigureAwait(false);

        //    return new FileResponse()
        //    {
        //        ContentType = selectedFile.ContentType,
        //        Content = buffer,
        //        Filename = selectedFile.Filename,
        //    };

        //    //return null;
        //}

        private static Random random = new Random();
        [Post("/[method]")]
        public async Task<string> LargeResponse()
        {
            return RandomString(100000000);

            static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }

        [Post("/[method]")]
        public XmlResult JsonToXml(List<KonachanApiResponse> items, string TestParam1, int TestParam2, DateTime DateTime)
        {
            return new XmlResult(items);
        }

        [Post("/[method]")]
        public JsonResult XmlToJson(List<KonachanApiResponse> items)
        {
            return new JsonResult(items);
        }

        public class KonachanApiResponse
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("tags")]
            public string Tags { get; set; }

            [JsonPropertyName("created_at")]
            public int CreatedAt { get; set; }

            [JsonPropertyName("creator_id")]
            public int CreatorId { get; set; }

            [JsonPropertyName("author")]
            public string Author { get; set; }

            [JsonPropertyName("change")]
            public int Change { get; set; }

            [JsonPropertyName("source")]
            public string Source { get; set; }

            [JsonPropertyName("score")]
            public int Score { get; set; }

            [JsonPropertyName("md5")]
            public string Md5 { get; set; }

            [JsonPropertyName("file_size")]
            public int FileSize { get; set; }

            [JsonPropertyName("file_url")]
            public string FileUrl { get; set; }

            [JsonPropertyName("is_shown_in_index")]
            public bool IsShownInIndex { get; set; }

            [JsonPropertyName("preview_url")]
            public string PreviewUrl { get; set; }

            [JsonPropertyName("preview_width")]
            public int PreviewWidth { get; set; }

            [JsonPropertyName("preview_height")]
            public int PreviewHeight { get; set; }

            [JsonPropertyName("actual_preview_width")]
            public int ActualPreviewWidth { get; set; }

            [JsonPropertyName("actual_preview_height")]
            public int ActualPreviewHeight { get; set; }

            [JsonPropertyName("sample_url")]
            public string SampleUrl { get; set; }

            [JsonPropertyName("sample_width")]
            public int SampleWidth { get; set; }

            [JsonPropertyName("sample_height")]
            public int SampleHeight { get; set; }

            [JsonPropertyName("sample_file_size")]
            public int SampleFileSize { get; set; }

            [JsonPropertyName("jpeg_url")]
            public string JpegUrl { get; set; }

            [JsonPropertyName("jpeg_width")]
            public int JpegWidth { get; set; }

            [JsonPropertyName("jpeg_height")]
            public int JpegHeight { get; set; }

            [JsonPropertyName("jpeg_file_size")]
            public int JpegFileSize { get; set; }

            [JsonPropertyName("rating")]
            public string Rating { get; set; }

            [JsonPropertyName("has_children")]
            public bool HasChildren { get; set; }

            [JsonPropertyName("parent_id")]
            public int? ParentId { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("width")]
            public int Width { get; set; }

            [JsonPropertyName("height")]
            public int Height { get; set; }

            [JsonPropertyName("is_held")]
            public bool IsHeld { get; set; }

            [JsonPropertyName("frames_pending_string")]
            public string FramesPendingString { get; set; }

            [JsonPropertyName("frames_pending")]
            public List<object> FramesPending { get; set; }

            [JsonPropertyName("frames_string")]
            public string FramesString { get; set; }

            [JsonPropertyName("frames")]
            public List<object> Frames { get; set; }
        }
    }
}