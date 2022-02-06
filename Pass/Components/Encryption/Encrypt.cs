using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Bridgefield.PersistentBits.FileSystem;
using MonadicBits;
using Pass.Components.FileSystem;
using Pass.Models;

namespace Pass.Components.Encryption;

public static class Encrypt
{
    public static async Task EncryptPassword(
        this PasswordRepository passwordRepository,
        KeyRepository keyRepository,
        Password password)
    {
        await (from file in passwordRepository.Create(password.Name)
                from keyStream in keyRepository.PublicKey.Bind(keyFile => keyFile.OpenRead())
                from fingerPrintFile in passwordRepository.Fingerprint()
                from fingerPrint in fingerPrintFile.ReadFirstLine()
                select EncryptPassword(password, file, keyStream, fingerPrint))
            .Match(r => r, () => Task.CompletedTask);
    }

    private static async Task<Unit> EncryptPassword(
        Password password,
        IEncryptedFile file,
        Stream keyStream, string fingerPrint)
    {
        await file.Write(async s =>
        {
            await using var streamWriter = new StreamWriter(s);
            await streamWriter.WriteAsync(password.Value);
        }, keyStream, fingerPrint);
        return default;
    }

    private static Maybe<string> ReadFirstLine(this IFile file) =>
        file.OpenRead().Map(s =>
        {
            using var stream = s;
            using var streamReader = new StreamReader(s);
            return streamReader.ReadLine();
        });
}