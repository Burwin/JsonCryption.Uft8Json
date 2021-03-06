# JsonCryption.Utf8Json
JsonCryption.Utf8Json offers Field Level Encryption (FLE) when serializing/deserializing between .NET objects and JSON.

![Build and Test](https://github.com/Burwin/JsonCryption.Uft8Json/workflows/Build%20and%20Test/badge.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Burwin_JsonCryption.Uft8Json&metric=alert_status)](https://sonarcloud.io/dashboard?id=Burwin_JsonCryption.Uft8Json)

### Installation

```
// Package Manager
Install-Package JsonCryption.Utf8Json

// .NET CLI
dotnet add package JsonCryption.Utf8Json
```

### Motivation
Field Level Encryption of C# objects during JSON serialization/deserialization should be:
- Relatively easy to use
- Powered by industry-standard cryptography best-practices

#### Relatively Easy to Use
With default configuration, encrypting a field/property just requires decorating it with `EncryptAttribute`, and serializing the object as usual:
```
// decorate properties to be encrypted
class Foo
{
    [Encrypt]
    public string MySecret { get; set; }
}

// serialize as normal
Foo foo = new Foo() { ... };
JsonSerializer.Serialize(foo);
```

More details on usage scenarios can be found below.

#### Industry-standard Cryptography
Currently, JsonCryption.Utf8Json is built on top of the `Microsoft.AspNetCore.DataProtection` library for handling encryption-related responsibilities:
- Encryption/decryption
- Key management
- Algorithm management
- etc.

Internally, we only depend on the two interfaces `IDataProtector` and `IDataProtectionProvider`. If you don't want to use Microsoft's implementations, you could just depend on `Microsoft.AspNetCore.DataProtection.Abstractions` and provide alternative implementations of `IDataProtector` and `IDataProtectionProvider`. One use case for this functionality might be creating a segregated `IDataProtector` per user, potentially making it easy to support GDPR's "right to forget" user data.

### Supported Types
JsonCryption.Utf8Json should support any type serializable by Utf8Json. If you spot a missing type or find odd behavior, please let me know (or better yet, create a PR!).

### Getting Started
#### Configuration
##### Step 1: Configure Microsoft.AspNetCore.DataProtection
JsonCryption.Utf8Json depends on the `Microsoft.AspNetCore.DataProtection` library. Therefore, you should first ensure that your DataProtection layer is [configured properly](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/).

##### Step 2: Configure Utf8Json
Next, you'll need to set your default `IJsonFormatterResolver` to be an instance of `EncryptedResolver`, which should have a Singleton lifetime in your app.

`EncryptedResolver` takes two arguments:
- An instance of `Utf8Json.IJsonFormatterResolver`
- An instance of `Microsoft.AspNetCore.DataProtection.Abstractions.IDataProtectionProvider`

The `IJsonFormatterResolver` serves two purposes. It is used to:
- Serialize/deserialize when encryption isn't needed for a given field/property
- Do unencrypted serialization/deserialization in the encrypted chain, prior to encrypting and after decrypting the resulting JSON string

The `IDataProtectionProvider` will provide an instance of `IDataProtector`, which is what encrypts your data.

###### Default Setup
```
IJsonFormatterResolver fallbackResolver = StandardResolver.AllowPrivate;
IDataProtectionProvider dataProtectionProvider = ...;
IJsonFormatterResolver encryptedResolver = new EncryptedResolver(fallbackResolver, dataProtectionProvider);
JsonSerializer.SetDefaultResolver(encryptedResolver);
```

###### Per Usage Setup
Alternatively, you can abstain from setting the default `IJsonFormatterResolver`, and instead explicitly pass an instance of `EncryptedResolver` whenever you want encryption functionality:
```
JsonSerializer.Serialize(foo, encryptedResolver);
JsonSerializer.Deserialize<Foo>(json, encryptedResolver);
```

#### Usage
Once configured, using JsonCryption.Utf8Json is just a matter of decorating the properties/fields you wish to encrypt with the `EncryptAttribute` and serializing your C# objects as you normally would:
```
var myFoo = new Foo("If the Foo shits, wear it.", "JsonCryption.Utf8Json");

class Foo
{
    [Encrypt]
    public string LaunchCodes { get; }
  
    public string FavoriteNugetPackage { get; }
	
	public Foo(string launchCodes, string favoriteNugetPackage)
	{
		LaunchCodes = launchCodes;
		FavoriteNugetPackage = favoriteNugetPackage;
	}
}

// serializing
var bytes = JsonSerializer.Serialize(myFoo);
var json = Encoding.Utf8.GetString(bytes);

// deserializing
var fromBytes = JsonSerializer.Deserialize<Foo>(bytes);
var fromJson = JsonSerializer.Deserialize<Foo>(json);
```

### Special Stuff
As much as possible, I'm trying to keep annotations usage as close to parity with Utf8Json as possible. Here's a current sampling:

#### Constructors
JsonCryption.Utf8Json resolves the constructor used during deserialization in a couple steps. It shouldn't matter whether or not the constructor is public or private. See the tests for details.
1. If a constructor is decorated with `SerializationConstructorAttribute`, it's the constructor that will be used
```
class Foo
{
	[Encrypt]
	public int MyInt { get; }
	
	// This constructor will be used
	[SerializationConstructor]
	private Foo() { }
	
	public Foo(int myInt) { ... }
}
```
2. Otherwise, we try to find the constructor with the most parameter matches (by name, case-insensitive)
```
class Foo
{
	[Encrypt]
	public int MyInt { get; }
	
	[Encrypt]
	public string MyString { get; }
	
	public Foo() { }
	public Foo(int myInt) { ... }
	
	// This constructor will be used
	public Foo(int myInt, string myString) { ... }
}
```
3. Otherwise, we use the default constructor

After the object is rehydrated via the resolved constructor, individual serialized fields and properties not covered by the constructor will still be set.

#### Non-public Properties and Fields
Set the `fallbackResolver` of the `EncryptedResolver` to any `IJsonFormatterResolver` with `AllowPrivate` set to true. Then it should just work.

#### Custom JSON Serialized Property Names
To customize the name used for the field/property in the resulting JSON, decorate the field/property with `DataMemberAttribute` and provide a `Name`:
```
class Foo
{
	[Encrypt]
	[DataMember(Name = "launchCode")]
	public int MyInt { get; }
}
```

#### Ignored Fields/Properties
To ignore a given field/property, decorate it with `IgnoreDataMemberAttribute`:
```
class Foo
{
	[IgnoreDataMember]
	public int MyInt { get; }
}
```

### Future Plans
JsonCryption.Utf8Json is open to PRs...

Future projects/enhancements:
- Benchmarking
