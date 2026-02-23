namespace Server.Silo;

public interface IDocumentGrain : IGrainWithGuidKey
{
//   /// <summary>
//   /// Retrieves the current user identity data from the grain's persistent state.
//   /// </summary>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing the <see cref="UserAccount"/> if the user exists,
//   /// or <c>null</c> if no user data has been set for this grain instance.
//   /// </returns>
//   /// <remarks>
//   /// This method provides read-only access to the user's identity information stored in the grain state.
//   /// The operation is optimized for performance and will return cached data from the grain's memory
//   /// when available, avoiding unnecessary database calls.
//   /// <para>
//   /// If the grain has not been initialized with user data, this method returns <c>null</c>,
//   /// indicating that no user exists for the current grain key.
//   /// </para>
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// var user = await userGrain.GetAsync();
//   /// 
//   /// if (user != null)
//   /// {
//   ///     Console.WriteLine($"User: {user.Username} ({user.Email})");
//   /// }
//   /// </code>
//   /// </example>
//   [Alias("GetAsync")]
//   ValueTask<GetResponse?> GetAsync();
//
//   /// <summary>
//   /// Creates or updates the complete user identity data in the grain's persistent state.
//   /// </summary>
//   /// <param name="user">The complete <see cref="UserAccount"/> data to store for this user.</param>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing the saved <see cref="UserAccount"/> after successful persistence.
//   /// </returns>
//   /// <remarks>
//   /// This method performs a complete replacement of the user's identity data in the grain state.
//   /// The operation is atomic and will persist the data to the configured storage provider before returning.
//   /// <para>
//   /// If this is the first time data is set for the grain, it effectively creates a new user.
//   /// If user data already exists, it completely replaces the existing data with the provided information.
//   /// </para>
//   /// <para>
//   /// The returned user data includes any server-side modifications or validations that may have
//   /// been applied during the save operation.
//   /// </para>
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// var newUser = new UserIdentity
//   /// {
//   ///     Id = userId.ToString(),
//   ///     Username = "john_doe",
//   ///     Email = "john@example.com",
//   ///     FirstName = "John",
//   ///     LastName = "Doe"
//   /// };
//   /// 
//   /// var savedUser = await userGrain.SetAsync(newUser);
//   /// </code>
//   /// </example>
//   [Alias("SetAsync")]
//   ValueTask<GetResponse> SetAsync(GetResponse user);
//
//   /// <summary>
//   /// Updates specific user properties while preserving other existing user data.
//   /// </summary>
//   /// <param name="username">Optional username to update. Pass <c>null</c> to leave unchanged.</param>
//   /// <param name="email">Optional email address to update. Pass <c>null</c> to leave unchanged.</param>
//   /// <param name="firstName">Optional first name to update. Pass <c>null</c> to leave unchanged.</param>
//   /// <param name="lastName">Optional last name to update. Pass <c>null</c> to leave unchanged.</param>
//   /// <param name="enabled">Optional enabled status to update. Pass <c>null</c> to leave unchanged.</param>
//   /// <param name="emailVerified">Optional email verification status to update. Pass <c>null</c> to leave unchanged.</param>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing the updated <see cref="UserAccount"/> after successful persistence.
//   /// </returns>
//   /// <remarks>
//   /// This method provides selective updating of user properties without requiring the complete user object.
//   /// Only the properties that are provided (non-null) will be updated, while other properties remain unchanged.
//   /// <para>
//   /// The operation is atomic and will persist changes to the configured storage provider before returning.
//   /// The returned user data reflects the complete user record after applying the partial updates.
//   /// </para>
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// 
//   /// // Update only email and last name
//   /// var updatedUser = await userGrain.UpdateAsync(
//   ///     username: null,  // Keep existing username
//   ///     email: "newemail@example.com",
//   ///     firstName: null,  // Keep existing first name
//   ///     lastName: "NewLastName"
//   /// );
//   /// </code>
//   /// </example>
//   [Alias("UpdateAsync")]
//   ValueTask<GetResponse> UpdateAsync(
//     string? username = null, 
//     string? email = null,
//     string? firstName = null, 
//     string? lastName = null,
//     bool? enabled = null, 
//     bool? emailVerified = null);
//
//   /// <summary>
//   /// Removes all user identity data from the grain's persistent state.
//   /// </summary>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing <c>true</c> if the user was deleted successfully, 
//   /// or <c>false</c> if no user data existed to delete.
//   /// </returns>
//   /// <remarks>
//   /// This method permanently deletes all user data associated with this grain instance from both
//   /// the grain's memory state and the persistent storage. This operation cannot be undone.
//   /// <para>
//   /// After deletion, subsequent calls to <see cref="GetAsync"/> will return <c>null</c>, and
//   /// the grain will behave as if no user data was ever set for this grain key.
//   /// </para>
//   /// <para>
//   /// The operation is atomic and will complete the deletion from persistent storage before returning.
//   /// </para>
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// 
//   /// // Permanently delete the user
//   /// var wasDeleted = await userGrain.DeleteAsync();
//   /// 
//   /// if (wasDeleted)
//   /// {
//   ///     Console.WriteLine("User was successfully deleted");
//   /// }
//   /// </code>
//   /// </example>
//   [Alias("DeleteAsync")]
//   ValueTask<bool> DeleteAsync();
//
//   /// <summary>
//   /// Checks if the user exists in the grain's persistent state.
//   /// </summary>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing <c>true</c> if the user exists, 
//   /// otherwise <c>false</c> if no user data has been set for this grain instance.
//   /// </returns>
//   /// <remarks>
//   /// This method provides a quick way to check if a user exists without retrieving the full user data.
//   /// It's useful for validation scenarios where you only need to confirm user existence.
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// 
//   /// if (await userGrain.ExistsAsync())
//   /// {
//   ///     var user = await userGrain.GetAsync();
//   ///     // Process existing user
//   /// }
//   /// </code>
//   /// </example>
//   [Alias("ExistsAsync")]
//   ValueTask<bool> ExistsAsync();
//
//   /// <summary>
//   /// Updates the last login time for the user.
//   /// </summary>
//   /// <param name="loginTime">The login time to set. If null, uses current UTC time.</param>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing the updated <see cref="UserAccount"/> after successful persistence.
//   /// </returns>
//   /// <remarks>
//   /// This method updates only the last login timestamp while preserving all other user data.
//   /// It's typically called after successful user authentication to track user activity.
//   /// <para>
//   /// If no login time is provided, the current UTC time will be used automatically.
//   /// </para>
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// 
//   /// // Update with current time
//   /// await userGrain.UpdateLastLoginTimeAsync();
//   /// 
//   /// // Update with specific time
//   /// await userGrain.UpdateLastLoginTimeAsync(DateTimeOffset.UtcNow.AddMinutes(-5));
//   /// </code>
//   /// </example>
//   [Alias("UpdateLastLoginTimeAsync")]
//   ValueTask<GetResponse> UpdateLastLoginTimeAsync(DateTimeOffset? loginTime = null);
//
//   /// <summary>
//   /// Updates the entire custom attributes dictionary for the user.
//   /// </summary>
//   /// <param name="customAttributes">The complete dictionary of custom attributes to set for the user.</param>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing the updated <see cref="UserAccount"/> after successful persistence.
//   /// </returns>
//   /// <remarks>
//   /// This method replaces the entire CustomAttributes dictionary with the provided values.
//   /// Any existing custom attributes that are not included in the new dictionary will be removed.
//   /// Standard user properties (username, email, etc.) are preserved unchanged.
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// 
//   /// var attributes = new Dictionary&lt;string, object&gt;
//   /// {
//   ///     ["Department"] = "Engineering",
//   ///     ["Role"] = "Senior Developer",
//   ///     ["LastLogin"] = DateTimeOffset.UtcNow
//   /// };
//   /// 
//   /// var updatedUser = await userGrain.UpdateCustomAttributesAsync(attributes);
//   /// </code>
//   /// </example>
//   [Alias("UpdateCustomAttributesAsync")]
//   ValueTask<GetResponse> UpdateCustomAttributesAsync(Dictionary<string, object> customAttributes);
//
//   /// <summary>
//   /// Sets or updates a single custom attribute for the user.
//   /// </summary>
//   /// <param name="key">The attribute key/name to set.</param>
//   /// <param name="value">The attribute value to set.</param>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing the updated <see cref="UserAccount"/> after successful persistence.
//   /// </returns>
//   /// <remarks>
//   /// This method updates or adds a single custom attribute while preserving all other custom attributes
//   /// and standard user properties. If the attribute already exists, its value will be updated.
//   /// If the grain is not yet initialized, it will be initialized with minimal state.
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// 
//   /// // Set a simple attribute
//   /// await userGrain.SetCustomAttributeAsync("Department", "Engineering");
//   /// 
//   /// // Set a complex attribute
//   /// await userGrain.SetCustomAttributeAsync("Preferences", new { Theme = "Dark", Language = "en-US" });
//   /// </code>
//   /// </example>
//   [Alias("SetCustomAttributeAsync")]
//   ValueTask<GetResponse> SetAttributeAsync(string key, object value);
//
//   /// <summary>
//   /// Removes a custom attribute from the user.
//   /// </summary>
//   /// <param name="key">The attribute key/name to remove.</param>
//   /// <returns>
//   /// A <see cref="ValueTask{T}"/> containing the updated <see cref="UserAccount"/> after successful persistence.
//   /// </returns>
//   /// <remarks>
//   /// This method removes the specified custom attribute while preserving all other custom attributes
//   /// and standard user properties. If the attribute does not exist, the operation succeeds without error.
//   /// If the user grain is not initialized, this method returns the current state without changes.
//   /// </remarks>
//   /// <example>
//   /// <code>
//   /// var userGrain = grainFactory.GetGrain&lt;IUserIdentityGrain&gt;(userId);
//   /// 
//   /// // Remove a custom attribute
//   /// var updatedUser = await userGrain.RemoveCustomAttributeAsync("Department");
//   /// </code>
//   /// </example>
//   [Alias("RemoveCustomAttributeAsync")]
//   ValueTask<GetResponse> RemoveCustomAttributeAsync(string key);
}