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

            var boundaryStr = "--" + parts[1];
            var boundary = boundaryStr.ToCharArray();
            char[] checkGroup = new char[boundary.Length + 1];

            var body = RawPayload;


            BaseFormInput current = null;
            Form form = new Form();

            StringBuilder temp = new StringBuilder();

            bool seeking = true;

            long fileInitPosition = 0;
            long fileEndPosition = 0;

            long positionDetected = 0;

            long position = -checkGroup.Length + 1;

            bool blockPositionDetector = false;

            while (position < body.Length + checkGroup.Length)
            {
                if (boundary.PatternEquals(checkGroup[1..]))
                {
                    fileEndPosition = position;
                    seekNextBlock();
                    ReadChar();
                    ReadChar();
                    await addElementToForm(current);
                    current = null;
                    temp = new StringBuilder();
                    seeking = false;
                    blockPositionDetector = false;
                }

                ReadChar();

                detectDataStart();

                if (!seeking)
                {
                    temp.Append(checkGroup[0]);
                }

                if (blockPositionDetector && !seeking && position == positionDetected  && temp.Length > 0)
                {
                    string formDataRaw = temp.ToString();

                    formDataRaw = formDataRaw[..^4];

                    var split1 = formDataRaw.Split("\r\n");
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

                void seekNextBlock()
                {
                    for (int asd = 0; asd < boundary.Length; asd++)
                    {
                        for (int z = 1; z < checkGroup.Length; z++)
                        {
                            checkGroup[z - 1] = checkGroup[z];
                        }

                        position++;
                        checkGroup[^1] = (char)body.ReadByte();

                        detectDataStart();
                    }
                }

                void detectDataStart()
                {
                    if (!blockPositionDetector && HttpDelimiter.PatternEquals(checkGroup[(checkGroup.Length - HttpDelimiter.Length)..]))
                    {
                        positionDetected = body.Position;
                        blockPositionDetector = true;
                    }
                }

                void ReadChar()
                {
                    for (int i = 1; i < checkGroup.Length; i++)
                    {
                        checkGroup[i - 1] = checkGroup[i];
                    }

                    position++;

                    if (body.Position < body.Length)
                    {
                        checkGroup[^1] = (char)body.ReadByte();
                    }
                }

                
            }

            await addElementToForm(current);

            async Task addElementToForm(BaseFormInput current)
            {
                if (current is FormFile file)
                {
                    var bodyLastPosition = body.Position;
                    var streamLenght = body.Length;

                    var StartPos = positionDetected;
                    // -2 to remove las \r\n para evitar corromper los datos del final del archivo
                    var EndPos = fileEndPosition - 2;
                    var FileLength = EndPos - StartPos;

                    body.Position = StartPos;
                    body.SetLength(StartPos + FileLength);

                    var ms = new MemoryStream();
                    await body.CopyToAsync(ms).ConfigureAwait(false);
                    file.Content = ms;
                    form.FormFile.Add(file);

                    body.SetLength(body.Length);
                    body.Position = bodyLastPosition;
                }
                else if (current is FormInput input)
                {
                    input.Value = temp.ToString()[..^2];
                    form.FormInput.Add(input);
                }
            }

            return form;
        }
    }
}