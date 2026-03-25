# MailboxSearch

MailboxSearch is a Windows Forms application for searching `.eml` files stored in a local folder tree.

The application scans a selected root folder recursively, finds email messages whose content matches the user query, and shows matching messages in a list with:

- title
- date
- sender
- file path

The app also provides:

- exact-phrase search when the full query is enclosed in double quotes
- keyword search with `OR` semantics for unquoted terms
- preview of the selected message
- double-click to open the original `.eml` file with the default Windows handler
- persistent storage of the selected source folder in the Windows registry
- on-disk cache in an `_cache` subfolder under the selected source folder
- incremental search results and progress updates during scanning

## Requirements

- Windows
- .NET 10 SDK preview or newer that supports `net10.0-windows`

This project currently targets `net10.0-windows`, so a stable older SDK will not build it without changing the target framework.

## Build

From the project directory:

```powershell
dotnet build
```

If the application is already running, Windows may lock `MailboxSearch.exe` during rebuild. In that case, close the running app and build again.

## Run

From the project directory:

```powershell
dotnet run
```

## Typical Use

1. Start the application.
2. Select a root folder that contains `.eml` files in one or more subfolders.
3. Enter a search query.
4. Review matching results as they appear during the scan.
5. Select a result to preview the message text.
6. Double-click a result to open the original `.eml` file in the default mail application.

## Search Behavior

### Keyword search

If the query is not wrapped in double quotes, the app splits the query into terms and returns messages containing any of those terms.

Example:

```text
invoice payment overdue
```

This matches messages containing `invoice` or `payment` or `overdue`.

### Exact phrase search

If the entire query is wrapped in double quotes, the app searches for that exact phrase.

Example:

```text
"umowa została podpisana"
```

### Supported message content

The search includes:

- subject
- sender text
- plain-text body
- HTML body converted to text

The solution also normalizes Unicode escape sequences such as `\u0119`, so Polish diacritic characters can be matched correctly when they appear in escaped form in message text.

## Caching

MailboxSearch creates a `_cache` subfolder inside the selected source folder.

For each `.eml` file, the cache stores:

- normalized searchable text
- parsed subject
- parsed sender
- parsed date
- preview text

The cache is reused on later searches when the source file has not changed. Cache invalidation is based on:

- source file last write time
- source file length
- internal cache version

## Use Cases

This application is suitable for:

- searching exported mailbox archives stored as `.eml` files
- reviewing historical email content stored in nested folders
- quickly locating messages by sender, subject text, or body text
- opening matching emails in the default Windows mail client or associated viewer
- searching mixed plain-text and HTML email exports without preprocessing them manually

## Limitations

- Windows only. The application uses Windows Forms and the Windows registry.
- The project currently requires a .NET 10 preview SDK because it targets `net10.0-windows`.
- Search is text-based only. It does not search attachments, embedded documents, or binary content.
- Exact phrase search only applies when the entire query is enclosed in double quotes.
- Unquoted terms are matched with `OR` semantics only. There is no support for `AND`, `NOT`, field-specific filters, or advanced query syntax.
- Results are streamed as they are found during scanning, so while the search is running the visible order reflects discovery order. The implementation does not continuously re-sort the displayed list during the active scan.
- HTML messages are previewed and searched as extracted text only. Original HTML formatting is not rendered in the preview pane.
- Cache invalidation uses file metadata, not a full content hash. In unusual cases, a changed file with identical size and timestamp could reuse stale cache data.
- The app scans one selected root folder at a time.
- The search is local only. There is no server-side index, database, or multi-user synchronization.
- Very large mail stores may still take time to scan on the first run, especially before the cache is populated.

## Project Files

- `Program.cs`: application entry point
- `Form1.cs`: WinForms UI behavior
- `Form1.Designer.cs`: WinForms layout definition
- `EmailSearchService.cs`: search, caching, parsing, progress, and cancellation logic
- `SearchQuery.cs`: query parsing
- `SearchTextNormalizer.cs`: text normalization and Unicode escape decoding
- `EmailSearchResult.cs`: search result model
- `FolderPreferenceStore.cs`: persisted folder selection in registry

## Notes

The selected source folder is stored under the current user registry hive so the same folder can be restored the next time the application starts.