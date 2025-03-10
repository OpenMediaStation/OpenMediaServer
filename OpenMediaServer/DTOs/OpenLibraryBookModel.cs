using System;
using System.Text.Json.Serialization;

namespace OpenMediaServer.DTOs;

public class OpenLibrarySearchResult
{
    public List<BookDoc> Docs { get; set; } = new();
}

public class BookDoc
{
    public string? Title { get; set; }

    [JsonPropertyName("author_name")]
    public List<string>? AuthorName { get; set; }

    public string? Key { get; set; }

    [JsonPropertyName("cover_i")]
    public int? CoverID { get; set; }
}

public class OpenLibraryWorkDetails
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("authors")]
    public List<OpenLibraryAuthor> Authors { get; set; } = new();

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("subject_places")]
    public List<string>? SubjectPlaces { get; set; }

    [JsonPropertyName("subjects")]
    public List<string>? Subjects { get; set; }

    [JsonPropertyName("subject_people")]
    public List<string>? SubjectPeople { get; set; }

    [JsonPropertyName("covers")]
    public List<int>? Covers { get; set; }

    [JsonPropertyName("latest_revision")]
    public int LatestRevision { get; set; }

    [JsonPropertyName("revision")]
    public int Revision { get; set; }

    [JsonPropertyName("created")]
    public OpenLibraryDate? Created { get; set; }

    [JsonPropertyName("last_modified")]
    public OpenLibraryDate? LastModified { get; set; }
}

public class OpenLibraryAuthor
{
    [JsonPropertyName("author")]
    public OpenLibraryAuthorKey? Author { get; set; }

    [JsonPropertyName("type")]
    public OpenLibraryType? Type { get; set; }
}

public class OpenLibraryAuthorKey
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}

public class OpenLibraryType
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}

public class OpenLibraryDate
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class BookEditionsResponse
{
    [JsonPropertyName("links")]
    public Links? Links { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("entries")]
    public List<BookEdition>? Entries { get; set; }
}

public class Links
{
    [JsonPropertyName("self")]
    public string? Self { get; set; }

    [JsonPropertyName("work")]
    public string? Work { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

public class BookEdition
{
    [JsonPropertyName("type")]
    public TypeInfo? Type { get; set; }

    [JsonPropertyName("authors")]
    public List<Author>? Authors { get; set; }

    [JsonPropertyName("isbn_13")]
    public List<string>? Isbn13 { get; set; }

    [JsonPropertyName("languages")]
    public List<Language>? Languages { get; set; }

    [JsonPropertyName("pagination")]
    public string? Pagination { get; set; }

    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    [JsonPropertyName("publishers")]
    public List<string>? Publishers { get; set; }

    [JsonPropertyName("source_records")]
    public List<string>? SourceRecords { get; set; }

    [JsonPropertyName("subjects")]
    public List<string>? Subjects { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("weight")]
    public string? Weight { get; set; }

    [JsonPropertyName("full_title")]
    public string? FullTitle { get; set; }

    [JsonPropertyName("works")]
    public List<Work>? Works { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("covers")]
    public List<int>? Covers { get; set; }

    [JsonPropertyName("number_of_pages")]
    public int? NumberOfPages { get; set; }

    [JsonPropertyName("latest_revision")]
    public int? LatestRevision { get; set; }

    [JsonPropertyName("revision")]
    public int? Revision { get; set; }

    [JsonPropertyName("created")]
    public DateInfo? Created { get; set; }

    [JsonPropertyName("last_modified")]
    public DateInfo? LastModified { get; set; }

    [JsonPropertyName("physical_format")]
    public string? PhysicalFormat { get; set; }
}

public class TypeInfo
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}

public class Author
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}

public class Language
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}

public class Work
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}

public class DateInfo
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
