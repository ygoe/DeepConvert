# DeepConvert

Converts a data type to another data type, including collections and their items. With special support for DateTime conversions.

[![NuGet](https://img.shields.io/nuget/v/Unclassified.DeepConvert.svg)](https://www.nuget.org/packages/Unclassified.DeepConvert)

Supports **.NET Standard 2.0** and **.NET Framework 4.5**.

## Description

The `System.Convert` class provides methods to convert values from one type into another. This is restricted to the `IConvertible` interface which mainly covers basic numeric types and a few more like boolean, string or DateTime. Its efforts in trying to convert more obscure are very limited. Collections of values are completely unsupported by that class.

Serialisation and deserialisation libraries provide data in a very specific structure that may not match the program’s requirements. Converting an array of integer values into a list of long values or the like often require manual copy implementations. It gets even more complicated when mapping a dictionary with objects to one with strings, or even a flat list with key/value pairs to a dictionary.

The `DeepConvert` class solves all these problems by supporting a larger number of basic data types and many collections thereof. Its special tricks include understanding localised boolean values and extended parsing and converting of time-based values. This also allows conversions between .NET ticks, UNIX timestamp seconds or JavaScript timestamp milliseconds.

## Tests

This library is covered by some unit tests but has no near full code coverage yet. The unit tests can be used to get an overview of supported conversions until a more detailed documentation is available.

## License

[MIT license](https://github.com/ygoe/DeepConvert/blob/master/LICENSE)
