// Copyright (C) 2022 VIRNECT CO., LTD.
// All rights reserved.

namespace VIRNECT {
/// <summary>
/// Interface declaration to utilize external scripts as image input source
/// </summary>
public interface ExternalImageSourceInterface
{
    /// <summary>
    /// Return the native texture pointer of the Texture which will be used as a Track-Framework input.
    /// Actual synchronization of content will happen when calling GL.IssuePluginEvent(LibraryInterface.GetPrepareExternalInputTextureEventFunc(), 0);
    /// </summary>
    /// <returns>Native texture pointer</returns>
    System.IntPtr GetTexturePointer();
}
}