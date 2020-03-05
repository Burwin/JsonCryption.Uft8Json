﻿using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Utf8Json.FLE
{
    /// <summary>
    /// Set to <see cref="JsonSerializer"/>'s default resolver to get built-in field-level encryption
    /// of fields and properties marked with the <see cref="EncryptAttribute"/>:
    /// 
    /// // configure
    /// IDataProtectionProvider dataProtectionProvider = ...
    /// var resolver = new EncryptedResolver(StandardResolver.AllowPrivate, dataProtectionProvider);
    /// JsonSerializer.SetDefaultResolver(resolver);
    /// 
    /// // set up class
    /// class Foo
    /// {
    ///     [Encrypt]
    ///     public int MyInt { get; set; }
    /// }
    /// 
    /// // serialize/deserialize
    /// var instance = new Foo { MyInt = 75 };
    /// JsonSerializer.Serialize(instance);
    /// 
    /// </summary>
    public sealed class EncryptedResolver : IJsonFormatterResolver
    {
        private readonly IJsonFormatterResolver _fallbackResolver;
        private readonly IDataProtectorFactory _dataProtectorFactory;
        private readonly ConcurrentDictionary<Type, IJsonFormatter> _formatterCache = new ConcurrentDictionary<Type, IJsonFormatter>();

        /// <summary>
        /// Creates a new <see cref="EncryptedResolver"/> with a given fallback default <see cref="IJsonFormatterResolver"/>
        /// and an <see cref="IDataProtector"/>
        /// </summary>
        /// <param name="fallbackResolver">Used to serialize properties that do not need encryption</param>
        /// <param name="dataProtectorFactory"></param>
        public EncryptedResolver(IJsonFormatterResolver fallbackResolver, IDataProtectorFactory dataProtectorFactory)
        {
            _fallbackResolver = fallbackResolver;
            _dataProtectorFactory = dataProtectorFactory;
        }
        
        /// <summary>
        /// Resolves an encryption-ready <see cref="IJsonFormatter{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IJsonFormatter<T> GetFormatter<T>()
        {
            var formatter = _formatterCache.GetOrAdd(typeof(T), CreateFormatter<T>()) as IJsonFormatter<T>;
            return formatter;
        }

        private IJsonFormatter<T> CreateFormatter<T>() => new EncryptedFormatter<T>(_dataProtectorFactory, _fallbackResolver, ExtendedMemberInfo.GetFrom(typeof(T), _fallbackResolver).ToArray());
    }
}
