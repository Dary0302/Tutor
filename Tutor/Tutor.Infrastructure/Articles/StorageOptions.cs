namespace Tutor.Infrastructure.Articles;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string BucketName { get; set; } = "kurator-backet"!;
    public string ServiceUrl { get; set; } = "https://s3.yandexcloud.net";
    public string Region { get; set; } = "ru-central1";
    public int PreSignedUrlTtlMinutes { get; set; } = 30;
}