namespace Bureau.Server.Contracts
{
    /// <summary>
    /// Provides functionality for obfuscating and de-obfuscating integer identifiers
    /// used in public server contracts.
    /// </summary>
    /// <remarks>
    /// This abstraction is intended to hide internal sequential integer identifiers
    /// (for example database primary keys) when exposing them through public APIs.
    /// Implementations should be deterministic and reversible, but must not be relied
    /// upon as a security mechanism.
    /// </remarks>
    public interface IIdObfuscator
    {
        /// <summary>
        /// Encodes an internal integer identifier into an obfuscated string
        /// representation suitable for use in public API contracts.
        /// </summary>
        /// <param name="value">
        /// The internal integer identifier to encode. Must be a non-negative value.
        /// </param>
        /// <returns>
        /// A string representation of the encoded identifier.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="value"/> is negative.
        /// </exception>
        string Encode(int value);

        /// <summary>
        /// Decodes an obfuscated identifier string back into its internal integer
        /// representation.
        /// </summary>
        /// <param name="id">
        /// The obfuscated identifier string received from an external client.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the decoded integer identifier on success.
        /// If decoding fails, returns a failure result describing the problem.
        /// </returns>
        /// <remarks>
        /// Implementations should return a failure result (rather than throwing)
        /// when the identifier cannot be decoded or is invalid.
        /// </remarks>
        Result<int> Decode(string? id);
    }
}
