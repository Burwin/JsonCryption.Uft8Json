﻿using JsonCryption.Encrypters;
using System;
using System.Text.Json;

namespace JsonCryption.Converters
{
    // TODO: I think this needs to be a factory?
    internal sealed class EnumConverter : EncryptedConverter<Enum>
    {
        public EnumConverter(Encrypter encrypter, JsonSerializerOptions options) : base(encrypter, options)
        {
        }

        public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options) => throw new NotImplementedException();
    }
}