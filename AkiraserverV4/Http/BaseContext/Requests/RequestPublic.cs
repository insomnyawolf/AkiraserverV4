using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Helper;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Context.Requests
{
    public partial class Request
    {
#if DEBUG
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public MemoryStream RawPayload
        {
            get
            {
                if (Body is null)
                {
                    return null;
                }

                Body.Position = 0;
                return Body;
            }
        }

        public string ReadStringPayload
        {
            get
            {
                var raw = RawPayload;
                if (raw is null)
                {
                    return null;
                }
                return new StreamReader(raw).ReadToEnd();
            }
        }

        public async Task<string> ReadStringPayloadAsync()
        {
            return await new StreamReader(RawPayload).ReadToEndAsync().ConfigureAwait(false);
        }

        public async Task<T> ReadJsonPayload<T>()
        {
            return (T)await ReadJsonPayload(typeof(T)).ConfigureAwait(false);
        }

        public async Task<object> ReadJsonPayload(Type type)
        {
            string data = await ReadStringPayloadAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize(data, type);
        }

        public T ReadXmlPayload<T>()
        {
            return (T)ReadXmlPayload(typeof(T));
        }

        public object ReadXmlPayload(Type type)
        {
            return XmlDeserialize.DeSerialize(type, RawPayload);
        }

        public async Task<List<FormInput>> ReadUrlEncodedPayload()
        {
            return DeserializeUrlEncoded(await ReadStringPayloadAsync().ConfigureAwait(false));
        }

        public async Task<Form> ReadMultipartPayload()
        {
            string contentTypeValue = null;
            foreach (var header in Header.RequestHeaders)
            {
                if (header.Key.StartsWith(HeaderNames.ContentType))
                {
                    contentTypeValue = header.Value;
                    break;
                }
            }

            if (contentTypeValue is null)
            {
                throw new BadRequestException();
            }

            var parts = contentTypeValue.Split("; boundary=");

            if (parts.Length != 2)
            {
                throw new BadRequestException();
            }

            int[] getBoundary()
            {
                var boundaryStr = "--" + parts[1];
                var boundaryTemp = boundaryStr.ToCharArray();
                var boundary = new int[boundaryTemp.Length];

                for (int i = 0; i < boundaryTemp.Length; i++)
                {
                    boundary[i] = boundaryTemp[i];
                }
                return boundary;
            }

            int[] httpDelimiterConverted()
            {
                var httpDelimiter = new int[HttpDelimiter.Length];

                for (int i = 0; i < httpDelimiter.Length; i++)
                {
                    httpDelimiter[i] = HttpDelimiter[i];
                }
                return httpDelimiter;
            }

            var boundary = getBoundary();
            var httpDelimiter = httpDelimiterConverted();

            int[] checkGroup = new int[boundary.Length];
            int[] tempBuffer = new int[boundary.Length];

            var body = RawPayload;

            Form form = new Form();
            BaseFormInput current = null;
            StringBuilder temp = new StringBuilder();

            bool seeking = true;

            long fileInitPosition = 0;
            long fileEndPosition = 0;

            long nextElementPosition = 0;

            long position = -checkGroup.Length;

            int nextChar;

            bool blockPositionDetector = false;

            while (position < body.Length - checkGroup.Length)
            {
                if (ArraysEqual(boundary, checkGroup))
                {
                    fileEndPosition = position;
                    blockPositionDetector = false;
                    seekNextBlock();
                    seeking = false;
                    addElementToForm(current);
                    temp = new StringBuilder();
                }

                ReadChar();

                parseInputHeaders();

                if (!seeking)
                {
                    temp.Append((char)checkGroup[0]);
                }
            }

            void parseInputHeaders()
            {
                if (blockPositionDetector && !seeking && position == nextElementPosition && temp.Length > 0)
                {
                    string formDataRaw = temp.ToString();

                    var split1 = formDataRaw.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                    var split = split1[0].Split("; ");

                    if (split1.Length > 1)
                    {
                        var split2 = new string[split.Length + 1];
                        split.CopyTo(split2, 0);
                        split2[split.Length] = split1[1];
                        split = split2;
                    }

                    string name = null;
                    string contentType = null;
                    string fileName = null;

                    foreach (var str in split)
                    {
                        const string nameTag = "name=\"";
                        const string fileNameTag = "filename=\"";
                        const string contentTypeTag = "Content-Type: ";
                        if (str.StartsWith(nameTag))
                        {
                            name = str[nameTag.Length..^1];
                        }
                        else if (str.StartsWith(fileNameTag))
                        {
                            fileName = str[fileNameTag.Length..^1];
                        }
                        else if (str.StartsWith(contentTypeTag))
                        {
                            contentType = str[contentTypeTag.Length..];
                        }
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        current = new FormFile()
                        {
                            Name = name,
                            ContentType = Mime.FromString(contentType),
                            Filename = fileName
                        };

                        fileInitPosition = position;
                        seeking = true;
                    }
                    else
                    {
                        current = new FormInput()
                        {
                            Name = name
                        };
                        temp = new StringBuilder();
                    }
                }
            }

            void seekNextBlock()
            {
                for (int asd = 0; asd < boundary.Length + 1; asd++)
                {
                    ReadChar();
                }
            }

            void detectDataStart()
            {
                if (!blockPositionDetector && ArrayContainsPatternAtEnd(checkGroup, httpDelimiter))
                {
                    nextElementPosition = body.Position;
                    blockPositionDetector = true;
                }
            }

            void ReadChar()
            {
                nextChar = checkGroup[0];

                checkGroup = checkGroup.ShiftLeft(1, tempBuffer);

                position++;

                if (body.Position < body.Length)
                {
                    checkGroup[checkGroup.Length - 1] = (char)body.ReadByte();
                }

                detectDataStart();
            }

            void addElementToForm(BaseFormInput current)
            {
                if (current is FormFile file)
                {
                    // -2 to remove las \r\n para evitar corromper los datos del final del archivo
                    var EndPos = fileEndPosition - 2;
                    var FileLength = EndPos - fileInitPosition;

                    file.Content = body;
                    file.StartingPosition = fileInitPosition;
                    file.Length = FileLength;
                    form.FormFile.Add(file);
                }
                else if (current is FormInput input)
                {
                    input.Value = temp.ToString()[..^3];
                    form.FormInput.Add(input);
                }

                current = null;
            }

            static bool ArraysEqual(int[] a1, int[] a2)
            {
                //if (a1.Length != a2.Length)
                //{
                    //return false;
                //}
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            static bool ArrayContainsPatternAtEnd(int[] a1, int[] pattern)
            {
                int patternPosition = 0;
                for (int i = a1.Length - pattern.Length; i < a1.Length; i++)
                {
                    if (a1[i] != pattern[patternPosition])
                    {
                        return false;
                    }
                    patternPosition++;
                }
                return true;
            }

            return form;
        }
    }
}