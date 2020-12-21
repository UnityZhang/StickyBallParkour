
namespace Game.Collections
{
    /// <summary>
    /// Interface for an object that can be added to an object pool.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Gets or sets the index of the object in the pool. This value should never be used by anything
        /// other than the pool that owns this object.
        /// </summary>
        int poolIndex { get; set; }
    }
}