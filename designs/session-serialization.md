# Session serialization

Session serialization is provided through the `ISessionSerializer` type. There are two modes that are available:

## Common structure

```mermaid
packet-beta
0: "M"
1-10: "Session Id (Variable length)"
11: "N"
12: "A"
13: "R"
14: "T"
15: "C"
16-24: "Key 1 Blob"
25-33: "Key 2 Blob"
34-42: "..."
43-50: "Flags (variable)"
```

Where:
- *M*: Mode
- *N*: New session
- *A*: Abandoned
- *R*: Readonly
- *T*: Timeout
- *C*: Key count

## Flags

Flags allow for additional information to be sent either direction that may not be known initially. This field was added v2 but is backwards compatible with the v1 deserializer and will operate as a no-op as it just reads the things it knows about and doesn't look for the end of a payload.

Structure:

```mermaid
packet-beta
0: "C"
1: "F1"
2: "F1L"
3-10: "Flag1 specific payload"
11: "F2"
12: "F2L"
13-20: "Flag2 specific payload"
21-25: "..."
```

Where:
- *Fn*: Flag `n`

Where `C` is the count of flags, and each `Fn` is a flag identifier an int with 7bit encoding. Each f

An example is the flag section used to indicate that there is support for diffing a session state on the server:

```mermaid
packet-beta
0: "1"
1: "100"
2: "0" 
```

## Full Copy (Mode = 1)

The following is the structure of the key blobs when the full state is serialized:

```mermaid
packet-beta
0-10: "Key name"
11-20: "Serialized value"
```

## Diffing Support (Mode = 2)

The following is the structure of the key blobs when only the difference is serialized:

```mermaid
packet-beta
0-10: "Key name"
11: "S"
12-20: "Serialized value"
```

Where:
- *S*: A value indicating the change the key has undergone from the values in `SessionItemChangeState`

