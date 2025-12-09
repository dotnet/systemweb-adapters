# Architecture

## Visual Studio Debugger Attachment

> **Note**: This custom debugger attachment is necessary because Aspire's DCP (Distributed Control Plane) doesn't currently support attaching debuggers to arbitrary processes. The extension works around this limitation by directly automating Visual Studio through COM interop.

When running IIS Express projects from Aspire, the debugger is automatically attached through a multi-step process:

1. **Detection**: On resource startup, the extension subscribes to resource lifecycle events to detect when the IIS Express process starts
2. **COM Discovery**: Uses the Windows Running Object Table (ROT) to enumerate active Visual Studio instances via COM interop
3. **Instance Matching**: Locates the VS instance debugging the current Aspire host by matching process IDs
4. **DTE Automation**: Uses the EnvDTE object model (manually defined COM interfaces to avoid dependencies) to programmatically attach the debugger
5. **Process Attachment**: Attaches to the IIS Express process with the appropriate debug engines (native + managed)

The implementation uses:
- **COM/ROT**: For discovering running VS instances
- **EnvDTE**: VS automation object model (custom interface definitions)
- **STA Thread + Message Pump**: Required for COM interop with Visual Studio
- **Aspire Resource Events**: For timing the attachment to the IIS process lifecycle