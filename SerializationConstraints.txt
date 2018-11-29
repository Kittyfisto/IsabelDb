# Serialization constraints

This page helps you to find out what is and what isn't possible with this database.

## Enums

This library supports serialization of enums so as long as they have one of the following underlying types:
- System.Byte
- System.SByte
- System.UInt16
- System.Int16
- System.Int32
The following types are unfortunately not supported as underlying types:
- System.UInt32
- System.Int64
- System.UInt64

