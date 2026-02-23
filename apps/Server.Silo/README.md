# Orleans Identity Silo

This is the Microsoft Orleans silo for the identity system. It provides stateful User grains that persist data to Azure Table Storage.

## Features

- **User Grains**: Stateful grains that store user information with GUID-based keys
- **Azure Table Storage**: Used for both Orleans clustering and grain state persistence
- **Aspire Integration**: Fully integrated with .NET Aspire for orchestration and service discovery
- **Identity API Client**: The Server.Identity.Api project acts as an Orleans client to consume User grains

## Configuration

The silo is configured with:
- **Clustering**: Azure Table Storage (`orleans-clustering` table)
- **Grain Storage**: Azure Table Storage (`orleans-grain-storage` table)
- **Storage Emulator**: Uses Azurite for local development

## User Grain Operations

The `IUserGrain` interface provides the following operations:

- `GetAsync()`: Retrieve user data
- `SetAsync(User user)`: Create or update a user
- `UpdateAsync(...)`: Update specific user fields
- `DeleteAsync()`: Delete a user
- `ExistsAsync()`: Check if a user exists

## Testing the Silo

The silo includes test endpoints for basic User grain operations:

### Get User
```bash
GET /users/{guid}
```

### Create User
```bash
POST /users
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "firstName": "Test",
  "lastName": "User",
  "enabled": true,
  "emailVerified": false
}
```

### Update User
```bash
PUT /users/{guid}
Content-Type: application/json

{
  "username": "updateduser",
  "email": "updated@example.com",
  "firstName": "Updated",
  "lastName": "User",
  "enabled": true,
  "emailVerified": true
}
```

### Delete User
```bash
DELETE /users/{guid}
```

## Identity API Orleans Client

The `Server.Identity.Api` project is configured as an Orleans client and provides:

### Get User via Identity API
```bash
GET /api/users/{guid}
```

This endpoint demonstrates how other services can consume User grains through Orleans clients.

## Running with Aspire

The silo is automatically started when you run the Aspire AppHost project. It will:

1. Start the Azure Storage emulator (Azurite)
2. Create the necessary table storage containers
3. Start 2 replicas of the silo for high availability
4. Configure Orleans clustering between the replicas
5. Configure the Identity API as an Orleans client

## Production Considerations

The implementation includes:
- **Error Handling**: Comprehensive try-catch blocks in grain operations
- **Logging**: Structured logging for debugging and monitoring
- **Input Validation**: Null checks and defensive programming
- **Observability**: Integration with .NET Aspire telemetry

## Dependencies

- **Ray.Identity.Common**: Shared models and types
- **Ray.Identity.Consts**: Service constants and configuration
- **Ray.Identity.ServerKit.Orleans.Domain**: Grain interfaces
- **Ray.ServerKit.Aspire.ServiceDefaults**: Aspire service defaults

## Accessing from Other Services

Other services can access the User grains by:

1. Adding a reference to `Ray.Identity.ServerKit.Orleans.Domain`
2. Adding Orleans client packages (`Microsoft.Orleans.Client`, `Aspire.Orleans.Clustering.AzureStorage`)
3. Configuring Orleans client in their startup with `builder.UseOrleansClient()`
4. Using `IGrainFactory.GetGrain<IUserGrain>(guid)` to get grain instances

Example:
```csharp
// In Program.cs
builder.UseOrleansClient();

// In your service/controller
app.MapGet("/api/users/{id:guid}", async (Guid id, IGrainFactory grainFactory) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(id);
    var user = await userGrain.GetAsync();
    return user is not null ? Results.Ok(user) : Results.NotFound();
});
``` 