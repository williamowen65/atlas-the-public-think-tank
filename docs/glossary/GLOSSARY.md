# Atlas domain glossary

This glossary defines shared product language. Code names may be more specific, but they should not contradict these meanings.

| Term | Meaning |
|---|---|
| Actor | The participant attempting an action. The actor may differ from the owner of the target resource. |
| Author | The participant credited with creating a node. Authorship is attribution and an input to authorization, not authorization by itself. |
| Boundary / bounded context | A model with explicit ownership, language, rules, and public interaction points. |
| Child / sub-node | A node that contains another node's ID in its parent collection. It remains a full node and may have multiple parents. |
| Content document | Content-owned descriptive material referenced by a node through a description identifier. It is plain text today and may become block-based later. |
| Contract | A versioned cross-boundary communication payload. It contains data, not domain behavior. |
| Domain event | A fact recorded by domain behavior about something that occurred. The current code keeps the name `DomainEvents` even when the public record is also used as an integration contract. |
| Integration event | A published, versioned fact that another boundary may consume or ignore. |
| Invariant | A rule that must always remain true for a domain object or boundary to be in a valid state, regardless of which screen, host, or workflow caused the change. For example, a node cannot be its own parent, and `UpdatedAt` cannot precede `CreatedAt`. A rule checked only by the Console is not yet a Graph invariant because another host could bypass it. |
| Ledger (Node creation quota) | The record of successful Node creations that consumed quota. Each `NodeCreationQuotaRow` stores the Node ID, participant ID and accounting timestamp in `NodeCreationQuotaRows`. Exempt creations and rolled-back operations do not consume quota. This history is separate from Node lifecycle so archiving or deleting a Node does not refund a recently consumed slot. |
| Node | The primary Graph entity representing a question, idea, issue, comment, or another typed contribution. |
| Node type | A globally reusable Graph definition identified by GUID. It controls classification and display behavior such as pluralization. |
| Parent | A node referenced by a child node's parent-ID collection. A node may have zero or multiple parents. |
| Participant | A person represented inside the Participants boundary by a public profile and stable ID. Authentication credentials are not currently part of this model. |
| Requested sub-node type | A type of response the node author invites. It does not prove that a child of that type exists and does not prevent other valid response types. |
| Relationship node | A node whose meaning connects multiple parents. Future rules may require at least two parents for particular relationship types. |
| Reconstitution | Rebuilding a domain object from its saved, data-only state. The repository reads stored values and supplies them to the domain model, producing an in-memory object that again has its methods and can enforce its invariants. Reconstitution preserves the original identity, status, relationships, and timestamps without replaying creation behavior or raising a new creation event. This resembles the hydration performed by an ORM, and Atlas uses EF Core to load persistence rows, then SQL repository adapters explicitly reconstitute domain objects. |
| Subscriber | A handler registered to receive one event type. The publisher does not require every possible consumer to subscribe. |
| Type pluralization | Presentation behavior that uses a counted plural label when enabled while leaving mass nouns such as Evidence unchanged. |

## Vocabulary questions still open

- The preferred product-facing distinction between author-requested and responder-supplied sub-node types.
- Whether Comment remains a Graph node type or later gains additional Discussion-boundary behavior.
- The final name for relationship-specific node types and their minimum-parent rules.
- Whether user, participant, member, and account remain separate concepts once authentication is introduced.

## HTTP hosting and test vocabulary

These implementation terms complement the product/domain language above. See
[HttpAbuseTests](../../tests/Atlas.Api.Tests/HttpAbuseTests.cs) and
[controller reference](../api/CONTROLLER-REFERENCE.md) for their use in Atlas.

| Term | Meaning in these API tests |
|---|---|
| API test factory / ApiFactory | Atlas's subclass of WebApplicationFactory<Program>. It starts the existing API with test configuration and creates clients; it does not generate a project or create a business domain. |
| Host / web host | The running application's configuration, service container, middleware and server lifetime. A host runs on a computer; it is not necessarily the computer itself. |
| TestServer | ASP.NET Core's in-memory HTTP test server. It exercises the application pipeline without a real network listener or TLS handshake. Kestrel-specific limits require a Kestrel test. |
| Kestrel | ASP.NET Core's web-server software. It accepts network connections, handles HTTP and enforces server limits. Atlas's body-size test starts it on the local machine. |
| Web host builder | The configuration interface used to adjust a test host before startup. WithWebHostBuilder derives a customized factory; ConfigureServices changes its dependency-injection registrations. |
| HTTP client / handler | HttpClient sends requests. HttpClientHandler manages transport behavior such as cookies and TLS certificate validation. Neither renders a page or executes browser JavaScript. |
| Cookie jar | A client's stored cookies, retained from Set-Cookie responses and automatically sent on matching requests. Separate test clients have separate jars, like separate browser profiles. |
| CSRF token pair | The antiforgery cookie plus the explicit request token sent in X-CSRF-TOKEN. A request token from client A does not match client B's cookie, so the cross-jar test expects 400. These are not the login/session cookie. |
| Origin / CORS preflight | An origin is scheme, host and port. A preflight is an OPTIONS permission inquiry before certain browser cross-origin requests. HttpClient does not enforce CORS; the tests inspect the server's permission headers. |
| Probe controller / application part | A test-only controller exposing controlled hold/body endpoints. AddApplicationPart makes MVC discover its assembly in the derived test factory; it does not add those routes to the production API. |
| AbuseGate / rendezvous | A test coordination object shared by the test and probe handlers. It signals when 64 admitted requests are waiting, then releases them together so saturation and recovery can be asserted. It is not production rate limiting. |
| TaskCompletionSource | A manually completed asynchronous signal. One side awaits its Task; another completes it with TrySetResult. Here Full signals admission of 64 handlers, and Release lets the handlers finish. |
| Loopback / ephemeral port | Loopback traffic stays on this machine. Binding port 0 asks the operating system to choose a free local port, which the test then reads from the server's address feature. |
| RSA | Rivest-Shamir-Adleman, an asymmetric cryptographic algorithm using a public/private key pair. RSA.Create(2048) generates a temporary pair; the private key signs the test certificate. The pair is not itself an encrypted key. |
| CertificateRequest | A .NET object describing a certificate to create. It is not a user's HTTP request. In this test its subject is CN=localhost, and CreateSelfSigned creates the certificate locally. |
| Self-signed certificate | A certificate signed with its own private key instead of a trusted certificate authority. The test generates one for its temporary HTTPS server; it does not enroll a public production certificate. |
| Certificate validation callback / fingerprint | The handler calls the callback during TLS server-certificate validation. This test accepts only a fingerprint matching its generated certificate. That narrowly scoped rule replaces normal public certificate validation for this client only. |
| Known-length / chunked body | A known-length body declares Content-Length. The test's unknown-length HttpContent forces HTTP/1.1 chunked streaming so the server must enforce its ceiling as bytes arrive. |
| Boundary assertion | A check on both sides of a limit: 65,537 bytes must get 413, while exactly 65,536 bytes must get 204 from the raw-body probe. Domain/JSON constraints can impose smaller limits on actual business requests. |

Reference: [.NET server certificate validation callback](https://learn.microsoft.com/dotnet/api/system.net.http.httpclienthandler.servercertificatecustomvalidationcallback?view=net-10.0).
