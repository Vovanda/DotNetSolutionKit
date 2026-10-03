# Authentication and permissions

## Authentication

A service authenticates every request itself, with one composite scheme:

- a JWT, from the `Authorization: Bearer` header or the access-token cookie, when a `Jwt` section is
  configured. A service that only validates tokens needs the public key; the signing key stays with the
  service that issues them;
- an `X-API-Key` for calls between services and from the platform itself.

`Common.Web` has helpers that set and clear the access and refresh token cookies.

A generated service registers its API key handler in `Setup/Authentication.cs`:

```csharp
builder.SetupServiceAuthentication<ApiKeyAuthenticationHandler>();
builder.Services.AddAuthorization();
```

## Permissions

An action states the permissions it needs:

```csharp
[HttpPost]
[RequiredPermissions("orders.create")]
public Task<OrderResponse> Create(CreateOrderRequest request) => service.CreateAsync(request);
```

A global filter checks them before the action:

| Caller | Answer |
|---|---|
| anonymous | 401 |
| authenticated, missing a permission | 403, a problem naming the permissions |
| authenticated, holding every permission | the action runs |
| a system call, made by the platform itself | the action runs |

An action without `[RequiredPermissions]` is not checked. Swagger lists the permissions of each operation.

By default the permissions come from the token: `ClaimsPermissionService` reads the `permissions` claims,
one claim per permission, as a JSON array in a JWT becomes. It needs no other service, so it works for a
solution with a single service. A service whose permissions live elsewhere registers its own
`IPermissionService`.

Behind a [gateway](../features/api-gateway.md) the token is validated once, at the gateway, and a service
receives the user and the permissions in headers that come with the internal API key; the same filter
and the same `ClaimsPermissionService` check them. Permissions asked from a separate service are planned;
see issue [#5](https://github.com/Vovanda/DotNetSolutionKit/issues/5).

## Tests

`Common.Tests` checks on a test server an open action, an anonymous caller, a missing and a held
permission, and a system call.
