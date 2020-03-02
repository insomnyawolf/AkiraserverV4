using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            // Don't serialize a null object, simply return the default for that object
            if (source is null)
            {
                return default;
            }

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static Stream ToStream<TClass>(this TClass source)
        {
            if (!typeof(TClass).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            // Don't serialize a null object, simply return the default for that object
            if (source is null)
            {
                return default;
            }

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }

        public static TClass ToObject<TClass>(this Stream source)
        {
            if (!typeof(TClass).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            IFormatter formatter = new BinaryFormatter();
            return (TClass)formatter.Deserialize(source);
        }

        public static TClass[] AsArray<TClass>(this TClass item)
        {
            return new TClass[] { item };
        }

        public static string Padding(this object current, ushort minSize, char character = ' ', PaddingPosition position = PaddingPosition.Right)
        {
            string str = current.ToString();
            if (position == PaddingPosition.Left)
            {
                while (str.Length < minSize)
                {
                    str = character + str;
                }
            }
            else
            {
                while (str.Length < minSize)
                {
                    str += character;
                }
            }
            return str;
        }
    }

    public enum PaddingPosition
    {
        Left,
        Right
    }
}