namespace MonadicTypes.AspNetCore;

// HTTP status identity is deliberately kept at the ASP.NET boundary. ErrorType
// remains a domain category and does not become a protocol registry.
internal static class HttpStatusIdentity
{
    private const string TypePrefix = "urn:problem-type:http-";

    internal static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        402 => "Payment Required",
        403 => "Forbidden",
        404 => "Not Found",
        405 => "Method Not Allowed",
        406 => "Not Acceptable",
        407 => "Proxy Authentication Required",
        408 => "Request Timeout",
        409 => "Conflict",
        410 => "Gone",
        411 => "Length Required",
        412 => "Precondition Failed",
        413 => "Content Too Large",
        414 => "URI Too Long",
        415 => "Unsupported Media Type",
        416 => "Range Not Satisfiable",
        417 => "Expectation Failed",
        421 => "Misdirected Request",
        422 => "Unprocessable Content",
        423 => "Locked",
        424 => "Failed Dependency",
        425 => "Too Early",
        426 => "Upgrade Required",
        428 => "Precondition Required",
        429 => "Too Many Requests",
        431 => "Request Header Fields Too Large",
        451 => "Unavailable For Legal Reasons",
        499 => "Client Closed Request",
        500 => "Internal Server Error",
        501 => "Not Implemented",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        504 => "Gateway Timeout",
        505 => "HTTP Version Not Supported",
        506 => "Variant Also Negotiates",
        507 => "Insufficient Storage",
        508 => "Loop Detected",
        510 => "Not Extended",
        511 => "Network Authentication Required",
        _ => "HTTP error"
    };

    internal static string GetTypeUri(int statusCode) => HttpStatusTypeUris.Get(statusCode);

    // The bounded array is initialized only when an HTTP identity is requested.
    // Each URI is then created independently so an application pays only for
    // statuses it actually emits.
    private static class HttpStatusTypeUris
    {
        private static readonly string?[] Values = new string[200];

        // Prevent beforefieldinit from moving cache initialization onto the
        // category-only path. HTTP-specific responses own this cold cost.
        static HttpStatusTypeUris()
        {
        }

        internal static string Get(int statusCode)
        {
            ref string? location = ref Values[statusCode - 400];
            string? existing = Volatile.Read(ref location);
            if (existing is not null)
            {
                return existing;
            }

            string created = string.Create(TypePrefix.Length + 3, statusCode, static (destination, value) =>
            {
                TypePrefix.AsSpan().CopyTo(destination);
                destination[^3] = (char)('0' + (value / 100));
                destination[^2] = (char)('0' + ((value / 10) % 10));
                destination[^1] = (char)('0' + (value % 10));
            });
            return Interlocked.CompareExchange(ref location, created, comparand: null) ?? created;
        }
    }
}
