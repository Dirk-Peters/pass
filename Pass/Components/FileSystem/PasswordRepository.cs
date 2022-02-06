using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bridgefield.PersistentBits.FileSystem;
using MonadicBits;
using Pass.Components.Extensions;

namespace Pass.Components.FileSystem;

using static Functional;

public sealed class PasswordRepository
{
    private const string PasswordFileEnding = ".gpg";
    private const string FingerPrintFileName = ".gpg-id";

    private readonly IDirectory rootDirectory;

    public PasswordRepository(IDirectory rootDirectory) =>
        this.rootDirectory = rootDirectory;

    public Maybe<IFile> Fingerprint() =>
        rootDirectory.Files.SingleOrNothing(file => file.Name == FingerPrintFileName);

    public Maybe<IEncryptedFile> Lookup(string name) =>
        All().SingleOrNothing(file => file.Name == name);

    public IEnumerable<IEncryptedFile> All() =>
        Directory
            .EnumerateFiles(rootDirectory.Path)
            .Where(IsPasswordFile)
            .Select(path => new EncryptedFile(path));

    public Maybe<IEncryptedFile> Create(string name) =>
        Lookup(name)
            .Match<Maybe<IEncryptedFile>>(
                _ => Nothing,
                () => new EncryptedFile(Path.Combine(rootDirectory.Path, name + PasswordFileEnding)));

    private static bool IsPasswordFile(string fileName) =>
        !fileName.StartsWith('.') && fileName.EndsWith(PasswordFileEnding);
}